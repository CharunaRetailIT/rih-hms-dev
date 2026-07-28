namespace Hms.Api.Features.KitchenStations;

public static class KitchenStationsEndpoint
{
    public static IEndpointRouteBuilder MapKitchenStationEndpoints(this IEndpointRouteBuilder app)
    {
        var printerTypes = app.MapGroup("/api/v1/printer-types")
            .WithTags("Printer Types");

        printerTypes.MapGet("", async (
            KitchenStationsService svc,
            CancellationToken ct,
            bool? all = null) =>
        {
            var list = await svc.ListPrinterTypesAsync(all, ct);
            return Results.Ok(list);
        })
        .WithName("PrinterTypes.List");

        printerTypes.MapGet("/paged", async (
            KitchenStationsService svc,
            CancellationToken ct,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            bool? isActive = null) =>
        {
            var result = await svc.GetPrinterTypesPagedAsync(
                pageNumber,
                pageSize,
                search,
                isActive,
                ct);

            return Results.Ok(result);
        })
        .WithName("PrinterTypes.Paged");

        printerTypes.MapGet("/{id:guid}", async (Guid id, KitchenStationsService svc, CancellationToken ct) =>
        {
            var item = await svc.GetPrinterTypeAsync(id, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        })
        .WithName("PrinterTypes.Get");

        printerTypes.MapPut("", async (SavePrinterTypeRequest req, KitchenStationsService svc, CancellationToken ct) =>
        {
            var result = await svc.SavePrinterTypeAsync(req, ct);

            return result.Success
                ? Results.Ok(result.Data)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("PrinterTypes.Save")
        .RequireAuthorization("BackOffice");

        printerTypes.MapDelete("/{id:guid}", async (Guid id, KitchenStationsService svc, CancellationToken ct) =>
        {
            var result = await svc.DeletePrinterTypeAsync(id, ct);

            return result.Success
                ? Results.NoContent()
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("PrinterTypes.Delete")
        .RequireAuthorization("BackOffice");

        var stations = app.MapGroup("/api/v1/kitchen-stations")
            .WithTags("Kitchen Stations");

        stations.MapGet("", async (
            KitchenStationsService svc,
            CancellationToken ct,
            Guid? locationId = null,
            bool? all = null) =>
        {
            var list = await svc.ListAsync(locationId, all, ct);
            return Results.Ok(list);
        })
        .WithName("KitchenStations.List");

        stations.MapGet("/paged", async (
            KitchenStationsService svc,
            CancellationToken ct,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            Guid? locationId = null,
            Guid? printerTypeId = null,
            bool? isActive = null) =>
        {
            var result = await svc.GetPagedAsync(
                pageNumber,
                pageSize,
                search,
                locationId,
                printerTypeId,
                isActive,
                ct);

            return Results.Ok(result);
        })
        .WithName("KitchenStations.Paged");

        stations.MapGet("/{id:guid}", async (Guid id, KitchenStationsService svc, CancellationToken ct) =>
        {
            var item = await svc.GetAsync(id, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        })
        .WithName("KitchenStations.Get");

        stations.MapPut("", async (SaveKitchenStationRequest req, KitchenStationsService svc, CancellationToken ct) =>
        {
            var result = await svc.SaveAsync(req, ct);

            return result.Success
                ? Results.Ok(result.Data)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("KitchenStations.Save")
        .RequireAuthorization("SupplyChain");

        stations.MapDelete("/{id:guid}", async (Guid id, KitchenStationsService svc, CancellationToken ct) =>
        {
            var result = await svc.DeleteAsync(id, ct);

            return result.Success
                ? Results.NoContent()
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("KitchenStations.Delete")
        .RequireAuthorization("SupplyChain");

        return app;
    }
}