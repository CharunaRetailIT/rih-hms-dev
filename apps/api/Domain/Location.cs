namespace Hms.Api.Domain;

/// <summary>
/// A physical outlet — restaurant, café, hotel F&amp;B venue. Tenants can have
/// multiple Locations from v2; v1 ships with one Location per Tenant.
/// </summary>
public class Location : BaseEntity
{
    public string Code { get; set; } = default!;          // short code, unique per tenant
    public string Name { get; set; } = default!;
    public string AddressLine1 { get; set; } = default!;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = default!;
    public string CountryCode { get; set; } = "LK";
    public string? Province { get; set; }
    public string? District { get; set; }
    public string? PostalCode { get; set; }
    public string TimeZone { get; set; } = "Asia/Colombo";
    public string Currency { get; set; } = "LKR";       // display/view currency (billing is always the business base)
    public bool VatExempt { get; set; }                  // duty-free / tax-exempt outlet → no VAT on its bills
    public string? PhoneE164 { get; set; }
    public bool IsActive { get; set; } = true;

    // Multi-outlet
    public string LocationType { get; set; } = "outlet";   // head_office | central_kitchen | warehouse | outlet
    public bool CanSell { get; set; } = true;
    public bool CanProduce { get; set; }
    public bool CanStock { get; set; } = true;
    public string? VatRegistrationNumber { get; set; }
    public int DefaultPrepMinutes { get; set; } = 20;
    public Guid? ReplenishSourceLocationId { get; set; }   // hub (central kitchen/warehouse) that tops this location up by transfer (#replenishment)

    // Per-location operating hours as a JSON string keyed by weekday (mon..sun),
    // each {open,close} in "HH:mm" or absent/null = closed that day. Null column = not configured.
    // POS shows a soft "outside operating hours" warning based on the venue's local time.
    public string? OperatingHours { get; set; }
}
