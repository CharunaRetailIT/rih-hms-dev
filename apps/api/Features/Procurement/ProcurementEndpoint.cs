using System.Security.Claims;
using Hms.Api.Features.Permissions;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Procurement;

public static class ProcurementEndpoint
{
    public static IEndpointRouteBuilder MapProcurementEndpoints(this IEndpointRouteBuilder app)
    {
       //var sup = app.MapGroup("/api/v1/suppliers").WithTags("Suppliers").RequireAuthorization("SupplyChain");
        var po = app.MapGroup("/api/v1/purchase-orders").WithTags("Purchase orders").RequireAuthorization("SupplyChain");
        var grn = app.MapGroup("/api/v1/grn").WithTags("Goods received").RequireAuthorization("SupplyChain");

        //sup.MapGet("", async (ProcurementService svc, CancellationToken ct) =>
        //    Results.Ok(await svc.ListSuppliersAsync(ct))).WithName("Suppliers.List");

        //sup.MapPost("", async (SupplierInput i, ProcurementService svc, CancellationToken ct) =>
        //{
        //    try { var s = await svc.CreateSupplierAsync(i, ct); return Results.Created($"/api/v1/suppliers/{s.Id}", new { s.Id, s.Code, s.Name }); }
        //    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        //}).WithName("Suppliers.Create");

        po.MapGet("/tax-rate", async (Guid locationId, ProcurementService svc, CancellationToken ct) =>
        {
            var (rate, flat) = await svc.GetLocationTaxRateAsync(locationId, ct);
            return Results.Ok(new { rate, flat });
        }).WithName("PurchaseOrders.LocationTaxRate")
          .WithSummary("Combined tax-type Charge rate for a location — shared by the PO and GRN forms.");

        po.MapGet("", async (ProcurementService svc, CancellationToken ct) =>
        {
            var list = await svc.ListPosAsync(ct);
            return Results.Ok(list.Select(p => new {
                p.Id, p.PoNumber, p.SupplierId, p.LocationId, p.Status, p.OrderDate, p.TotalAmount }));
        }).WithName("PurchaseOrders.List");

        po.MapGet("/paged", async (
            ProcurementService svc, CancellationToken ct,
            int pageNumber = 1, int pageSize = 10, string? search = null,
            Guid? supplierId = null, Guid? locationId = null, string? status = null) =>
        {
            var result = await svc.GetPoPagedAsync(pageNumber, pageSize, search, supplierId, locationId, status, ct);
            return Results.Ok(result);
        }).WithName("PurchaseOrders.ListPaged");

        po.MapGet("/{id:guid}", async (Guid id, ProcurementService svc, CancellationToken ct) =>
        {
            var p = await svc.GetPoAsync(id, ct);
            return p is null ? Results.NotFound() : Results.Ok(PoDto(p));
        }).WithName("PurchaseOrders.Get");

        po.MapPost("", async (CreatePoInput i, ProcurementService svc, CancellationToken ct) =>
        {
            try { return Results.Created($"/api/v1/purchase-orders", PoDto(await svc.CreatePoAsync(i, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("PurchaseOrders.Create").WithSummary("Create a PO with lines.");

        po.MapPut("/{id:guid}", async (Guid id, CreatePoInput i, ProcurementService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(PoDto(await svc.EditPoAsync(id, i, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("PurchaseOrders.Edit").WithSummary("Edit a draft PO.");

        po.MapPost("/{id:guid}/submit", async (Guid id, ClaimsPrincipal user, ProcurementService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(PoDto(await svc.SubmitPoAsync(id, CurrentUserId(user), ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("PurchaseOrders.Submit").WithSummary("Submit a draft — resolves to approved or pending depending on approval policy.");

        po.MapPost("/{id:guid}/approve", async (Guid id, ClaimsPrincipal user, ProcurementService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(PoDto(await svc.ApprovePoAsync(id, CurrentUserId(user), PermissionService.RoleIdOf(user), CurrentUserName(user), ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("PurchaseOrders.Approve").WithSummary("Approve a PO awaiting authorisation.");

        po.MapPost("/{id:guid}/reject", async (Guid id, ClaimsPrincipal user, ReasonInput? body, ProcurementService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(PoDto(await svc.RejectPoAsync(id, CurrentUserId(user), PermissionService.RoleIdOf(user), CurrentUserName(user), body?.Reason, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("PurchaseOrders.Reject");

        po.MapDelete("/{id:guid}", async (Guid id, ProcurementService svc, CancellationToken ct) =>
        {
            try { await svc.RemovePoAsync(id, ct); return Results.NoContent(); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("PurchaseOrders.Remove").WithSummary("Soft-delete a PO that hasn't been received yet.");

        grn.MapGet("", async (Hms.Api.Infrastructure.ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var list = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
                db.GoodsReceivedNotes.AsQueryable().OrderByDescending(g => g.CreatedAt).Take(100)
                    .Select(g => new { g.Id, g.GrnNumber, g.SupplierId, g.LocationId, g.PurchaseOrderId, g.Status, g.TotalCost, g.ReceivedDate }), ct);
            return Results.Ok(list);
        }).WithName("Grn.List");

        grn.MapGet("/paged", async (
            ProcurementService svc, CancellationToken ct,
            int pageNumber = 1, int pageSize = 10, string? search = null,
            Guid? supplierId = null, Guid? locationId = null, string? status = null,
            DateOnly? dateFrom = null, DateOnly? dateTo = null) =>
        {
            var result = await svc.GetGrnPagedAsync(pageNumber, pageSize, search, supplierId, locationId, status, dateFrom, dateTo, ct);
            return Results.Ok(result);
        }).WithName("Grn.Paged");

        grn.MapPost("", async (ReceiveInput i, ProcurementService svc, CancellationToken ct) =>
        {
            try { return Results.Created($"/api/v1/grn", GrnDto(await svc.ReceiveAsync(i, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Grn.Receive").WithSummary("Save a GRN as a draft — no stock impact until it's submitted and approved.");

        grn.MapGet("/{id:guid}", async (Guid id, ProcurementService svc, Hms.Api.Infrastructure.ITenantDbContextFactory f, CancellationToken ct) =>
        {
            var g = await svc.GetGrnAsync(id, ct);
            if (g is null) return Results.NotFound();

            // Trace each line back to whichever request note specifically asked for that
            // product, if this GRN's PO was itself bundled from one or more request notes —
            // purely informational (a TOG's "GRN based" mode surfaces this per line).
            object[] fulfilledRequestNotes = [];
            var noteByProduct = new Dictionary<Guid, (string Number, DateTime Date)>();
            if (g.PurchaseOrderId is Guid poId)
            {
                await using var db = await f.CreateForCurrentAsync(ct);
                fulfilledRequestNotes = await db.RequestNotes.AsNoTracking()
                    .Where(r => r.PurchaseOrderId == poId)
                    .Select(r => (object)new { r.Id, r.RequestNumber, r.ToLocationId, r.ExpectedDeliveryDate })
                    .ToArrayAsync(ct);

                var noteLines = await (
                    from rn in db.RequestNotes.AsNoTracking()
                    join rl in db.RequestNoteLines.AsNoTracking() on rn.Id equals rl.RequestId
                    where rn.PurchaseOrderId == poId
                    select new { rl.ProductId, rn.RequestNumber, rn.CreatedAt }
                ).ToListAsync(ct);
                foreach (var nl in noteLines)
                    noteByProduct.TryAdd(nl.ProductId, (nl.RequestNumber, nl.CreatedAt));
            }

            return Results.Ok(new
            {
                g.Id, g.GrnNumber, g.SupplierId, g.LocationId, g.PurchaseOrderId, g.Status, g.ReceivedDate,
                g.TotalCost, g.TaxAmount, g.SupplierInvoiceNo, g.PostedAt, g.VoidedAt, g.VoidReason,
                g.CurrencyCode, g.CurrencyRate, g.PaymentTermsDays, g.PaymentMethod,
                g.DiscountAmount, g.OtherCharges, g.ReferenceNo,
                g.SubmittedAt, g.ApprovedBy, g.ApprovedAt, g.RejectedBy, g.RejectedAt, g.RejectReason,
                lines = g.Lines.Where(l => !l.IsDeleted).Select(l => new {
                    l.ProductId, l.Sku, l.ProductName, l.QuantityReceived, l.UnitCost, l.LineTotal,
                    l.TaxRate, l.TaxAmount, l.BatchNo, l.ExpiryDate, l.UnitId, l.UnitSymbol, l.FreeQuantity, l.DiscountAmount, l.StockQuantity,
                    requestNoteNumber = noteByProduct.TryGetValue(l.ProductId, out var rn) ? rn.Number : null,
                    requestNoteDate = noteByProduct.TryGetValue(l.ProductId, out var rn2) ? rn2.Date : (DateTime?)null,
                }),
                fulfilledRequestNotes,
            });
        }).WithName("Grn.Get");

        grn.MapPost("/{id:guid}/submit", async (Guid id, ClaimsPrincipal user, ProcurementService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(GrnDto(await svc.SubmitGrnAsync(id, CurrentUserId(user), ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Grn.Submit").WithSummary("Submit a draft — resolves to approved (posts it) or pending depending on approval policy.");

        grn.MapPost("/{id:guid}/approve", async (Guid id, ClaimsPrincipal user, ProcurementService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(GrnDto(await svc.ApproveGrnAsync(id, CurrentUserId(user), PermissionService.RoleIdOf(user), CurrentUserName(user), ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Grn.Approve").WithSummary("Approve a GRN awaiting authorisation — posts it (stock + weighted-avg cost).");

        grn.MapPost("/{id:guid}/reject", async (Guid id, ClaimsPrincipal user, ReasonInput? body, ProcurementService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(GrnDto(await svc.RejectGrnAsync(id, CurrentUserId(user), PermissionService.RoleIdOf(user), CurrentUserName(user), body?.Reason, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Grn.Reject");

        grn.MapDelete("/{id:guid}", async (Guid id, ProcurementService svc, CancellationToken ct) =>
        {
            try { await svc.RemoveGrnAsync(id, ct); return Results.NoContent(); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Grn.Remove").WithSummary("Soft-delete a GRN that never posted to stock.");

        grn.MapPost("/{id:guid}/void", async (Guid id, VoidGrnInput? body, ProcurementService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(GrnDto(await svc.VoidGrnAsync(id, body?.Reason, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Grn.Void").WithSummary("Void a posted GRN — reverses the stock it added.");

        // ── supplier returns / debit notes (PRN) ──
        var ret = app.MapGroup("/api/v1/purchase-returns").WithTags("Purchase returns").RequireAuthorization("SupplyChain");

        ret.MapGet("", async (ProcurementService svc, CancellationToken ct) =>
        {
            var list = await svc.ListReturnsAsync(ct);
            return Results.Ok(list.Select(r => new { r.Id, r.PrnNumber, r.SupplierId, r.LocationId, r.Status, r.ReturnDate, r.TotalCost, r.TaxAmount }));
        }).WithName("PurchaseReturns.List");

        ret.MapGet("/{id:guid}", async (Guid id, ProcurementService svc, CancellationToken ct) =>
        {
            var r = await svc.GetReturnAsync(id, ct);
            return r is null ? Results.NotFound() : Results.Ok(PrnDto(r));
        }).WithName("PurchaseReturns.Get");

        ret.MapPost("", async (PurchaseReturnInput i, ProcurementService svc, CancellationToken ct) =>
        {
            try { return Results.Created("/api/v1/purchase-returns", PrnDto(await svc.ReturnToSupplierAsync(i, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("PurchaseReturns.Create").WithSummary("Return goods to a supplier → decrements stock, reverses input VAT.");

        ret.MapPost("/{id:guid}/void", async (Guid id, ReasonInput? body, ProcurementService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(PrnDto(await svc.VoidPurchaseReturnAsync(id, body?.Reason, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("PurchaseReturns.Void");

        return app;
    }

    private static object PoDto(Hms.Api.Domain.PurchaseOrder p) => new
    {
        p.Id, p.PoNumber, p.SupplierId, p.LocationId, p.Status, p.OrderDate, p.ExpectedDate,
        p.SubtotalAmount, p.DiscountAmount, p.Deductions, p.OtherCharges, p.TaxAmount, p.ExcludeTax, p.TotalAmount,
        p.CurrencyCode, p.CurrencyRate, p.DeliveryLocationId, p.DeliveryAddress, p.ReferenceNo, p.Notes,
        p.PaymentTermsDays, p.PaymentMethod,
        p.SubmittedAt, p.ApprovedBy, p.ApprovedAt, p.RejectedBy, p.RejectedAt, p.RejectReason,
        lines = p.Lines.Where(l => !l.IsDeleted).Select(l => new {
            l.Id, l.ProductId, l.Sku, l.ProductName, l.QuantityOrdered, l.QuantityReceived,
            l.UnitCost, l.LineTotal, l.TaxRate, l.TaxAmount, l.UnitId, l.UnitSymbol, l.DiscountAmount }),
    };

    private static object GrnDto(Hms.Api.Domain.GoodsReceivedNote g) => new
    {
        g.Id, g.GrnNumber, g.SupplierId, g.LocationId, g.PurchaseOrderId, g.Status, g.ReceivedDate,
        g.TotalCost, g.TaxAmount, g.SupplierInvoiceNo, g.PostedAt, g.VoidedAt, g.VoidReason,
        g.CurrencyCode, g.CurrencyRate, g.PaymentTermsDays, g.PaymentMethod,
        g.DiscountAmount, g.OtherCharges, g.ReferenceNo,
        g.SubmittedAt, g.ApprovedBy, g.ApprovedAt, g.RejectedBy, g.RejectedAt, g.RejectReason,
        lines = g.Lines.Where(l => !l.IsDeleted).Select(l => new {
            l.ProductId, l.Sku, l.ProductName, l.QuantityReceived, l.UnitCost, l.LineTotal,
            l.TaxRate, l.TaxAmount, l.BatchNo, l.ExpiryDate, l.UnitId, l.UnitSymbol, l.FreeQuantity, l.DiscountAmount, l.StockQuantity }),
    };

    private static object PrnDto(Hms.Api.Domain.PurchaseReturn r) => new
    {
        r.Id, r.PrnNumber, r.SupplierId, r.LocationId, r.GrnId, r.Status, r.ReturnDate,
        r.Reason, r.SupplierCreditNo, r.TotalCost, r.TaxAmount, r.PostedAt, r.VoidedAt, r.VoidReason, r.Notes,
        lines = r.Lines.Where(l => !l.IsDeleted).Select(l => new {
            l.ProductId, l.Sku, l.ProductName, l.Quantity, l.UnitId, l.UnitSymbol,
            l.UnitCost, l.LineTotal, l.TaxRate, l.TaxAmount, l.StockQuantity, l.BatchNo }),
    };

    /// <summary>The acting user's id from the JWT (sub / NameIdentifier), for approval audit.</summary>
    private static Guid? CurrentUserId(ClaimsPrincipal user)
    {
        var raw = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out var g) ? g : null;
    }

    private static string CurrentUserName(ClaimsPrincipal user) =>
        user.FindFirst("name")?.Value ?? user.FindFirst(ClaimTypes.Email)?.Value ?? "Approver";
}

public record ReasonInput(string? Reason);
