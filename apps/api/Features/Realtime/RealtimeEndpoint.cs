using Hms.Api.Infrastructure;

namespace Hms.Api.Features.Realtime;

/// <summary>
/// App-wide Server-Sent Events stream. Any authenticated client opens this once
/// and receives a one-line "topic changed" signal whenever something relevant to
/// the current tenant happens — a new aggregator order, an order settled/voided,
/// etc. The browser reacts per topic (the notification bell re-fetches its feed;
/// the delivery board reloads its queue) instead of polling on a timer.
///
/// Consumed with fetch (not EventSource) so the JWT rides the Authorization
/// header — no token in the URL. The data line carries the topic; a comment
/// heartbeat every 20s keeps idle connections alive through proxies.
/// </summary>
public static class RealtimeEndpoint
{
    public static IEndpointRouteBuilder MapRealtimeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/events/stream", async (HttpContext ctx, ITenantContext tenant, RealtimeBus bus, CancellationToken ct) =>
        {
            var tid = tenant.TenantIdOrThrow();
            ctx.Response.Headers.ContentType = "text/event-stream";
            ctx.Response.Headers.CacheControl = "no-cache";
            ctx.Response.Headers["X-Accel-Buffering"] = "no";   // don't let nginx buffer the stream

            // Tracked so a floor-scoped publish (a guest order landing on this steward's
            // floor) can target just this connection instead of the whole tenant.
            var rawSub = ctx.User.FindFirst("sub")?.Value ?? ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = Guid.TryParse(rawSub, out var uid) ? uid : null;

            var (handle, reader) = bus.Subscribe(tid, userId);
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
                        while (reader.TryRead(out var topic))
                            await ctx.Response.WriteAsync($"data: {topic}\n\n", ct);
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
        .RequireAuthorization()
        .WithName("Realtime.Stream")
        .WithTags("Realtime")
        .WithSummary("Server-Sent Events: pushes a per-tenant 'topic changed' signal (notifications, orders, …).");

        return app;
    }
}
