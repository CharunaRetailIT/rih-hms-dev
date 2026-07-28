namespace Hms.Api.Features.Charges;

public static class ChargesEndpoint
{
    public static IEndpointRouteBuilder MapChargeEndpoints(this IEndpointRouteBuilder app)
    {
        var charges = app.MapGroup("/api/v1/charges")
            .WithTags("Charges")
            .RequireAuthorization("BackOffice");

        charges.MapGet("/paged", async (
            ChargesService svc,
            CancellationToken ct,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            Guid? chargeTypeId = null,
            bool? isActive = null) =>
        {
            var result = await svc.GetPagedAsync(pageNumber, pageSize, search, chargeTypeId, isActive, ct);
            return Results.Ok(result);
        })
        .WithName("Charges.Paged");

        charges.MapGet("/{id:guid}", async (Guid id, ChargesService svc, CancellationToken ct) =>
        {
            var item = await svc.GetAsync(id, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        })
        .WithName("Charges.Get");

        charges.MapPut("", async (SaveChargeRequest req, ChargesService svc, CancellationToken ct) =>
        {
            var result = await svc.SaveAsync(req, ct);

            return result.Success
                ? Results.Ok(result.Data)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("Charges.Save");

        charges.MapDelete("/{id:guid}", async (Guid id, ChargesService svc, CancellationToken ct) =>
        {
            var result = await svc.DeleteAsync(id, ct);

            return result.Success
                ? Results.NoContent()
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("Charges.Delete");

        return app;
    }
}
