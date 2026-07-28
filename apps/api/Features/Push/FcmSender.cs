using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;

namespace Hms.Api.Features.Push;

/// <summary>Firebase Cloud Messaging seam (#floor-push, Phase 4) — the mobile sibling of
/// <see cref="IPushSender"/> (which is web/VAPID). Sends to the Flutter handheld app.</summary>
public interface IFcmSender
{
    Task<PushSendResult> SendAsync(string token, PushPayload payload, CancellationToken ct);
    bool IsConfigured { get; }
}

public sealed class FirebaseOptions
{
    /// <summary>Path to the Firebase service-account JSON key (Project Settings → Service
    /// accounts → Generate new private key, in the Firebase console). Never commit the real
    /// file — point this at a path outside the repo (e.g. /opt/hms/firebase-service-account.json).</summary>
    public string? ServiceAccountKeyPath { get; set; }
}

/// <summary>Sends via the Firebase Admin SDK when a service-account key is configured.</summary>
public sealed class FirebaseFcmSender : IFcmSender
{
    private readonly FirebaseApp? _app;
    private readonly ILogger<FirebaseFcmSender> _log;

    public FirebaseFcmSender(IOptions<FirebaseOptions> opt, ILogger<FirebaseFcmSender> log)
    {
        _log = log;
        var path = opt.Value.ServiceAccountKeyPath;
        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
        {
            // One FirebaseApp per process — reuse the default instance across requests/restarts
            // in dev (hot reload can re-run this constructor without the process actually restarting).
            _app = FirebaseApp.DefaultInstance ?? FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(path),
            });
        }
    }

    public bool IsConfigured => _app is not null;

    public async Task<PushSendResult> SendAsync(string token, PushPayload payload, CancellationToken ct)
    {
        if (_app is null) return PushSendResult.Failed;
        try
        {
            var message = new Message
            {
                Token = token,
                Notification = new Notification { Title = payload.Title, Body = payload.Body },
                Data = new Dictionary<string, string> { ["url"] = payload.Url },
            };
            await FirebaseMessaging.GetMessaging(_app).SendAsync(message, ct);
            return PushSendResult.Sent;
        }
        catch (FirebaseMessagingException ex) when (
            ex.MessagingErrorCode is MessagingErrorCode.Unregistered or MessagingErrorCode.InvalidArgument)
        {
            return PushSendResult.Gone;   // token is dead (app uninstalled / re-registered) — caller should remove it
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "FCM send failed for token ending {TokenTail}", token.Length > 6 ? token[^6..] : token);
            return PushSendResult.Failed;
        }
    }
}

/// <summary>Fallback when no Firebase service-account key is configured: logs instead of sending.</summary>
public sealed class LogOnlyFcmSender(ILogger<LogOnlyFcmSender> log) : IFcmSender
{
    public bool IsConfigured => false;
    public Task<PushSendResult> SendAsync(string token, PushPayload payload, CancellationToken ct)
    {
        log.LogInformation("FCM (no service-account key configured) → {Token} | {Title} | {Body}", token, payload.Title, payload.Body);
        return Task.FromResult(PushSendResult.Sent);
    }
}
