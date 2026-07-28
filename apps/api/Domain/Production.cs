namespace Hms.Api.Domain;

/// <summary>Recipe / BOM for one finished product.</summary>
public class Recipe : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid? OutputUnitId { get; set; }            // recipe keyed by output unit (null = default)
    public Guid? LocationId { get; set; }              // recipe keyed by location too (null = all locations)
    public decimal YieldQuantity { get; set; } = 1;   // output units per batch of the ingredients
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal TotalCost { get; set; }             // sum of line costs, for one batch (yield_quantity output units)
    public List<RecipeLine> Lines { get; set; } = new();
}

public class RecipeLine : BaseEntity
{
    public Guid RecipeId { get; set; }
    public Guid IngredientProductId { get; set; }
    public string Sku { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public decimal Quantity { get; set; }            // per batch (per yield), in UnitId
    public Guid? UnitId { get; set; }                // unit the quantity is in (null = ingredient's stock unit)
    public string? UnitSymbol { get; set; }          // display, e.g. "g"
    public decimal CostPrice { get; set; }           // ingredient's cost per stock unit, snapshotted when saved
    public decimal LineCost { get; set; }            // Quantity (converted to stock units) * CostPrice
}

public class ProductionOrder : BaseEntity
{
    public Guid LocationId { get; set; }               // where ingredients are consumed
    public Guid? ReceiptLocationId { get; set; }       // where finished goods land (null = LocationId)
    public string ProductionNumber { get; set; } = default!;
    public string? DocumentNo { get; set; }            // groups a multi-product batch
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public decimal Quantity { get; set; }
    public string Status { get; set; } = "posted";     // draft | posted | void
    public bool IsCustom { get; set; }                 // ad-hoc (no predefined recipe)
    public Guid? RequestId { get; set; }               // requisition/transfer fulfilled
    public decimal TotalInputCost { get; set; }
    public decimal UnitCost { get; set; }
    public string? Notes { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? PostedAt { get; set; }
    public DateTime? VoidedAt { get; set; }
    public string? VoidReason { get; set; }
    public List<ProductionConsumption> Consumptions { get; set; } = new();
}

public class ProductionConsumption : BaseEntity
{
    public Guid ProductionOrderId { get; set; }
    public Guid IngredientProductId { get; set; }
    public string Sku { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public decimal QuantityConsumed { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineTotal { get; set; }
}
