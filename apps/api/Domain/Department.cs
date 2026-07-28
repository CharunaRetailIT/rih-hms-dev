namespace Hms.Api.Domain;

/// <summary>
/// Business department used to group products, reports, permissions, and operational areas.
/// Example: Restaurant, Bar, Kitchen, Housekeeping.
/// </summary>
public class Department : BaseEntity
{
    public string Code { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string? Remark { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional branch/outlet specific department.
    /// If null, department is tenant-wide.
    /// </summary>
    public Guid? LocationId { get; set; }

    public Location? Location { get; set; }

    /// <summary>
    /// Optional color used in dashboard/cards.
    /// Example: #22C55E
    /// </summary>
    public string? DashboardColor { get; set; }
}