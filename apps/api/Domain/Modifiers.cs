namespace Hms.Api.Domain;

/// <summary>A group of choices attached to products, e.g. "Add-ons", "Size".</summary>
public class ModifierGroup : BaseEntity
{
    public string Name { get; set; } = default!;
    public int MinSelect { get; set; }
    public int MaxSelect { get; set; }       // 0 = unlimited
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public List<ModifierItem> Items { get; set; } = new();
}

public class ModifierItem : BaseEntity
{
    public Guid GroupId { get; set; }
    public string Name { get; set; } = default!;
    public decimal PriceDelta { get; set; }
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>Attaches a reusable modifier group to a product.</summary>
public class ProductModifierGroup : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid GroupId { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>A choice made on an order line (priced into the line).</summary>
public class OrderItemModifier : BaseEntity
{
    public Guid OrderItemId { get; set; }
    public Guid? ModifierItemId { get; set; }
    public string Name { get; set; } = default!;
    public decimal PriceDelta { get; set; }
}
