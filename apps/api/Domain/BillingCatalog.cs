namespace Hms.Api.Domain;

/// <summary>Control-plane (RIT-owned) base plan in the pricing catalog (#109).</summary>
public class Plan : ControlEntity
{
    public string Code { get; set; } = default!;        // starter | standard | growth | enterprise
    public string Name { get; set; } = default!;
    public decimal MonthlyPrice { get; set; }
    public string Currency { get; set; } = "LKR";
    public int IncludedLocations { get; set; } = 1;
    public int IncludedUsers { get; set; } = 5;        // hard cap on login users (0 = unlimited)
    public int MaxLocations { get; set; }              // hard cap on total outlets (0 = unlimited) — #6 tier ceiling
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string> Features { get; set; } = new();   // admin-configurable plan-card bullets (text[])
}

/// <summary>Control-plane add-on in the pricing catalog — tab device seat, guest-QR, extra outlet (#109).</summary>
public class Addon : ControlEntity
{
    public string Code { get; set; } = default!;        // tab_device | guest_qr | extra_location
    public string Name { get; set; } = default!;
    public string Unit { get; set; } = default!;        // per_device_month | flat_month | per_location_month
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "LKR";
    public int IncludedQty { get; set; }                 // monthly allowance for metered add-ons (e-receipts #79); 0 = n/a / unlimited
    public bool IsActive { get; set; } = true;
}
