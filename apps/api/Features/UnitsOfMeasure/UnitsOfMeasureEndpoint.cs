namespace Hms.Api.Features.UnitsOfMeasure;

public static class UnitsOfMeasureEndpoint
{
    public static IEndpointRouteBuilder MapUnitsOfMeasureEndpoints(this IEndpointRouteBuilder app)
    {
        var uoms = app.MapGroup("/api/v1/units-of-measure").WithTags("Units of Measure");

        uoms.MapGet("", async (UnitsOfMeasureService svc, CancellationToken ct) =>
        {
            var list = await svc.ListAsync(ct);
            return Results.Ok(list);
        })
        .WithName("Uoms.List");

        uoms.MapGet("/paged", async (UnitsOfMeasureService svc, CancellationToken ct, int pageNumber = 1, int pageSize = 10, string? search = null, string? dimension = null) =>
        {
            var result = await svc.GetPagedAsync(pageNumber, pageSize, search, dimension, ct);
            return Results.Ok(result);
        })
        .WithName("Uoms.ListPaged");

        uoms.MapGet("/{id:guid}", async (Guid id, UnitsOfMeasureService svc, CancellationToken ct) =>
        {
            var item = await svc.GetAsync(id, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        })
        .WithName("Uoms.Get");

        uoms.MapPut("", async (SaveUnitOfMeasureRequest req, UnitsOfMeasureService svc, CancellationToken ct) =>
        {
            var result = await svc.SaveAsync(req, ct);
            return result.Success ? Results.Ok(result.Data) : Results.BadRequest(new { error = result.Error });
        })
        .WithName("Uoms.Save")
        .RequireAuthorization("SupplyChain");

        uoms.MapDelete("/{id:guid}", async (Guid id, UnitsOfMeasureService svc, CancellationToken ct) =>
        {
            var result = await svc.DeleteAsync(id, ct);
            return result.Success ? Results.NoContent() : Results.BadRequest(new { error = result.Error });
        })
        .WithName("Uoms.Delete")
        .RequireAuthorization("SupplyChain");

        return app;
    }
}