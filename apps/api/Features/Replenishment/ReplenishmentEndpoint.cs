using Hms.Api.Infrastructure;

namespace Hms.Api.Features.Replenishment;

public static class ReplenishmentEndpoint
{
    public static IEndpointRouteBuilder MapReplenishmentEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/replenishment").WithTags("Replenishment").RequireAuthorization("SupplyChain");

        // The below-par worksheet for a location + the transfer/PO split.
        g.MapGet("", async (Guid locationId, ReplenishmentService svc, LocationScope scope, CancellationToken ct) =>
        {
            scope.Assert(locationId);   // #scoping-P2
            try { return Results.Ok(await svc.WorksheetAsync(locationId, ct)); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Replenishment.Worksheet").WithSummary("Below-par items for a location with suggested transfer + PO quantities.");

        // Turn the worksheet into draft transfers (from the hub) and/or draft POs (by supplier).
        g.MapPost("/generate", async (GenerateInput i, ReplenishmentService svc, LocationScope scope, CancellationToken ct) =>
        {
            scope.Assert(i.LocationId);   // #scoping-P2
            if (!i.Transfers && !i.Pos) return Results.BadRequest(new { error = "Pick transfers, POs, or both" });
            try { return Results.Ok(await svc.GenerateAsync(i.LocationId, i.Transfers, i.Pos, ct)); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Replenishment.Generate").WithSummary("Create draft transfers/POs from the worksheet.");

        // Per-location level grid (effective + override) for configuration.
        g.MapGet("/levels", async (Guid locationId, ReplenishmentService svc, LocationScope scope, CancellationToken ct) =>
        {
            scope.Assert(locationId);
            try { return Results.Ok(await svc.ListLevelsAsync(locationId, ct)); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Replenishment.Levels").WithSummary("Stocked products with effective + override reorder/par levels.");

        g.MapPut("/levels", async (SetLevelsInput i, ReplenishmentService svc, LocationScope scope, CancellationToken ct) =>
        {
            scope.Assert(i.LocationId);
            if (i.Rows is null || i.Rows.Count == 0) return Results.BadRequest(new { error = "No level rows" });
            try { await svc.SetLevelsAsync(i.LocationId, i.Rows, ct); return Results.Ok(new { saved = i.Rows.Count }); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Replenishment.SetLevels").RequireAuthorization("BackOffice").WithSummary("Upsert per-location reorder/par overrides.");

        return app;
    }
}

public record GenerateInput(Guid LocationId, bool Transfers, bool Pos);
public record SetLevelsInput(Guid LocationId, List<SetLevelInput> Rows);
