using System.Security.Claims;
using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Push;

/// <summary>
/// Push subscription capture (#floor-push) for both platforms: Web Push (browser, VAPID) and
/// FCM device tokens (Flutter handheld). A steward opts in from the Floor screen / signs in on
/// the handheld; the subscription/token is stored here so a floor-scoped guest order can push
/// to them even with the tab/app closed. See GuestEndpoint for the send side.
/// </summary>
public static class PushEndpoint
{
    public static IEndpointRouteBuilder MapPushEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/push").WithTags("Push").RequireAuthorization();

        // The client needs this to call pushManager.subscribe({ applicationServerKey: ... }).
        g.MapGet("/vapid-public-key", (IPushSender sender) =>
            string.IsNullOrWhiteSpace(sender.PublicKey)
                ? Results.Json(new { error = "Push isn't configured for this server yet." }, statusCode: 503)
                : Results.Ok(new { publicKey = sender.PublicKey }))
            .WithName("Push.VapidPublicKey");

        g.MapPost("/subscribe", async (SubscribeInput i, ITenantDbContextFactory f, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = CurrentUserId(user);
            if (userId is null) return Results.Unauthorized();
            if (string.IsNullOrWhiteSpace(i.Endpoint) || string.IsNullOrWhiteSpace(i.Keys?.P256dh) || string.IsNullOrWhiteSpace(i.Keys?.Auth))
                return Results.BadRequest(new { error = "Invalid subscription." });

            await using var db = await f.CreateForCurrentAsync(ct);
            var row = await db.PushSubscriptions.FirstOrDefaultAsync(x => x.Endpoint == i.Endpoint, ct);
            if (row is null) { row = new PushSubscription { Endpoint = i.Endpoint }; db.PushSubscriptions.Add(row); }
            row.UserId = userId.Value;
            row.P256dh = i.Keys.P256dh;
            row.Auth = i.Keys.Auth;
            row.LastUsedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { row.Id });
        }).WithName("Push.Subscribe");

        g.MapPost("/unsubscribe", async (UnsubscribeInput i, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var row = await db.PushSubscriptions.FirstOrDefaultAsync(x => x.Endpoint == i.Endpoint, ct);
            if (row is null) return Results.NoContent();
            row.IsDeleted = true;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).WithName("Push.Unsubscribe");

        // Whether the CURRENT device (endpoint) already has an active subscription — lets the
        // Floor screen show "Notifications on" instead of the opt-in prompt on repeat visits.
        g.MapGet("/status", async (string? endpoint, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(endpoint)) return Results.Ok(new { subscribed = false });
            await using var db = await f.CreateForCurrentAsync(ct);
            var subscribed = await db.PushSubscriptions.AsNoTracking().AnyAsync(x => x.Endpoint == endpoint, ct);
            return Results.Ok(new { subscribed });
        }).WithName("Push.Status");

        // ── FCM device tokens (Flutter handheld — #floor-push Phase 4) ──
        // Registered once the waiter signs in; re-registering (fresh token, e.g. after a
        // reinstall) just upserts, so the handheld can call this unconditionally on sign-in.
        g.MapPost("/device-token", async (DeviceTokenInput i, ITenantDbContextFactory f, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = CurrentUserId(user);
            if (userId is null) return Results.Unauthorized();
            if (string.IsNullOrWhiteSpace(i.Token)) return Results.BadRequest(new { error = "Invalid device token." });
            var platform = i.Platform is "ios" ? "ios" : "android";

            await using var db = await f.CreateForCurrentAsync(ct);
            var row = await db.DeviceTokens.FirstOrDefaultAsync(x => x.Token == i.Token, ct);
            if (row is null) { row = new DeviceToken { Token = i.Token }; db.DeviceTokens.Add(row); }
            row.UserId = userId.Value;
            row.Platform = platform;
            row.LastUsedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { row.Id });
        }).WithName("Push.RegisterDeviceToken");

        g.MapPost("/device-token/unregister", async (DeviceTokenUnregisterInput i, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var row = await db.DeviceTokens.FirstOrDefaultAsync(x => x.Token == i.Token, ct);
            if (row is null) return Results.NoContent();
            row.IsDeleted = true;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).WithName("Push.UnregisterDeviceToken");

        return app;
    }

    private static Guid? CurrentUserId(ClaimsPrincipal user)
    {
        var raw = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out var g) ? g : null;
    }
}

public record SubscribeInput(string Endpoint, PushKeys Keys);
public record PushKeys(string P256dh, string Auth);
public record UnsubscribeInput(string Endpoint);
public record DeviceTokenInput(string Token, string Platform = "android");
public record DeviceTokenUnregisterInput(string Token);
