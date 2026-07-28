namespace Hms.Api.Features.ServingUnits;

public static class ServingUnitsEndpoint
{
    public static IEndpointRouteBuilder MapServingUnitEndpoints(this IEndpointRouteBuilder app)
    {
        var servingUnits = app.MapGroup("/api/v1/serving-units")
            .WithTags("Serving Units");

        servingUnits.MapGet("", async (
            ServingUnitsService svc,
            CancellationToken ct,
            bool? all = null) =>
        {
            var list = await svc.ListAsync(all, ct);
            return Results.Ok(list);
        })
        .WithName("ServingUnits.List");

        servingUnits.MapGet("/paged", async (
            ServingUnitsService svc,
            CancellationToken ct,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            bool? isActive = null) =>
        {
            var result = await svc.GetPagedAsync(pageNumber, pageSize, search, isActive, ct);
            return Results.Ok(result);
        })
        .WithName("ServingUnits.Paged");

        servingUnits.MapGet("/{id:guid}", async (Guid id, ServingUnitsService svc, CancellationToken ct) =>
        {
            var item = await svc.GetAsync(id, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        })
        .WithName("ServingUnits.Get");

        servingUnits.MapPut("", async (SaveServingUnitRequest req, ServingUnitsService svc, CancellationToken ct) =>
        {
            var result = await svc.SaveAsync(req, ct);

            return result.Success
                ? Results.Ok(result.Data)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("ServingUnits.Save")
        .RequireAuthorization("BackOffice");

        servingUnits.MapDelete("/{id:guid}", async (Guid id, ServingUnitsService svc, CancellationToken ct) =>
        {
            var result = await svc.DeleteAsync(id, ct);

            return result.Success
                ? Results.NoContent()
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("ServingUnits.Delete")
        .RequireAuthorization("BackOffice");

        return app;
    }
}