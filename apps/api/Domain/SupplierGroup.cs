namespace Hms.Api.Domain;

public class SupplierGroup : BaseEntity
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Remark { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Supplier> Suppliers { get; set; } = [];

}