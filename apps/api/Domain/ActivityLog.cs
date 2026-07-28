namespace Hms.Api.Domain;

/// <summary>
/// An append-only audit entry (#77): who did what, when. CreatedAt is the event
/// time. Never updated or deleted in normal operation.
/// </summary>
public class ActivityLog : BaseEntity
{
    public Guid? ActorId { get; set; }
    public string? ActorName { get; set; }
    public string? ActorRole { get; set; }
    public string Action { get; set; } = default!;   // e.g. order.settle, order.void
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? Summary { get; set; }
    public string? Meta { get; set; }
}
