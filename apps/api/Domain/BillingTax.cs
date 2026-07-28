namespace Hms.Api.Domain;

/// <summary>
/// A tax RIT applies to its OWN subscription invoices (#110/#111) — distinct from the tax a
/// tenant charges on their restaurant sales. Lives in the control plane, managed by RIT admin.
/// Supports one or many (e.g. VAT + a levy); each row is summed when applicable.
///
/// Scope decides who pays, by the tenant's country vs RIT's home country (Billing:HomeCountry, "LK"):
///   • domestic → only when the tenant is in RIT's home country (e.g. SL VAT 18%).
///   • export   → only when the tenant is abroad (export of services; usually nil for SL).
///   • all      → always.
/// </summary>
public class BillingTax : ControlEntity
{
    public string Code { get; set; } = default!;          // e.g. "vat"
    public string Name { get; set; } = default!;          // e.g. "VAT" / "GST"
    public decimal RatePercent { get; set; }              // e.g. 18.00
    public string Scope { get; set; } = "domestic";       // domestic | export | all
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
