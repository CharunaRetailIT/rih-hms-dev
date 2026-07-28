using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Tables;

public static class TablesEndpoint
{
    public static IEndpointRouteBuilder MapTableEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Tables (setup = SupplyChain; reads = any authed for the floor view) ──
        var t = app.MapGroup("/api/v1/tables").WithTags("Tables");

        t.MapGet("", async (ITenantDbContextFactory factory, CancellationToken ct, Guid? locationId, bool? all) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var q = db.RestaurantTables.AsNoTracking();
            if (all != true) q = q.Where(x => x.IsActive);   // floor/POS use active only; manage view passes all=true
            if (locationId is Guid l) q = q.Where(x => x.LocationId == l);
            var list = await q.OrderBy(x => x.Area).ThenBy(x => x.SortOrder).ThenBy(x => x.Code)
                .Select(x => new { x.Id, x.LocationId, x.Code, x.Name, x.Seats, x.Area, x.FloorId, x.SortOrder, x.IsActive })
                .ToListAsync(ct);
            return Results.Ok(list);
        }).WithName("Tables.List");

        // Floor view: each table + whether it currently holds an open/confirmed order.
        t.MapGet("/status", async (ITenantDbContextFactory factory, CancellationToken ct, Guid? locationId) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var tq = db.RestaurantTables.AsNoTracking().Where(x => x.IsActive);
            if (locationId is Guid l) tq = tq.Where(x => x.LocationId == l);
            var tables = await tq.OrderBy(x => x.Area).ThenBy(x => x.SortOrder).ThenBy(x => x.Code).ToListAsync(ct);

            var openOrders = await db.Orders.AsNoTracking()
                .Where(o => (o.Status == "open" || o.Status == "confirmed") && o.TableId != null)
                .Select(o => new { o.Id, o.OrderNumber, o.TableId, o.TotalAmount, o.Status, o.PendingAcceptance, o.OpenedAt })
                .ToListAsync(ct);
            var byTable = openOrders.GroupBy(o => o.TableId!.Value).ToDictionary(g => g.Key, g => g.OrderBy(o => o.OpenedAt).First());

            var result = tables.Select(x =>
            {
                byTable.TryGetValue(x.Id, out var o);
                return new
                {
                    x.Id, x.LocationId, x.Code, x.Name, x.Seats, x.Area, x.FloorId, x.PosX, x.PosY,
                    occupied = o is not null,
                    orderId = o?.Id, orderNumber = o?.OrderNumber, total = o?.TotalAmount ?? 0m, orderStatus = o?.Status,
                    pendingAcceptance = o?.PendingAcceptance ?? false,
                };
            });
            return Results.Ok(result);
        }).WithName("Tables.Status");

        t.MapPut("", async (TableInput i, ITenantDbContextFactory factory, LocationScope scope, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(i.Code) || i.LocationId == Guid.Empty)
                return Results.BadRequest(new { error = "Code and location are required" });
            scope.Assert(i.LocationId);   // #scoping-P2
            await using var db = await factory.CreateForCurrentAsync(ct);
            var code = i.Code.Trim().ToUpperInvariant();
            var row = i.Id is Guid id
                ? await db.RestaurantTables.FirstOrDefaultAsync(x => x.Id == id, ct)
                : await db.RestaurantTables.FirstOrDefaultAsync(x => x.LocationId == i.LocationId && x.Code == code, ct);
            if (row is null) { row = new RestaurantTable { LocationId = i.LocationId, Code = code }; db.RestaurantTables.Add(row); }
            row.Code = code; row.Name = i.Name; row.Seats = i.Seats <= 0 ? 2 : i.Seats;
            row.Area = string.IsNullOrWhiteSpace(i.Area) ? null : i.Area.Trim(); row.FloorId = i.FloorId; row.SortOrder = i.SortOrder; row.IsActive = true;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { row.Id, row.Code, row.Name, row.Seats, row.Area, row.FloorId });
        }).WithName("Tables.Set").RequireAuthorization("SupplyChain");

        // Save the visual floor-plan layout (#68) — managers arrange the floor.
        t.MapPut("/layout", async (LayoutInput body, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var ids = body.Items.Select(i => i.Id).ToList();
            var rows = await db.RestaurantTables.Where(x => ids.Contains(x.Id)).ToListAsync(ct);
            foreach (var r in rows)
            {
                var i = body.Items.First(z => z.Id == r.Id);
                r.PosX = Math.Max(0, i.PosX); r.PosY = Math.Max(0, i.PosY);
            }
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { saved = rows.Count });
        }).WithName("Tables.SaveLayout").RequireAuthorization("Operations");

        // Temporarily take a table out of service / bring it back (#68). Unlike
        // Remove this is reversible and keeps the table; an inactive table is hidden
        // from the floor + can't be seated. Can't deactivate one that's occupied.
        t.MapPost("/{id:guid}/active", async (Guid id, TableActiveInput body, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var row = await db.RestaurantTables.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (row is null) return Results.NotFound();
            if (!body.IsActive)
            {
                var occupied = await db.Orders.AnyAsync(o => o.TableId == id && (o.Status == "open" || o.Status == "confirmed"), ct);
                if (occupied) return Results.BadRequest(new { error = "Table is occupied — settle or move the bill first" });
            }
            row.IsActive = body.IsActive;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { row.Id, row.IsActive });
        }).WithName("Tables.SetActive").RequireAuthorization("SupplyChain");

        t.MapDelete("/{id:guid}", async (Guid id, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var row = await db.RestaurantTables.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (row is null) return Results.NotFound();
            row.IsDeleted = true; row.IsActive = false;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).WithName("Tables.Delete").RequireAuthorization("SupplyChain");

        // ── Reservations (front-of-house = Operations) ──
        var r = app.MapGroup("/api/v1/reservations").WithTags("Reservations").RequireAuthorization("Operations");

        r.MapGet("", async (ITenantDbContextFactory factory, CancellationToken ct, Guid? locationId, DateTime? from, DateTime? to) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var fromD = from is { } f ? OrderServiceUtc(f) : DateTime.UtcNow.Date;
            var toD = to is { } tt ? OrderServiceUtc(tt) : fromD.AddDays(1);
            var q = db.Reservations.AsNoTracking().Where(x => x.ReservedAt >= fromD && x.ReservedAt < toD);
            if (locationId is Guid l) q = q.Where(x => x.LocationId == l);
            var list = await q.OrderBy(x => x.ReservedAt)
                .Select(x => new { x.Id, x.LocationId, x.TableId, x.CustomerName, x.Phone, x.PartySize, x.ReservedAt, x.DurationMinutes, x.Status, x.Notes })
                .ToListAsync(ct);
            return Results.Ok(list);
        }).WithName("Reservations.List");

        r.MapPut("", async (ReservationInput i, ITenantDbContextFactory factory, LocationScope scope, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(i.CustomerName) || i.LocationId == Guid.Empty)
                return Results.BadRequest(new { error = "Customer name and location are required" });
            scope.Assert(i.LocationId);   // #scoping-P2
            await using var db = await factory.CreateForCurrentAsync(ct);
            var row = i.Id is Guid id ? await db.Reservations.FirstOrDefaultAsync(x => x.Id == id, ct) : null;
            if (row is null) { row = new Reservation { LocationId = i.LocationId }; db.Reservations.Add(row); }
            row.LocationId = i.LocationId; row.TableId = i.TableId; row.CustomerName = i.CustomerName.Trim();
            row.Phone = i.Phone; row.PartySize = i.PartySize <= 0 ? 2 : i.PartySize;
            row.ReservedAt = OrderServiceUtc(i.ReservedAt); row.DurationMinutes = i.DurationMinutes <= 0 ? 90 : i.DurationMinutes;
            row.Notes = i.Notes;
            if (!string.IsNullOrWhiteSpace(i.Status)) row.Status = i.Status;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { row.Id });
        }).WithName("Reservations.Set");

        r.MapPost("/{id:guid}/status", async (Guid id, StatusInput body, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            var allowed = new[] { "booked", "seated", "cancelled", "no_show", "completed" };
            if (!allowed.Contains(body.Status)) return Results.BadRequest(new { error = "Invalid status" });
            await using var db = await factory.CreateForCurrentAsync(ct);
            var row = await db.Reservations.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (row is null) return Results.NotFound();
            row.Status = body.Status;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { row.Id, row.Status });
        }).WithName("Reservations.SetStatus");

        r.MapDelete("/{id:guid}", async (Guid id, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var row = await db.Reservations.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (row is null) return Results.NotFound();
            row.IsDeleted = true;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).WithName("Reservations.Delete");

        return app;
    }

    private static DateTime OrderServiceUtc(DateTime d) => d.Kind switch
    {
        DateTimeKind.Utc => d,
        DateTimeKind.Local => d.ToUniversalTime(),
        _ => DateTime.SpecifyKind(d, DateTimeKind.Utc),
    };
}

public record TableInput(Guid? Id, Guid LocationId, string Code, string? Name, int Seats = 2, string? Area = null, Guid? FloorId = null, int SortOrder = 0);
public record TableActiveInput(bool IsActive);
public record LayoutInput(List<TablePos> Items);
public record TablePos(Guid Id, int PosX, int PosY);
public record ReservationInput(Guid? Id, Guid LocationId, Guid? TableId, string CustomerName, string? Phone,
    int PartySize, DateTime ReservedAt, int DurationMinutes = 90, string? Status = null, string? Notes = null);
public record StatusInput(string Status);
