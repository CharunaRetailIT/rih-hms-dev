namespace Hms.Api.Features.UnitConversions;

public static class UnitConversionsEndpoint
{
    public static IEndpointRouteBuilder MapUnitConversionsEndpoints(this IEndpointRouteBuilder app)
    {
        var conversions = app.MapGroup("/api/v1/uom-conversions").WithTags("UOM Conversions");

        conversions.MapGet("/paged", async (UnitConversionsService svc,CancellationToken ct,int pageNumber = 1,int pageSize = 10,string? search = null,Guid? unitOfMeasureId = null) =>
        {
            var result = await svc.GetPagedAsync(pageNumber, pageSize, search, unitOfMeasureId, ct);
            return Results.Ok(result);
        })
        .WithName("UomConversions.Paged");

        conversions.MapGet("", async (UnitConversionsService svc, CancellationToken ct) =>
        {
            var list = await svc.ListAsync(ct);
            return Results.Ok(list);
        })
        .WithName("UomConversions.List");

        conversions.MapPut("", async (SaveUnitConversionRequest req, UnitConversionsService svc, CancellationToken ct) =>
        {
            var result = await svc.SaveAsync(req, ct);
            return result.Success
                ? Results.Ok(result.Data)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("UomConversions.Save")
        .RequireAuthorization("SupplyChain");

        conversions.MapDelete("/{id:guid}", async (Guid id, UnitConversionsService svc, CancellationToken ct) =>
        {
            var result = await svc.DeleteAsync(id, ct);
            return result.Success
                ? Results.NoContent()
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("UomConversions.Delete")
        .RequireAuthorization("SupplyChain");

        conversions.MapPost("/convert", async (UnitConversionRequest req, UnitConversionsService svc, CancellationToken ct) =>
        {
            var result = await svc.ConvertAsync(req, ct);
            return result.Success
                ? Results.Ok(result.Data)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("UomConversions.Convert");

        return app;
    }
}