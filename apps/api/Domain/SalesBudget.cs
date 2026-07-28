namespace Hms.Api.Domain;

/// <summary>A sales target for a month (#72 budget vs actual). LocationId null = company-wide (all outlets).</summary>
public class SalesBudget : BaseEntity
{
    public Guid? LocationId { get; set; }
    public DateOnly PeriodMonth { get; set; }   // 1st of the budgeted month
    public decimal Amount { get; set; }
}
