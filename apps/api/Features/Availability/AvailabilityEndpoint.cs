using Hms.Api.Domain;
using Hms.Api.Features.Realtime;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Availability;

/// <summary>
/// Per-location availability (#112) — what POS / Tab / Guest read to grey-out 86'd items,
/// plus a manual override to force-86 (or un-86) an item at an outlet.
/// </summary>
public static class AvailabilityEndpoint
{
    public static IEndpointRouteBuilder MapAvailabilityEndpoints(this IEndpointRouteBuilder app)
    {
        // Live availability for one outlet (productId → available + reason). POS/Tab/Guest merge by productId.
        app.MapGet("/api/v1/availability", async (Guid? locationId, AvailabilityService svc, CancellationToken ct) =>
        {
            if (locationId is not Guid loc) return Results.BadRequest(new { error = "locationId is required" });
            var items = await svc.ComputeAsync(loc, ct);
            return Results.Ok(items.Select(i => new { i.ProductId, i.Available, i.Reason }));
        }).WithName("Availability.List").WithTags("Availability").RequireAuthorization();

        // Manual override: force "available" / "unavailable", or "auto" to clear (revert to stock-driven).
        app.MapPut("/api/v1/availability", async (SetAvailabilityInput i, ITenantDbContextFactory f, RealtimeBus bus, CancellationToken ct) =>
        {
            if (i.LocationId == Guid.Empty || i.ProductId == Guid.Empty)
                return Results.BadRequest(new { error = "locationId and productId are required" });
            var mode = (i.Mode ?? "").Trim().ToLowerInvariant();
            if (mode is not ("available" or "unavailable" or "auto"))
                return Results.BadRequest(new { error = "mode must be available, unavailable, or auto" });

            await using var db = await f.CreateForCurrentAsync(ct);
            var existing = await db.ProductAvailabilityOverrides
                .FirstOrDefaultAsync(o => o.LocationId == i.LocationId && o.ProductId == i.ProductId, ct);

            if (mode == "auto")
            {
                if (existing is not null) { db.ProductAvailabilityOverrides.Remove(existing); await db.SaveChangesAsync(ct); }
            }
            else
            {
                var avail = mode == "available";
                if (existing is null)
                    db.ProductAvailabilityOverrides.Add(new ProductAvailabilityOverride
                    { TenantId = db.TenantId, LocationId = i.LocationId, ProductId = i.ProductId, Available = avail });
                else existing.Available = avail;
                await db.SaveChangesAsync(ct);
            }
            bus.Publish(db.TenantId, "availability");   // POS/Tab/Guest refresh
            return Results.Ok(new { i.LocationId, i.ProductId, mode });
        }).WithName("Availability.Set").WithTags("Availability").RequireAuthorization("Operations");

        return app;
    }
}

public record SetAvailabilityInput(Guid LocationId, Guid ProductId, string? Mode);
