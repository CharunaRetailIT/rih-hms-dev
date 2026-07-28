namespace Hms.Api.Domain;

/// <summary>
/// Menu / product category with simple parent-child hierarchy.
/// One level deep is usually enough — Sri Lankan restaurants typically:
///   Mains > Curries / Kottus / Rices / Sides
///   Beverages > Soft drinks / Beers / Spirits / Coffees
/// </summary>
public class Category : BaseEntity
{
    public Guid? ParentId { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ColorHex { get; set; }                // POS button colour, e.g. "#0F766E"
    public string? IconName { get; set; }                // Material Symbols name, e.g. "lunch_dining"
}
