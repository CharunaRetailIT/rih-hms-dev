using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Promotions;

/// <summary>
/// Evaluates auto-apply promotions for an order and returns the total discount +
/// a snapshot of which promotions fired. Handles product_discount, bill_value,
/// buy_x_get_y (BOGO) and bundle (combo). Promo discount is netted off the bill
/// like a manual discount, computed on the pre-tax line value — consistent with
/// the existing engine and tax stack.
/// </summary>
public static class PromotionEngine
{
    // Sri-Lanka-first: evaluate date/day/time windows in this zone. Multi-tz is a follow-on.
    private static DateTime NowLocal()
    {
        try { return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Asia/Colombo")); }
        catch { return DateTime.UtcNow; }
    }

    public static async Task<(decimal total, List<OrderPromotion> applied)> EvaluateAsync(
        TenantDbContext db, Order order, CancellationToken ct)
    {
        var items = order.Items.Where(i => !i.IsDeleted).ToList();
        var subtotal = items.Sum(i => i.LineSubtotal);
        if (subtotal <= 0) return (0m, new());

        var now = NowLocal();
        var promos = await db.Promotions.Include(p => p.Lines).AsNoTracking()
            .Where(p => p.IsActive && p.AutoApply)
            .OrderBy(p => p.Priority).ThenBy(p => p.Code)
            .ToListAsync(ct);

        // Customer-segment scope (#65): the attached customer's category, if any.
        Guid? custCat = order.CustomerId is Guid cid
            ? await db.Customers.AsNoTracking().Where(c => c.Id == cid).Select(c => c.CategoryId).FirstOrDefaultAsync(ct)
            : null;

        // For product-CATEGORY discounts (#4): map each cart product to its category, and
        // the category tree so a parent category's discount also covers its sub-categories.
        var prodIds = items.Select(i => i.ProductId).Distinct().ToList();
        var prodCat = await db.Products.IgnoreQueryFilters()
            .Where(p => prodIds.Contains(p.Id))
            .Select(p => new { p.Id, p.CategoryId })
            .ToDictionaryAsync(x => x.Id, x => x.CategoryId, ct);
        var catTree = await db.Categories.AsNoTracking()
            .Select(c => new { c.Id, c.ParentId }).ToListAsync(ct);
        HashSet<Guid> CategoryWithDescendants(Guid root)
        {
            var set = new HashSet<Guid> { root };
            var queue = new Queue<Guid>(); queue.Enqueue(root);
            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                foreach (var child in catTree.Where(c => c.ParentId == cur))
                    if (set.Add(child.Id)) queue.Enqueue(child.Id);
            }
            return set;
        }

        decimal total = 0;
        var applied = new List<OrderPromotion>();

        foreach (var p in promos)
        {
            if (!p.IsLive(now, order.OrderType)) continue;
            if (p.AppliesToCategoryId is Guid seg && custCat != seg) continue;   // segment-scoped, customer not in it
            var lines = p.Lines.Where(l => !l.IsDeleted).ToList();
            decimal d = 0;

            switch (p.PromoType)
            {
                case "product_discount":
                    foreach (var ln in lines)
                    {
                        // A line targets either a whole category (+ its sub-categories) or a single product.
                        IEnumerable<OrderItem> targets;
                        if (ln.CategoryId is Guid catId)
                        {
                            var catSet = CategoryWithDescendants(catId);
                            targets = items.Where(i => i.Quantity >= ln.MinQty
                                && prodCat.TryGetValue(i.ProductId, out var pc) && pc is Guid g && catSet.Contains(g));
                        }
                        else if (ln.ProductId is Guid pid)
                            targets = items.Where(i => i.ProductId == pid && i.Quantity >= ln.MinQty);
                        else continue;

                        foreach (var it in targets)
                            d += ln.DiscountPercent > 0
                                ? Math.Round(it.LineSubtotal * ln.DiscountPercent / 100m, 4)
                                : Math.Round(ln.DiscountAmount * it.Quantity, 4);
                    }
                    break;

                case "time_based":
                    // "Happy hour": a flat % or amount off the whole bill, live only inside the
                    // promotion's time window (enforced generically by IsLive above). One rule row
                    // carries the discount; a category-scoped row limits it to that category's items.
                    foreach (var ln in lines)
                    {
                        if (ln.CategoryId is Guid thCat)
                        {
                            var catSet = CategoryWithDescendants(thCat);
                            var catBase = items.Where(i => prodCat.TryGetValue(i.ProductId, out var pc) && pc is Guid g && catSet.Contains(g))
                                .Sum(i => i.LineSubtotal);
                            d += ln.DiscountPercent > 0 ? Math.Round(catBase * ln.DiscountPercent / 100m, 4) : ln.DiscountAmount;
                        }
                        else
                            d += ln.DiscountPercent > 0 ? Math.Round(subtotal * ln.DiscountPercent / 100m, 4) : ln.DiscountAmount;
                    }
                    break;

                case "bill_value":
                    var match = lines
                        .Where(l => subtotal >= l.BillFrom && (l.BillTo == null || subtotal <= l.BillTo))
                        .OrderByDescending(l => l.BillFrom).FirstOrDefault();
                    if (match is not null)
                        d = match.DiscountPercent > 0 ? Math.Round(subtotal * match.DiscountPercent / 100m, 4) : match.DiscountAmount;
                    break;

                case "buy_x_get_y":
                    // For every `MinQty` of the buy product, reward `GetQty` of the get
                    // product (default free, or % / flat off). When buy == get ("buy 1
                    // get 1"), a group is buy+get units so we never give the whole lot away.
                    foreach (var ln in lines)
                    {
                        if (ln.ProductId is null || ln.GetProductId is null || ln.MinQty <= 0 || ln.GetQty <= 0) continue;
                        var boughtQty = items.Where(i => i.ProductId == ln.ProductId).Sum(i => i.Quantity);
                        if (boughtQty <= 0) continue;
                        var groupSize = ln.GetProductId == ln.ProductId ? ln.MinQty + ln.GetQty : ln.MinQty;
                        var sets = Math.Floor(boughtQty / groupSize);
                        if (sets <= 0) continue;

                        var getItems = items.Where(i => i.ProductId == ln.GetProductId).ToList();
                        var getAvailable = getItems.Sum(i => i.Quantity);
                        var rewardUnits = Math.Min(sets * ln.GetQty, getAvailable);   // can't reward items not in the cart
                        if (rewardUnits <= 0) continue;

                        var unitPrice = getItems.Min(i => i.UnitPrice);               // reward the cheapest matching units
                        var perUnit = ln.DiscountPercent > 0 ? unitPrice * ln.DiscountPercent / 100m
                                    : ln.DiscountAmount > 0 ? ln.DiscountAmount
                                    : unitPrice;                                       // default: the "get" is free
                        perUnit = Math.Min(perUnit, unitPrice);                       // never discount below the price
                        d += Math.Round(rewardUnits * perUnit, 4);
                    }
                    break;

                case "bundle":
                    // A combo: when the cart holds every component (≥ its MinQty), charge
                    // the fixed BundlePrice instead of the sum of the parts. Discount =
                    // (normal price of the parts − bundle price) × complete bundles.
                    var comps = lines.Where(l => l.ProductId is not null && l.MinQty > 0).ToList();
                    var bundlePrice = lines.Select(l => l.BundlePrice).FirstOrDefault(bp => bp is not null);
                    if (comps.Count == 0 || bundlePrice is null) break;

                    decimal bundles = decimal.MaxValue, normalPerBundle = 0; var formable = true;
                    foreach (var c in comps)
                    {
                        var ci = items.Where(i => i.ProductId == c.ProductId).ToList();
                        var have = ci.Sum(i => i.Quantity);
                        if (ci.Count == 0 || have < c.MinQty) { formable = false; break; }
                        bundles = Math.Min(bundles, Math.Floor(have / c.MinQty));
                        normalPerBundle += ci.Min(i => i.UnitPrice) * c.MinQty;
                    }
                    if (!formable || bundles <= 0 || bundles == decimal.MaxValue) break;

                    var perBundle = normalPerBundle - bundlePrice.Value;
                    if (perBundle > 0) d = Math.Round(bundles * perBundle, 4);
                    break;

                case "lowest_price":
                    // "3-for-2" / cheapest-free: for every group of MinQty units, wave
                    // GetQty (default 1) of the cheapest at DiscountPercent (default 100%).
                    // ProductId optionally scopes the pool; else the whole basket.
                    foreach (var ln in lines)
                    {
                        var group = (int)ln.MinQty;
                        if (group <= 0) continue;
                        var pool = ln.ProductId is Guid pid ? items.Where(i => i.ProductId == pid) : items;
                        var units = pool.SelectMany(i => Enumerable.Repeat(i.UnitPrice, (int)Math.Floor(i.Quantity)))
                            .OrderBy(u => u).ToList();
                        if (units.Count < group) continue;
                        var perGroup = ln.GetQty > 0 ? (int)ln.GetQty : 1;
                        var wave = units.Count / group * perGroup;
                        var pct = ln.DiscountPercent > 0 ? ln.DiscountPercent : 100m;
                        d += units.Take(wave).Sum(u => Math.Round(u * pct / 100m, 4));
                    }
                    break;
            }

            if (d <= 0) continue;
            d = Math.Min(d, subtotal - total);   // never discount the bill below zero
            if (d <= 0) continue;
            total += d;
            applied.Add(new OrderPromotion { OrderId = order.Id, PromotionId = p.Id, Code = p.Code, Name = p.Name, DiscountAmount = d });
        }

        return (total, applied);
    }
}
