using System.Security.Claims;
using Hms.Api.Features.Orders;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Notifications;

/// <summary>
/// In-app notification centre. Aggregates actionable items for the current tenant
/// into a single feed the topbar bell can render with a live count — low stock,
/// pending aggregator (Uber Eats / PickMe) orders awaiting acceptance, pending guest
/// QR orders (floor-scoped — see below), today's table reservations, and upcoming
/// catering events. Read-only, any authenticated user (everyone sees the bell). The
/// badge count is items.Count.
/// </summary>
public static class NotificationsEndpoint
{
    public sealed record NotifItem(string Type, string Severity, string Title, string Detail, string Href, DateTime At);

    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/notifications").WithTags("Notifications").RequireAuthorization();

        g.MapGet("", async (ITenantDbContextFactory f, ClaimsPrincipal user, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var dayStart = OrderService.AsUtc(DateTime.UtcNow.Date);
            var dayEnd = dayStart.AddDays(1);
            var weekEnd = dayStart.AddDays(7);

            var items = new List<NotifItem>();

            // 1) Pending aggregator orders awaiting acceptance.
            var agg = await db.Orders.AsNoTracking()
                .Where(o => (o.OrderSource == "ubereats" || o.OrderSource == "pickme")
                         && (o.Status == "open" || o.Status == "confirmed")
                         && (o.AggregatorStatus == null || o.AggregatorStatus == "pending"))
                .OrderByDescending(o => o.OpenedAt)
                .Select(o => new { o.OrderNumber, o.OrderSource, o.OpenedAt, o.CustomerName })
                .Take(20).ToListAsync(ct);
            foreach (var o in agg)
            {
                var src = o.OrderSource == "ubereats" ? "Uber Eats" : "PickMe";
                items.Add(new NotifItem("aggregator", "info",
                    $"New {src} order {o.OrderNumber}",
                    string.IsNullOrWhiteSpace(o.CustomerName) ? "Awaiting acceptance" : $"{o.CustomerName} · awaiting acceptance",
                    "/delivery", o.OpenedAt));
            }

            // 1b) Pending guest QR orders (#108) awaiting a steward's acceptance — floor-scoped
            // (#floor-push): if the viewer covers specific floors, only show orders on tables on
            // one of THEIR floors (or on a table with no floor set, since that's nobody's in
            // particular). A viewer with no floor assignments at all sees every pending order —
            // same "no assignment = covers everything" convention used for push routing.
            var guestPending = await db.Orders.AsNoTracking()
                .Where(o => o.OrderSource == "guest_qr" && o.PendingAcceptance)
                .OrderByDescending(o => o.OpenedAt)
                .Select(o => new { o.OrderNumber, o.OpenedAt, o.TableLabel, o.TableId, o.TotalAmount })
                .Take(20).ToListAsync(ct);
            if (guestPending.Count > 0)
            {
                var rawSub = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var myFloorIds = Guid.TryParse(rawSub, out var uid)
                    ? (await db.UserFloors.AsNoTracking().Where(x => x.UserId == uid).Select(x => x.FloorId).ToListAsync(ct)).ToHashSet()
                    : new HashSet<Guid>();

                var tableIds = guestPending.Where(o => o.TableId != null).Select(o => o.TableId!.Value).Distinct().ToList();
                var tableFloors = await db.RestaurantTables.AsNoTracking()
                    .Where(t => tableIds.Contains(t.Id)).ToDictionaryAsync(t => t.Id, t => t.FloorId, ct);

                foreach (var o in guestPending)
                {
                    var floorId = o.TableId is Guid tid && tableFloors.TryGetValue(tid, out var fid) ? fid : null;
                    if (myFloorIds.Count > 0 && floorId is Guid f2 && !myFloorIds.Contains(f2)) continue;   // not my floor
                    items.Add(new NotifItem("guest_order", "warning",
                        o.TableLabel is { } tl ? $"New order — Table {tl}" : $"New order — {o.OrderNumber}",
                        $"{o.OrderNumber} · LKR {o.TotalAmount:0.00} · awaiting acceptance",
                        "/floor", o.OpenedAt));
                }
            }

            // 2) Low / out-of-stock products (summed across locations vs reorder level).
            var low = await db.Products.AsNoTracking()
                .Where(p => p.IsActive && p.ReorderLevel != null && p.ReorderLevel > 0
                         && (db.ProductStocks.Where(s => s.ProductId == p.Id).Sum(s => (decimal?)s.QuantityOnHand) ?? 0m) <= p.ReorderLevel!.Value)
                .Select(p => new
                {
                    p.Name,
                    Level = p.ReorderLevel!.Value,
                    Qoh = db.ProductStocks.Where(s => s.ProductId == p.Id).Sum(s => (decimal?)s.QuantityOnHand) ?? 0m
                })
                .OrderBy(x => x.Qoh)
                .Take(20).ToListAsync(ct);
            foreach (var p in low)
                items.Add(new NotifItem("low_stock", p.Qoh <= 0 ? "critical" : "warning",
                    p.Qoh <= 0 ? $"Out of stock: {p.Name}" : $"Low stock: {p.Name}",
                    $"{p.Qoh:0.##} on hand · reorder at {p.Level:0.##}",
                    "/inventory", dayStart));

            // 3) Today's table reservations still booked.
            var resv = await db.Reservations.AsNoTracking()
                .Where(r => r.Status == "booked" && r.ReservedAt >= dayStart && r.ReservedAt < dayEnd)
                .OrderBy(r => r.ReservedAt)
                .Select(r => new { r.CustomerName, r.PartySize, r.ReservedAt })
                .Take(20).ToListAsync(ct);
            foreach (var r in resv)
                items.Add(new NotifItem("reservation", "info",
                    $"Reservation: {r.CustomerName}",
                    $"{r.PartySize} pax · {r.ReservedAt:HH:mm}",
                    "/floor", r.ReservedAt));

            // 4) Upcoming catering events (next 7 days), confirmed or running.
            var events = await db.CateringEvents.AsNoTracking()
                .Where(e => (e.Status == "confirmed" || e.Status == "running")
                         && e.StartsAt >= dayStart && e.StartsAt < weekEnd)
                .OrderBy(e => e.StartsAt)
                .Select(e => new { e.EventNo, e.CustomerName, e.StartsAt })
                .Take(20).ToListAsync(ct);
            foreach (var e in events)
                items.Add(new NotifItem("catering", "info",
                    $"Event {e.EventNo}",
                    $"{(string.IsNullOrWhiteSpace(e.CustomerName) ? "" : e.CustomerName + " · ")}{e.StartsAt:ddd dd MMM, HH:mm}",
                    "/catering", e.StartsAt));

            // 5) Near-expiry / expired stock batches (posted GRN lines, within 14 days).
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var expHorizon = today.AddDays(14);
            var exp = await (from gl in db.GrnLines.AsNoTracking()
                             join g in db.GoodsReceivedNotes.AsNoTracking() on gl.GrnId equals g.Id
                             where !gl.IsDeleted && g.Status == "posted"
                                   && gl.ExpiryDate != null && gl.ExpiryDate <= expHorizon
                             orderby gl.ExpiryDate
                             select new { gl.ProductName, gl.BatchNo, gl.ExpiryDate })
                            .Take(20).ToListAsync(ct);
            foreach (var e in exp)
            {
                var dte = e.ExpiryDate!.Value.DayNumber - today.DayNumber;
                items.Add(new NotifItem("expiry", dte <= 0 ? "critical" : "warning",
                    dte <= 0 ? $"Expired: {e.ProductName}" : $"Expiring soon: {e.ProductName}",
                    $"Batch {(string.IsNullOrWhiteSpace(e.BatchNo) ? "—" : e.BatchNo)} · {(dte <= 0 ? "expired" : dte + "d left")} · {e.ExpiryDate:yyyy-MM-dd}",
                    "/inventory", dayStart));
            }

            var ordered = items
                .OrderByDescending(i => i.Severity == "critical")
                .ThenByDescending(i => i.At)
                .ToList();

            return Results.Ok(new { count = ordered.Count, items = ordered });
        });

        return app;
    }
}
