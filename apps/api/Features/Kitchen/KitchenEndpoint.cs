using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Kitchen;

/// <summary>
/// Kitchen Display System endpoints. Reads active tickets; lets the kitchen
/// mark them preparing / ready / served. In production a Print Agent at the
/// venue subscribes for new tickets (Sprint: Print Agent design).
/// </summary>
public static class KitchenEndpoint
{
    public static IEndpointRouteBuilder MapKitchenEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/kitchen").WithTags("Kitchen").RequireAuthorization("KitchenView");

        g.MapGet("/tickets", async (
            ITenantDbContextFactory factory, CancellationToken ct, string? station, Guid? locationId, bool? recall) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var baseQ = db.KitchenTickets.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(station) && station != "all") baseQ = baseQ.Where(t => t.Station == station);
            if (locationId.HasValue) baseQ = baseQ.Where(t => t.LocationId == locationId.Value);

            // recall=true → the last 10 served/bumped tickets (newest first) so the
            // line can pull a completed ticket back up. Otherwise → the live board:
            // every ticket the kitchen still has to finish. Payment does NOT clear a
            // ticket (a paid bill still has to be cooked + bumped); only the kitchen
            // bumping it, or the order being voided, takes it off the board.
            var q = recall == true
                ? baseQ.Where(t => t.Status == "served").OrderByDescending(t => t.ReadyAt ?? t.CreatedAt).Take(10)
                : baseQ.Where(t => t.Status == "new" || t.Status == "preparing" || t.Status == "ready")
                       .OrderBy(t => t.CreatedAt);

            var list = await q
                .Select(t => new
                {
                    t.Id, t.TicketNumber, t.Station, t.OrderLabel, t.OrderSource,
                    t.Status, t.ItemsJson, t.CreatedAt, t.ReadyAt,
                    // Parent-order status + number so the board can flag a settled bill
                    // ("PAID") and map a ticket back to its order (mobile/POS traceability).
                    OrderStatus = db.Orders.Where(o => o.Id == t.OrderId).Select(o => o.Status).FirstOrDefault(),
                    OrderNumber = db.Orders.Where(o => o.Id == t.OrderId).Select(o => o.OrderNumber).FirstOrDefault(),
                })
                .ToListAsync(ct);
            return Results.Ok(list);
        })
        .WithName("Kitchen.ListTickets")
        .WithSummary("Active KOT/BOT tickets — or the last 10 served when recall=true.");

        g.MapPost("/tickets/{id:guid}/status", async (
            Guid id, TicketStatusInput input, ITenantDbContextFactory factory,
            ITenantContext tenant, KitchenBroadcaster bus, CancellationToken ct) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var t = await db.KitchenTickets.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (t is null) return Results.NotFound();

            t.Status = input.Status; // preparing | ready | served
            if (input.Status == "ready") t.ReadyAt = DateTime.UtcNow;
            if (input.Status == "served") t.ServedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            bus.Publish(db.TenantId);   // push the bump to every connected board
            return Results.Ok(new { t.Id, t.Status });
        })
        .WithName("Kitchen.SetTicketStatus")
        .WithSummary("Mark a ticket preparing / ready / served.");

        // Server-Sent Events stream: the KDS opens this once and gets a "changed"
        // line whenever the board moves (new ticket, bump, settle, void) — so it
        // refreshes in real time instead of waiting for its poll. Plain HTTP, so
        // it rides the same /api proxy and keeps the JWT on the Authorization
        // header (the browser reads it with fetch, not EventSource). A comment
        // heartbeat every 20s keeps idle connections alive through proxies.
        g.MapGet("/stream", async (HttpContext ctx, ITenantContext tenant, KitchenBroadcaster bus, CancellationToken ct) =>
        {
            var tid = tenant.TenantIdOrThrow();
            ctx.Response.Headers.ContentType = "text/event-stream";
            ctx.Response.Headers.CacheControl = "no-cache";
            ctx.Response.Headers["X-Accel-Buffering"] = "no";   // don't let nginx buffer the stream

            var (handle, reader) = bus.Subscribe(tid);
            try
            {
                await ctx.Response.WriteAsync("retry: 3000\n\n", ct);
                await ctx.Response.WriteAsync("data: connected\n\n", ct);
                await ctx.Response.Body.FlushAsync(ct);

                Task<bool>? pending = null;
                while (!ct.IsCancellationRequested)
                {
                    pending ??= reader.WaitToReadAsync(ct).AsTask();
                    var done = await Task.WhenAny(pending, Task.Delay(TimeSpan.FromSeconds(20), ct));
                    if (done == pending)
                    {
                        if (!await pending) break;     // channel completed
                        pending = null;
                        while (reader.TryRead(out var evt))
                            await ctx.Response.WriteAsync($"data: {evt}\n\n", ct);
                    }
                    else
                    {
                        await ctx.Response.WriteAsync(": ping\n\n", ct);   // heartbeat comment
                    }
                    await ctx.Response.Body.FlushAsync(ct);
                }
            }
            catch (OperationCanceledException) { /* client went away — normal */ }
            finally { handle.Dispose(); }
        })
        .WithName("Kitchen.Stream")
        .WithSummary("Server-Sent Events: pushes a 'changed' signal when the kitchen board updates.");

        return app;
    }
}

public record TicketStatusInput(string Status);
