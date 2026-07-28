namespace Hms.Api.Domain;

/// <summary>
/// A browser's Web Push subscription (VAPID) for a staff member — captured once they opt in
/// from the Floor screen. Drives floor-scoped push for new guest QR orders (#floor-push):
/// when an order lands, every target steward's active subscriptions get a push, even if their
/// tab/app is closed. One row per (browser, device) — a user can have several.
/// </summary>
public class PushSubscription : BaseEntity
{
    public Guid UserId { get; set; }
    public string Endpoint { get; set; } = default!;
    public string P256dh { get; set; } = default!;
    public string Auth { get; set; } = default!;
    public DateTime? LastUsedAt { get; set; }
}
