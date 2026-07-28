namespace Hms.Api.Domain;

/// <summary>
/// Control-plane subscription header for a tenant (#109). One per tenant. Carries the
/// chosen plan + lifecycle status; the priced line-items (plan + add-on quantities)
/// live in <see cref="SubscriptionItem"/>. Entitlements are PROJECTED from this into
/// the tenant's org_settings for runtime enforcement.
/// </summary>
public class Subscription : ControlEntity
{
    public Guid TenantId { get; set; }
    public string Provider { get; set; } = "manual";       // stripe | payhere | manual
    public string? ProviderCustomerId { get; set; }
    public string? ProviderSubId { get; set; }
    public string Plan { get; set; } = "lite";
    public string Status { get; set; } = "trialing";        // trialing | active | past_due | cancelled
    public DateTime? CurrentPeriodStart { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Saved payment method (PayHere preapproval tokenization, #110). The token lets the
    // Charging API bill the customer server-to-server on renewal / mid-cycle upgrade.
    // Stored control-side only; never exposed to the tenant.
    public string? CustomerToken { get; set; }
    public string? CardBrand { get; set; }                  // VISA / MASTER / AMEX …
    public string? CardLast4 { get; set; }
    public DateTime? PaymentMethodUpdatedAt { get; set; }
}

/// <summary>A priced line on a subscription: the plan, an add-on (with quantity), or extra locations.</summary>
public class SubscriptionItem : ControlEntity
{
    public Guid SubscriptionId { get; set; }
    public string ItemType { get; set; } = "addon";         // plan | addon | location
    public string ItemCode { get; set; } = default!;        // plan/addon code (or 'location')
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }                  // price snapshot at purchase
    public string Currency { get; set; } = "LKR";
}
