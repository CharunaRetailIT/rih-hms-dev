namespace Hms.Api.Domain;

/// <summary>
/// A tax (VAT, GST, service charge, etc.).
/// v1: a product references at most one tax. v2 introduces compound tax stacking.
/// </summary>
public class Tax : BaseEntity
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public decimal RatePercent { get; set; }            // e.g. 18.0000 = 18%
    public bool IsActive { get; set; } = true;
    public bool IsInclusive { get; set; }                // true = price already includes the tax
    public string ApplyOn { get; set; } = "line";        // "line" | "bill" | "service_charge"
}
