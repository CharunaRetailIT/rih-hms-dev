namespace Hms.Api.Domain;

/// <summary>
/// Units used for stock and recipe quantities (kg, L, ea, etc.).
/// Seeded per tenant at provisioning time.
/// </summary>
public class UnitOfMeasure : BaseEntity
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Symbol { get; set; }
    public bool IsBaseUnit { get; set; }

    /// <summary>Convertibility group: "mass", "volume" or "count".</summary>
    public string Dimension { get; set; } = "count";

    /// <summary>How many BASE units one of this unit equals (mass base = g, volume base = ml).</summary>
    public decimal FactorToBase { get; set; } = 1;
}
