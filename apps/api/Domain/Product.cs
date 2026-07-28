using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;

namespace Hms.Api.Domain;

/// <summary>
/// A menu item / sellable product.
/// v1 model is intentionally narrow — modifiers, recipes (BOM), batch tracking,
/// serial numbers, multiple price levels all live in Sprint 3+.
/// </summary>
public class Product : BaseEntity
{
    public Guid? LocationId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? CategoryId { get; set; }

    public string Sku { get; set; } = default!;
    public string? Barcode { get; set; }
    public string Name { get; set; } = default!;
    public string? NameOnInvoice { get; set; }
    public string? Description { get; set; }

    public string? RefCode01 { get; set; }
    public string? RefCode02 { get; set; }

    public Guid UnitOfMeasureId { get; set; }

    public decimal BasePrice { get; set; }
    public decimal CostPrice { get; set; }

    public bool IsTaxable { get; set; } = true;
    public bool IsTaxInclusive { get; set; }
    public string TaxClass { get; set; } = "standard";

    public string ProductType { get; set; } = "finished";

    public bool IsSold { get; set; } = true;
    public bool IsPurchased { get; set; } = true;
    public bool IsStocked { get; set; } = true;
    public bool IsScaleItem { get; set; }
    public bool IsOpenItem { get; set; }
    public bool IsLocked { get; set; }
    public bool TracksExpiry { get; set; }
    public bool AllowBelowCost { get; set; }
    public bool AllowDiscount { get; set; } = true;

    public string DiscountType { get; set; } = "none";
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MaxDiscountPercentage { get; set; }

    public bool IsActive { get; set; } = true;
    public decimal? ReorderLevel { get; set; }
    public decimal? ReorderQuantity { get; set; }
    public decimal? ParLevel { get; set; }

    public string? ColorHex { get; set; }
    public string? ImageUrl { get; set; }
    public int SortOrder { get; set; }

    public bool IsAvailableOnline { get; set; } = true;

    //[Column("kitchen_station_id")]
    //public Guid? KitchenStationId { get; set; }

    //[ForeignKey(nameof(KitchenStationId))]
    //public KitchenStation? KitchenStation { get; set; }

    public Guid? PreferredSupplierId { get; set; }
    public string? KitchenStationCode { get; set; }
}

/// <summary>A kitchen/bar/dessert station (and its printer) that KOT lines route to.</summary>
public class KitchenStation : BaseEntity
{
    public Guid? LocationId { get; set; }

    public Guid PrinterTypeId { get; set; }

    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;

    public string? PrinterName { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public PrinterType? PrinterType { get; set; }
}

/// <summary>Printers KOT/BOT lines route to.</summary>
public class PrinterType : BaseEntity
{
    public string Code { get; set; } = default!;   // KOT, BOT, NONE
    public string Name { get; set; } = default!;   // Kitchen Order Ticket, Bar Order Ticket, None

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>A sellable serving size for a product (Cup/Pot, S/M/L) with its own price.</summary>
public class ProductVariant : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid? ServingUnitId { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool DeductStockOnRecipe { get; set; } = true;
    public decimal ServingQty { get; set; }
    public decimal CostPrice { get; set; }

}

/// <summary>A named price list (Dine-in / Delivery / a branch) for per-channel pricing.</summary>
public class PriceLevel : BaseEntity
{
    public Guid? LocationId { get; set; }

    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public bool IsDefault { get; set; }
    public string? AppliesToOrderType { get; set; }     // dine_in | takeaway | delivery
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>A product's price override for a given price level (no row ⇒ base price).</summary>
public class ProductPrice : BaseEntity
{
    public Guid? LocationId { get; set; }

    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public Guid PriceLevelId { get; set; }

    public decimal CostPrice { get; set; }
    public decimal Price { get; set; }
    //public decimal Qty { get; set; } = 1;
}

public class ServingUnit : BaseEntity
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ProductSupplier : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid SupplierId { get; set; }

    public bool IsPreferred { get; set; }
    public bool IsActive { get; set; } = true;

    public Supplier? Supplier { get; set; }
}

/// <summary>Which kitchen station(s) a product routes to, optionally per location.</summary>
public class ProductKitchenStation : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid KitchenStationId { get; set; }
    public Guid? LocationId { get; set; }

    public KitchenStation? KitchenStation { get; set; }
}