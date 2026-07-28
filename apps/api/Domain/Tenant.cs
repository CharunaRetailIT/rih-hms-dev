namespace Hms.Api.Domain;

/// <summary>
/// A customer of the SaaS platform. Lives in hms_control. Each tenant has its
/// own Postgres database <c>hms_tenant_&lt;id&gt;</c> referenced by <see cref="DatabaseName"/>.
/// </summary>
public class Tenant : ControlEntity
{
    public string Slug { get; set; } = default!;          // e.g. "raffles", URL-safe
    public string DisplayName { get; set; } = default!;
    public string DatabaseName { get; set; } = default!;  // hms_tenant_<slug>
    public string DatabaseHost { get; set; } = default!;
    public TenantStatus Status { get; set; } = TenantStatus.Pending;
    public string Plan { get; set; } = "starter";         // starter | standard | growth | enterprise
    public DateTime? TrialEndsAt { get; set; }
    public string? OwnerEmail { get; set; }
    public string CountryCode { get; set; } = "LK";
    public string DefaultCurrency { get; set; } = "LKR";
    public string TimeZone { get; set; } = "Asia/Colombo";
}

public enum TenantStatus
{
    Pending = 0,        // signup recorded; provisioning queued
    Provisioning = 1,   // Hangfire job in flight
    Trialing = 2,       // active, trial period
    Active = 3,         // active, paid
    PastDue = 4,        // payment failed
    Suspended = 5,      // ops-initiated suspension
    Cancelled = 6,      // customer cancelled
    Deleted = 7         // 90-day retention elapsed; data purged
}

/// <summary>A rotating refresh token (control plane). Only the SHA-256 hash is stored.</summary>
public class RefreshToken : ControlEntity
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public bool IsValid(DateTime now) => RevokedAt is null && !IsDeleted && ExpiresAt > now;
}
