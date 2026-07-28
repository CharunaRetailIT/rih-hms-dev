namespace Hms.Api.Features.Production;

public static class ProductionEndpoint
{
    public static IEndpointRouteBuilder MapProductionEndpoints(this IEndpointRouteBuilder app)
    {
        var rec = app.MapGroup("/api/v1/recipes").WithTags("Recipes").RequireAuthorization("SupplyChain");
        var prod = app.MapGroup("/api/v1/production").WithTags("Production").RequireAuthorization("SupplyChain");

        rec.MapGet("", async (ProductionService svc, CancellationToken ct) =>
            Results.Ok(await svc.ListRecipesAsync(ct)))
            .WithName("Recipes.List").WithSummary("All recipes (a product may have several, keyed by output unit).");

        rec.MapGet("/{productId:guid}", async (Guid productId, Guid? outputUnitId, Guid? locationId, ProductionService svc, CancellationToken ct) =>
        {
            var r = await svc.GetRecipeAsync(productId, outputUnitId, locationId, ct);
            return r is null ? Results.NotFound() : Results.Ok(RecipeDto(r));
        }).WithName("Recipes.Get");

        rec.MapPut("", async (SetRecipeInput i, ProductionService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(RecipeDto(await svc.SetRecipeAsync(i, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Recipes.Set").WithSummary("Create/replace a recipe (BOM) for a product + output unit.");

        // ── Production ──
        prod.MapPost("", async (ProduceInput i, ProductionService svc, CancellationToken ct) =>
        {
            try { return Results.Created("/api/v1/production", ProdDto(await svc.ProduceAsync(i, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Production.Produce").WithSummary("Recipe-based production (post now, or draft with post=false).");

        prod.MapPost("/custom", async (CustomProduceInput i, ProductionService svc, CancellationToken ct) =>
        {
            try { return Results.Created("/api/v1/production", ProdDto(await svc.ProduceCustomAsync(i, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Production.Custom").WithSummary("Custom/ad-hoc production — supply materials directly, no recipe.");

        prod.MapPost("/batch", async (BatchProduceInput i, ProductionService svc, CancellationToken ct) =>
        {
            try { return Results.Ok((await svc.ProduceBatchAsync(i, ct)).Select(ProdDto)); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Production.Batch").WithSummary("Produce several finished goods in one document.");

        prod.MapPost("/{id:guid}/post", async (Guid id, ProductionService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(ProdDto(await svc.PostAsync(id, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Production.Post").WithSummary("Post a draft — applies stock movements.");

        prod.MapPost("/{id:guid}/void", async (Guid id, VoidProductionInput? body, ProductionService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(ProdDto(await svc.VoidAsync(id, body?.Reason, ct))); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Production.Void").WithSummary("Void — reverses a posted order's stock movements.");

        prod.MapGet("", async (ProductionService svc, CancellationToken ct) =>
            Results.Ok((await svc.ListProductionAsync(100, ct)).Select(ProdDto)))
            .WithName("Production.List").WithSummary("Recent production orders (most recent first).");

        prod.MapGet("/{id:guid}", async (Guid id, ProductionService svc, CancellationToken ct) =>
        {
            var p = await svc.GetProductionAsync(id, ct);
            return p is null ? Results.NotFound() : Results.Ok(ProdDto(p));
        }).WithName("Production.Get");

        return app;
    }

    private static object RecipeDto(Hms.Api.Domain.Recipe r) => new
    {
        r.Id, r.ProductId, r.OutputUnitId, r.LocationId, r.YieldQuantity, r.Notes, r.IsActive, r.TotalCost,
        lines = r.Lines.Where(l => !l.IsDeleted).Select(l => new {
            l.IngredientProductId, l.Sku, l.ProductName, l.Quantity, l.UnitId, l.UnitSymbol, l.CostPrice, l.LineCost }),
    };

    private static object ProdDto(Hms.Api.Domain.ProductionOrder p) => new
    {
        p.Id, p.ProductionNumber, p.DocumentNo, p.LocationId, p.ReceiptLocationId, p.ProductId, p.ProductName,
        p.Quantity, p.Status, p.IsCustom, p.TotalInputCost, p.UnitCost, p.CompletedAt, p.PostedAt, p.VoidedAt, p.VoidReason,
        consumptions = p.Consumptions.Where(c => !c.IsDeleted).Select(c => new {
            c.IngredientProductId, c.Sku, c.ProductName, c.QuantityConsumed, c.UnitCost, c.LineTotal }),
    };
}

public record VoidProductionInput(string? Reason);
