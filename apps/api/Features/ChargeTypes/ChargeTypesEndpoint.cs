namespace Hms.Api.Features.ChargeTypes;

public static class ChargeTypesEndpoint
{
    public static IEndpointRouteBuilder MapChargeTypeEndpoints(this IEndpointRouteBuilder app)
    {
        var chargeTypes = app.MapGroup("/api/v1/charge-types")
            .WithTags("Charge Types")
            .RequireAuthorization("BackOffice");

        chargeTypes.MapGet("/paged", async (
            ChargeTypesService svc,
            CancellationToken ct,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            bool? isActive = null) =>
        {
            var result = await svc.GetPagedAsync(pageNumber, pageSize, search, isActive, ct);
            return Results.Ok(result);
        })
        .WithName("ChargeTypes.Paged");

        chargeTypes.MapGet("/{id:guid}", async (Guid id, ChargeTypesService svc, CancellationToken ct) =>
        {
            var item = await svc.GetAsync(id, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        })
        .WithName("ChargeTypes.Get");

        chargeTypes.MapPut("", async (SaveChargeTypeRequest req, ChargeTypesService svc, CancellationToken ct) =>
        {
            var result = await svc.SaveAsync(req, ct);

            return result.Success
                ? Results.Ok(result.Data)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("ChargeTypes.Save");

        chargeTypes.MapDelete("/{id:guid}", async (Guid id, ChargeTypesService svc, CancellationToken ct) =>
        {
            var result = await svc.DeleteAsync(id, ct);

            return result.Success
                ? Results.NoContent()
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("ChargeTypes.Delete");

        return app;
    }
}
