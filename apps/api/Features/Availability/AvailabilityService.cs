using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Availability;

/// <summary>
/// Real-time per-location sellability (#112). A sold item is available at a location when:
///   1. a manual override row says so (force 86 / un-86), else
///   2. it's on hand (stocked finished good, qty > 0) OR makeable (every ingredient of at least
///      one active recipe has stock at that outlet), else
///   3. it's an untracked sold item (no stock, no recipe) → always available.
/// This is what auto-86s "rice &amp; curry" the moment an ingredient hits zero at an outlet.
/// </summary>
public class AvailabilityService(ITenantDbContextFactory factory)
{
    public record ItemAvailability(Guid ProductId, bool Available, string Reason);

    public async Task<List<ItemAvailability>> ComputeAsync(Guid locationId, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        return await ComputeAsync(db, locationId, ct);
    }

    public async Task<List<ItemAvailability>> ComputeAsync(TenantDbContext db, Guid locationId, CancellationToken ct)
    {
        var products = await db.Products.AsNoTracking()
            .Where(p => p.IsActive && p.IsSold)
            .Select(p => new { p.Id, p.IsStocked })
            .ToListAsync(ct);

        var stock = await db.ProductStocks.AsNoTracking()
            .Where(s => s.LocationId == locationId)
            .ToDictionaryAsync(s => s.ProductId, s => s.QuantityOnHand, ct);

        // Smallest serving a product can sell (e.g. a 50ml shot of a bottle stocked in ml).
        // An item is out of stock once it can't pour even its smallest size — not just at 0.
        var minServe = (await db.ProductVariants.AsNoTracking()
            .Where(v => v.IsActive && v.ServingQty > 0)
            .GroupBy(v => v.ProductId)
            .Select(g => new { ProductId = g.Key, Min = g.Min(v => v.ServingQty) })
            .ToListAsync(ct)).ToDictionary(x => x.ProductId, x => x.Min);
        decimal MinServe(Guid pid) => minServe.TryGetValue(pid, out var m) && m > 0 ? m : 1m;

        var recipesByProduct = (await db.Recipes.AsNoTracking().Include(r => r.Lines)
            .Where(r => r.IsActive).ToListAsync(ct))
            .GroupBy(r => r.ProductId).ToDictionary(g => g.Key, g => g.ToList());

        var overrides = await db.ProductAvailabilityOverrides.AsNoTracking()
            .Where(o => o.LocationId == locationId)
            .ToDictionaryAsync(o => o.ProductId, o => o.Available, ct);

        decimal Qty(Guid pid) => stock.TryGetValue(pid, out var q) ? q : 0m;

        var result = new List<ItemAvailability>(products.Count);
        foreach (var p in products)
        {
            if (overrides.TryGetValue(p.Id, out var ov))
            {
                result.Add(new(p.Id, ov, ov ? "manual_on" : "manual_86"));
                continue;
            }
            var hasRecipe = recipesByProduct.TryGetValue(p.Id, out var rs) && rs!.Count > 0;
            var makeable = hasRecipe && rs!.Any(r => r.Lines.All(l => Qty(l.IngredientProductId) > 0));
            var hasStock = p.IsStocked && Qty(p.Id) >= MinServe(p.Id);

            if (hasStock || makeable) result.Add(new(p.Id, true, "ok"));
            else if (!p.IsStocked && !hasRecipe) result.Add(new(p.Id, true, "ok"));   // untracked / service item
            else result.Add(new(p.Id, false, hasRecipe ? "ingredient_out" : "out_of_stock"));
        }
        return result;
    }
}
