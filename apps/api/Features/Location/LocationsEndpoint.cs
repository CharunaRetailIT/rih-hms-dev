using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Locations;

public static class LocationsEndpoint
{
    public static IEndpointRouteBuilder MapLocationEndpoints(this IEndpointRouteBuilder app)
    {
        var locations = app.MapGroup("/api/v1/locations")
            .WithTags("Locations");


        // -------- Get All Locations --------
        locations.MapGet("", async (ITenantDbContextFactory factory, CancellationToken ct, bool? all) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var q = db.Locations.AsNoTracking();
            if (all != true) q = q.Where(l => l.IsActive);
            var list = await q.OrderBy(l => l.Name)
                .Select(l => new { l.Id, l.Code, l.Name, l.City, l.Currency, l.LocationType, l.CanSell, l.CanProduce, l.CanStock, l.IsActive })
                .ToListAsync(ct);
            return Results.Ok(list);
        })
        .WithName("Locations.List");

        locations.MapGet("/paged", async (
            LocationsService svc,
            CancellationToken ct,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            string? locationType = null,
            bool? isActive = null) =>
        {
            var result = await svc.GetPagedAsync(pageNumber, pageSize, search, locationType, isActive, ct);
            return Results.Ok(result);
        })
        .WithName("Locations.Paged");

        locations.MapGet("/{id:guid}", async (Guid id, LocationsService svc, CancellationToken ct) =>
        {
            var item = await svc.GetAsync(id, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        })
        .WithName("Locations.Get");

        locations.MapPut("", async (SaveLocationRequest req, LocationsService svc, CancellationToken ct) =>
        {
            var result = await svc.SaveAsync(req, ct);

            return result.Success
                ? Results.Ok(result.Data)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("Locations.Save")
        .RequireAuthorization("BackOffice");

        locations.MapDelete("/{id:guid}", async (Guid id, LocationsService svc, CancellationToken ct) =>
        {
            var result = await svc.DeleteAsync(id, ct);

            return result.Success
                ? Results.NoContent()
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("Locations.Delete")
        .RequireAuthorization("BackOffice");

        return app;
    }
}