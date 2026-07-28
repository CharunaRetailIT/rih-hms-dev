namespace Hms.Api.Domain;

/// <summary>
/// A cashier's session at an outlet (open → close). Closing reconciles the cash
/// drawer: expected cash = opening float + cash takings; variance = declared −
/// expected. Aggregation is by (location, settled-at window); the partial unique
/// index guarantees one open shift per location, so windows never overlap.
/// </summary>
public class Shift : BaseEntity
{
    public Guid LocationId { get; set; }
    public Guid? UserId { get; set; }
    public string? OpenedByName { get; set; }
    public string ShiftNumber { get; set; } = default!;
    public DateTime OpenedAt { get; set; }
    public decimal OpeningFloat { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string Status { get; set; } = "open";   // open | closed
    public decimal? DeclaredCash { get; set; }
    public decimal? ExpectedCash { get; set; }
    public decimal? CashVariance { get; set; }
    public decimal TotalSales { get; set; }
    public decimal CashSales { get; set; }          // cash that should be in the drawer
    public decimal CardSales { get; set; }
    public decimal OtherSales { get; set; }         // aggregator prepaid, etc.
    public int OrderCount { get; set; }
    public string? Notes { get; set; }
}
