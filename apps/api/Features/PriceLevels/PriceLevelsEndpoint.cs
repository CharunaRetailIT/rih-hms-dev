namespace Hms.Api.Features.PriceLevels;

public static class PriceLevelsEndpoint
{
    public static IEndpointRouteBuilder MapPriceLevelEndpoints(this IEndpointRouteBuilder app)
    {
        var priceLevels = app.MapGroup("/api/v1/price-levels")
            .WithTags("Price Levels");

        priceLevels.MapGet("", async (
            PriceLevelsService svc,
            CancellationToken ct,
            Guid? locationId = null,
            bool? all = null) =>
        {
            var list = await svc.ListAsync(locationId, all, ct);
            return Results.Ok(list);
        })
        .WithName("PriceLevels.List");

        priceLevels.MapGet("/paged", async (
            PriceLevelsService svc,
            CancellationToken ct,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            Guid? locationId = null,
            string? appliesToOrderType = null,
            bool? isActive = null) =>
        {
            var result = await svc.GetPagedAsync(
                pageNumber,
                pageSize,
                search,
                locationId,
                appliesToOrderType,
                isActive,
                ct);

            return Results.Ok(result);
        })
        .WithName("PriceLevels.Paged");

        priceLevels.MapGet("/{id:guid}", async (Guid id, PriceLevelsService svc, CancellationToken ct) =>
        {
            var item = await svc.GetAsync(id, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        })
        .WithName("PriceLevels.Get");

        priceLevels.MapPut("", async (SavePriceLevelRequest req, PriceLevelsService svc, CancellationToken ct) =>
        {
            var result = await svc.SaveAsync(req, ct);

            return result.Success
                ? Results.Ok(result.Data)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("PriceLevels.Save")
        .RequireAuthorization("BackOffice");

        priceLevels.MapDelete("/{id:guid}", async (Guid id, PriceLevelsService svc, CancellationToken ct) =>
        {
            var result = await svc.DeleteAsync(id, ct);

            return result.Success
                ? Results.NoContent()
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("PriceLevels.Delete")
        .RequireAuthorization("BackOffice");

        return app;
    }
}