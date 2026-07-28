namespace Hms.Api.Domain;

/// <summary>
/// A signed-in waiter's Firebase Cloud Messaging registration token for the handheld app —
/// the mobile sibling of <see cref="PushSubscription"/> (which is web/VAPID). Captured on
/// sign-in; drives floor-scoped push for new guest QR orders reaching the Flutter app even
/// when it's backgrounded or fully closed. One row per (app install, device) — FCM tokens
/// rotate, so re-registering on sign-in just upserts.
/// </summary>
public class DeviceToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = default!;
    public string Platform { get; set; } = "android";   // android | ios
    public DateTime? LastUsedAt { get; set; }
}
