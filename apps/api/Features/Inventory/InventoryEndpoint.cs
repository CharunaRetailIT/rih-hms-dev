using System.Security.Claims;
using Hms.Api.Features.Permissions;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Inventory;

public static class InventoryEndpoint
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var inv = app.MapGroup("/api/v1/inventory").WithTags("Inventory").RequireAuthorization("SupplyChain");
        var tr = app.MapGroup("/api/v1/transfers").WithTags("Stock transfers").RequireAuthorization("SupplyChain");
        var adjDoc = app.MapGroup("/api/v1/stock-adjustments").WithTags("Stock adjustment documents").RequireAuthorization("SupplyChain");
        var wsDoc = app.MapGroup("/api/v1/wastage-notes").WithTags("Wastage documents").RequireAuthorization("SupplyChain");

        // Stock-on-hand across locations (the Inventory screen).
        inv.MapGet("/stock", async (ITenantDbContextFactory f, CancellationToken ct, Guid? locationId) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var q = from s in db.ProductStocks.AsNoTracking()
                    join p in db.Products.AsNoTracking() on s.ProductId equals p.Id
                    join l in db.Locations.AsNoTracking() on s.LocationId equals l.Id
                    where !p.IsDeleted
                    select new { s.ProductId, p.Sku, p.Name, p.ColorHex,
                                 ReorderLevel = p.ReorderLevel ?? 0m,
                                 LocationId = l.Id, LocationCode = l.Code, LocationName = l.Name,
                                 s.QuantityOnHand, s.AverageCost, s.LastReceivedAt,
                                 BelowReorder = (p.ReorderLevel ?? 0m) > 0m && s.QuantityOnHand <= (p.ReorderLevel ?? 0m),
                                 StockValue = s.QuantityOnHand * s.AverageCost };
            if (locationId.HasValue) q = q.Where(x => x.LocationId == locationId.Value);
            return Results.Ok(await q.OrderBy(x => x.Name).ToListAsync(ct));
        }).WithName("Inventory.Stock").WithSummary("Stock-on-hand per product per location.");

        // Near-expiry batches: posted GRN lines that carry an expiry date, within the
        // horizon (default 30 days, or already expired). Batch-level (received qty),
        // since stock-on-hand isn't tracked per batch. Drives the Inventory expiry view.
        inv.MapGet("/expiring", async (ITenantDbContextFactory f, CancellationToken ct, int? days, Guid? locationId) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var horizon = today.AddDays(days is int d && d > 0 ? d : 30);
            var q = from gl in db.GrnLines.AsNoTracking()
                    join g in db.GoodsReceivedNotes.AsNoTracking() on gl.GrnId equals g.Id
                    join l in db.Locations.AsNoTracking() on g.LocationId equals l.Id
                    where !gl.IsDeleted && g.Status == "posted"
                          && gl.ExpiryDate != null && gl.ExpiryDate <= horizon
                    select new {
                        gl.Id, gl.Sku, gl.ProductName, gl.BatchNo, gl.ExpiryDate,
                        gl.QuantityReceived, gl.StockQuantity, gl.UnitSymbol,
                        LocationId = l.Id, LocationCode = l.Code, LocationName = l.Name,
                        g.GrnNumber, g.ReceivedDate,
                    };
            if (locationId.HasValue) q = q.Where(x => x.LocationId == locationId.Value);
            var rows = await q.OrderBy(x => x.ExpiryDate).ThenBy(x => x.ProductName).ToListAsync(ct);
            return Results.Ok(rows.Select(x => new {
                x.Id, x.Sku, x.ProductName, x.BatchNo,
                expiryDate = x.ExpiryDate!.Value.ToString("yyyy-MM-dd"),
                daysToExpiry = x.ExpiryDate!.Value.DayNumber - today.DayNumber,
                x.QuantityReceived, x.StockQuantity, x.UnitSymbol,
                x.LocationCode, x.LocationName, x.GrnNumber,
                receivedDate = x.ReceivedDate.ToString("yyyy-MM-dd"),
            }));
        }).WithName("Inventory.Expiring").WithSummary("GRN batches at/near expiry (batch-level received qty).");

        tr.MapGet("/paged", async (
            InventoryMovementService svc, CancellationToken ct,
            int pageNumber, int pageSize, string? search, Guid? fromLocationId, Guid? toLocationId,
            string? status, bool? isReturn, DateOnly? dateFrom, DateOnly? dateTo) =>
            Results.Ok(await svc.GetTransfersPagedAsync(pageNumber, pageSize, search, fromLocationId, toLocationId, status, isReturn, dateFrom, dateTo, ct)))
            .WithName("Transfers.Paged").WithSummary("Server-paged list of transfer documents.");

        tr.MapPost("", async (CreateTransferInput i, InventoryMovementService svc, LocationScope scope, CancellationToken ct) =>
        {
            scope.Assert(i.FromLocationId);   // #scoping-P2: a pinned user can only transfer out of their own outlet
            return await Guard(() => svc.CreateTransferAsync(i, ct), TransferDto);
        }).WithName("Transfers.Create").WithSummary("Create a transfer (draft). Set isReturn for outlet→CK returns.");

        tr.MapPut("/{id:guid}", async (Guid id, CreateTransferInput i, InventoryMovementService svc, CancellationToken ct) =>
            await Guard(() => svc.EditTransferAsync(id, i, ct), TransferDto))
            .WithName("Transfers.Edit").WithSummary("Edit a draft transfer.");

        tr.MapPost("/{id:guid}/submit", async (Guid id, ClaimsPrincipal user, InventoryMovementService svc, CancellationToken ct) =>
            await Guard(() => svc.SubmitTransferAsync(id, CurrentUserId(user), ct), TransferDto))
            .WithName("Transfers.Submit").WithSummary("Submit a draft — resolves to dispatched (posts stock) or pending depending on approval policy.");

        tr.MapPost("/{id:guid}/approve", async (Guid id, ClaimsPrincipal user, InventoryMovementService svc, CancellationToken ct) =>
            await Guard(() => svc.ApproveTransferAsync(id, CurrentUserId(user), PermissionService.RoleIdOf(user), CurrentUserName(user), ct), TransferDto))
            .WithName("Transfers.Approve").WithSummary("Approve a transfer awaiting authorisation — dispatches it (decrements sender stock).");

        tr.MapPost("/{id:guid}/reject", async (Guid id, ClaimsPrincipal user, ReasonInput? body, InventoryMovementService svc, CancellationToken ct) =>
            await Guard(() => svc.RejectTransferAsync(id, CurrentUserId(user), PermissionService.RoleIdOf(user), CurrentUserName(user), body?.Reason, ct), TransferDto))
            .WithName("Transfers.Reject");

        tr.MapPost("/{id:guid}/receive", async (Guid id, InventoryMovementService svc, CancellationToken ct) =>
            await Guard(() => svc.ReceiveTransferAsync(id, ct), TransferDto))
            .WithName("Transfers.Receive").WithSummary("Receive → increment destination stock (weighted avg).");

        tr.MapDelete("/{id:guid}", async (Guid id, InventoryMovementService svc, CancellationToken ct) =>
        {
            try { await svc.RemoveTransferAsync(id, ct); return Results.NoContent(); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Transfers.Remove").WithSummary("Soft-delete a transfer that hasn't dispatched.");

        tr.MapGet("/{id:guid}", async (Guid id, InventoryMovementService svc, CancellationToken ct) =>
        {
            var t = await svc.GetTransferAsync(id, ct);
            return t is null ? Results.NotFound() : Results.Ok(TransferDto(t));
        }).WithName("Transfers.Get");

        adjDoc.MapGet("/paged", async (
            ITenantDbContextFactory f, InventoryMovementService svc, CancellationToken ct,
            int pageNumber, int pageSize, string? search, Guid? locationId, string? status, string? reason,
            DateOnly? dateFrom, DateOnly? dateTo) =>
            Results.Ok(await svc.GetAdjustmentsPagedAsync(pageNumber, pageSize, search, locationId, status, reason, dateFrom, dateTo, ct)))
            .WithName("StockAdjustments.Paged").WithSummary("Server-paged list of adjustment documents.");

        adjDoc.MapGet("/{id:guid}", async (Guid id, InventoryMovementService svc, CancellationToken ct) =>
        {
            var a = await svc.GetAdjustmentAsync(id, ct);
            return a is null ? Results.NotFound() : Results.Ok(AdjustmentDto(a));
        }).WithName("StockAdjustments.Get");

        adjDoc.MapPost("", async (AdjustmentDraftInput i, InventoryMovementService svc, LocationScope scope, CancellationToken ct) =>
        {
            scope.Assert(i.LocationId);   // #scoping-P2
            try { return Results.Created($"/api/v1/stock-adjustments", AdjustmentDto(await svc.CreateAdjustmentDraftAsync(i, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("StockAdjustments.Create").WithSummary("Create an adjustment document (draft).");

        adjDoc.MapPut("/{id:guid}", async (Guid id, AdjustmentDraftInput i, InventoryMovementService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(AdjustmentDto(await svc.EditAdjustmentDraftAsync(id, i, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("StockAdjustments.Edit").WithSummary("Edit a draft adjustment.");

        adjDoc.MapPost("/{id:guid}/submit", async (Guid id, ClaimsPrincipal user, InventoryMovementService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(AdjustmentDto(await svc.SubmitAdjustmentAsync(id, CurrentUserId(user), ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("StockAdjustments.Submit").WithSummary("Submit a draft — resolves to approved (posts to stock) or pending depending on approval policy.");

        adjDoc.MapPost("/{id:guid}/approve", async (Guid id, ClaimsPrincipal user, InventoryMovementService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(AdjustmentDto(await svc.ApproveAdjustmentAsync(id, CurrentUserId(user), PermissionService.RoleIdOf(user), CurrentUserName(user), ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("StockAdjustments.Approve").WithSummary("Approve an adjustment awaiting authorisation — posts it to stock.");

        adjDoc.MapPost("/{id:guid}/reject", async (Guid id, ClaimsPrincipal user, ReasonInput? body, InventoryMovementService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(AdjustmentDto(await svc.RejectAdjustmentAsync(id, CurrentUserId(user), PermissionService.RoleIdOf(user), CurrentUserName(user), body?.Reason, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("StockAdjustments.Reject");

        adjDoc.MapDelete("/{id:guid}", async (Guid id, InventoryMovementService svc, CancellationToken ct) =>
        {
            try { await svc.RemoveAdjustmentAsync(id, ct); return Results.NoContent(); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("StockAdjustments.Remove").WithSummary("Soft-delete an adjustment that hasn't posted to stock.");

        wsDoc.MapGet("/paged", async (
            ITenantDbContextFactory f, InventoryMovementService svc, CancellationToken ct,
            int pageNumber, int pageSize, string? search, Guid? locationId, string? status, string? reason,
            DateOnly? dateFrom, DateOnly? dateTo) =>
            Results.Ok(await svc.GetWastagePagedAsync(pageNumber, pageSize, search, locationId, status, reason, dateFrom, dateTo, ct)))
            .WithName("WastageNotes.Paged").WithSummary("Server-paged list of wastage documents.");

        wsDoc.MapGet("/{id:guid}", async (Guid id, InventoryMovementService svc, CancellationToken ct) =>
        {
            var w = await svc.GetWastageAsync(id, ct);
            return w is null ? Results.NotFound() : Results.Ok(WastageDto(w));
        }).WithName("WastageNotes.Get");

        wsDoc.MapPost("", async (WastageDraftInput i, InventoryMovementService svc, LocationScope scope, CancellationToken ct) =>
        {
            scope.Assert(i.LocationId);   // #scoping-P2
            try { return Results.Created($"/api/v1/wastage-notes", WastageDto(await svc.CreateWastageDraftAsync(i, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("WastageNotes.Create").WithSummary("Create a wastage document (draft).");

        wsDoc.MapPut("/{id:guid}", async (Guid id, WastageDraftInput i, InventoryMovementService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(WastageDto(await svc.EditWastageDraftAsync(id, i, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("WastageNotes.Edit").WithSummary("Edit a draft wastage note.");

        wsDoc.MapPost("/{id:guid}/submit", async (Guid id, ClaimsPrincipal user, InventoryMovementService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(WastageDto(await svc.SubmitWastageAsync(id, CurrentUserId(user), ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("WastageNotes.Submit").WithSummary("Submit a draft — resolves to approved (posts to stock) or pending depending on approval policy.");

        wsDoc.MapPost("/{id:guid}/approve", async (Guid id, ClaimsPrincipal user, InventoryMovementService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(WastageDto(await svc.ApproveWastageAsync(id, CurrentUserId(user), PermissionService.RoleIdOf(user), CurrentUserName(user), ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("WastageNotes.Approve").WithSummary("Approve a wastage note awaiting authorisation — posts it to stock.");

        wsDoc.MapPost("/{id:guid}/reject", async (Guid id, ClaimsPrincipal user, ReasonInput? body, InventoryMovementService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(WastageDto(await svc.RejectWastageAsync(id, CurrentUserId(user), PermissionService.RoleIdOf(user), CurrentUserName(user), body?.Reason, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("WastageNotes.Reject");

        wsDoc.MapDelete("/{id:guid}", async (Guid id, InventoryMovementService svc, CancellationToken ct) =>
        {
            try { await svc.RemoveWastageAsync(id, ct); return Results.NoContent(); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("WastageNotes.Remove").WithSummary("Soft-delete a wastage note that hasn't posted to stock.");

        // Opening-stock CSV import (#master-data-import): SET on-hand per product at a location.
        inv.MapPost("/opening-stock/import", async (OpeningStockImportInput i, InventoryMovementService svc, LocationScope scope, CancellationToken ct) =>
        {
            if (i.Lines is null || i.Lines.Count == 0) return Results.BadRequest(new { error = "No rows to import" });
            scope.Assert(i.LocationId);   // #scoping-P2
            try { return Results.Ok(await svc.OpeningStockImportAsync(i.LocationId, i.Lines, ct)); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Inventory.OpeningStockImport").WithSummary("Bulk set opening stock on-hand per product at a location.");

        return app;
    }

    private static async Task<IResult> Guard<T>(Func<Task<T>> op, Func<T, object> dto)
    {
        try { return Results.Ok(dto(await op())); }
        catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
    }

    private static object TransferDto(Hms.Api.Domain.StockTransfer t) => new
    {
        t.Id, t.TransferNumber, t.FromLocationId, t.ToLocationId, t.Status, t.IsReturn,
        t.TransferDate, t.ReferenceNo, t.Notes,
        t.TotalCost, t.SubmittedAt, t.ApprovedBy, t.ApprovedAt, t.RejectedBy, t.RejectedAt, t.RejectReason,
        t.DispatchedAt, t.ReceivedAt, t.CreatedAt,
        lines = t.Lines.Where(l => !l.IsDeleted).Select(l => new { l.ProductId, l.Sku, l.ProductName, l.Quantity, l.UnitCost, l.LineTotal }),
    };

    private static object AdjustmentDto(Hms.Api.Domain.StockAdjustment a) => new
    {
        a.Id, a.AdjustmentNumber, a.LocationId, a.Reason, a.Status, a.Notes, a.TotalValue,
        a.SubmittedAt, a.ApprovedBy, a.ApprovedAt, a.RejectedBy, a.RejectedAt, a.RejectReason, a.CreatedAt,
        lines = a.Lines.Where(l => !l.IsDeleted).Select(l => new {
            l.ProductId, l.Sku, l.ProductName, l.AdjustmentType,
            l.QuantityDelta, l.CurrentStock, l.NewStock, l.UnitCost, l.LineTotal }),
    };

    private static object WastageDto(Hms.Api.Domain.WastageNote w) => new
    {
        w.Id, w.WastageNumber, w.LocationId, w.Reason, w.Status, w.Notes, w.TotalCost,
        w.SubmittedAt, w.ApprovedBy, w.ApprovedAt, w.RejectedBy, w.RejectedAt, w.RejectReason, w.CreatedAt,
        lines = w.Lines.Where(l => !l.IsDeleted).Select(l => new {
            l.ProductId, l.Sku, l.ProductName,
            l.Quantity, l.CurrentStock, l.NewStock, l.UnitCost, l.LineTotal }),
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
