namespace Hms.Api.Domain;

/// <summary>A dining table at an outlet. Occupancy is derived from open orders.</summary>
public class RestaurantTable : BaseEntity
{
    public Guid LocationId { get; set; }
    public string Code { get; set; } = default!;
    public string? Name { get; set; }
    public int Seats { get; set; } = 2;
    public string? Area { get; set; }
    public Guid? FloorId { get; set; }   // optional structured floor (Area stays as a display fallback)
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public int PosX { get; set; }   // visual floor-plan position (px); 0/0 = unplaced
    public int PosY { get; set; }
}

/// <summary>A table booking with a simple status lifecycle.</summary>
public class Reservation : BaseEntity
{
    public Guid LocationId { get; set; }
    public Guid? TableId { get; set; }
    public string CustomerName { get; set; } = default!;
    public string? Phone { get; set; }
    public int PartySize { get; set; } = 2;
    public DateTime ReservedAt { get; set; }
    public int DurationMinutes { get; set; } = 90;
    public string Status { get; set; } = "booked";   // booked|seated|cancelled|no_show|completed
    public string? Notes { get; set; }
}
