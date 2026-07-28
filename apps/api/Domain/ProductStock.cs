namespace Hms.Api.Domain;

/// <summary>
/// Per-location stock-on-hand for a product.
/// v1 = simple counter. v2 introduces batch/lot, expiry, and serial tracking.
/// </summary>
public class ProductStock : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid LocationId { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal AverageCost { get; set; }
    public DateTime? LastReceivedAt { get; set; }
    public DateTime? LastCountedAt { get; set; }
}
