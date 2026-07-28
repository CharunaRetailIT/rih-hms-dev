namespace Hms.Api.Domain;

/// <summary>
/// Supplier / Vendor master.
/// Used for purchasing, GRNs, stock replenishment and supplier analysis.
/// </summary>
public class Supplier : BaseEntity
{
    public string Code { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string? ContactName { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public int PaymentTermsDays { get; set; }

    public bool IsActive { get; set; } = true;

    // Tax
    public bool IsVatRegistered { get; set; }

    public string? VatRegistrationNumber { get; set; }

    // Location / Address
    public string? CountryCode { get; set; }

    public string? Province { get; set; }

    public string? District { get; set; }

    public string? PostalCode { get; set; }

    // Classification
    public Guid? SupplierGroupId { get; set; }

    public SupplierGroup? SupplierGroup { get; set; }

    public Guid? SupplierTypeId { get; set; }

    public SupplierType? SupplierType { get; set; }
}