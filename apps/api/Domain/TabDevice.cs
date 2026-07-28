namespace Hms.Api.Domain;

/// <summary>
/// A registered tab-ordering device (a waiter handheld). Each ACTIVE device consumes
/// one of the tenant's <c>OrgSettings.TabDeviceLimit</c> licensed seats (#106). The raw
/// device token is shown once at registration; only its SHA-256 hash is stored.
/// </summary>
public class TabDevice : BaseEntity
{
    public string Name { get; set; } = default!;
    public string TokenHash { get; set; } = default!;
    public string? Fingerprint { get; set; }     // device id / browser fingerprint — soft anti-share signal
    public Guid? LocationId { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public bool IsActive { get; set; } = true;
}
