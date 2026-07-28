using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Production;

/// <summary>
/// Production (recipes/BOM) with full parity to the legacy module:
///   • multiple recipes per product, keyed by output unit
///   • recipe units convert to stock units (500 g → 0.5 kg) at avg cost
///   • lifecycle: draft (no stock move) → post (apply) → void (reverse)
///   • custom/ad-hoc production (no predefined recipe)
///   • multiple-product production documents (shared document number)
///   • inter-location production (consume here, deliver finished elsewhere)
/// Finished stock cost is rolled up by weighted average from what was consumed.
/// </summary>
public class ProductionService(ITenantDbContextFactory factory)
{
    // ─────────────────────────────── Recipes ───────────────────────────────
    public async Task<Recipe> SetRecipeAsync(SetRecipeInput input, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        if (!await db.Products.AnyAsync(p => p.Id == input.ProductId, ct))
            throw new InvalidOperationException("Finished product not found");
        if (input.Lines is null || input.Lines.Count == 0)
            throw new InvalidOperationException("A recipe needs at least one ingredient");
        if ((input.YieldQuantity ?? 1) <= 0)
            throw new InvalidOperationException("Yield quantity must be > 0");

        // Upsert by (product, output unit, location) — a product may have several recipes.
        var existing = await db.Recipes.Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.ProductId == input.ProductId && r.OutputUnitId == input.OutputUnitId
                && r.LocationId == input.LocationId, ct);
        if (existing is not null) { db.RecipeLines.RemoveRange(existing.Lines); db.Recipes.Remove(existing); await db.SaveChangesAsync(ct); }

        var units = await db.UnitsOfMeasure.AsNoTracking().ToDictionaryAsync(u => u.Id, ct);
        var recipe = new Recipe
        {
            ProductId = input.ProductId, OutputUnitId = input.OutputUnitId, LocationId = input.LocationId,
            YieldQuantity = input.YieldQuantity ?? 1, Notes = input.Notes, IsActive = true,
        };
        foreach (var l in input.Lines)
        {
            if (l.IngredientProductId == input.ProductId)
                throw new InvalidOperationException("A product cannot be its own ingredient");
            var ing = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == l.IngredientProductId, ct)
                ?? throw new InvalidOperationException($"Ingredient {l.IngredientProductId} not found");
            if (l.Quantity <= 0) throw new InvalidOperationException("Ingredient quantity must be > 0");

            units.TryGetValue(ing.UnitOfMeasureId, out var stockUnit);
            var lineUnit = l.UnitId is Guid uid && units.TryGetValue(uid, out var u) ? u : stockUnit;
            if (lineUnit is not null && stockUnit is not null && !Convertible(lineUnit, stockUnit))
                throw new InvalidOperationException(
                    $"Unit '{lineUnit.Symbol ?? lineUnit.Code}' can't be converted to '{stockUnit.Symbol ?? stockUnit.Code}' for {ing.Name}");

            // Cost per stock unit: this recipe's location's current average stock
            // cost if it has one, else the ingredient's master cost price.
            var costPrice = ing.CostPrice;
            if (input.LocationId is Guid locId)
            {
                var stock = await db.ProductStocks.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.ProductId == ing.Id && s.LocationId == locId, ct);
                if (stock is not null && stock.AverageCost > 0) costPrice = stock.AverageCost;
            }

            var factor = lineUnit is not null && stockUnit is not null && stockUnit.FactorToBase != 0
                ? lineUnit.FactorToBase / stockUnit.FactorToBase
                : 1m;
            var lineCost = Math.Round(l.Quantity * factor * costPrice, 4);

            recipe.Lines.Add(new RecipeLine
            {
                IngredientProductId = ing.Id, Sku = ing.Sku, ProductName = ing.Name, Quantity = l.Quantity,
                UnitId = lineUnit?.Id, UnitSymbol = lineUnit?.Symbol ?? lineUnit?.Code,
                CostPrice = costPrice, LineCost = lineCost,
            });
        }
        recipe.TotalCost = recipe.Lines.Sum(x => x.LineCost);
        db.Recipes.Add(recipe);
        await db.SaveChangesAsync(ct);
        return recipe;
    }

    public async Task<Recipe?> GetRecipeAsync(Guid productId, Guid? outputUnitId, Guid? locationId, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var q = db.Recipes.Include(r => r.Lines).AsNoTracking().Where(r => r.ProductId == productId);
        // If an output unit is requested, match it; otherwise return the default (null) or any.
        if (outputUnitId is not null)
            q = q.Where(r => r.OutputUnitId == outputUnitId);
        // If a location is requested, prefer it, then a location-agnostic recipe, then any.
        return locationId is null
            ? await q.OrderBy(r => r.OutputUnitId == null ? 0 : 1).ThenBy(r => r.LocationId == null ? 0 : 1).FirstOrDefaultAsync(ct)
            : await q.FirstOrDefaultAsync(r => r.LocationId == locationId, ct)
                ?? await q.FirstOrDefaultAsync(r => r.LocationId == null, ct)
                ?? await q.FirstOrDefaultAsync(ct);
    }

    public async Task<List<RecipeSummary>> ListRecipesAsync(CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var recipes = await db.Recipes.Include(r => r.Lines).AsNoTracking().ToListAsync(ct);
        var prods = await db.Products.AsNoTracking().Select(p => new { p.Id, p.Name, p.Sku }).ToListAsync(ct);
        var units = await db.UnitsOfMeasure.AsNoTracking().ToDictionaryAsync(u => u.Id, ct);
        var locs = await db.Locations.AsNoTracking().Select(l => new { l.Id, l.Code }).ToDictionaryAsync(l => l.Id, ct);
        var pmap = prods.ToDictionary(p => p.Id);
        return recipes
            .Select(r => new RecipeSummary(
                r.Id, r.ProductId,
                pmap.TryGetValue(r.ProductId, out var p) ? p.Name : "(unknown)",
                pmap.TryGetValue(r.ProductId, out var p2) ? p2.Sku : "",
                r.YieldQuantity, r.Lines.Count(l => !l.IsDeleted), r.IsActive,
                r.OutputUnitId, r.OutputUnitId is Guid ouid && units.TryGetValue(ouid, out var ou) ? ou.Symbol ?? ou.Code : null,
                r.LocationId, r.LocationId is Guid lid && locs.TryGetValue(lid, out var loc) ? loc.Code : null))
            .OrderBy(x => x.ProductName)
            .ToList();
    }

    // ────────────────────────────── Production ──────────────────────────────
    /// <summary>Recipe-based production. Posts immediately unless input.Post is false (draft).</summary>
    public async Task<ProductionOrder> ProduceAsync(ProduceInput input, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        if (input.Quantity <= 0) throw new InvalidOperationException("Production quantity must be > 0");
        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == input.ProductId, ct)
            ?? throw new InvalidOperationException("Finished product not found");
        var recipe = await ResolveRecipeAsync(db, input.ProductId, input.OutputUnitId, input.LocationId, product.Name, ct);

        var batches = input.Quantity / recipe.YieldQuantity;
        var specs = recipe.Lines.Select(l => new LineSpec(
            l.IngredientProductId, l.Sku, l.ProductName, l.Quantity * batches, l.UnitId)).ToList();

        var po = await NewOrderAsync(db, product, input.Quantity, input.LocationId, input.ReceiptLocationId,
            isCustom: false, documentNo: null, requestId: input.RequestId, notes: input.Notes, ct);
        await FillConsumptionsAsync(db, po, specs, input.LocationId, ct);
        if (input.Post) await ApplyAsync(db, po, ct);

        db.ProductionOrders.Add(po);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return po;
    }

    /// <summary>Custom/ad-hoc production: materials supplied directly, no recipe.</summary>
    public async Task<ProductionOrder> ProduceCustomAsync(CustomProduceInput input, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        if (input.Quantity <= 0) throw new InvalidOperationException("Production quantity must be > 0");
        if (input.Lines is null || input.Lines.Count == 0)
            throw new InvalidOperationException("Custom production needs at least one material");
        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == input.ProductId, ct)
            ?? throw new InvalidOperationException("Finished product not found");

        var specs = new List<LineSpec>();
        foreach (var l in input.Lines)
        {
            if (l.Quantity <= 0) throw new InvalidOperationException("Material quantity must be > 0");
            if (l.IngredientProductId == input.ProductId)
                throw new InvalidOperationException("A material cannot be the finished product");
            var ing = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == l.IngredientProductId, ct)
                ?? throw new InvalidOperationException($"Material {l.IngredientProductId} not found");
            specs.Add(new LineSpec(ing.Id, ing.Sku, ing.Name, l.Quantity, l.UnitId));
        }

        var po = await NewOrderAsync(db, product, input.Quantity, input.LocationId, input.ReceiptLocationId,
            isCustom: true, documentNo: null, requestId: null, notes: input.Notes, ct);
        await FillConsumptionsAsync(db, po, specs, input.LocationId, ct);
        if (input.Post) await ApplyAsync(db, po, ct);

        db.ProductionOrders.Add(po);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return po;
    }

    /// <summary>Multiple finished goods in one production document (shared document number).</summary>
    public async Task<List<ProductionOrder>> ProduceBatchAsync(BatchProduceInput input, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        if (input.Items is null || input.Items.Count == 0)
            throw new InvalidOperationException("A production document needs at least one product");
        var settings = await db.OrgSettings.AsNoTracking().FirstOrDefaultAsync(ct) ?? new OrgSettings();
        var documentNo = await NextNumberAsync(db, "PRDDOC", "PB", ct);

        var created = new List<ProductionOrder>();
        foreach (var item in input.Items)
        {
            if (item.Quantity <= 0) throw new InvalidOperationException("Production quantity must be > 0");
            var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == item.ProductId, ct)
                ?? throw new InvalidOperationException("Finished product not found");
            var recipe = await ResolveRecipeAsync(db, item.ProductId, item.OutputUnitId, input.LocationId, product.Name, ct);

            var batches = item.Quantity / recipe.YieldQuantity;
            var specs = recipe.Lines.Select(l => new LineSpec(
                l.IngredientProductId, l.Sku, l.ProductName, l.Quantity * batches, l.UnitId)).ToList();

            var po = await NewOrderAsync(db, product, item.Quantity, input.LocationId, input.ReceiptLocationId,
                isCustom: false, documentNo: documentNo, requestId: null, notes: item.Notes, ct);
            await FillConsumptionsAsync(db, po, specs, input.LocationId, ct);
            if (input.Post) await ApplyAsync(db, po, ct);
            db.ProductionOrders.Add(po);
            created.Add(po);
        }

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return created;
    }

    /// <summary>Post a draft: apply stock movements at the current avg cost.</summary>
    public async Task<ProductionOrder> PostAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var po = await db.ProductionOrders.Include(p => p.Consumptions).FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new InvalidOperationException("Production order not found");
        if (po.Status != "draft") throw new InvalidOperationException($"Only a draft can be posted (is {po.Status})");
        await ApplyAsync(db, po, ct);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return po;
    }

    /// <summary>Void: reverse a posted order's stock movements (or just cancel a draft).</summary>
    public async Task<ProductionOrder> VoidAsync(Guid id, string? reason, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var po = await db.ProductionOrders.Include(p => p.Consumptions).FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new InvalidOperationException("Production order not found");
        if (po.Status == "void") throw new InvalidOperationException("Already void");
        await PeriodGuard.EnsureOpenAsync(db, po.PostedAt ?? po.CreatedAt, ct);   // #77

        if (po.Status == "posted")
        {
            var receiptLoc = po.ReceiptLocationId ?? po.LocationId;
            // add the consumed materials back
            foreach (var c in po.Consumptions.Where(c => !c.IsDeleted))
            {
                var stock = await db.ProductStocks.FirstOrDefaultAsync(
                    s => s.ProductId == c.IngredientProductId && s.LocationId == po.LocationId, ct);
                if (stock is not null) stock.QuantityOnHand += c.QuantityConsumed;
            }
            // remove the produced finished goods
            var fin = await db.ProductStocks.FirstOrDefaultAsync(s => s.ProductId == po.ProductId && s.LocationId == receiptLoc, ct);
            if (fin is not null) fin.QuantityOnHand -= po.Quantity;
        }

        po.Status = "void";
        po.VoidedAt = DateTime.UtcNow;
        po.VoidReason = reason;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return po;
    }

    public async Task<List<ProductionOrder>> ListProductionAsync(int take, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        return await db.ProductionOrders.Include(p => p.Consumptions).AsNoTracking()
            .OrderByDescending(p => p.CompletedAt ?? p.CreatedAt).Take(take).ToListAsync(ct);
    }

    public async Task<ProductionOrder?> GetProductionAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        return await db.ProductionOrders.Include(p => p.Consumptions).AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    // ─────────────────────────────── internals ──────────────────────────────
    private record LineSpec(Guid IngredientProductId, string Sku, string ProductName, decimal Quantity, Guid? UnitId);

    /// <summary>Picks the active recipe for a product at production time: an exact
    /// (output unit, location) match wins, then a location-agnostic recipe for that
    /// output unit, then any active recipe for the product.</summary>
    private static async Task<Recipe> ResolveRecipeAsync(
        TenantDbContext db, Guid productId, Guid? outputUnitId, Guid locationId, string productLabel, CancellationToken ct)
    {
        var recipe = await db.Recipes.Include(r => r.Lines)
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.IsActive
                    && r.OutputUnitId == outputUnitId && r.LocationId == locationId, ct)
            ?? await db.Recipes.Include(r => r.Lines)
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.IsActive
                    && r.OutputUnitId == outputUnitId && r.LocationId == null, ct)
            ?? await db.Recipes.Include(r => r.Lines)
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.IsActive, ct);
        return recipe ?? throw new InvalidOperationException($"No active recipe for {productLabel}");
    }

    private async Task<ProductionOrder> NewOrderAsync(TenantDbContext db, Product product, decimal quantity,
        Guid locationId, Guid? receiptLocationId, bool isCustom, string? documentNo, Guid? requestId, string? notes, CancellationToken ct)
    {
        var settings = await db.OrgSettings.AsNoTracking().FirstOrDefaultAsync(ct) ?? new OrgSettings();
        return new ProductionOrder
        {
            LocationId = locationId,
            ReceiptLocationId = receiptLocationId is Guid rl && rl != locationId ? rl : null,
            ProductionNumber = await NextNumberAsync(db, "PRD", settings.ProductionPrefix, ct),
            DocumentNo = documentNo,
            ProductId = product.Id, ProductName = product.Name, Quantity = quantity,
            Status = "draft", IsCustom = isCustom, RequestId = requestId, Notes = notes,
        };
    }

    /// <summary>Compute consumption rows (converting recipe units → stock units) at current avg cost. No stock move yet.</summary>
    private async Task FillConsumptionsAsync(TenantDbContext db, ProductionOrder po, List<LineSpec> specs, Guid consumeLocation, CancellationToken ct)
    {
        var ingIds = specs.Select(s => s.IngredientProductId).Distinct().ToList();
        var ingProducts = await db.Products.AsNoTracking().Where(p => ingIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, ct);
        var units = await db.UnitsOfMeasure.AsNoTracking().ToDictionaryAsync(u => u.Id, ct);

        decimal totalInput = 0;
        foreach (var s in specs)
        {
            var factor = 1m;
            if (s.UnitId is Guid lineUnitId && ingProducts.TryGetValue(s.IngredientProductId, out var ip)
                && units.TryGetValue(lineUnitId, out var lineUnit) && units.TryGetValue(ip.UnitOfMeasureId, out var stockUnit)
                && stockUnit.FactorToBase != 0)
                factor = lineUnit.FactorToBase / stockUnit.FactorToBase;

            var required = s.Quantity * factor;   // stock units
            var stock = await db.ProductStocks.AsNoTracking().FirstOrDefaultAsync(
                x => x.ProductId == s.IngredientProductId && x.LocationId == consumeLocation, ct);
            var cost = stock?.AverageCost ?? 0m;
            var lineTotal = Math.Round(required * cost, 4);
            totalInput += lineTotal;
            po.Consumptions.Add(new ProductionConsumption
            {
                IngredientProductId = s.IngredientProductId, Sku = s.Sku, ProductName = s.ProductName,
                QuantityConsumed = required, UnitCost = cost, LineTotal = lineTotal,
            });
        }
        po.TotalInputCost = totalInput;
        po.UnitCost = po.Quantity > 0 ? Math.Round(totalInput / po.Quantity, 4) : 0m;
    }

    /// <summary>Apply stock: issue materials at consume location, yield finished at receipt location.</summary>
    private async Task ApplyAsync(TenantDbContext db, ProductionOrder po, CancellationToken ct)
    {
        var receiptLoc = po.ReceiptLocationId ?? po.LocationId;
        decimal totalInput = 0;
        foreach (var c in po.Consumptions.Where(c => !c.IsDeleted))
        {
            var stock = await db.ProductStocks.FirstOrDefaultAsync(
                x => x.ProductId == c.IngredientProductId && x.LocationId == po.LocationId, ct);
            if (stock is null || stock.QuantityOnHand < c.QuantityConsumed)
                throw new InvalidOperationException($"Insufficient {c.ProductName} to produce (need {c.QuantityConsumed})");
            c.UnitCost = stock.AverageCost;                       // refresh to current avg at post time
            c.LineTotal = Math.Round(c.QuantityConsumed * c.UnitCost, 4);
            stock.QuantityOnHand -= c.QuantityConsumed;           // issue — avg unchanged
            totalInput += c.LineTotal;
        }
        po.TotalInputCost = totalInput;
        po.UnitCost = po.Quantity > 0 ? Math.Round(totalInput / po.Quantity, 4) : 0m;

        var fin = await db.ProductStocks.FirstOrDefaultAsync(s => s.ProductId == po.ProductId && s.LocationId == receiptLoc, ct);
        if (fin is null)
        {
            fin = new ProductStock { ProductId = po.ProductId, LocationId = receiptLoc, QuantityOnHand = 0, AverageCost = 0 };
            db.ProductStocks.Add(fin);
        }
        var oldQty = fin.QuantityOnHand;
        var newQty = oldQty + po.Quantity;
        fin.AverageCost = newQty > 0 ? Math.Round((oldQty * fin.AverageCost + totalInput) / newQty, 4) : po.UnitCost;
        fin.QuantityOnHand = newQty;
        fin.LastReceivedAt = DateTime.UtcNow;

        po.Status = "posted";
        po.PostedAt = DateTime.UtcNow;
        po.CompletedAt = DateTime.UtcNow;
    }

    private static bool Convertible(UnitOfMeasure lineUnit, UnitOfMeasure stockUnit)
    {
        if (lineUnit.Id == stockUnit.Id) return true;
        if (lineUnit.Dimension != stockUnit.Dimension) return false;
        return lineUnit.Dimension is "mass" or "volume";
    }

    private static async Task<string> NextNumberAsync(TenantDbContext db, string counterKey, string prefix, CancellationToken ct)
    {
        var rows = await db.Database.SqlQuery<int>($@"
            INSERT INTO document_counters (tenant_id, doc_type, last_value)
            VALUES ({db.TenantId}, {counterKey}, 1)
            ON CONFLICT (tenant_id, doc_type)
            DO UPDATE SET last_value = document_counters.last_value + 1
            RETURNING last_value AS ""Value""").ToListAsync(ct);
        return $"{prefix}-{rows[0]:D4}";
    }
}

public record SetRecipeInput(Guid ProductId, decimal? YieldQuantity, string? Notes, List<RecipeLineInput> Lines, Guid? OutputUnitId = null, Guid? LocationId = null);
public record RecipeLineInput(Guid IngredientProductId, decimal Quantity, Guid? UnitId = null);
public record ProduceInput(Guid LocationId, Guid ProductId, decimal Quantity, string? Notes,
    Guid? OutputUnitId = null, Guid? ReceiptLocationId = null, bool Post = true, Guid? RequestId = null);
public record CustomLineInput(Guid IngredientProductId, decimal Quantity, Guid? UnitId = null);
public record CustomProduceInput(Guid LocationId, Guid ProductId, decimal Quantity, List<CustomLineInput> Lines,
    string? Notes = null, Guid? ReceiptLocationId = null, bool Post = true);
public record BatchItemInput(Guid ProductId, decimal Quantity, Guid? OutputUnitId = null, string? Notes = null);
public record BatchProduceInput(Guid LocationId, List<BatchItemInput> Items, Guid? ReceiptLocationId = null, bool Post = true);
public record RecipeSummary(Guid Id, Guid ProductId, string ProductName, string Sku, decimal YieldQuantity,
    int LineCount, bool IsActive, Guid? OutputUnitId = null, string? OutputUnitSymbol = null,
    Guid? LocationId = null, string? LocationCode = null);
