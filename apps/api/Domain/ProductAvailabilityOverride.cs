namespace Hms.Api.Domain;

/// <summary>
/// Manual per-location availability override (#112 / #100). When ABSENT, an item's
/// availability is computed automatically from stock + recipe sellability. When PRESENT,
/// it forces the item available or unavailable at that location regardless of stock —
/// i.e. a manual "86" (Available=false) or a manual un-86 (Available=true).
/// One row per (location, product).
/// </summary>
public class ProductAvailabilityOverride : BaseEntity
{
    public Guid LocationId { get; set; }
    public Guid ProductId { get; set; }
    public bool Available { get; set; }
}
