namespace Hms.Api.Domain;

/// <summary>
/// A category of charge — Tax, Service Charge, Levy, A/C Cost, etc.
/// <see cref="AppliesPerProduct"/> decides how charges of this type are assigned:
/// true = attached to specific products (a product may carry several), false =
/// tenant-wide, applied to every order regardless of product.
/// </summary>
public class ChargeType : BaseEntity
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public bool AppliesPerProduct { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// A configured charge — either a percentage rate or a flat amount (mutually
/// exclusive), categorized by <see cref="ChargeType"/>. Per-product charges are
/// assigned via <see cref="ProductCharge"/>; tenant-wide charges apply to every
/// order without needing an assignment.
/// </summary>
public class Charge : BaseEntity
{
    public Guid ChargeTypeId { get; set; }
    public string Code { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal? Percentage { get; set; }
    public decimal? Amount { get; set; }
    public bool IsActive { get; set; } = true;

    public ChargeType? ChargeType { get; set; }
}

/// <summary>Which charge(s) — normally tax-type — apply to a product. A product can carry several.</summary>
public class ProductCharge : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid ChargeId { get; set; }

    public Charge? Charge { get; set; }
}

/// <summary>Computed snapshot of one bill-level charge applied to an order (for the invoice + VAT return).</summary>
public class OrderCharge
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid OrderId { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string ChargeType { get; set; } = default!;
    public decimal RatePercent { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal ChargeAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Computed snapshot of one per-product charge applied to a single order line (invoice detail + VAT return).</summary>
public class OrderItemCharge
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid OrderItemId { get; set; }
    public string Code { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal? Percentage { get; set; }
    public decimal? Amount { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal ChargeAmount { get; set; }
    public DateTime CreatedAt { get; set; }

    public OrderItem? OrderItem { get; set; }
}

/// <summary>Per-tenant org-level configuration, edited from the ERP dashboard.</summary>
public class OrgSettings
{
    public Guid TenantId { get; set; }
    public string? LegalName { get; set; }
    public string? VatRegistrationNumber { get; set; }
    public string? BusinessRegistrationNo { get; set; }
    public bool VatEnabled { get; set; } = true;
    public string TaxLabel { get; set; } = "VAT";        // country-derived display name for the consumption tax: VAT / GST / Sales Tax (#tax-label)
    public string InvoicePrefix { get; set; } = "INV";
    public string? TaxInvoiceFooter { get; set; }
    public string BaseCurrency { get; set; } = "LKR";
    public string? LogoUrl { get; set; }                  // tenant logo — data URL or hosted URL (#82a)
    public string VatFilingFrequency { get; set; } = "monthly";

    // Document number prefixes (admin-configurable)
    public string OrderPrefix { get; set; } = "ORD";
    public string PoPrefix { get; set; } = "PO";
    public string GrnPrefix { get; set; } = "GRN";
    public string SupplierPrefix { get; set; } = "SUP";
    public string TransferPrefix { get; set; } = "TRF";
    public string WastagePrefix { get; set; } = "WST";
    public string AdjustmentPrefix { get; set; } = "ADJ";
    public string RequestNotePrefix { get; set; } = "REQ";
    public string ProductionPrefix { get; set; } = "PRD";
    public string ProductCodePrefix { get; set; } = "ITM";   // prefix for auto-generated product codes (#2)
    public bool AllowDiscountBelowCost { get; set; }         // admin override: let per-product discounts go below cost (#3b)
    public string ShiftPrefix { get; set; } = "SH";

    // Procurement governance (maker-checker)
    public bool RequirePoApproval { get; set; }
    public decimal PoApprovalThreshold { get; set; }
    public bool RequireGrnApproval { get; set; }
    public decimal GrnApprovalThreshold { get; set; }
    public bool RequireStockAdjustmentApproval { get; set; }
    public decimal StockAdjustmentApprovalThreshold { get; set; }
    public bool RequireWastageApproval { get; set; }
    public decimal WastageApprovalThreshold { get; set; }
    public bool RequireRequestNoteApproval { get; set; }
    public bool RequireTransferApproval { get; set; }
    public decimal TransferApprovalThreshold { get; set; }

    // POS cash control: a bill can't be settled without an open shift for the outlet.
    public bool RequireOpenShiftForBilling { get; set; } = true;

    // Loyalty (#66)
    public bool LoyaltyEnabled { get; set; }
    public decimal LoyaltyEarnRate { get; set; }          // points earned per 1 LKR spent
    public decimal LoyaltyRedeemValue { get; set; } = 1m; // LKR a point is worth when redeemed
    public int LoyaltyExpiryDays { get; set; }            // points lapse after N days of inactivity (0 = never)

    // KOT workflow modes (#KOT)
    // KdsEnabled  — show the kitchen display board; some venues opt out entirely.
    // KotAutoPrint — when an order is sent/accepted, fire a KOT print (for venues
    //               with a KOT printer, or printing from the POS to hand over).
    public bool KdsEnabled { get; set; } = true;
    public bool KotAutoPrint { get; set; }

    // Licensed add-ons + plan limits — PROJECTED from the control-plane subscription (#106/#109)
    public int TabDeviceLimit { get; set; }          // waiter-handheld device seats purchased
    public bool GuestQrEnabled { get; set; }          // guest QR ordering monthly add-on
    public int LocationLimit { get; set; }            // max outlets (plan included + extra_location add-on); 0 = unlimited
    public int UserLimit { get; set; }                // max users (plan included); 0 = unlimited
    public string? PlanCode { get; set; }             // which plan the projection came from
    // E-receipts add-on (#79): projected channels + a rolling monthly message meter.
    public string? EReceiptChannels { get; set; }     // csv of allowed channels: "" | "email" | "email,sms,whatsapp"
    public int EReceiptQuota { get; set; }            // monthly message allowance from the add-on (0 = none purchased)
    public int EReceiptUsed { get; set; }             // messages sent this period
    public DateTime? EReceiptPeriodStart { get; set; }// first-of-month anchor; meter resets when the month rolls over

    // Month-end period lock (#77): books closed through this date (inclusive).
    public DateOnly? BooksLockedThrough { get; set; }

    // Tax certificates + bill branding
    public string? SvatNumber { get; set; }
    public string? BillHeader { get; set; }
    public string? BillFooter { get; set; }
    public bool ShowVatOnBills { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class InvoiceSeries
{
    public Guid TenantId { get; set; }
    public int SeriesYear { get; set; }
    public int LastValue { get; set; }
}
