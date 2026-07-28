using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Floors;

/// <summary>
/// Floors — a physical floor/section of an outlet. Foundation for floor-scoped
/// notification routing on guest QR orders (a steward only gets pushed orders for
/// tables on their assigned floor). See also <c>UsersEndpoint</c> for steward↔floor
/// assignment (kept there since it's a Team/Owner-only action on the user record).
/// </summary>
public static class FloorsEndpoint
{
    public static IEndpointRouteBuilder MapFloorEndpoints(this IEndpointRouteBuilder app)
    {
        var f = app.MapGroup("/api/v1/floors").WithTags("Floors");

        f.MapGet("", async (ITenantDbContextFactory factory, CancellationToken ct, Guid? locationId, bool? all) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var q = db.Floors.AsNoTracking();
            if (all != true) q = q.Where(x => x.IsActive);
            if (locationId is Guid l) q = q.Where(x => x.LocationId == l);
            var list = await q.OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Select(x => new { x.Id, x.LocationId, x.Name, x.SortOrder, x.IsActive })
                .ToListAsync(ct);
            return Results.Ok(list);
        }).WithName("Floors.List");

        f.MapPut("", async (FloorInput i, ITenantDbContextFactory factory, LocationScope scope, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(i.Name) || i.LocationId == Guid.Empty)
                return Results.BadRequest(new { error = "Name and location are required" });
            scope.Assert(i.LocationId);
            await using var db = await factory.CreateForCurrentAsync(ct);
            var name = i.Name.Trim();
            var row = i.Id is Guid id
                ? await db.Floors.FirstOrDefaultAsync(x => x.Id == id, ct)
                : await db.Floors.FirstOrDefaultAsync(x => x.LocationId == i.LocationId && x.Name == name, ct);
            if (row is null) { row = new Floor { LocationId = i.LocationId, Name = name }; db.Floors.Add(row); }
            row.Name = name; row.SortOrder = i.SortOrder; row.IsActive = true;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { row.Id, row.Name, row.SortOrder });
        }).WithName("Floors.Set").RequireAuthorization("SupplyChain");

        f.MapDelete("/{id:guid}", async (Guid id, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var row = await db.Floors.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (row is null) return Results.NotFound();
            row.IsDeleted = true; row.IsActive = false;
            // Unlink tables pointed at this floor rather than leaving a dangling reference.
            await db.RestaurantTables.Where(t => t.FloorId == id).ExecuteUpdateAsync(s => s.SetProperty(t => t.FloorId, (Guid?)null), ct);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).WithName("Floors.Delete").RequireAuthorization("SupplyChain");

        return app;
    }
}

public record FloorInput(Guid? Id, Guid LocationId, string Name, int SortOrder = 0);
