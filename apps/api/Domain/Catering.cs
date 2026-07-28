namespace Hms.Api.Domain;

/// <summary>A bookable venue/hall for catering events (#75).</summary>
public class EventHall : BaseEntity
{
    public Guid? LocationId { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int Capacity { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>A per-head catering menu package (#75).</summary>
public class CateringPackage : BaseEntity
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public decimal PricePerHead { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? RecipeProductId { get; set; }         // #75: output product whose recipe = the meal's ingredients
}

/// <summary>
/// A catering booking + function-bill header (#75): hall + date + pax + package,
/// priced pax × price/head + extras − discount, with deposits reducing the
/// balance. Off-site jobs carry own-fleet delivery details.
/// </summary>
public class CateringEvent : BaseEntity
{
    public string EventNo { get; set; } = default!;
    public string? Title { get; set; }
    public Guid? LocationId { get; set; }
    public Guid? HallId { get; set; }
    public Guid? PackageId { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public int Pax { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public string Status { get; set; } = "enquiry";   // enquiry|confirmed|running|completed|cancelled

    public decimal PricePerHead { get; set; }
    public decimal PackageTotal { get; set; }
    public decimal ExtrasTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ServiceCharge { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }

    public bool IsOffsite { get; set; }
    public string? DeliveryAddress { get; set; }       // free-text street line of the off-site venue
    public string? CountryCode { get; set; }
    public string? Province { get; set; }
    public string? District { get; set; }
    public string? PostalCode { get; set; }
    public string? Vehicle { get; set; }
    public string? Driver { get; set; }
    public string? DispatchStatus { get; set; }        // pending|dispatched|delivered

    public string? Notes { get; set; }

    // Production / inventory tie-in (#75)
    public Guid? ProductionOrderId { get; set; }       // the production order that consumed ingredients
    public decimal FoodCost { get; set; }              // ingredient cost consumed (from the production order)
    public DateTime? ProducedAt { get; set; }

    public List<CateringEventItem> Items { get; set; } = new();
    public List<CateringEventPayment> Payments { get; set; } = new();
}

/// <summary>An ad-hoc extra line on a catering event (plain entity).</summary>
public class CateringEventItem
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EventId { get; set; }
    public string Description { get; set; } = default!;
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}

/// <summary>A deposit / advance / balance payment against a catering event (plain entity).</summary>
public class CateringEventPayment
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EventId { get; set; }
    public decimal Amount { get; set; }
    public string PayType { get; set; } = "cash";      // cash|card|bank|advance
    public string Kind { get; set; } = "deposit";      // deposit|advance|balance
    public string? Reference { get; set; }
    public DateTime PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
