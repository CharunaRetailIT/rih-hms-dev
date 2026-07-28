namespace Hms.Api.Domain;

/// <summary>A customer group (e.g. Staff, Corporate) carrying a group discount.</summary>
public class CustomerCategory : BaseEntity
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public decimal DiscountPercent { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public bool IsVat { get; set; }   // customers in this group are VAT-registered by default
}

/// <summary>
/// A customer in the CRM master. Carries an optional per-customer default discount
/// (falls back to the category's), and, for credit customers, a credit limit and a
/// running outstanding balance (AR) that credit settlements add to and customer
/// payments reduce.
/// </summary>
public class Customer : BaseEntity
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public Guid? CategoryId { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }               // free-text street line
    public string? CountryCode { get; set; }
    public string? Province { get; set; }
    public string? District { get; set; }
    public string? PostalCode { get; set; }
    public string? TaxNo { get; set; }                 // NIC / VAT / BR for the invoice
    public decimal? DiscountPercent { get; set; }      // per-customer override; else category
    public bool IsCreditCustomer { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }        // outstanding AR
    public decimal AdvanceBalance { get; set; }        // prepaid deposit drawable at the till (#70)
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public DateOnly? DateOfBirth { get; set; }         // optional birthday — greetings / offers (#70)

    // Loyalty (#66)
    public decimal LoyaltyPoints { get; set; }          // current redeemable balance
    public decimal LoyaltyLifetimePoints { get; set; }  // total ever earned (drives tiers)
    public DateTime? LoyaltyLastActivityAt { get; set; } // last earn/redeem (drives inactivity expiry)
    public string? LoyaltyCardNo { get; set; }           // scannable card/membership number (#66)
    public Guid? LoyaltyCardSchemeId { get; set; }       // assigned Loyalty Card Scheme, if enrolled
}

/// <summary>A loyalty tier: an earn multiplier + optional discount, reached at a lifetime-points threshold.</summary>
public class LoyaltyTier : BaseEntity
{
    public string Name { get; set; } = default!;
    public decimal MinLifetimePoints { get; set; }
    public decimal EarnMultiplier { get; set; } = 1m;
    public decimal DiscountPercent { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// A loyalty card scheme — matches the legacy "Card Type" master. One of three kinds:
/// a flat Discount %, a Points scheme with tiered bill-value earn rules, or a link to
/// an existing Promotion. A customer enrolls into exactly one scheme at a time.
/// </summary>
public class LoyaltyCardScheme : BaseEntity
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string Type { get; set; } = "points";   // discount | points | promotion
    public decimal DiscountPercent { get; set; }   // Type == discount
    public Guid? PromotionId { get; set; }         // Type == promotion
    public bool IsActive { get; set; } = true;

    public List<LoyaltyCardSchemeTier> Tiers { get; set; } = [];
}

/// <summary>
/// One tiered earn rule for a Points-type <see cref="LoyaltyCardScheme"/>: a bill-value
/// range earns Points per Increment of bill value within that range.
/// </summary>
public class LoyaltyCardSchemeTier : BaseEntity
{
    public Guid SchemeId { get; set; }
    public decimal BillFromValue { get; set; }
    public decimal BillToValue { get; set; }
    public decimal Increment { get; set; }
    public decimal Points { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>A loyalty point movement (earn / redeem / adjust) — append-only ledger.</summary>
public class LoyaltyTransaction : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Guid? OrderId { get; set; }
    public string TxnType { get; set; } = "earn";   // earn | redeem | adjust
    public decimal Points { get; set; }             // + earned, − redeemed
    public decimal BalanceAfter { get; set; }
    public string? Note { get; set; }
}

/// <summary>A contract price: a fixed price a customer (or category) pays for a product.</summary>
public class CustomerProductPrice : BaseEntity
{
    public Guid? CustomerId { get; set; }   // exactly one of CustomerId / CategoryId is set
    public Guid? CategoryId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Price { get; set; }
}

/// <summary>An AR receipt: a payment a credit customer makes against their balance.</summary>
public class CustomerPayment : BaseEntity
{
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string PayType { get; set; } = "cash";
    public string? Reference { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string? Notes { get; set; }
}
