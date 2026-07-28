namespace Hms.Api.Features.Products;

//public record ProductListDto(
//    Guid Id,
//    string Sku,
//    string Name,
//    string? Barcode,
//    Guid? DepartmentId,
//    Guid? CategoryId,
//    Guid UnitOfMeasureId,
//    decimal BasePrice,
//    decimal CostPrice,
//    bool IsActive,
//    bool IsSold,
//    bool IsPurchased,
//    bool IsStocked,
//    string ProductType,
//    string? KitchenStationCode,
//    int VariantCount
//);

public record ProductListDto
{
    public Guid Id { get; init; }

    public string Sku { get; init; } = default!;

    public string Name { get; init; } = default!;

    public string? Barcode { get; init; }

    public Guid? LocationId { get; init; }

    public Guid? DepartmentId { get; init; }

    public Guid? CategoryId { get; init; }

    public Guid UnitOfMeasureId { get; init; }

    public decimal BasePrice { get; init; }

    public decimal CostPrice { get; init; }

    public bool IsActive { get; init; }

    public bool IsSold { get; init; }

    public bool IsPurchased { get; init; }

    public bool IsStocked { get; init; }

    public string ProductType { get; init; } = default!;

    public string TaxClass { get; init; } = default!;

    public string? KitchenStationCode { get; init; }

    public int VariantCount { get; init; }

    public string? ColorHex { get; init; }
}

public record ProductDetailDto(
    Guid Id,
    Guid? LocationId,
    Guid? DepartmentId,
    Guid? CategoryId,
    string Sku,
    string? Barcode,
    string Name,
    string? NameOnInvoice,
    string? Description,
    string? RefCode01,
    string? RefCode02,
    Guid UnitOfMeasureId,
    Guid? PurchaseUnitOfMeasureId,
    decimal BasePrice,
    decimal CostPrice,
    bool IsTaxable,
    bool IsTaxInclusive,
    string TaxClass,
    string ProductType,
    bool IsSold,
    bool IsPurchased,
    bool IsStocked,
    bool IsScaleItem,
    bool IsOpenItem,
    bool IsLocked,
    bool TracksExpiry,
    bool AllowBelowCost,
    bool AllowDiscount,
    string DiscountType,
    decimal DiscountValue,
    decimal? MaxDiscountAmount,
    decimal? MaxDiscountPercentage,
    decimal? ReorderLevel,
    decimal? ReorderQuantity,
    decimal? ParLevel,
    string? ColorHex,
    string? ImageUrl,
    int SortOrder,
    bool IsAvailableOnline,
    string? KitchenStationCode,
    Guid? PreferredSupplierId,
    bool IsActive,
    List<ProductVariantDto> Variants,
    List<ProductSupplierDto> Suppliers,
    List<ProductPriceDto> Prices,
    List<ProductKitchenStationDto> KitchenStations,
    List<ProductChargeDto> Charges
);

public record ProductVariantDto(
    Guid? Id,
    Guid? ServingUnitId,
    string Code,
    string Name,
    decimal ServingQty,
    decimal CostPrice,
    decimal Price,
    bool DeductStockOnRecipe,
    int SortOrder,
    bool IsActive
);

public record ProductSupplierDto(
    Guid? Id,
    Guid SupplierId,
    bool IsPreferred,
    bool IsActive
);

public record ProductPriceDto(
    Guid? Id,
    Guid? LocationId,
    Guid? ProductVariantId,
    Guid PriceLevelId,
    decimal CostPrice,
    decimal Price,
    //decimal Qty
    string? VariantCode = null
);

public record ProductKitchenStationDto(
    Guid? Id,
    Guid KitchenStationId,
    Guid? LocationId
);

public record ProductChargeDto(
    Guid? Id,
    Guid ChargeId
);

public record SaveProductRequest(
    Guid? Id,
    Guid? DepartmentId,
    Guid? LocationId,
    Guid? KitchenStationId,
    Guid? CategoryId,
    string Sku,
    string Name,
    string? Barcode,
    string? NameOnInvoice,
    string? Description,
    string? RefCode01,
    string? RefCode02,
    Guid UnitOfMeasureId,
    Guid? PurchaseUnitOfMeasureId,
    decimal BasePrice,
    decimal CostPrice,
    bool IsTaxable,
    bool IsTaxInclusive,
    string TaxClass,
    string ProductType,
    bool IsSold,
    bool IsPurchased,
    bool IsStocked,
    bool IsScaleItem,
    bool IsOpenItem,
    bool IsLocked,
    bool TracksExpiry,
    bool AllowBelowCost,
    bool AllowDiscount,
    string DiscountType,
    decimal DiscountValue,
    decimal? MaxDiscountAmount,
    decimal? MaxDiscountPercentage,
    decimal? ReorderLevel,
    decimal? ReorderQuantity,
    decimal? ParLevel,
    string? ColorHex,
    string? ImageUrl,
    int SortOrder,
    bool IsAvailableOnline,
    string? KitchenStationCode,
    Guid? PreferredSupplierId,
    bool IsActive,
    List<ProductVariantDto>? Variants,
    List<ProductSupplierDto>? Suppliers,
    List<ProductPriceDto>? Prices,
    List<ProductKitchenStationDto>? KitchenStations,
    List<ProductChargeDto>? Charges
);

/// <summary>One row of a bulk CSV import (#master-data-import) — matches the Data Import hub's
/// Products dataset columns exactly (Product Code, Name, Barcode, Category, Unit, Sell Price, Cost,
/// Type, Station, Tax Class).</summary>
public record ProductImportRow(
    string? Sku,
    string? Name,
    string? Barcode,
    string? Category,
    string? Unit,
    decimal? SellPrice,
    decimal? Cost,
    string? Type,
    string? Station,
    string? TaxClass
);

public record PagedProductResult(
    List<ProductListDto> Data,
    PaginationMeta Pagination
);

public record PaginationMeta(
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);