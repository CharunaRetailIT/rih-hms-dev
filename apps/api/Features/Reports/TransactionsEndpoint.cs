using Hms.Api.Features.Orders;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Reports;

/// <summary>
/// Transaction explorer — a payment-gateway-style log of every bill (settled,
/// void, open) with tender methods, cashier/steward, filters + search, and a
/// row drill-down to items, charge breakdown and individual tenders. Read-only,
/// back-office (Owner/Manager/Accountant). The transaction date is
/// settled_at ?? voided_at ?? opened_at.
/// </summary>
public static class TransactionsEndpoint
{
    public static IEndpointRouteBuilder MapTransactionEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/transactions").WithTags("Transactions").RequireAuthorization("BackOffice");

        // List + filter. Returns one row per order with its tender methods.
        g.MapGet("", async (ITenantDbContextFactory f, CancellationToken ct,
            DateTime? from, DateTime? to, string? status, string? payType, Guid? locationId, string? search,
            int? limit, int pageNumber = 1, int pageSize = 25) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            pageSize = Math.Clamp(limit ?? pageSize, 1, 100);

            await using var db = await f.CreateForCurrentAsync(ct);
            var toD = to is { } tt ? OrderService.AsUtc(tt) : OrderService.AsUtc(DateTime.UtcNow.Date.AddDays(1));
            var fromD = from is { } ff ? OrderService.AsUtc(ff) : toD.AddDays(-1);   // default: today

            var q = db.Orders.AsNoTracking()
                .Where(o => (o.SettledAt ?? o.VoidedAt ?? o.OpenedAt) >= fromD
                         && (o.SettledAt ?? o.VoidedAt ?? o.OpenedAt) < toD);
            if (!string.IsNullOrWhiteSpace(status)) q = q.Where(o => o.Status == status);
            if (locationId is { } loc) q = q.Where(o => o.LocationId == loc);
            if (!string.IsNullOrWhiteSpace(payType))
                q = q.Where(o => db.Payments.Any(p => p.OrderId == o.Id && p.PayType == payType));
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = $"%{search.Trim()}%";
                q = q.Where(o => EF.Functions.ILike(o.OrderNumber, s)
                    || (o.InvoiceNumber != null && EF.Functions.ILike(o.InvoiceNumber, s))
                    || (o.CustomerName != null && EF.Functions.ILike(o.CustomerName, s)));
            }

            var totalCount = await q.CountAsync(ct);
            // Settled total across the WHOLE filtered range (not just the current page).
            var settledTotal = await q.Where(o => o.Status == "settled").SumAsync(o => o.TotalAmount, ct);

            var rows = await q
                .OrderByDescending(o => o.SettledAt ?? o.VoidedAt ?? o.OpenedAt)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(o => new
                {
                    o.Id, o.OrderNumber, o.InvoiceNumber, o.OrderType, o.OrderSource, o.Status,
                    o.CustomerName, o.CashierId, o.StewardId, o.LocationId, o.Covers,
                    o.TotalAmount, o.TipAmount, o.DiscountAmount, o.PromotionDiscountAmount, o.TaxAmount,
                    o.OpenedAt, o.SettledAt, o.VoidedAt, o.VoidReason,
                })
                .ToListAsync(ct);

            var ids = rows.Select(r => r.Id).ToList();
            // Tender methods per order (settled bills) + base totals.
            var tenders = ids.Count == 0 ? new() : await db.Payments.AsNoTracking()
                .Where(p => ids.Contains(p.OrderId))
                .Select(p => new { p.OrderId, p.PayType, p.CurrencyCode, p.BaseAmount, p.Amount })
                .ToListAsync(ct);
            var tendersByOrder = tenders.GroupBy(t => t.OrderId).ToDictionary(grp => grp.Key, grp => grp.ToList());

            // Resolve cashier/steward display names in one shot.
            var userIds = rows.SelectMany(r => new[] { r.CashierId, r.StewardId }).Where(x => x is { }).Select(x => x!.Value).Distinct().ToList();
            var names = userIds.Count == 0 ? new() : await db.Users.AsNoTracking()
                .Where(u => userIds.Contains(u.Id)).Select(u => new { u.Id, u.DisplayName }).ToListAsync(ct);
            var nameById = names.ToDictionary(u => u.Id, u => u.DisplayName);

            var locs = await db.Locations.AsNoTracking().Select(l => new { l.Id, l.Code }).ToListAsync(ct);
            var locById = locs.ToDictionary(l => l.Id, l => l.Code);

            var result = rows.Select(r =>
            {
                var ts = tendersByOrder.TryGetValue(r.Id, out var list) ? list : new();
                return new
                {
                    r.Id,
                    number = r.InvoiceNumber ?? r.OrderNumber,
                    r.OrderNumber, r.InvoiceNumber,
                    txnAt = r.SettledAt ?? r.VoidedAt ?? r.OpenedAt,
                    r.Status, r.OrderType, r.OrderSource,
                    location = r.LocationId is { } lid && locById.TryGetValue(lid, out var lc) ? lc : null,
                    customer = r.CustomerName,
                    cashier = r.CashierId is { } cid && nameById.TryGetValue(cid, out var cn) ? cn : null,
                    steward = r.StewardId is { } sid && nameById.TryGetValue(sid, out var sn) ? sn : null,
                    r.Covers, r.TotalAmount, r.TipAmount, r.VoidReason,
                    tenderMethods = ts.Select(t => t.PayType).Distinct().ToList(),
                    tenderSummary = ts.Select(t => new { t.PayType, t.Amount, t.CurrencyCode, t.BaseAmount }).ToList(),
                };
            }).ToList();

            return Results.Ok(new
            {
                from = fromD, to = toD, count = result.Count,
                total = settledTotal,
                rows = result,
                pagination = new PaginationMeta(totalCount, pageNumber, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize)),
            });
        }).WithName("Transactions.List").WithSummary("Filterable, paginated transaction log (all statuses) with tender methods.");

        // Drill-down: full detail for one transaction.
        g.MapGet("/{id:guid}", async (Guid id, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var o = await db.Orders.AsNoTracking().Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, ct);
            if (o is null) return Results.NotFound();
            var charges = await db.OrderCharges.AsNoTracking().Where(c => c.OrderId == id).OrderBy(c => c.Name)
                .Select(c => new { c.Name, c.RatePercent, c.BaseAmount, c.ChargeAmount }).ToListAsync(ct);
            var payments = await db.Payments.AsNoTracking().Where(p => p.OrderId == id)
                .Select(p => new { p.PayType, p.Amount, p.CurrencyCode, p.FxRate, p.BaseAmount, p.Reference, p.CreatedAt }).ToListAsync(ct);
            string? nameOf(Guid? uid) => uid is { } u ? db.Users.AsNoTracking().Where(x => x.Id == u).Select(x => x.DisplayName).FirstOrDefault() : null;

            return Results.Ok(new
            {
                o.Id, number = o.InvoiceNumber ?? o.OrderNumber, o.OrderNumber, o.InvoiceNumber, o.IsTaxInvoice,
                o.Status, o.OrderType, o.OrderSource, o.TableLabel, o.Covers,
                txnAt = o.SettledAt ?? o.VoidedAt ?? o.OpenedAt, o.OpenedAt, o.SettledAt, o.VoidedAt, o.VoidReason,
                customer = o.CustomerName, customerVatNo = o.CustomerVatNo,
                cashier = nameOf(o.CashierId), steward = nameOf(o.StewardId),
                o.SubtotalAmount, o.DiscountAmount, o.PromotionDiscountAmount, o.ServiceChargeAmount,
                o.TaxAmount, o.TipAmount, o.TotalAmount, o.TourCommissionAmount,
                items = o.Items.Where(i => !i.IsDeleted).OrderBy(i => i.CreatedAt).ThenBy(i => i.Id).Select(i => new
                {
                    i.ProductName, i.VariantName, i.Quantity, i.UnitPrice, i.LineTotal, i.Station,
                    modifiers = i.Modifiers.Where(m => !m.IsDeleted).Select(m => new { m.Name, m.PriceDelta }),
                }),
                charges, payments,
            });
        }).WithName("Transactions.Detail").WithSummary("Full drill-down: items, charges, tenders for one transaction.");

        return app;
    }
}

public record PaginationMeta(int TotalCount, int PageNumber, int PageSize, int TotalPages);
