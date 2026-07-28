using System.Security.Claims;
using Hms.Api.Domain;
using Hms.Api.Features.Kitchen;
using Hms.Api.Features.Permissions;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Orders;

public static class OrdersEndpoint
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/orders").WithTags("Orders").RequireAuthorization("Operations");

        g.MapGet("/open", async (OrderService svc, CancellationToken ct, Guid? locationId) =>
            Results.Ok((await svc.ListOpenAsync(locationId, ct)).Select(ToDto)))
            .WithName("Orders.ListOpen")
            .WithSummary("List open + confirmed orders (the POS tab strip).");

        // Handheld tab strip — light DTO + per-order kitchen readiness (aggregated across
        // its station tickets) so a waiter can see at a glance what's still cooking vs ready.
        g.MapGet("/open-tabs", async (OrderService svc, ITenantDbContextFactory f, CancellationToken ct, Guid? locationId) =>
        {
            var orders = await svc.ListOpenAsync(locationId, ct);
            await using var db = await f.CreateForCurrentAsync(ct);
            var ids = orders.Select(o => o.Id).ToList();
            var byOrder = (await db.KitchenTickets.AsNoTracking()
                    .Where(t => ids.Contains(t.OrderId) && t.Status != "void")
                    .Select(t => new { t.OrderId, t.Status }).ToListAsync(ct))
                .GroupBy(t => t.OrderId).ToDictionary(grp => grp.Key, grp => grp.Select(x => x.Status).ToHashSet());
            var locById = await db.Locations.AsNoTracking().Select(l => new { l.Id, l.Code }).ToDictionaryAsync(l => l.Id, l => l.Code, ct);
            string kitchen(Guid orderId)
            {
                if (!byOrder.TryGetValue(orderId, out var s) || s.Count == 0) return "building";   // not sent yet
                if (s.All(x => x == "served")) return "served";
                if (s.All(x => x is "ready" or "served") && s.Contains("ready")) return "ready";    // all stations done
                return "preparing";                                                                 // still cooking
            }
            return Results.Ok(orders.Select(o => new
            {
                o.Id, o.OrderNumber, o.OrderType, o.OrderSource, o.TableLabel, o.TableId, o.Covers, o.Status,
                o.TotalAmount, o.OpenedAt, o.LocationId,
                locationCode = locById.GetValueOrDefault(o.LocationId),
                itemCount = o.Items.Count(i => !i.IsDeleted),
                kitchenStatus = kitchen(o.Id),
            }));
        }).WithName("Orders.OpenTabs")
          .WithSummary("Handheld open tabs — adds per-order kitchen readiness (building|preparing|ready|served).");

        g.MapGet("/{id:guid}", async (Guid id, OrderService svc, CancellationToken ct) =>
        {
            var o = await svc.GetAsync(id, ct);
            return o is null ? Results.NotFound() : Results.Ok(ToDto(o));
        }).WithName("Orders.Get");

        g.MapPost("", async (CreateOrderInput input, OrderService svc, LocationScope scope, CancellationToken ct) =>
        {
            try
            {
                scope.Assert(input.LocationId);   // #scoping-P2: a pinned user can only open orders at their outlet
                var o = await svc.CreateAsync(input, ct);
                return Results.Created($"/api/v1/orders/{o.Id}", ToDto(o));
            }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Orders.Create").WithSummary("Open a new order/tab (POS needs an open shift).");

        g.MapPost("/{id:guid}/items", async (Guid id, AddItemInput input, OrderService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(ToDto(await svc.AddItemAsync(id, input, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Orders.AddItem").WithSummary("Add a line item to an open order.");

        g.MapPut("/{id:guid}/items/{itemId:guid}", async (Guid id, Guid itemId, UpdateQtyInput body, OrderService svc, PermissionService perms, Hms.Api.Features.Audit.AuditService audit, ClaimsPrincipal user, CancellationToken ct) =>
        {
            try
            {
                var gate = await GateFiredItemVoidAsync(svc, perms, audit, user, id, itemId, body.Quantity, body.ManagerPin, ct);
                if (gate is not null) return gate;
                return Results.Ok(ToDto(await svc.UpdateItemQtyAsync(id, itemId, body.Quantity, ct)));
            }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Orders.UpdateItemQty").WithSummary("Change a line quantity (0 removes the line). Voiding a line already sent to the kitchen needs void permission / manager PIN.");

        g.MapDelete("/{id:guid}/items/{itemId:guid}", async (Guid id, Guid itemId, string? managerPin, OrderService svc, PermissionService perms, Hms.Api.Features.Audit.AuditService audit, ClaimsPrincipal user, CancellationToken ct) =>
        {
            try
            {
                var gate = await GateFiredItemVoidAsync(svc, perms, audit, user, id, itemId, 0, managerPin, ct);
                if (gate is not null) return gate;
                return Results.Ok(ToDto(await svc.RemoveItemAsync(id, itemId, ct)));
            }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Orders.RemoveItem");

        g.MapPost("/{id:guid}/custom-item", async (Guid id, CustomItemInput body, OrderService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(ToDto(await svc.AddCustomItemAsync(id, body.Name, body.UnitPrice, body.Quantity ?? 1, body.Station, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Orders.AddCustomItem").WithSummary("Add an open/custom-priced line (cashier types name + price).");

        g.MapPost("/{id:guid}/discount", async (Guid id, DiscountInput body, OrderService svc, PermissionService perms, Hms.Api.Features.Audit.AuditService audit, ClaimsPrincipal user, CancellationToken ct) =>
        {
            try
            {
                // #71: a role's max-discount cap is enforced here. Owner bypasses; a
                // manager PIN (#71b) can authorise an over-limit discount at the till.
                string? approvedBy = null;
                if (!PermissionService.IsOwner(user))
                {
                    var rp = await perms.GetForRoleAsync(PermissionService.RoleIdOf(user), ct);
                    var wantsDiscount = (body.Amount ?? 0) > 0 || (body.Percent ?? 0) > 0;
                    var pct = body.Percent ?? 0m;
                    if (body.Amount is > 0)
                    {
                        var o = await svc.GetAsync(id, ct);
                        var sub = o?.Items.Where(i => !i.IsDeleted).Sum(i => i.LineSubtotal) ?? 0m;
                        pct = sub > 0 ? Math.Round(body.Amount.Value / sub * 100m, 2) : 0m;
                    }
                    var blocked = (wantsDiscount && !rp.CanApplyDiscount) || pct > rp.MaxDiscountPercent + 0.001m;
                    if (blocked)
                    {
                        var approver = await perms.ResolveApproverAsync(body.ManagerPin,
                            p => p.CanApplyDiscount && pct <= p.MaxDiscountPercent + 0.001m, ct);
                        if (approver is null)
                            return Results.Json(new { error = $"Discount {pct:0.#}% exceeds your limit — manager approval required.", needsApproval = true }, statusCode: 403);
                        approvedBy = approver.DisplayName;
                    }
                }
                var d = await svc.SetDiscountAsync(id, body.Amount, body.Percent, ct);
                await audit.LogAsync("order.discount", "order", d.Id,
                    $"Discount {(body.Percent is { } pp ? $"{pp}%" : $"LKR {body.Amount ?? 0:F2}")} on {d.OrderNumber}{(approvedBy is null ? "" : $" (approved by {approvedBy})")}", ct);
                return Results.Ok(ToDto(d));
            }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Orders.SetDiscount").WithSummary("Apply a fixed or percentage discount to the bill (role-capped).");

        g.MapPost("/{id:guid}/confirm", async (Guid id, OrderService svc, ITenantContext tenant, KitchenBroadcaster bus, CancellationToken ct) =>
        {
            try { var o = await svc.ConfirmAsync(id, ct); bus.Publish(tenant.TenantIdOrThrow()); return Results.Ok(ToDto(o)); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Orders.Confirm").WithSummary("Send to KOT/BOT — R2: generates kitchen tickets.");

        // A steward accepting a guest QR order (#108). Functionally the same as Confirm —
        // reuses ConfirmAsync — but only for an order that's actually awaiting acceptance,
        // so it can't be used to short-circuit a cashier's in-progress POS cart.
        g.MapPost("/{id:guid}/accept", async (Guid id, OrderService svc, ITenantContext tenant, KitchenBroadcaster bus, Hms.Api.Features.Realtime.RealtimeBus rtBus, CancellationToken ct) =>
        {
            var existing = await svc.GetAsync(id, ct);
            if (existing is null) return Results.NotFound();
            if (!existing.PendingAcceptance) return Results.BadRequest(new { error = "This order isn't awaiting acceptance." });
            try
            {
                var o = await svc.ConfirmAsync(id, ct);
                var tid = tenant.TenantIdOrThrow();
                bus.Publish(tid); rtBus.Publish(tid, "orders");
                return Results.Ok(ToDto(o));
            }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Orders.Accept").WithSummary("Steward accepts a guest QR order and sends it to the kitchen.");

        g.MapPost("/{id:guid}/settle", async (Guid id, SettleInput input, OrderService svc, Hms.Api.Features.Audit.AuditService audit, ITenantContext tenant, KitchenBroadcaster bus, Hms.Api.Features.Realtime.RealtimeBus rtBus, CancellationToken ct) =>
        {
            try
            {
                var o = await svc.SettleAsync(id, input, ct);
                await audit.LogAsync("order.settle", "order", o.Id, $"Settled {o.InvoiceNumber ?? o.OrderNumber} · LKR {o.TotalAmount:F2}", ct);
                bus.Publish(tenant.TenantIdOrThrow());   // board flips the ticket to PAID
                rtBus.Publish(tenant.TenantIdOrThrow(), "orders");   // dashboard/bell/delivery react (e.g. aggregator order clears)
                rtBus.Publish(tenant.TenantIdOrThrow(), "availability");   // stock just dropped — POS/Tab/Guest re-check 86 (#112)
                return Results.Ok(ToDto(o));
            }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Orders.Settle").WithSummary("Settle the bill — R1: decrements stock atomically with payment.");

        g.MapPost("/{id:guid}/void", async (Guid id, VoidInput? body, OrderService svc, PermissionService perms, Hms.Api.Features.Audit.AuditService audit, ClaimsPrincipal user, ITenantContext tenant, KitchenBroadcaster bus, Hms.Api.Features.Realtime.RealtimeBus rtBus, CancellationToken ct) =>
        {
            try
            {
                // #71: voiding a bill is gated by the role's can-void permission. Owner
                // bypasses; a manager PIN (#71b) can authorise it at the till.
                string? approvedBy = null;
                if (!PermissionService.IsOwner(user))
                {
                    var rp = await perms.GetForRoleAsync(PermissionService.RoleIdOf(user), ct);
                    if (!rp.CanVoid)
                    {
                        var approver = await perms.ResolveApproverAsync(body?.ManagerPin, p => p.CanVoid, ct);
                        if (approver is null)
                            return Results.Json(new { error = "Your role can't void bills — manager approval required.", needsApproval = true }, statusCode: 403);
                        approvedBy = approver.DisplayName;
                    }
                }
                var o = await svc.VoidAsync(id, body?.Reason, ct);
                await audit.LogAsync("order.void", "order", o.Id, $"Voided {o.OrderNumber}{(string.IsNullOrWhiteSpace(body?.Reason) ? "" : $" · {body!.Reason}")}{(approvedBy is null ? "" : $" (approved by {approvedBy})")}", ct);
                bus.Publish(tenant.TenantIdOrThrow());   // tickets came off the board
                rtBus.Publish(tenant.TenantIdOrThrow(), "orders");   // dashboard/bell/delivery react
                return Results.Ok(ToDto(o));
            }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Orders.Void");

        // Merge another open order's lines into this one (combine tables onto one bill).
        g.MapPost("/{id:guid}/merge", async (Guid id, MergeInput body, OrderService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(ToDto(await svc.MergeAsync(id, body.SourceOrderId, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Orders.Merge").WithSummary("Merge a source order into this one and void the source.");

        // Move this order to a different table.
        g.MapPost("/{id:guid}/transfer", async (Guid id, TransferTableInput body, OrderService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(ToDto(await svc.TransferTableAsync(id, body.TableId, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Orders.TransferTable").WithSummary("Move the order to another table.");

        // Split selected item quantities onto a new bill (#69 item-level split).
        g.MapPost("/{id:guid}/split", async (Guid id, SplitInput body, OrderService svc, CancellationToken ct) =>
        {
            try
            {
                var moves = (body.Lines ?? new()).Select(l => new SplitMove(l.ItemId, l.Quantity)).ToList();
                return Results.Ok(ToDto(await svc.SplitAsync(id, moves, ct)));
            }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Orders.Split").WithSummary("Split chosen items (with optional partial qty) onto a new bill at the same table.");

        // Set covers / steward / tour operator on the bill (#76).
        g.MapPost("/{id:guid}/meta", async (Guid id, OrderMetaInput body, OrderService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(ToDto(await svc.SetMetaAsync(id, body.Covers, body.StewardId, body.TourOperatorId, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Orders.SetMeta").WithSummary("Set covers / steward / tour operator on the bill.");

        // Set the discretionary tip on the bill (#76).
        g.MapPost("/{id:guid}/tip", async (Guid id, TipInput body, OrderService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(ToDto(await svc.SetTipAsync(id, body.Amount, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Orders.SetTip").WithSummary("Set a tip on the bill (added on top, not taxed).");

        // Attach / detach a CRM customer (applies their default discount). #70
        g.MapPost("/{id:guid}/customer", async (Guid id, AttachCustomerInput body, OrderService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(ToDto(await svc.AttachCustomerAsync(id, body.CustomerId, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Orders.AttachCustomer").WithSummary("Attach a customer to the bill + apply their discount.");

        g.MapDelete("/{id:guid}/customer", async (Guid id, OrderService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(ToDto(await svc.DetachCustomerAsync(id, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Orders.DetachCustomer");

        // Compliant SL VAT tax invoice (header + lines + charge breakdown).
        g.MapGet("/{id:guid}/invoice", async (Guid id, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var o = await db.Orders.Include(x => x.Items).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (o is null) return Results.NotFound();
            var settings = await db.OrgSettings.AsNoTracking().FirstOrDefaultAsync(ct);
            var charges = await db.OrderCharges.AsNoTracking()
                .Where(c => c.OrderId == id).OrderBy(c => c.Name)
                .Select(c => new { c.Name, c.RatePercent, c.BaseAmount, c.ChargeAmount }).ToListAsync(ct);
            return Results.Ok(new
            {
                invoiceNumber = o.InvoiceNumber, isTaxInvoice = o.IsTaxInvoice, o.OrderNumber,
                settledAt = o.SettledAt,
                supplier = new {
                    legalName = settings?.LegalName, vatNo = settings?.VatRegistrationNumber,
                    brNo = settings?.BusinessRegistrationNo, svatNo = settings?.SvatNumber,
                    footer = settings?.TaxInvoiceFooter,
                    billHeader = settings?.BillHeader, billFooter = settings?.BillFooter,
                    showVatOnBills = settings?.ShowVatOnBills ?? true,
                },
                customer = new { name = o.CustomerName, vatNo = o.CustomerVatNo },
                o.TableLabel, o.Covers, o.ReprintCount,
                lines = o.Items.Where(i => !i.IsDeleted).OrderBy(i => i.CreatedAt).ThenBy(i => i.Id).Select(i => new {
                    i.ProductName, i.Quantity, i.UnitPrice, i.LineSubtotal }),
                o.SubtotalAmount, o.DiscountAmount, o.PromotionDiscountAmount, charges,
                o.ServiceChargeAmount, o.TaxAmount, o.TotalAmount,
            });
        }).WithName("Orders.Invoice").WithSummary("Compliant SL VAT tax invoice for the order.");

        // Recall — recent settled bills (search by number / invoice / customer) for reprint.
        g.MapGet("/settled", async (ITenantDbContextFactory f, CancellationToken ct, string? search, int? limit) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var q = db.Orders.AsNoTracking().Where(o => o.Status == "settled");
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = $"%{search.Trim()}%";
                q = q.Where(o => EF.Functions.ILike(o.OrderNumber, s)
                    || (o.InvoiceNumber != null && EF.Functions.ILike(o.InvoiceNumber, s))
                    || (o.CustomerName != null && EF.Functions.ILike(o.CustomerName, s)));
            }
            var list = await q.OrderByDescending(o => o.SettledAt).Take(Math.Clamp(limit ?? 30, 1, 100))
                .Select(o => new { o.Id, o.OrderNumber, o.InvoiceNumber, o.TableLabel, o.CustomerName, o.TotalAmount, o.SettledAt, o.ReprintCount })
                .ToListAsync(ct);
            return Results.Ok(list);
        }).WithName("Orders.Settled").WithSummary("Recent settled bills for recall + reprint.");

        // Reprint — record a duplicate copy (the bill print marks REPRINT + copy no).
        g.MapPost("/{id:guid}/reprint", async (Guid id, ITenantDbContextFactory f, Hms.Api.Features.Audit.AuditService audit, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var o = await db.Orders.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (o is null) return Results.NotFound();
            if (o.Status != "settled") return Results.BadRequest(new { error = "Only settled bills can be reprinted" });
            o.ReprintCount += 1;
            await db.SaveChangesAsync(ct);
            await audit.LogAsync("order.reprint", "order", o.Id, $"Reprint #{o.ReprintCount} of {o.InvoiceNumber ?? o.OrderNumber}", ct);
            return Results.Ok(new { o.Id, reprintCount = o.ReprintCount });
        }).WithName("Orders.Reprint").WithSummary("Record a duplicate-bill reprint (copy count).");

        // E-receipt (#79) — email (Resend) or SMS (Sender RT) a settled bill to the guest.
        g.MapPost("/{id:guid}/send-receipt", async (Guid id, SendReceiptInput input, ITenantDbContextFactory f,
            Hms.Api.Features.Email.IEmailSender email, Hms.Api.Features.Sms.ISmsSender sms,
            Hms.Api.Features.Audit.AuditService audit, ILoggerFactory lf, CancellationToken ct) =>
        {
            var channel = (input.Channel ?? "").Trim().ToLowerInvariant();
            if (channel is not ("email" or "sms" or "whatsapp")) return Results.BadRequest(new { error = "Channel must be email, sms or whatsapp" });
            if (channel == "whatsapp") return Results.BadRequest(new { error = "WhatsApp receipts are coming soon — use email or SMS for now." });
            await using var db = await f.CreateForCurrentAsync(ct);
            var o = await db.Orders.Include(x => x.Items).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (o is null) return Results.NotFound();
            if (o.Status != "settled") return Results.BadRequest(new { error = "Only settled bills can be sent" });
            var settings = await db.OrgSettings.FirstOrDefaultAsync(ct);   // tracked — the usage meter is updated below

            // Add-on entitlement (#79): the tenant must have bought a tier that includes this channel.
            var entitled = (settings?.EReceiptChannels ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (!entitled.Contains(channel))
                return Results.BadRequest(new { error = $"E-receipts by {channel} aren’t on your plan — add the E-Receipts add-on in Settings → Billing." });

            // Rolling monthly meter: reset when the calendar month rolls over, then enforce the allowance.
            if (settings is not null && settings.EReceiptQuota > 0)
            {
                var now = DateTime.UtcNow;
                var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                if (settings.EReceiptPeriodStart is null || settings.EReceiptPeriodStart < monthStart)
                { settings.EReceiptUsed = 0; settings.EReceiptPeriodStart = monthStart; }
                if (settings.EReceiptUsed >= settings.EReceiptQuota)
                    return Results.BadRequest(new { error = $"Monthly e-receipt allowance reached ({settings.EReceiptUsed}/{settings.EReceiptQuota}). It resets next month, or raise the bundle in Settings → Billing." });
            }
            var ccy = settings?.BaseCurrency ?? "LKR";
            var bizName = string.IsNullOrWhiteSpace(settings?.LegalName) ? "RIT HMS" : settings!.LegalName!;

            // Recipient: explicit `to`, else the bill's customer contact on file.
            var to = string.IsNullOrWhiteSpace(input.To) ? null : input.To!.Trim();
            if (to is null && o.CustomerId is Guid cid)
            {
                var c = await db.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == cid, ct);
                to = channel == "email" ? c?.Email : c?.Phone;
            }
            if (string.IsNullOrWhiteSpace(to))
                return Results.BadRequest(new { error = channel == "email" ? "No email address on file — type one in." : "No phone number on file — type one in." });

            var no = o.InvoiceNumber ?? o.OrderNumber;
            // VAT-aware: a tax invoice (VAT broken out) → "Tax Invoice" + VAT line; else a plain itemised receipt.
            var isTaxInvoice = o.IsTaxInvoice && o.TaxAmount > 0m && (settings?.VatEnabled ?? true);
            bool ok;
            bool pdfAttached = false;
            if (channel == "email")
            {
                var logo = DecodeDataUri(settings?.LogoUrl);
                byte[]? pdf = null;
                try { pdf = Hms.Api.Features.Receipts.ReceiptPdf.Build(o, settings, ccy, bizName, isTaxInvoice, logo, o.CustomerName); }
                catch (Exception ex) { lf.CreateLogger("EReceipt").LogWarning(ex, "Receipt PDF generation failed for {No} — sending email without the attachment", no); }
                pdfAttached = pdf is { Length: > 0 };
                var subject = isTaxInvoice ? $"Tax Invoice {no} — {bizName}" : $"Your receipt {no} — {bizName}";
                var html = ReceiptEmailHtml(o, settings, ccy, bizName, isTaxInvoice, settings?.LogoUrl, o.CustomerName, pdf != null);
                var atts = pdf is { Length: > 0 }
                    ? new List<Hms.Api.Features.Email.EmailAttachment> { new($"{(isTaxInvoice ? "Tax-Invoice" : "Receipt")}-{no.Replace('/', '-')}.pdf", pdf) }
                    : null;
                ok = await email.SendAsync(to!, subject, html, ct, atts);
            }
            else
                ok = await sms.SendAsync(to!, $"{bizName}: thank you! {(isTaxInvoice ? "Tax invoice" : "Receipt")} {no} — total {ccy} {o.TotalAmount:N2}.", ct);

            if (!ok) return Results.Problem("The receipt couldn't be sent — the gateway may not be configured yet.", statusCode: 502);
            if (settings is not null && settings.EReceiptQuota > 0) { settings.EReceiptUsed += 1; await db.SaveChangesAsync(ct); }
            await audit.LogAsync("order.receipt_sent", "order", o.Id, $"{channel} receipt for {no} → {to}", ct);
            return Results.Ok(new { sent = true, channel, to, pdf = pdfAttached, used = settings?.EReceiptUsed ?? 0, quota = settings?.EReceiptQuota ?? 0 });
        }).WithName("Orders.SendReceipt").WithSummary("Email or SMS a receipt for a settled bill (#79).");

        return app;
    }

    /// <summary>Decode a `data:...;base64,XXXX` data URI (e.g. the tenant logo) to raw bytes; null if not one.</summary>
    private static byte[]? DecodeDataUri(string? dataUri)
    {
        if (string.IsNullOrWhiteSpace(dataUri)) return null;
        var i = dataUri.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
        if (i < 0) return null;
        try { return Convert.FromBase64String(dataUri[(i + 7)..]); } catch { return null; }
    }

    /// <summary>Branded HTML receipt for email delivery (#79) — logo header, greeting, itemised lines,
    /// VAT-aware totals, and a note that the full PDF invoice is attached. Mirrors the printed bill.</summary>
    private static string ReceiptEmailHtml(Order o, OrgSettings? s, string ccy, string bizName, bool isTaxInvoice, string? logoDataUri, string? customerName, bool hasPdf)
    {
        string Esc(string? v) => System.Net.WebUtility.HtmlEncode(v ?? "");
        string Money(decimal n) => $"{ccy} {n:N2}";
        var taxLabel = string.IsNullOrWhiteSpace(s?.TaxLabel) ? "VAT" : s!.TaxLabel;
        var green = "#15803d";
        var rows = string.Join("", o.Items.Where(i => !i.IsDeleted).OrderBy(i => i.CreatedAt).ThenBy(i => i.Id)
            .Select(i => $"<tr><td style='padding:5px 0'>{Esc(i.ProductName)} <span style='color:#999'>× {i.Quantity:0.##}</span></td><td style='padding:5px 0;text-align:right;white-space:nowrap'>{Money(i.LineSubtotal)}</td></tr>"));
        string Line(string label, decimal amt) => amt == 0 ? "" : $"<tr><td style='padding:2px 0;color:#555'>{Esc(label)}</td><td style='padding:2px 0;text-align:right;color:#555'>{Money(amt)}</td></tr>";
        var no = o.InvoiceNumber ?? o.OrderNumber;
        var when = (o.SettledAt ?? DateTime.UtcNow).ToString("yyyy-MM-dd HH:mm");
        var discount = o.DiscountAmount + o.PromotionDiscountAmount;
        var docName = isTaxInvoice ? "Tax Invoice" : "receipt";
        var hasLogo = !string.IsNullOrWhiteSpace(logoDataUri) && logoDataUri!.StartsWith("data:", StringComparison.OrdinalIgnoreCase);
        var logoHtml = hasLogo ? $"<img src='{logoDataUri}' alt='{Esc(bizName)}' style='max-height:54px;max-width:200px;margin-bottom:6px'><br>" : "";
        var greeting = string.IsNullOrWhiteSpace(customerName) ? "Hello," : $"Hi {Esc(customerName)},";
        var vatNoHtml = isTaxInvoice && !string.IsNullOrWhiteSpace(s?.VatRegistrationNumber)
            ? $"<div style='color:#999;font-size:11px'>{Esc(taxLabel)} Reg: {Esc(s!.VatRegistrationNumber)}</div>" : "";

        return $@"<div style='font-family:Arial,Helvetica,sans-serif;max-width:520px;margin:0 auto;color:#222'>
<div style='border-bottom:3px solid {green};padding-bottom:10px;margin-bottom:16px'>
  {logoHtml}<div style='font-size:18px;font-weight:bold;color:{green}'>{Esc(bizName)}</div>{vatNoHtml}
</div>
<p style='font-size:15px;margin:0 0 4px'>{greeting}</p>
<p style='color:#444;margin:0 0 14px'>Thank you for your visit. Here is your <b>{docName}</b>{(hasPdf ? " — a printable copy is attached as a PDF." : ".")}</p>
<div style='background:#f6f8f7;border-radius:8px;padding:10px 14px;margin-bottom:14px;font-size:13px'>
  <b>{(isTaxInvoice ? "Tax Invoice" : "Receipt")} No.</b> &nbsp; {Esc(no)}<br>
  <b>Date</b> &nbsp; {when}{(string.IsNullOrWhiteSpace(o.TableLabel) ? "" : $"<br><b>Table</b> &nbsp; {Esc(o.TableLabel)}")}
</div>
<table style='width:100%;border-collapse:collapse;font-size:14px'>{rows}
<tr><td colspan='2'><hr style='border:none;border-top:1px solid #e5e5e5;margin:8px 0'></td></tr>
{Line("Subtotal", o.SubtotalAmount)}{Line("Discount", -discount)}{Line("Service charge", o.ServiceChargeAmount)}{(isTaxInvoice ? Line(taxLabel, o.TaxAmount) : "")}
<tr><td style='padding:8px 0;font-weight:bold;font-size:16px'>Total</td><td style='padding:8px 0;text-align:right;font-weight:bold;font-size:16px'>{Money(o.TotalAmount)}</td></tr>
</table>
<p style='color:#999;font-size:12px;margin-top:18px;border-top:1px solid #eee;padding-top:10px'>{(string.IsNullOrWhiteSpace(s?.BillFooter) ? "We hope to see you again soon." : Esc(s!.BillFooter))}<br>This is an automated email, please do not reply.</p>
</div>";
    }

    /// <summary>#71b: reducing/removing a line ALREADY sent to the kitchen is a void — it needs the role's
    /// can-void permission or a manager PIN (same gate as voiding a bill). Returns a 403 IResult when blocked,
    /// or null to proceed (auditing the void when it's allowed). No gate for un-fired ("pending") lines.</summary>
    private static async Task<IResult?> GateFiredItemVoidAsync(OrderService svc, PermissionService perms, Hms.Api.Features.Audit.AuditService audit, ClaimsPrincipal user, Guid orderId, Guid itemId, decimal newQty, string? managerPin, CancellationToken ct)
    {
        var st = await svc.GetItemKotAsync(orderId, itemId, ct);
        if (st is not { } s || s.KotStatus == "pending" || newQty >= s.Quantity) return null;   // not fired, or not a reduction → no gate
        string? approvedBy = null;
        if (!PermissionService.IsOwner(user))
        {
            var rp = await perms.GetForRoleAsync(PermissionService.RoleIdOf(user), ct);
            if (!rp.CanVoid)
            {
                var approver = await perms.ResolveApproverAsync(managerPin, p => p.CanVoid, ct);
                if (approver is null)
                    return Results.Json(new { error = "That item was already sent to the kitchen — manager approval is required to void it.", needsApproval = true }, statusCode: 403);
                approvedBy = approver.DisplayName;
            }
        }
        await audit.LogAsync("order.item_void", "order", orderId, $"Voided fired line ({s.Quantity:0.##} → {newQty:0.##}){(approvedBy is null ? "" : $" (approved by {approvedBy})")}", ct);
        return null;
    }

    private static object ToDto(Order o) => new
    {
        o.Id, o.OrderNumber, o.OrderType, o.OrderSource, o.ExternalOrderId,
        o.TableLabel, o.TableId, o.Covers, o.Status, o.PendingAcceptance, o.LocationId,
        o.StewardId, o.TourOperatorId, o.TourCommissionAmount,
        o.SubtotalAmount, o.DiscountAmount, o.PromotionDiscountAmount, o.ServiceChargeAmount, o.TaxAmount, o.TipAmount, o.TotalAmount,
        o.OpenedAt, o.ConfirmedAt, o.SettledAt,
        o.InvoiceNumber, o.IsTaxInvoice, o.CustomerName, o.CustomerId, o.CustomerVatNo,
        // Stable order: items must never reshuffle when one line is edited (an
        // unordered Include returns rows in a DB order that shifts after UPDATE,
        // which made the POS list jump under the cashier's finger).
        items = o.Items.Where(i => !i.IsDeleted).OrderBy(i => i.CreatedAt).ThenBy(i => i.Id).Select(i => new
        {
            i.Id, i.ProductId, i.Sku, i.ProductName, i.VariantId, i.VariantName, i.Quantity, i.UnitPrice,
            i.DiscountAmount, i.LineSubtotal, i.TaxRate, i.TaxAmount, i.LineTotal,
            i.Station, i.Notes, i.KotStatus,
            modifiers = i.Modifiers.Where(m => !m.IsDeleted).Select(m => new { m.Name, m.PriceDelta }),
        }),
    };
}

public record VoidInput(string? Reason, string? ManagerPin = null);
public record AttachCustomerInput(Guid CustomerId);
public record MergeInput(Guid SourceOrderId);
public record SplitInput(List<SplitLineInput>? Lines);
public record SplitLineInput(Guid ItemId, decimal Quantity);
public record TransferTableInput(Guid TableId);
public record UpdateQtyInput(decimal Quantity, string? ManagerPin = null);
public record OrderMetaInput(int? Covers, Guid? StewardId, Guid? TourOperatorId);
public record TipInput(decimal Amount);
public record SendReceiptInput(string? Channel, string? To);
public record DiscountInput(decimal? Amount, decimal? Percent, string? ManagerPin = null);
public record CustomItemInput(string Name, decimal UnitPrice, decimal? Quantity, string? Station);
