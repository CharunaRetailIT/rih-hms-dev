using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Products;

public class ProductsService(ITenantDbContextFactory factory)
{
    public async Task<PagedProductResult> GetPagedAsync(int pageNumber, int pageSize, string? search, Guid? locationId, Guid? departmentId, Guid? categoryId, string? productType, bool? isActive, CancellationToken ct)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        await using var db = await factory.CreateForCurrentAsync(ct);

        var query = db.Products
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (locationId.HasValue)
            query = query.Where(x => x.LocationId == locationId.Value);

        if (departmentId.HasValue)
            query = query.Where(x => x.DepartmentId == departmentId.Value);

        if (categoryId.HasValue)
            query = query.Where(x => x.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(productType))
            query = query.Where(x => x.ProductType == productType.Trim().ToLowerInvariant());

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = $"%{search.Trim()}%";

            query = query.Where(x =>
                EF.Functions.ILike(x.Sku, s) ||
                EF.Functions.ILike(x.Name, s) ||
                EF.Functions.ILike(x.Barcode ?? "", s));
        }

        var totalCount = await query.CountAsync(ct);

        var data = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ProductListDto
                {
                    Id = x.Id,
                    Sku = x.Sku,
                    Name = x.Name,
                    Barcode = x.Barcode,
                    LocationId = x.LocationId,
                    DepartmentId = x.DepartmentId,
                    CategoryId = x.CategoryId,
                    UnitOfMeasureId = x.UnitOfMeasureId,
                    BasePrice = x.BasePrice,
                    CostPrice = x.CostPrice,
                    IsActive = x.IsActive,
                    IsSold = x.IsSold,
                    IsPurchased = x.IsPurchased,
                    IsStocked = x.IsStocked,
                    ProductType = x.ProductType,
                    TaxClass = x.TaxClass,
                    KitchenStationCode = db.ProductKitchenStations
                                    .Where(pk => pk.ProductId == x.Id && !pk.IsDeleted)
                                    .Select(pk => pk.KitchenStation!.Code)
                                    .FirstOrDefault(),
                VariantCount = db.ProductVariants.Count(v => v.ProductId == x.Id && v.IsActive && !v.IsDeleted),
                ColorHex = x.ColorHex
                })
            .ToListAsync(ct);

        return new PagedProductResult(
            data,
            new PaginationMeta(
                totalCount,
                pageNumber,
                pageSize,
                (int)Math.Ceiling(totalCount / (double)pageSize)
            )
        );
    }

    /// <summary>
    /// Auto Product-Code (#2): next code in the global sequence — {prefix}-{NNNN}. The number is
    /// max existing suffix + 1 (ignores soft-delete filter so codes never get reused).
    /// </summary>
    public async Task<(string Code, string Prefix)> GetNextCodeAsync(CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var prefix = await db.OrgSettings.AsNoTracking().Select(o => o.ProductCodePrefix).FirstOrDefaultAsync(ct);
        if (string.IsNullOrWhiteSpace(prefix)) prefix = "ITM";
        var pat = prefix + "-";
        var skus = await db.Products.IgnoreQueryFilters()
            .Where(p => p.Sku != null && p.Sku.StartsWith(pat)).Select(p => p.Sku!).ToListAsync(ct);
        var max = 0;
        foreach (var s in skus)
            if (int.TryParse(s.AsSpan(pat.Length), out var n) && n > max) max = n;
        return ($"{prefix}-{(max + 1):D4}", prefix);
    }

    /// <summary>
    /// Flat, unpaginated product list — kept for pages that just need a full
    /// catalog for a dropdown/picker (contract pricing, delivery menu, recipe
    /// ingredients, promotions, reports) rather than a paged grid.
    /// </summary>
    public async Task<List<ProductListDto>> GetAllAsync(string? search, Guid? categoryId, bool? activeOnly, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        var query = db.Products.AsNoTracking().Where(x => !x.IsDeleted);

        if (categoryId.HasValue) query = query.Where(x => x.CategoryId == categoryId.Value);
        if (activeOnly == true) query = query.Where(x => x.IsActive && x.IsSold);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = $"%{search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.Sku, s) ||
                EF.Functions.ILike(x.Name, s) ||
                EF.Functions.ILike(x.Barcode ?? "", s));
        }

        return await query
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
            .Select(x => new ProductListDto
            {
                Id = x.Id,
                Sku = x.Sku,
                Name = x.Name,
                Barcode = x.Barcode,
                LocationId = x.LocationId,
                DepartmentId = x.DepartmentId,
                CategoryId = x.CategoryId,
                UnitOfMeasureId = x.UnitOfMeasureId,
                BasePrice = x.BasePrice,
                CostPrice = x.CostPrice,
                IsActive = x.IsActive,
                IsSold = x.IsSold,
                IsPurchased = x.IsPurchased,
                IsStocked = x.IsStocked,
                ProductType = x.ProductType,
                TaxClass = x.TaxClass,
                KitchenStationCode = db.ProductKitchenStations
                    .Where(pk => pk.ProductId == x.Id && !pk.IsDeleted)
                    .Select(pk => pk.KitchenStation!.Code)
                    .FirstOrDefault(),
                VariantCount = db.ProductVariants.Count(v => v.ProductId == x.Id && v.IsActive && !v.IsDeleted),
                ColorHex = x.ColorHex,
            })
            .ToListAsync(ct);
    }

    public async Task<ProductDetailDto?> GetAsync(Guid id, CancellationToken ct)
    {
        try
        {
            await using var db = await factory.CreateForCurrentAsync(ct);

            var product = await db.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

            if (product is null) return null;

            // Kitchen station is a many-to-many via ProductKitchenStations now;
            // use the first mapped station as the summary code shown in list/detail views.
            var kitchenStationCode = await db.ProductKitchenStations
                .Where(pk => pk.ProductId == id && !pk.IsDeleted)
                .Select(pk => pk.KitchenStation!.Code)
                .FirstOrDefaultAsync(ct);

            // Loaded sequentially, not concurrently — they all share one DbContext,
            // and EF Core doesn't support running more than one operation on the
            // same context at a time (it throws, or worse, races silently).
            var variants = await db.ProductVariants
                .AsNoTracking()
                .Where(x => x.ProductId == id && !x.IsDeleted)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(x => new ProductVariantDto(
                    x.Id,
                    x.ServingUnitId,
                    x.Code,
                    x.Name,
                    x.ServingQty,
                    x.CostPrice,
                    x.Price,
                    x.DeductStockOnRecipe,
                    x.SortOrder,
                    x.IsActive
                ))
                .ToListAsync(ct);

            var suppliers = await db.ProductSuppliers
                .AsNoTracking()
                .Where(x => x.ProductId == id && !x.IsDeleted)
                .Select(x => new ProductSupplierDto(
                    x.Id,
                    x.SupplierId,
                    x.IsPreferred,
                    x.IsActive
                ))
                .ToListAsync(ct);

            var prices = await db.ProductPrices
                .AsNoTracking()
                .Where(x => x.ProductId == id && !x.IsDeleted)
                .Select(x => new ProductPriceDto(
                    x.Id,
                    x.LocationId,
                    x.ProductVariantId,
                    x.PriceLevelId,
                    x.CostPrice,
                    x.Price,
                  //  x.Qty
                    null
                ))
                .ToListAsync(ct);

            var kitchenStations = await db.ProductKitchenStations
                .AsNoTracking()
                .Where(x => x.ProductId == id && !x.IsDeleted)
                .Select(x => new ProductKitchenStationDto(
                    x.Id,
                    x.KitchenStationId,
                    x.LocationId
                ))
                .ToListAsync(ct);

            var charges = await db.ProductCharges
                .AsNoTracking()
                .Where(x => x.ProductId == id && !x.IsDeleted)
                .Select(x => new ProductChargeDto(
                    x.Id,
                    x.ChargeId
                ))
                .ToListAsync(ct);

            return new ProductDetailDto(
                Id: product.Id,
                LocationId: product.LocationId,
                DepartmentId: product.DepartmentId,
                CategoryId: product.CategoryId,
                Sku: product.Sku,
                Barcode: product.Barcode,
                Name: product.Name,
                NameOnInvoice: product.NameOnInvoice,
                Description: product.Description,
                RefCode01: product.RefCode01,
                RefCode02: product.RefCode02,
                UnitOfMeasureId: product.UnitOfMeasureId,
                PurchaseUnitOfMeasureId: null,
                BasePrice: product.BasePrice,
                CostPrice: product.CostPrice,
                IsTaxable: product.IsTaxable,
                IsTaxInclusive: product.IsTaxInclusive,
                TaxClass: product.TaxClass,
                ProductType: product.ProductType,
                IsSold: product.IsSold,
                IsPurchased: product.IsPurchased,
                IsStocked: product.IsStocked,
                IsScaleItem: product.IsScaleItem,
                IsOpenItem: product.IsOpenItem,
                IsLocked: product.IsLocked,
                TracksExpiry: product.TracksExpiry,
                AllowBelowCost: product.AllowBelowCost,
                AllowDiscount: product.AllowDiscount,
                DiscountType: product.DiscountType,
                DiscountValue: product.DiscountValue,
                MaxDiscountAmount: product.MaxDiscountAmount,
                MaxDiscountPercentage: product.MaxDiscountPercentage,
                ReorderLevel: product.ReorderLevel,
                ReorderQuantity: product.ReorderQuantity,
                ParLevel: product.ParLevel,
                ColorHex: product.ColorHex,
                ImageUrl: product.ImageUrl,
                SortOrder: product.SortOrder,
                IsAvailableOnline: product.IsAvailableOnline,
                KitchenStationCode: kitchenStationCode,
                PreferredSupplierId: product.PreferredSupplierId,
                IsActive: product.IsActive,
                Variants: variants,
                Suppliers: suppliers,
                Prices: prices,
                KitchenStations: kitchenStations,
                Charges: charges
            );
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"GetAsync was cancelled for product {id}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetAsync: {ex.Message}");
            return null;
        }
    }
    //public async Task<ProductDetailDto?> GetAsync(Guid id, CancellationToken ct)
    //{
    //    await using var db = await factory.CreateForCurrentAsync(ct);

    //    var product = await db.Products
    //        .AsNoTracking()
    //        .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

    //    if (product is null) return null;

    //    // Get kitchen station code separately using a query
    //    string? kitchenStationCode = null;
    //    if (product.KitchenStationId.HasValue)
    //    {
    //        kitchenStationCode = await db.KitchenStations
    //            .Where(k => k.Id == product.KitchenStationId.Value && !k.IsDeleted)
    //            .Select(k => k.Code)
    //            .FirstOrDefaultAsync(ct);
    //    }

    //    var variants = await db.ProductVariants
    //        .AsNoTracking()
    //        .Where(x => x.ProductId == id && !x.IsDeleted)
    //        .OrderBy(x => x.SortOrder)
    //        .ThenBy(x => x.Name)
    //        .Select(x => new ProductVariantDto(
    //            x.Id,
    //            x.ServingUnitId,
    //            x.Code,
    //            x.Name,
    //            x.ServingQty,
    //            x.CostPrice,
    //            x.Price,
    //            x.DeductStockOnRecipe,
    //            x.SortOrder,
    //            x.IsActive
    //        ))
    //        .ToListAsync(ct);

    //    var suppliers = await db.ProductSuppliers
    //        .AsNoTracking()
    //        .Where(x => x.ProductId == id && !x.IsDeleted)
    //        .Select(x => new ProductSupplierDto(
    //            x.Id,
    //            x.SupplierId,
    //            x.IsPreferred,
    //            x.IsActive
    //        ))
    //        .ToListAsync(ct);

    //    var prices = await db.ProductPrices
    //        .AsNoTracking()
    //        .Where(x => x.ProductId == id && !x.IsDeleted)
    //        .Select(x => new ProductPriceDto(
    //            x.Id,
    //            x.LocationId,
    //            x.ProductVariantId,
    //            x.PriceLevelId,
    //            x.CostPrice,
    //            x.Price,
    //            x.Qty
    //        ))
    //        .ToListAsync(ct);

    //    return new ProductDetailDto(
    //        Id: product.Id,
    //        DepartmentId: product.DepartmentId,
    //        CategoryId: product.CategoryId,
    //        Sku: product.Sku,
    //        Barcode: product.Barcode,
    //        Name: product.Name,
    //        NameOnInvoice: product.NameOnInvoice,
    //        Description: product.Description,
    //        RefCode01: product.RefCode01,
    //        RefCode02: product.RefCode02,
    //        UnitOfMeasureId: product.UnitOfMeasureId,
    //        PurchaseUnitOfMeasureId: null,
    //        BasePrice: product.BasePrice,
    //        CostPrice: product.CostPrice,
    //        TaxId: product.TaxId,
    //        IsTaxable: product.IsTaxable,
    //        IsTaxInclusive: product.IsTaxInclusive,
    //        TaxClass: product.TaxClass,
    //        ProductType: product.ProductType,
    //        IsSold: product.IsSold,
    //        IsPurchased: product.IsPurchased,
    //        IsStocked: product.IsStocked,
    //        IsScaleItem: product.IsScaleItem,
    //        IsOpenItem: product.IsOpenItem,
    //        IsLocked: product.IsLocked,
    //        TracksExpiry: product.TracksExpiry,
    //        AllowBelowCost: product.AllowBelowCost,
    //        AllowDiscount: product.AllowDiscount,
    //        DiscountType: product.DiscountType,
    //        DiscountValue: product.DiscountValue,
    //        MaxDiscountAmount: product.MaxDiscountAmount,
    //        MaxDiscountPercentage: product.MaxDiscountPercentage,
    //        ReorderLevel: product.ReorderLevel,
    //        ReorderQuantity: product.ReorderQuantity,
    //        ParLevel: product.ParLevel,
    //        ColorHex: product.ColorHex,
    //        ImageUrl: product.ImageUrl,
    //        SortOrder: product.SortOrder,
    //        IsAvailableOnline: product.IsAvailableOnline,
    //        KitchenStationCode: kitchenStationCode,
    //        KitchenStationCodes: kitchenStationCode is null ? Array.Empty<string>() : new[] { kitchenStationCode },
    //        PreferredSupplierId: product.PreferredSupplierId,
    //        IsActive: product.IsActive,
    //        Variants: variants,
    //        Suppliers: suppliers,
    //        Prices: prices
    //    );
    //}

    public async Task<(bool Success, string? Error, ProductDetailDto? Data)> SaveAsync(SaveProductRequest req, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(req.Sku))
                return (false, "Product code is required.", null);

            if (string.IsNullOrWhiteSpace(req.Name))
                return (false, "Product name is required.", null);

            await using var db = await factory.CreateForCurrentAsync(ct);

            // IMPORTANT: Create a new CancellationTokenSource that won't be cancelled by the client
            // This ensures the transaction completes even if the client disconnects
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(120)); // 2 minute timeout
            var dbCt = cts.Token;

            await using var trx = await db.Database.BeginTransactionAsync(dbCt);

            var sku = req.Sku.Trim().ToUpperInvariant();

            var duplicate = await db.Products.AnyAsync(x =>
                !x.IsDeleted &&
                x.Sku == sku &&
                (!req.Id.HasValue || x.Id != req.Id.Value), dbCt);

            if (duplicate)
            {
                await trx.RollbackAsync(CancellationToken.None);
                return (false, "Product code already exists.", null);
            }

            var unitExists = await db.UnitsOfMeasure.AnyAsync(x =>
                x.Id == req.UnitOfMeasureId &&
                !x.IsDeleted, dbCt);

            if (!unitExists)
            {
                await trx.RollbackAsync(CancellationToken.None);
                return (false, "Invalid unit of measure.", null);
            }

            if (req.LocationId.HasValue)
            {
                var locationExists = await db.Locations.AnyAsync(x =>
                    x.Id == req.LocationId.Value &&
                    x.IsActive &&
                    !x.IsDeleted, dbCt);

                if (!locationExists)
                {
                    await trx.RollbackAsync(CancellationToken.None);
                    return (false, "Invalid location.", null);
                }
            }

            if (req.DepartmentId.HasValue)
            {
                var departmentExists = await db.Departments.AnyAsync(x =>
                    x.Id == req.DepartmentId.Value &&
                    x.IsActive &&
                    !x.IsDeleted, dbCt);

                if (!departmentExists)
                {
                    await trx.RollbackAsync(CancellationToken.None);
                    return (false, "Invalid department.", null);
                }
            }

            if (req.CategoryId.HasValue)
            {
                var categoryExists = await db.Categories.AnyAsync(x =>
                    x.Id == req.CategoryId.Value &&
                    x.IsActive &&
                    !x.IsDeleted, dbCt);

                if (!categoryExists)
                {
                    await trx.RollbackAsync(CancellationToken.None);
                    return (false, "Invalid category.", null);
                }
            }

            //if (req.KitchenStationId.HasValue)
            //{
            //    var stationExists = await db.KitchenStations.AnyAsync(x =>
            //        x.Id == req.KitchenStationId.Value &&
            //        x.IsActive &&
            //        !x.IsDeleted, dbCt);

            //    if (!stationExists)
            //    {
            //        await trx.RollbackAsync(CancellationToken.None);
            //        return (false, "Invalid kitchen station.", null);
            //    }
            //}

            var entity = req.Id.HasValue
                ? await db.Products.FirstOrDefaultAsync(x => x.Id == req.Id.Value && !x.IsDeleted, dbCt)
                : null;

            if (req.Id.HasValue && entity is null)
            {
                await trx.RollbackAsync(CancellationToken.None);
                return (false, "Product not found.", null);
            }

            if (entity is null)
            {
                entity = new Product();
                db.Products.Add(entity);
            }

            entity.LocationId = req.LocationId;
            entity.DepartmentId = req.DepartmentId;
            entity.CategoryId = req.CategoryId;
            entity.Sku = sku;
            entity.Barcode = string.IsNullOrWhiteSpace(req.Barcode) ? null : req.Barcode.Trim();
            entity.Name = req.Name.Trim();
            entity.NameOnInvoice = string.IsNullOrWhiteSpace(req.NameOnInvoice) ? null : req.NameOnInvoice.Trim();
            entity.Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim();
            entity.RefCode01 = string.IsNullOrWhiteSpace(req.RefCode01) ? null : req.RefCode01.Trim();
            entity.RefCode02 = string.IsNullOrWhiteSpace(req.RefCode02) ? null : req.RefCode02.Trim();
            entity.UnitOfMeasureId = req.UnitOfMeasureId;
            entity.BasePrice = req.BasePrice < 0 ? 0 : req.BasePrice;
            entity.CostPrice = req.CostPrice < 0 ? 0 : req.CostPrice;
            entity.TaxClass = NormalizeTaxClass(req.TaxClass);
            entity.IsTaxable = req.IsTaxable && entity.TaxClass == "standard";
            entity.IsTaxInclusive = req.IsTaxInclusive;
            entity.ProductType = NormalizeProductType(req.ProductType);
            entity.IsSold = req.IsSold;
            entity.IsPurchased = req.IsPurchased;
            entity.IsStocked = req.IsStocked;
            entity.IsScaleItem = req.IsScaleItem;
            entity.IsOpenItem = req.IsOpenItem;
            entity.IsLocked = req.IsLocked;
            entity.TracksExpiry = req.TracksExpiry;
            entity.AllowBelowCost = req.AllowBelowCost;
            entity.AllowDiscount = req.AllowDiscount;
            entity.DiscountType = NormalizeDiscountType(req.DiscountType);
            entity.DiscountValue = req.DiscountValue < 0 ? 0 : req.DiscountValue;
            entity.MaxDiscountAmount = req.MaxDiscountAmount;
            entity.MaxDiscountPercentage = req.MaxDiscountPercentage;
            entity.ReorderLevel = req.ReorderLevel;
            entity.ReorderQuantity = req.ReorderQuantity;
            entity.ParLevel = req.ParLevel;
            entity.ColorHex = req.ColorHex;
            entity.ImageUrl = req.ImageUrl;
            entity.SortOrder = req.SortOrder < 0 ? 0 : req.SortOrder;
            entity.IsAvailableOnline = req.IsAvailableOnline;
            entity.PreferredSupplierId = req.PreferredSupplierId;
            entity.IsActive = req.IsActive;

            // Save the product - handle cancellation specially
            try
            {
                await db.SaveChangesAsync(dbCt);
            }
            catch (OperationCanceledException)
            {
                // If the client cancelled, we still want to try to complete the operation
                // So we'll retry with a new token that won't be cancelled
                Console.WriteLine("Operation was cancelled by client, attempting to continue...");
                try
                {
                    // Use a new token that won't be cancelled
                    using var retryCts = new CancellationTokenSource();
                    retryCts.CancelAfter(TimeSpan.FromSeconds(60));
                    await db.SaveChangesAsync(retryCts.Token);
                }
                catch (Exception retryEx)
                {
                    await trx.RollbackAsync(CancellationToken.None);
                    return (false, $"Product save failed after cancellation: {retryEx.Message}", null);
                }
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync(CancellationToken.None);
                return (false, $"Error saving product: {ex.Message}", null);
            }

            // Save variants
            try
            {
                await SaveVariantsAsync(db, entity.Id, req.Variants ?? [], dbCt);
            }
            catch (OperationCanceledException)
            {
                // Retry with a new token
                try
                {
                    using var retryCts = new CancellationTokenSource();
                    retryCts.CancelAfter(TimeSpan.FromSeconds(30));
                    await SaveVariantsAsync(db, entity.Id, req.Variants ?? [], retryCts.Token);
                }
                catch (Exception ex)
                {
                    await trx.RollbackAsync(CancellationToken.None);
                    return (false, $"Error saving variants after cancellation: {ex.Message}", null);
                }
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync(CancellationToken.None);
                return (false, $"Error saving variants: {ex.Message}", null);
            }

            // Flush now so newly-added variants get their server-generated Ids
            // before prices (below) are correlated to them by variant code —
            // a variant created in this same request has no Id yet otherwise.
            await db.SaveChangesAsync(dbCt);

            // Save suppliers
            try
            {
                await SaveSuppliersAsync(db, entity.Id, req.Suppliers ?? [], dbCt);
            }
            catch (OperationCanceledException)
            {
                try
                {
                    using var retryCts = new CancellationTokenSource();
                    retryCts.CancelAfter(TimeSpan.FromSeconds(30));
                    await SaveSuppliersAsync(db, entity.Id, req.Suppliers ?? [], retryCts.Token);
                }
                catch (Exception ex)
                {
                    await trx.RollbackAsync(CancellationToken.None);
                    return (false, $"Error saving suppliers after cancellation: {ex.Message}", null);
                }
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync(CancellationToken.None);
                return (false, $"Error saving suppliers: {ex.Message}", null);
            }

            // Save prices
            try
            {
                await SavePricesAsync(db, entity.Id, req.Prices ?? [], dbCt);
            }
            catch (OperationCanceledException)
            {
                try
                {
                    using var retryCts = new CancellationTokenSource();
                    retryCts.CancelAfter(TimeSpan.FromSeconds(30));
                    await SavePricesAsync(db, entity.Id, req.Prices ?? [], retryCts.Token);
                }
                catch (Exception ex)
                {
                    await trx.RollbackAsync(CancellationToken.None);
                    return (false, $"Error saving prices after cancellation: {ex.Message}", null);
                }
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync(CancellationToken.None);
                return (false, $"Error saving prices: {ex.Message}", null);
            }

            // Save kitchen stations
            try
            {
                await SaveKitchenStationsAsync(db, entity.Id, req.KitchenStations ?? [], dbCt);
            }
            catch (OperationCanceledException)
            {
                try
                {
                    using var retryCts = new CancellationTokenSource();
                    retryCts.CancelAfter(TimeSpan.FromSeconds(30));
                    await SaveKitchenStationsAsync(db, entity.Id, req.KitchenStations ?? [], retryCts.Token);
                }
                catch (Exception ex)
                {
                    await trx.RollbackAsync(CancellationToken.None);
                    return (false, $"Error saving kitchen stations after cancellation: {ex.Message}", null);
                }
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync(CancellationToken.None);
                return (false, $"Error saving kitchen stations: {ex.Message}", null);
            }

            // Save charges
            try
            {
                await SaveProductChargesAsync(db, entity.Id, req.Charges ?? [], dbCt);
            }
            catch (OperationCanceledException)
            {
                try
                {
                    using var retryCts = new CancellationTokenSource();
                    retryCts.CancelAfter(TimeSpan.FromSeconds(30));
                    await SaveProductChargesAsync(db, entity.Id, req.Charges ?? [], retryCts.Token);
                }
                catch (Exception ex)
                {
                    await trx.RollbackAsync(CancellationToken.None);
                    return (false, $"Error saving charges after cancellation: {ex.Message}", null);
                }
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync(CancellationToken.None);
                return (false, $"Error saving charges: {ex.Message}", null);
            }

            // Final save and commit
            try
            {
                await db.SaveChangesAsync(dbCt);
                await trx.CommitAsync(dbCt);
            }
            catch (OperationCanceledException)
            {
                try
                {
                    using var retryCts = new CancellationTokenSource();
                    retryCts.CancelAfter(TimeSpan.FromSeconds(30));
                    await db.SaveChangesAsync(retryCts.Token);
                    await trx.CommitAsync(retryCts.Token);
                }
                catch (Exception ex)
                {
                    await trx.RollbackAsync(CancellationToken.None);
                    return (false, $"Failed to commit transaction after cancellation: {ex.Message}", null);
                }
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync(CancellationToken.None);
                return (false, $"Error completing save: {ex.Message}", null);
            }

            // Get the saved product - use a fresh context
            try
            {
                // Use a new token that won't be cancelled
                using var getCts = new CancellationTokenSource();
                getCts.CancelAfter(TimeSpan.FromSeconds(30));
                var saved = await GetAsync(entity.Id, getCts.Token);
                return (true, null, saved);
            }
            catch (OperationCanceledException)
            {
                // Product was saved, but retrieval was cancelled - still return success
                return (true, null, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving saved product: {ex.Message}");
                // Product was saved, so return success even if retrieval fails
                return (true, null, null);
            }
        }
        catch (OperationCanceledException)
        {
            // Final catch for any cancellation that wasn't handled
            return (false, "The operation was canceled. Please check if the product was created.", null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FATAL ERROR in SaveAsync: {ex.Message}");
            return (false, $"An unexpected error occurred: {ex.Message}", null);
        }
    }

    //public async Task<(bool Success, string? Error, ProductDetailDto? Data)> SaveAsync(SaveProductRequest req, CancellationToken ct)
    //{
    //    try
    //    {
    //        if (string.IsNullOrWhiteSpace(req.Sku))
    //            return (false, "Product code is required.", null);

    //        if (string.IsNullOrWhiteSpace(req.Name))
    //            return (false, "Product name is required.", null);

    //        await using var db = await factory.CreateForCurrentAsync(ct);
    //        await using var trx = await db.Database.BeginTransactionAsync(ct);

    //        var sku = req.Sku.Trim().ToUpperInvariant();

    //        var duplicate = await db.Products.AnyAsync(x =>
    //            !x.IsDeleted &&
    //            x.Sku == sku &&
    //            (!req.Id.HasValue || x.Id != req.Id.Value), ct);

    //        if (duplicate)
    //            return (false, "Product code already exists.", null);

    //        var unitExists = await db.UnitsOfMeasure.AnyAsync(x =>
    //            x.Id == req.UnitOfMeasureId &&
    //            !x.IsDeleted, ct);

    //        if (!unitExists)
    //            return (false, "Invalid unit of measure.", null);

    //        if (req.LocationId.HasValue)
    //        {
    //            var locationExists = await db.Locations.AnyAsync(x =>
    //                x.Id == req.LocationId.Value &&
    //                x.IsActive &&
    //                !x.IsDeleted, ct);

    //            if (!locationExists)
    //                return (false, "Invalid location.", null);
    //        }

    //        if (req.DepartmentId.HasValue)
    //        {
    //            var departmentExists = await db.Departments.AnyAsync(x =>
    //                x.Id == req.DepartmentId.Value &&
    //                x.IsActive &&
    //                !x.IsDeleted, ct);

    //            if (!departmentExists)
    //                return (false, "Invalid department.", null);
    //        }

    //        if (req.CategoryId.HasValue)
    //        {
    //            var categoryExists = await db.Categories.AnyAsync(x =>
    //                x.Id == req.CategoryId.Value &&
    //                x.IsActive &&
    //                !x.IsDeleted, ct);

    //            if (!categoryExists)
    //                return (false, "Invalid category.", null);
    //        }

    //        if (req.KitchenStationId.HasValue)
    //        {
    //            var stationExists = await db.KitchenStations.AnyAsync(x =>
    //                x.Id == req.KitchenStationId.Value &&
    //                x.IsActive &&
    //                !x.IsDeleted, ct);

    //            if (!stationExists)
    //                return (false, "Invalid kitchen station.", null);
    //        }

    //        var entity = req.Id.HasValue
    //            ? await db.Products.FirstOrDefaultAsync(x => x.Id == req.Id.Value && !x.IsDeleted, ct)
    //            : null;

    //        if (req.Id.HasValue && entity is null)
    //            return (false, "Product not found.", null);

    //        if (entity is null)
    //        {
    //            entity = new Product();
    //            db.Products.Add(entity);
    //        }

    //        entity.LocationId = req.LocationId;
    //        entity.DepartmentId = req.DepartmentId;
    //        entity.CategoryId = req.CategoryId;
    //        entity.Sku = sku;
    //        entity.Barcode = string.IsNullOrWhiteSpace(req.Barcode) ? null : req.Barcode.Trim();
    //        entity.Name = req.Name.Trim();
    //        entity.NameOnInvoice = string.IsNullOrWhiteSpace(req.NameOnInvoice) ? null : req.NameOnInvoice.Trim();
    //        entity.Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim();
    //        entity.RefCode01 = string.IsNullOrWhiteSpace(req.RefCode01) ? null : req.RefCode01.Trim();
    //        entity.RefCode02 = string.IsNullOrWhiteSpace(req.RefCode02) ? null : req.RefCode02.Trim();
    //        entity.UnitOfMeasureId = req.UnitOfMeasureId;
    //        entity.BasePrice = req.BasePrice < 0 ? 0 : req.BasePrice;
    //        entity.CostPrice = req.CostPrice < 0 ? 0 : req.CostPrice;
    //        entity.TaxId = req.TaxId;
    //        entity.TaxClass = NormalizeTaxClass(req.TaxClass);
    //        entity.IsTaxable = req.IsTaxable && entity.TaxClass == "standard";
    //        entity.IsTaxInclusive = req.IsTaxInclusive;
    //        entity.ProductType = NormalizeProductType(req.ProductType);
    //        entity.IsSold = req.IsSold;
    //        entity.IsPurchased = req.IsPurchased;
    //        entity.IsStocked = req.IsStocked;
    //        entity.IsScaleItem = req.IsScaleItem;
    //        entity.IsOpenItem = req.IsOpenItem;
    //        entity.IsLocked = req.IsLocked;
    //        entity.TracksExpiry = req.TracksExpiry;
    //        entity.AllowBelowCost = req.AllowBelowCost;
    //        entity.AllowDiscount = req.AllowDiscount;
    //        entity.DiscountType = NormalizeDiscountType(req.DiscountType);
    //        entity.DiscountValue = req.DiscountValue < 0 ? 0 : req.DiscountValue;
    //        entity.MaxDiscountAmount = req.MaxDiscountAmount;
    //        entity.MaxDiscountPercentage = req.MaxDiscountPercentage;
    //        entity.ReorderLevel = req.ReorderLevel;
    //        entity.ReorderQuantity = req.ReorderQuantity;
    //        entity.ParLevel = req.ParLevel;
    //        entity.ColorHex = req.ColorHex;
    //        entity.ImageUrl = req.ImageUrl;
    //        entity.SortOrder = req.SortOrder < 0 ? 0 : req.SortOrder;
    //        entity.IsAvailableOnline = req.IsAvailableOnline;
    //        entity.KitchenStationId = req.KitchenStationId;
    //        entity.PreferredSupplierId = req.PreferredSupplierId;
    //        entity.IsActive = req.IsActive;

    //        await db.SaveChangesAsync(ct);

    //        await SaveVariantsAsync(db, entity.Id, req.Variants ?? [], ct);
    //        await SaveSuppliersAsync(db, entity.Id, req.Suppliers ?? [], ct);
    //        await SavePricesAsync(db, entity.Id, req.Prices ?? [], ct);

    //        await db.SaveChangesAsync(ct);
    //        await trx.CommitAsync(ct);

    //        var saved = await GetAsync(entity.Id, ct);
    //        return (true, null, saved);
    //    }
    //    catch(Exception ex)
    //    {
    //        return (false, $"An error occurred while saving the product: {ex.Message}", null);
    //    }

    //}

    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        var entity = await db.Products.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

        if (entity is null)
            return (false, "Product not found.");

        entity.IsDeleted = true;
        entity.IsActive = false;

        await db.SaveChangesAsync(ct);

        return (true, null);
    }

    public async Task<object> GetLookupsAsync(CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        var locations = await db.Locations.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Code, x.Name })
            .ToListAsync(ct);

        var departments = await db.Departments.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Code, x.Name, x.LocationId })
            .ToListAsync(ct);

        var categories = await db.Categories.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.ParentId, x.Code, x.Name })
            .ToListAsync(ct);

        var units = await db.UnitsOfMeasure.AsNoTracking()
            .OrderBy(x => x.Code)
            .Select(x => new { x.Id, x.Code, x.Name, x.Symbol })
            .ToListAsync(ct);

        // Only charges assignable to a product (e.g. Tax) — bill-level ones
        // (Service Charge, Levy, ...) apply tenant-wide and aren't picked per product.
        var charges = await db.Charges.AsNoTracking()
            .Where(x => x.IsActive && x.ChargeType!.AppliesPerProduct)
            .OrderBy(x => x.Code)
            .Select(x => new { x.Id, x.Code, x.Description, x.Percentage, x.Amount, ChargeTypeName = x.ChargeType!.Name })
            .ToListAsync(ct);

        var suppliers = await db.Suppliers.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Code, x.Name })
            .ToListAsync(ct);

        var servingUnits = await db.ServingUnits.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new { x.Id, x.Code, x.Name })
            .ToListAsync(ct);

        var priceLevels = await db.PriceLevels.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.SortOrder)
            .Select(x => new { x.Id, x.Code, x.Name, x.LocationId, x.IsDefault, x.AppliesToOrderType })
            .ToListAsync(ct);

        var kitchenStations = await db.KitchenStations.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new { x.Id, x.Code, x.Name, x.LocationId, x.PrinterName, x.PrinterTypeId })
            .ToListAsync(ct);

        return new
        {
            locations,
            departments,
            categories,
            units,
            charges,
            suppliers,
            servingUnits,
            priceLevels,
            kitchenStations
        };
    }

    //private static async Task SaveVariantsAsync(TenantDbContext db, Guid productId, List<ProductVariantDto> incoming, CancellationToken ct)
    //{
    //    var existing = await db.ProductVariants
    //        .Where(x => x.ProductId == productId)
    //        .ToListAsync(ct);

    //    var keepIds = incoming
    //        .Where(x => x.Id.HasValue)
    //        .Select(x => x.Id!.Value)
    //        .ToHashSet();

    //    foreach (var old in existing.Where(x => !keepIds.Contains(x.Id)))
    //    {
    //        old.IsDeleted = true;
    //        old.IsActive = false;
    //    }

    //    var order = 0;

    //    foreach (var item in incoming.Where(x => !string.IsNullOrWhiteSpace(x.Code) && !string.IsNullOrWhiteSpace(x.Name)))
    //    {
    //        var row = item.Id.HasValue
    //            ? existing.FirstOrDefault(x => x.Id == item.Id.Value)
    //            : null;

    //        if (row is null)
    //        {
    //            row = new ProductVariant { ProductId = productId };
    //            db.ProductVariants.Add(row);
    //        }

    //        row.ServingUnitId = item.ServingUnitId;
    //        row.Code = item.Code.Trim().ToUpperInvariant();
    //        row.Name = item.Name.Trim();
    //        row.ServingQty = item.ServingQty <= 0 ? 1 : item.ServingQty;
    //        row.CostPrice = item.CostPrice < 0 ? 0 : item.CostPrice;
    //        row.Price = item.Price < 0 ? 0 : item.Price;
    //        row.DeductStockOnRecipe = item.DeductStockOnRecipe;
    //        row.SortOrder = item.SortOrder < 0 ? order : item.SortOrder;
    //        row.IsActive = item.IsActive;
    //        row.IsDeleted = false;

    //        order++;
    //    }
    //}
    private static async Task SaveVariantsAsync(TenantDbContext db, Guid productId, List<ProductVariantDto> incoming, CancellationToken ct)
    {
        var existing = await db.ProductVariants
            .Where(x => x.ProductId == productId)
            .ToListAsync(ct);

        var keepIds = incoming
            .Where(x => x.Id.HasValue)
            .Select(x => x.Id!.Value)
            .ToHashSet();

        foreach (var old in existing.Where(x => !keepIds.Contains(x.Id)))
        {
            old.IsDeleted = true;
            old.IsActive = false;
        }

        var order = 0;

        foreach (var item in incoming.Where(x => !string.IsNullOrWhiteSpace(x.Code) && !string.IsNullOrWhiteSpace(x.Name)))
        {
            var row = item.Id.HasValue
                ? existing.FirstOrDefault(x => x.Id == item.Id.Value)
                : null;

            if (row is null)
            {
                row = new ProductVariant { ProductId = productId };
                db.ProductVariants.Add(row);
            }

            // Handle nullable ServingUnitId
            row.ServingUnitId = item.ServingUnitId; // This is already Guid?
            row.Code = item.Code.Trim().ToUpperInvariant();
            row.Name = item.Name.Trim();
            row.ServingQty = item.ServingQty <= 0 ? 1 : item.ServingQty;
            row.CostPrice = item.CostPrice < 0 ? 0 : item.CostPrice;
            row.Price = item.Price < 0 ? 0 : item.Price;
            row.DeductStockOnRecipe = item.DeductStockOnRecipe;
            row.SortOrder = item.SortOrder < 0 ? order : item.SortOrder;
            row.IsActive = item.IsActive;
            row.IsDeleted = false;

            order++;
        }
    }

    //private static async Task SaveSuppliersAsync(TenantDbContext db, Guid productId, List<ProductSupplierDto> incoming, CancellationToken ct)
    //{
    //    var existing = await db.ProductSuppliers
    //        .Where(x => x.ProductId == productId)
    //        .ToListAsync(ct);

    //    var keepIds = incoming
    //        .Where(x => x.Id.HasValue)
    //        .Select(x => x.Id!.Value)
    //        .ToHashSet();

    //    foreach (var old in existing.Where(x => !keepIds.Contains(x.Id)))
    //    {
    //        old.IsDeleted = true;
    //        old.IsActive = false;
    //    }

    //    var preferredFound = false;

    //    foreach (var item in incoming)
    //    {
    //        var row = item.Id.HasValue
    //            ? existing.FirstOrDefault(x => x.Id == item.Id.Value)
    //            : null;

    //        if (row is null)
    //        {
    //            row = new ProductSupplier { ProductId = productId };
    //            db.ProductSuppliers.Add(row);
    //        }

    //        row.SupplierId = item.SupplierId;
    //        row.IsPreferred = item.IsPreferred && !preferredFound;
    //        row.IsActive = item.IsActive;
    //        row.IsDeleted = false;

    //        if (row.IsPreferred)
    //            preferredFound = true;
    //    }
    //}

    private static async Task SaveSuppliersAsync(TenantDbContext db, Guid productId, List<ProductSupplierDto> incoming, CancellationToken ct)
    {
        var existing = await db.ProductSuppliers
            .Where(x => x.ProductId == productId)
            .ToListAsync(ct);

        var keepIds = incoming
            .Where(x => x.Id.HasValue)
            .Select(x => x.Id!.Value)
            .ToHashSet();

        foreach (var old in existing.Where(x => !keepIds.Contains(x.Id)))
        {
            old.IsDeleted = true;
            old.IsActive = false;
        }

        var preferredFound = false;

        foreach (var item in incoming)
        {
            var row = item.Id.HasValue
                ? existing.FirstOrDefault(x => x.Id == item.Id.Value)
                : null;

            if (row is null)
            {
                row = new ProductSupplier { ProductId = productId };
                db.ProductSuppliers.Add(row);
            }

            row.SupplierId = item.SupplierId;
            row.IsPreferred = item.IsPreferred && !preferredFound;
            row.IsActive = item.IsActive;
            row.IsDeleted = false;

            if (row.IsPreferred)
                preferredFound = true;
        }
    }

    //private static async Task SavePricesAsync(TenantDbContext db, Guid productId, List<ProductPriceDto> incoming, CancellationToken ct)
    //{
    //    var existing = await db.ProductPrices
    //        .Where(x => x.ProductId == productId)
    //        .ToListAsync(ct);

    //    var keepIds = incoming
    //        .Where(x => x.Id.HasValue)
    //        .Select(x => x.Id!.Value)
    //        .ToHashSet();

    //    foreach (var old in existing.Where(x => !keepIds.Contains(x.Id)))
    //        old.IsDeleted = true;

    //    foreach (var item in incoming)
    //    {
    //        var row = item.Id.HasValue
    //            ? existing.FirstOrDefault(x => x.Id == item.Id.Value)
    //            : null;

    //        if (row is null)
    //        {
    //            row = new ProductPrice { ProductId = productId };
    //            db.ProductPrices.Add(row);
    //        }

    //        row.LocationId = item.LocationId;
    //        row.ProductVariantId = item.ProductVariantId;
    //        row.PriceLevelId = item.PriceLevelId;
    //        row.CostPrice = item.CostPrice < 0 ? 0 : item.CostPrice;
    //        row.Price = item.Price < 0 ? 0 : item.Price;
    //        row.Qty = item.Qty <= 0 ? 1 : item.Qty;
    //        row.IsDeleted = false;
    //    }
    //}
    private static async Task SavePricesAsync(TenantDbContext db, Guid productId, List<ProductPriceDto> incoming, CancellationToken ct)
    {
        var existing = await db.ProductPrices
            .Where(x => x.ProductId == productId)
            .ToListAsync(ct);

        var keepIds = incoming
            .Where(x => x.Id.HasValue)
            .Select(x => x.Id!.Value)
            .ToHashSet();

        foreach (var old in existing.Where(x => !keepIds.Contains(x.Id)))
            old.IsDeleted = true;

        // A price row for a variant created in this same request only knows the
        // variant's code (not its Id) at the time the client built the payload —
        // resolve it now that SaveVariantsAsync + the flush before this call have
        // given every variant a real Id.
        var variantIdByCode = await db.ProductVariants
            .Where(x => x.ProductId == productId && !x.IsDeleted)
            .ToDictionaryAsync(x => x.Code, x => x.Id, StringComparer.OrdinalIgnoreCase, ct);

        foreach (var item in incoming)
        {
            var row = item.Id.HasValue
                ? existing.FirstOrDefault(x => x.Id == item.Id.Value)
                : null;

            if (row is null)
            {
                row = new ProductPrice { ProductId = productId };
                db.ProductPrices.Add(row);
            }

            var variantId = item.ProductVariantId
                ?? (!string.IsNullOrWhiteSpace(item.VariantCode) && variantIdByCode.TryGetValue(item.VariantCode, out var vid)
                    ? vid
                    : (Guid?)null);

            // Handle nullable fields
            row.LocationId = item.LocationId;
            row.ProductVariantId = variantId;
            row.PriceLevelId = item.PriceLevelId;
            row.CostPrice = item.CostPrice < 0 ? 0 : item.CostPrice;
            row.Price = item.Price < 0 ? 0 : item.Price;
            //row.Qty = item.Qty <= 0 ? 1 : item.Qty;
            row.IsDeleted = false;
        }
    }

    private static async Task SaveKitchenStationsAsync(TenantDbContext db, Guid productId, List<ProductKitchenStationDto> incoming, CancellationToken ct)
    {
        var existing = await db.ProductKitchenStations
            .Where(x => x.ProductId == productId)
            .ToListAsync(ct);

        var keepIds = incoming
            .Where(x => x.Id.HasValue)
            .Select(x => x.Id!.Value)
            .ToHashSet();

        foreach (var old in existing.Where(x => !keepIds.Contains(x.Id)))
            old.IsDeleted = true;

        foreach (var item in incoming.Where(x => x.KitchenStationId != Guid.Empty))
        {
            var row = item.Id.HasValue
                ? existing.FirstOrDefault(x => x.Id == item.Id.Value)
                : null;

            if (row is null)
            {
                row = new ProductKitchenStation { ProductId = productId };
                db.ProductKitchenStations.Add(row);
            }

            row.KitchenStationId = item.KitchenStationId;
            row.LocationId = item.LocationId;
            row.IsDeleted = false;
        }
    }

    private static async Task SaveProductChargesAsync(TenantDbContext db, Guid productId, List<ProductChargeDto> incoming, CancellationToken ct)
    {
        var existing = await db.ProductCharges
            .Where(x => x.ProductId == productId)
            .ToListAsync(ct);

        var keepIds = incoming
            .Where(x => x.Id.HasValue)
            .Select(x => x.Id!.Value)
            .ToHashSet();

        foreach (var old in existing.Where(x => !keepIds.Contains(x.Id)))
            old.IsDeleted = true;

        foreach (var item in incoming.Where(x => x.ChargeId != Guid.Empty))
        {
            var row = item.Id.HasValue
                ? existing.FirstOrDefault(x => x.Id == item.Id.Value)
                : null;

            if (row is null)
            {
                row = new ProductCharge { ProductId = productId };
                db.ProductCharges.Add(row);
            }

            row.ChargeId = item.ChargeId;
            row.IsDeleted = false;
        }
    }

    /// <summary>
    /// Bulk CSV import (#master-data-import): upsert products by Product Code (Sku); auto-creates
    /// Category and Unit of Measure by name when they don't already exist (matches the Customers/
    /// Suppliers import convention). Station is resolved against existing Kitchen Stations only —
    /// never auto-created, since a station also needs a printer type — and simply left unlinked if
    /// no name matches (supplementary metadata, not a hard error). Per-row error report so good rows
    /// land and bad rows show.
    /// </summary>
    public async Task<object> ImportAsync(List<ProductImportRow> rows, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var products = await db.Products.Where(x => !x.IsDeleted).ToListAsync(ct);
        var categories = await db.Categories.Where(x => !x.IsDeleted).ToListAsync(ct);
        var units = await db.UnitsOfMeasure.Where(x => !x.IsDeleted).ToListAsync(ct);
        var stations = await db.KitchenStations.Where(x => !x.IsDeleted).ToListAsync(ct);
        var links = await db.ProductKitchenStations.Where(x => !x.IsDeleted).ToListAsync(ct);

        int created = 0, updated = 0;
        var errors = new List<object>();
        var rn = 0;

        foreach (var row in rows)
        {
            rn++;
            try
            {
                var sku = row.Sku?.Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(sku)) { errors.Add(new { row = rn, sku = row.Sku, error = "Product Code is required." }); continue; }
                if (string.IsNullOrWhiteSpace(row.Name)) { errors.Add(new { row = rn, sku, error = "Name is required." }); continue; }

                Guid? unitId = null;
                if (!string.IsNullOrWhiteSpace(row.Unit))
                {
                    var unitName = row.Unit!.Trim();
                    var u = units.FirstOrDefault(x => string.Equals(x.Code, unitName, StringComparison.OrdinalIgnoreCase))
                        ?? units.FirstOrDefault(x => string.Equals(x.Name, unitName, StringComparison.OrdinalIgnoreCase));
                    if (u is null)
                    {
                        u = new UnitOfMeasure { Id = Guid.NewGuid(), Code = unitName.ToUpperInvariant()[..Math.Min(12, unitName.Length)], Name = unitName };
                        db.UnitsOfMeasure.Add(u); units.Add(u);
                        // Product.UnitOfMeasureId has no EF-configured relationship (DB-only FK), so EF
                        // can't order a same-batch insert correctly — flush now so the row exists before
                        // any product in this same import references it.
                        await db.SaveChangesAsync(ct);
                    }
                    unitId = u.Id;
                }

                var p = products.FirstOrDefault(x => x.Sku == sku);
                if (p is null)
                {
                    if (unitId is null) { errors.Add(new { row = rn, sku, error = "Unit is required for a new product." }); continue; }
                    p = new Product { Id = Guid.NewGuid(), Sku = sku, UnitOfMeasureId = unitId.Value };
                    db.Products.Add(p); products.Add(p); created++;
                }
                else updated++;

                p.Name = row.Name!.Trim();
                if (unitId is { } uid) p.UnitOfMeasureId = uid;
                if (row.Barcode?.Trim() is { Length: > 0 } bc) p.Barcode = bc;
                if (row.SellPrice is { } sp) p.BasePrice = sp;
                if (row.Cost is { } cost) p.CostPrice = cost;
                if (!string.IsNullOrWhiteSpace(row.TaxClass)) p.TaxClass = NormalizeImportTaxClass(row.TaxClass);
                if (!string.IsNullOrWhiteSpace(row.Type)) p.ProductType = NormalizeImportProductType(row.Type);

                if (!string.IsNullOrWhiteSpace(row.Category))
                {
                    var catName = row.Category!.Trim();
                    var cat = categories.FirstOrDefault(x => string.Equals(x.Code, catName, StringComparison.OrdinalIgnoreCase))
                        ?? categories.FirstOrDefault(x => string.Equals(x.Name, catName, StringComparison.OrdinalIgnoreCase));
                    if (cat is null)
                    {
                        cat = new Category { Id = Guid.NewGuid(), Code = catName.ToUpperInvariant()[..Math.Min(12, catName.Length)], Name = catName };
                        db.Categories.Add(cat); categories.Add(cat);
                        // Same reasoning as the Unit flush above — Product.CategoryId is also a DB-only FK.
                        await db.SaveChangesAsync(ct);
                    }
                    p.CategoryId = cat.Id;
                }

                if (!string.IsNullOrWhiteSpace(row.Station))
                {
                    var stName = row.Station!.Trim();
                    var st = stations.FirstOrDefault(x => string.Equals(x.Code, stName, StringComparison.OrdinalIgnoreCase))
                        ?? stations.FirstOrDefault(x => string.Equals(x.Name, stName, StringComparison.OrdinalIgnoreCase));
                    if (st is not null && !links.Any(x => x.ProductId == p.Id && x.KitchenStationId == st.Id))
                    {
                        var link = new ProductKitchenStation { ProductId = p.Id, KitchenStationId = st.Id };
                        db.ProductKitchenStations.Add(link); links.Add(link);
                    }
                }
            }
            catch (Exception ex) { errors.Add(new { row = rn, sku = row.Sku, error = ex.Message }); }
        }

        await db.SaveChangesAsync(ct);
        return new { created, updated, errors };
    }

    /// <summary>Import-friendly Type synonyms ("raw" as well as "raw_material") — the template's
    /// own hint promises these short forms, unlike the strict single-product save.</summary>
    private static string NormalizeImportProductType(string? value)
    {
        var v = value?.Trim().ToLowerInvariant();
        return v switch
        {
            "raw" or "raw_material" or "raw material" => "raw_material",
            "addon" or "add-on" => "addon",
            "pack" => "pack",
            "bundle" => "bundle",
            _ => "finished",
        };
    }

    /// <summary>Import-friendly Tax Class synonyms ("zero" as well as "zero_rated") — same reasoning
    /// as <see cref="NormalizeImportProductType"/>.</summary>
    private static string NormalizeImportTaxClass(string? value)
    {
        var v = value?.Trim().ToLowerInvariant();
        return v switch
        {
            "zero" or "zero_rated" or "zero rated" => "zero_rated",
            "exempt" => "exempt",
            _ => "standard",
        };
    }

    private static string NormalizeTaxClass(string? value)
    {
        var v = value?.Trim().ToLowerInvariant();
        return v is "zero_rated" or "exempt" ? v : "standard";
    }

    private static string NormalizeProductType(string? value)
    {
        var v = value?.Trim().ToLowerInvariant();
        return v is "raw_material" or "addon" or "pack" or "bundle" ? v : "finished";
    }

    private static string NormalizeDiscountType(string? value)
    {
        var v = value?.Trim().ToLowerInvariant();
        return v is "percentage" or "fixed" ? v : "none";
    }
}