using Hms.Api.Features.Audit;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.StockCounts;

public static class StockCountEndpoint
{
    public static IEndpointRouteBuilder MapStockCountEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/stock-counts").WithTags("Stock counts").RequireAuthorization("SupplyChain");

        g.MapPost("", async (CreateStockCountInput body, StockCountService svc, LocationScope scope, CancellationToken ct) =>
        {
            scope.Assert(body.LocationId);   // #scoping-P2
            try { var c = await svc.CreateAsync(body.LocationId, body.Notes, ct); return Results.Created($"/api/v1/stock-counts/{c.Id}", new { c.Id, c.CountNumber }); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("StockCounts.Create").WithSummary("Open a count — snapshots current on-hand for every stocked product at the outlet.");

        g.MapGet("", async (StockCountService svc, CancellationToken ct, Guid? locationId) =>
        {
            var list = await svc.ListAsync(locationId, ct);
            return Results.Ok(list.Select(c => new { c.Id, c.CountNumber, c.LocationId, c.Status, c.Notes, c.CreatedAt, c.PostedAt }));
        }).WithName("StockCounts.List");

        g.MapGet("/paged", async (StockCountService svc, CancellationToken ct,
            int pageNumber = 1, int pageSize = 10, Guid? locationId = null, string? status = null) =>
            Results.Ok(await svc.GetPagedAsync(pageNumber, pageSize, locationId, status, ct)))
        .WithName("StockCounts.ListPaged");

        g.MapGet("/{id:guid}", async (Guid id, StockCountService svc, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            var c = await svc.GetAsync(id, ct);
            if (c is null) return Results.NotFound();
            await using var db = await f.CreateForCurrentAsync(ct);
            // IgnoreQueryFilters so a product deleted AFTER the count still shows its name/SKU —
            // a count is a historical record (same tenant DB + RLS, so this stays tenant-safe).
            var prods = await db.Products.IgnoreQueryFilters().AsNoTracking()
                .Select(p => new { p.Id, p.Sku, p.Name }).ToDictionaryAsync(p => p.Id, ct);
            return Results.Ok(new
            {
                c.Id, c.CountNumber, c.LocationId, c.Status, c.Notes, c.CreatedAt, c.PostedAt,
                lines = c.Lines.Where(l => !l.IsDeleted).Select(l => new
                {
                    l.Id, l.ProductId,
                    sku = prods.GetValueOrDefault(l.ProductId)?.Sku,
                    name = prods.GetValueOrDefault(l.ProductId)?.Name,
                    l.SystemQty, l.CountedQty, l.Variance,
                }).OrderBy(l => l.name),
            });
        }).WithName("StockCounts.Get");

        g.MapPut("/{id:guid}/lines", async (Guid id, SaveCountLinesInput body, StockCountService svc, CancellationToken ct) =>
        {
            try
            {
                var counted = (body.Lines ?? new()).ToDictionary(l => l.ProductId, l => l.CountedQty);
                await svc.SaveLinesAsync(id, counted, ct);
                return Results.Ok(new { saved = counted.Count });
            }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("StockCounts.SaveLines").WithSummary("Save counted quantities on a draft count.");

        g.MapDelete("/{id:guid}", async (Guid id, StockCountService svc, AuditService audit, CancellationToken ct) =>
        {
            try { await svc.DiscardAsync(id, ct); await audit.LogAsync("stockcount.discard", "stock_count", id, "Discarded a draft count", ct); return Results.NoContent(); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("StockCounts.Discard").WithSummary("Discard a draft count (it never touched stock).");

        g.MapPost("/{id:guid}/post", async (Guid id, StockCountService svc, AuditService audit, CancellationToken ct) =>
        {
            try
            {
                var r = await svc.PostAsync(id, ct);
                await audit.LogAsync("stockcount.post", "stock_count", r.Id,
                    $"Posted {r.CountNumber} · {r.VarianceLines} line(s) varied (net {r.NetVarianceQty:0.###})", ct);
                return Results.Ok(r);
            }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("StockCounts.Post").WithSummary("Post the count — writes variances to stock on hand.");

        return app;
    }
}

public record CreateStockCountInput(Guid LocationId, string? Notes);
public record SaveCountLinesInput(List<CountLineInput>? Lines);
public record CountLineInput(Guid ProductId, decimal CountedQty);
