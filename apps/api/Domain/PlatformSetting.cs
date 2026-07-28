namespace Hms.Api.Domain;

/// <summary>
/// RIT-wide platform setting (control-plane key/value). Lets RIT admin flip platform behaviour
/// (e.g. require_card_at_signup) at runtime without a code change or redeploy. Not per-tenant.
/// </summary>
public class PlatformSetting
{
    public string Key { get; set; } = default!;     // PK
    public string Value { get; set; } = default!;
    public DateTime UpdatedAt { get; set; }
}
