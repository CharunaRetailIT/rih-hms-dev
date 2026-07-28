namespace Hms.Api.Domain;

/// <summary>
/// A per-location override of a product's replenishment levels (#replenishment).
/// An absent row means the location inherits the product's default reorder/par/
/// preferred supplier. A present row may set any subset; null fields still fall
/// back to the product default.
/// </summary>
public class ProductReplenishmentLevel : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid LocationId { get; set; }
    public decimal? ReorderLevel { get; set; }            // min / trigger point
    public decimal? ParLevel { get; set; }                // max / order-up-to target
    public Guid? PreferredSupplierId { get; set; }
}
