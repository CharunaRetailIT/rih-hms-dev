using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Catering;

/// <summary>
/// Catering / banquet API (#75). Hall + package masters are back-office;
/// bookings/billing are front-of-house (Operations). All read/write goes through
/// CateringService.
/// </summary>
public static class CateringEndpoint
{
    public static IEndpointRouteBuilder MapCateringEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Halls (master) ──
        var h = app.MapGroup("/api/v1/catering/halls").WithTags("Catering").AddEndpointFilter(new Hms.Api.Infrastructure.RequireFeatureFilter("catering"));
        h.MapGet("", async (CateringService svc, CancellationToken ct, bool? all) =>
            Results.Ok((await svc.ListHallsAsync(all == true, ct))
                .Select(x => new { x.Id, x.Code, x.Name, x.Capacity, x.Notes, x.IsActive, x.LocationId })))
            .WithName("Catering.Halls").RequireAuthorization();
        h.MapPost("", async (HallInput b, CateringService svc, CancellationToken ct) =>
        {
            try { var x = await svc.SaveHallAsync(b.Id, b.Code ?? "", b.Name ?? "", b.Capacity ?? 0, b.Notes, b.IsActive ?? true, b.LocationId, ct); return Results.Ok(new { x.Id }); }
            catch (DbUpdateException) { return Results.BadRequest(new { error = "A hall with that code already exists" }); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Catering.SaveHall").RequireAuthorization("BackOffice");
        h.MapDelete("/{id:guid}", async (Guid id, CateringService svc, CancellationToken ct) => { await svc.DeleteHallAsync(id, ct); return Results.NoContent(); })
            .WithName("Catering.DeleteHall").RequireAuthorization("BackOffice");

        // ── Packages (master) ──
        var p = app.MapGroup("/api/v1/catering/packages").WithTags("Catering").AddEndpointFilter(new Hms.Api.Infrastructure.RequireFeatureFilter("catering"));
        p.MapGet("", async (CateringService svc, CancellationToken ct, bool? all) =>
            Results.Ok((await svc.ListPackagesAsync(all == true, ct))
                .Select(x => new { x.Id, x.Code, x.Name, x.PricePerHead, x.Description, x.IsActive, x.RecipeProductId })))
            .WithName("Catering.Packages").RequireAuthorization();
        p.MapPost("", async (PackageInput b, CateringService svc, CancellationToken ct) =>
        {
            try { var x = await svc.SavePackageAsync(b.Id, b.Code ?? "", b.Name ?? "", b.PricePerHead ?? 0, b.Description, b.IsActive ?? true, b.RecipeProductId, ct); return Results.Ok(new { x.Id }); }
            catch (DbUpdateException) { return Results.BadRequest(new { error = "A package with that code already exists" }); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Catering.SavePackage").RequireAuthorization("BackOffice");
        p.MapDelete("/{id:guid}", async (Guid id, CateringService svc, CancellationToken ct) => { await svc.DeletePackageAsync(id, ct); return Results.NoContent(); })
            .WithName("Catering.DeletePackage").RequireAuthorization("BackOffice");

        // ── Events (bookings + billing) ──
        var e = app.MapGroup("/api/v1/catering/events").WithTags("Catering").RequireAuthorization("Operations").AddEndpointFilter(new Hms.Api.Infrastructure.RequireFeatureFilter("catering"));

        e.MapGet("", async (CateringService svc, ITenantDbContextFactory f, CancellationToken ct, DateTime? from, DateTime? to, string? status, Guid? locationId) =>
        {
            var events = await svc.ListEventsAsync(from, to, status, locationId, ct);
            await using var db = await f.CreateForCurrentAsync(ct);
            var halls = await db.EventHalls.AsNoTracking().Select(x => new { x.Id, x.Name }).ToListAsync(ct);
            var hallById = halls.ToDictionary(x => x.Id, x => x.Name);
            return Results.Ok(events.Select(ev => Row(ev, hallById)));
        }).WithName("Catering.Events").WithSummary("List catering bookings for a period.");

        e.MapGet("/{id:guid}", async (Guid id, CateringService svc, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            var ev = await svc.GetEventAsync(id, ct);
            if (ev is null) return Results.NotFound();
            await using var db = await f.CreateForCurrentAsync(ct);
            var hall = ev.HallId is Guid hid ? await db.EventHalls.AsNoTracking().Where(x => x.Id == hid).Select(x => x.Name).FirstOrDefaultAsync(ct) : null;
            var pkg = ev.PackageId is Guid pid ? await db.CateringPackages.AsNoTracking().Where(x => x.Id == pid).Select(x => new { x.Name, x.PricePerHead }).FirstOrDefaultAsync(ct) : null;
            return Results.Ok(Detail(ev, hall, pkg?.Name));
        }).WithName("Catering.Event");

        e.MapPost("", async (SaveEventInput body, CateringService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(Row(await svc.SaveEventAsync(body, ct), null)); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Catering.SaveEvent").WithSummary("Create or update a booking (hall double-booking blocked).");

        e.MapPost("/{id:guid}/status", async (Guid id, StatusBody b, CateringService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(Row(await svc.SetStatusAsync(id, b.Status ?? "", ct), null)); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Catering.SetStatus");

        e.MapPost("/{id:guid}/dispatch", async (Guid id, DispatchBody b, CateringService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(Row(await svc.SetDispatchAsync(id, b.Status ?? "", b.Vehicle, b.Driver, ct), null)); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Catering.SetDispatch").WithSummary("Set own-fleet dispatch status (pending/dispatched/delivered) + assign vehicle/driver.");

        e.MapPost("/{id:guid}/items", async (Guid id, ItemBody b, CateringService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(Row(await svc.AddItemAsync(id, b.Description ?? "", b.Quantity ?? 1, b.UnitPrice ?? 0, ct), null)); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Catering.AddItem");

        e.MapDelete("/{id:guid}/items/{itemId:guid}", async (Guid id, Guid itemId, CateringService svc, CancellationToken ct) =>
            Results.Ok(Row(await svc.RemoveItemAsync(id, itemId, ct), null))).WithName("Catering.RemoveItem");

        e.MapPost("/{id:guid}/payments", async (Guid id, PaymentBody b, CateringService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(Row(await svc.AddPaymentAsync(id, b.Amount, b.PayType ?? "cash", b.Kind ?? "deposit", b.Reference, ct), null)); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Catering.AddPayment").WithSummary("Record a deposit / advance / balance payment.");

        e.MapPost("/{id:guid}/produce", async (Guid id, CateringService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(Row(await svc.ProduceEventAsync(id, ct), null)); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).WithName("Catering.Produce").WithSummary("Consume the package recipe × pax from inventory (posts a production order).");

        return app;
    }

    static object Row(CateringEvent ev, Dictionary<Guid, string>? hallById) => new
    {
        ev.Id, ev.EventNo, ev.Title, ev.Status, ev.Pax, ev.StartsAt, ev.EndsAt,
        ev.HallId, hall = ev.HallId is Guid hid && hallById is not null && hallById.TryGetValue(hid, out var hn) ? hn : null,
        ev.PackageId, ev.CustomerName, ev.CustomerPhone, ev.LocationId,
        ev.PricePerHead, ev.PackageTotal, ev.ExtrasTotal, ev.DiscountAmount, ev.ServiceCharge, ev.TaxAmount,
        ev.TotalAmount, ev.PaidAmount, balance = ev.TotalAmount - ev.PaidAmount,
        ev.IsOffsite, ev.DeliveryAddress, ev.CountryCode, ev.Province, ev.District, ev.PostalCode, ev.Vehicle, ev.Driver, ev.DispatchStatus, ev.Notes,
        ev.FoodCost, ev.ProducedAt, ev.ProductionOrderId, produced = ev.ProducedAt != null,
        margin = ev.TotalAmount - ev.FoodCost,
    };

    static object Detail(CateringEvent ev, string? hallName, string? packageName) => new
    {
        ev.Id, ev.EventNo, ev.Title, ev.Status, ev.Pax, ev.StartsAt, ev.EndsAt,
        ev.HallId, hall = hallName, ev.PackageId, package = packageName,
        ev.CustomerId, ev.CustomerName, ev.CustomerPhone, ev.LocationId,
        ev.PricePerHead, ev.PackageTotal, ev.ExtrasTotal, ev.DiscountAmount, ev.ServiceCharge, ev.TaxAmount,
        ev.TotalAmount, ev.PaidAmount, balance = ev.TotalAmount - ev.PaidAmount,
        ev.IsOffsite, ev.DeliveryAddress, ev.CountryCode, ev.Province, ev.District, ev.PostalCode, ev.Vehicle, ev.Driver, ev.DispatchStatus, ev.Notes,
        ev.FoodCost, ev.ProducedAt, ev.ProductionOrderId, produced = ev.ProducedAt != null,
        margin = ev.TotalAmount - ev.FoodCost,
        items = ev.Items.Where(i => !i.IsDeleted).OrderBy(i => i.CreatedAt)
            .Select(i => new { i.Id, i.Description, i.Quantity, i.UnitPrice, i.LineTotal }),
        payments = ev.Payments.Where(pp => !pp.IsDeleted).OrderBy(pp => pp.PaidAt)
            .Select(pp => new { pp.Id, pp.Amount, pp.PayType, pp.Kind, pp.Reference, pp.PaidAt }),
    };
}

public record HallInput(Guid? Id, string? Code, string? Name, int? Capacity, string? Notes, bool? IsActive, Guid? LocationId);
public record PackageInput(Guid? Id, string? Code, string? Name, decimal? PricePerHead, string? Description, bool? IsActive, Guid? RecipeProductId);
public record StatusBody(string? Status);
public record DispatchBody(string? Status, string? Vehicle = null, string? Driver = null);
public record ItemBody(string? Description, decimal? Quantity, decimal? UnitPrice);
public record PaymentBody(decimal Amount, string? PayType, string? Kind, string? Reference);
