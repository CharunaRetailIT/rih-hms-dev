namespace Hms.Api.Domain;

/// <summary>A physical stock-count sheet for an outlet (#77).</summary>
public class StockCount : BaseEntity
{
    public Guid LocationId { get; set; }
    public string? CountNumber { get; set; }
    public string Status { get; set; } = "draft";   // draft | posted | void
    public string? Notes { get; set; }
    public DateTime? PostedAt { get; set; }
    public List<StockCountLine> Lines { get; set; } = new();
}

public class StockCountLine : BaseEntity
{
    public Guid StockCountId { get; set; }
    public Guid ProductId { get; set; }
    public decimal SystemQty { get; set; }   // on-hand snapshot when the count was opened
    public decimal CountedQty { get; set; }
    public decimal Variance { get; set; }    // counted − system, fixed at post
}
