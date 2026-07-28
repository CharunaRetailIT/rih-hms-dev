namespace Hms.Api.Domain;

/// <summary>
/// A human user within a tenant. v1 supports email + magic-link auth.
/// Roles are enforced at the API layer; permissions are role-bundled in v1.
/// </summary>
public class User : BaseEntity
{
    public string? Email { get; set; }                 // optional: POS staff may have none
    public string? Username { get; set; }              // login id for PIN sign-in (unique per tenant)
    public string DisplayName { get; set; } = default!;
    public UserRole Role { get; set; } = UserRole.Cashier;
    public Guid? HomeLocationId { get; set; }          // pinned outlet (null = head-office / all-access; Owner is always all-access)
    public bool IsActive { get; set; } = true;
    public bool IsServer { get; set; }                 // #76: can be attributed as the steward/server on a bill (independent of Role)
    public DateTime? LastLoginAt { get; set; }
    public string? PhoneE164 { get; set; }

    // Staff PIN login. PBKDF2 hash (never plaintext); brute-force guarded by a
    // failed-attempt counter + temporary lockout. Null = no PIN (magic-link only).
    public string? PasscodeHash { get; set; }
    public int PinFailedCount { get; set; }
    public DateTime? PinLockedUntil { get; set; }
}

public enum UserRole
{
    Owner = 0,        // tenant owner — billing, all permissions
    Manager = 1,      // outlet management, reports
    Cashier = 2,      // POS, take orders, settle
    Kitchen = 3,      // KOT view, mark-ready
    Accountant = 4,   // reports, GL export — no POS
    Admin = 5         // owner-level access EXCEPT removing/altering the owner + editing business identity
}
