using Microsoft.Extensions.Options;
using WebPush;

namespace Hms.Api.Features.Push;

/// <summary>A single push notification payload — matches what public/sw.js expects on the client.</summary>
public sealed record PushPayload(string Title, string Body, string Url);

/// <summary>Web Push seam (#floor-push): sends a browser push to one subscription. Returns
/// false (and the caller should delete the subscription) when the push service reports the
/// endpoint is gone (404/410 — the browser unsubscribed, cleared data, or the device is retired).</summary>
public interface IPushSender
{
    Task<PushSendResult> SendAsync(string endpoint, string p256dh, string auth, PushPayload payload, CancellationToken ct);
    string? PublicKey { get; }
}

public enum PushSendResult { Sent, Gone, Failed }

public sealed class VapidOptions
{
    public string? PublicKey { get; set; }
    public string? PrivateKey { get; set; }
    public string Subject { get; set; } = "mailto:support@retailit.lk";
}

/// <summary>Sends via the standard Web Push protocol (VAPID-signed) when keys are configured.</summary>
public sealed class VapidPushSender(IOptions<VapidOptions> opt, ILogger<VapidPushSender> log) : IPushSender
{
    private readonly VapidOptions _o = opt.Value;
    public string? PublicKey => _o.PublicKey;

    public async Task<PushSendResult> SendAsync(string endpoint, string p256dh, string auth, PushPayload payload, CancellationToken ct)
    {
        try
        {
            var client = new WebPushClient();
            var sub = new PushSubscription(endpoint, p256dh, auth);
            var vapid = new VapidDetails(_o.Subject, _o.PublicKey, _o.PrivateKey);
            var json = System.Text.Json.JsonSerializer.Serialize(new
            {
                title = payload.Title,
                body = payload.Body,
                url = payload.Url,
            });
            await client.SendNotificationAsync(sub, json, vapid, ct);
            return PushSendResult.Sent;
        }
        catch (WebPushException ex) when (ex.StatusCode is System.Net.HttpStatusCode.NotFound or System.Net.HttpStatusCode.Gone)
        {
            return PushSendResult.Gone;   // subscription is dead — caller should remove it
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Web push send failed for endpoint {Endpoint}", endpoint);
            return PushSendResult.Failed;
        }
    }
}

/// <summary>Fallback when no VAPID keys are configured: logs instead of sending (dev / pre-setup).</summary>
public sealed class LogOnlyPushSender(ILogger<LogOnlyPushSender> log) : IPushSender
{
    public string? PublicKey => null;
    public Task<PushSendResult> SendAsync(string endpoint, string p256dh, string auth, PushPayload payload, CancellationToken ct)
    {
        log.LogInformation("PUSH (no VAPID keys configured) → {Endpoint} | {Title} | {Body}", endpoint, payload.Title, payload.Body);
        return Task.FromResult(PushSendResult.Sent);
    }
}
