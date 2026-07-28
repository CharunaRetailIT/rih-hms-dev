namespace Hms.Api.Features.Billing;

/// <summary>
/// Platform billing config (bound from the "Billing" section). HomeCountry is RIT's own
/// country — used to decide whether a tenant's subscription is a domestic supply (charge
/// domestic taxes like SL VAT) or an export of services (foreign → export-scoped taxes only).
/// </summary>
public sealed class BillingOptions
{
    public string HomeCountry { get; set; } = "LK";

    /// <summary>Fallback default for the card-at-signup behaviour when the control-plane
    /// platform_settings row is missing. The live toggle is RIT-configurable in the DB
    /// (control.platform_settings 'require_card_at_signup'); default = card-required.</summary>
    public bool RequireCardAtSignup { get; set; } = true;
}
