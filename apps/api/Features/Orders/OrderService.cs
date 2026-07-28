using System.Text.Json;
using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Orders;

/// <summary>
/// The transactional core of the POS. Implements the two critical business
/// rules from docs/v2-business-rules.md as explicit, testable service methods
/// (NOT SQL triggers, unlike the legacy system):
///
///   R1  Stock decrement on settlement      → SettleAsync()
///   R2  KOT/BOT generation on confirm       → ConfirmAsync()
///
/// Tax composition is the v1-simplified model: per-line VAT + a bill-level
/// service charge. Compound stacking is deferred to v2.
/// </summary>
public class OrderService(ITenantDbContextFactory factory)
{
    public async Task<Order> CreateAsync(CreateOrderInput input, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        // POS orders need an open shift (cash session). Aggregator orders don't.
        if ((input.OrderSource ?? "pos") == "pos")
        {
            var rules = await GetSettingsAsync(db, ct);
            if (rules.RequireOpenShiftForBilling
                && !await db.Shifts.AnyAsync(s => s.LocationId == input.LocationId && s.Status == "open", ct))
                throw new InvalidOperationException("Open a shift before taking orders");
        }

        var orderNumber = await NextOrderNumberAsync(db, ct);
        var orderType = input.OrderType ?? "dine_in";
        var priceLevelId = await ResolvePriceLevelAsync(db, input.PriceLevelId, orderType, ct);
        var order = new Order
        {
            LocationId = input.LocationId,
            OrderNumber = orderNumber,
            OrderType = orderType,
            PriceLevelId = priceLevelId,
            OrderSource = input.OrderSource ?? "pos",
            ExternalOrderId = input.ExternalOrderId,
            TableLabel = input.TableLabel,
            TableId = input.TableId,
            Covers = input.Covers,
            StewardId = input.StewardId,
            TourOperatorId = input.TourOperatorId,
            Status = "open",
            PendingAcceptance = input.PendingAcceptance,
            CashierId = input.CashierId,
            OpenedAt = DateTime.UtcNow,
            CustomerName = input.CustomerName,
            DeliveryAddress = input.DeliveryAddress,
            DeliveryPhone = input.DeliveryPhone,
            DeliveryNotes = input.DeliveryNotes,
            AggregatorPayload = input.AggregatorPayload,
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);
        return order;
    }

    public async Task<Order?> GetAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        return await db.Orders.Include(o => o.Items).ThenInclude(i => i.Modifiers)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<List<Order>> ListOpenAsync(Guid? locationId, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var q = db.Orders.Include(o => o.Items).Where(o => o.Status == "open" || o.Status == "confirmed");
        if (locationId.HasValue) q = q.Where(o => o.LocationId == locationId.Value);
        return await q.OrderBy(o => o.OpenedAt).ToListAsync(ct);
    }

    public async Task<Order> AddItemAsync(Guid orderId, AddItemInput input, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId, ct)
            ?? throw new InvalidOperationException("Order not found");
        if (order.Status is not ("open" or "confirmed"))
            throw new InvalidOperationException($"Cannot add items to a {order.Status} order");

        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == input.ProductId, ct)
            ?? throw new InvalidOperationException("Product not found");

        // Resolve any selected modifiers (validated against the product's groups),
        // sum their price deltas into the unit price, and snapshot them on the line.
        var (modifiers, modDelta) = await ResolveModifiersAsync(db, product.Id, input.ModifierItemIds, ct);

        // If a serving size was chosen, its price replaces the product base price.
        ProductVariant? variant = null;
        if (input.VariantId is Guid vid)
        {
            variant = await db.ProductVariants.FirstOrDefaultAsync(
                v => v.Id == vid && v.ProductId == product.Id && v.IsActive, ct)
                ?? throw new InvalidOperationException("Invalid serving size for this product");
        }

        // A product can map to more than one kitchen station now (ProductKitchenStations).
        // Prefer a mapping scoped to this order's location, then any location-agnostic
        // mapping, then the caller's hint / legacy single-station field.
        var stationCode = await db.ProductKitchenStations
            .Where(k => k.ProductId == product.Id && !k.IsDeleted &&
                        (k.LocationId == null || k.LocationId == order.LocationId))
            .OrderByDescending(k => k.LocationId != null)
            .Select(k => k.KitchenStation!.Code)
            .FirstOrDefaultAsync(ct);
        if (string.IsNullOrWhiteSpace(stationCode)) stationCode = product.KitchenStationCode;

        var qty = input.Quantity <= 0 ? 1 : input.Quantity;
        // Price source precedence: a chosen serving size wins; then a customer
        // contract price (#70); then the order's price-level override; else base.
        var basePrice = await ResolveBasePriceAsync(db, order, product.Id, product.BasePrice, variant, ct);
        var unitPrice = basePrice + modDelta;
        // Per-product auto discount (#3b): percent or fixed per unit, floored at the cost price
        // (can't sell below cost) unless an admin has enabled the below-cost override.
        var perUnitDiscount = product.DiscountType switch
        {
            "percent" when product.DiscountValue > 0 => Math.Round(unitPrice * product.DiscountValue / 100m, 4),
            "fixed"   when product.DiscountValue > 0 => product.DiscountValue,
            _ => 0m,
        };
        if (perUnitDiscount > 0)
        {
            var allowBelowCost = await db.OrgSettings.AsNoTracking().Select(o => o.AllowDiscountBelowCost).FirstOrDefaultAsync(ct);
            var floor = allowBelowCost ? 0m : product.CostPrice;
            perUnitDiscount = Math.Clamp(perUnitDiscount, 0m, Math.Max(0m, unitPrice - floor));
        }
        var lineDiscount = perUnitDiscount * qty;
        var grossLineAmount = unitPrice * qty - lineDiscount;

        // A product can carry several Tax-type charges (ChargeType.AppliesPerProduct).
        // Resolved fresh (not snapshotted on Product) so rate changes apply to new lines
        // immediately. Non-tax charge types (Service Charge, Levy, ...) are tenant-wide
        // and handled separately in RecalculateAsync, not here.
        var lineCharges = (product.IsTaxable && product.TaxClass == "standard")
            ? await db.ProductCharges.AsNoTracking()
                .Where(pc => pc.ProductId == product.Id && !pc.IsDeleted && pc.Charge!.IsActive)
                .Select(pc => new { pc.Charge!.Code, pc.Charge.Description, pc.Charge.Percentage, pc.Charge.Amount })
                .ToListAsync(ct)
            : [];

        // IsTaxInclusive means the listed price already contains the tax — back
        // it out instead of adding it on top, so LineTotal still equals the
        // listed price × qty. Otherwise (the default) charges add on top of it.
        decimal lineSubtotal;
        if (product.IsTaxInclusive && lineCharges.Count > 0)
        {
            var flatTotal = lineCharges.Where(c => !c.Percentage.HasValue).Sum(c => c.Amount ?? 0);
            var combinedRate = lineCharges.Sum(c => c.Percentage ?? 0);
            lineSubtotal = combinedRate > 0
                ? Math.Round((grossLineAmount - flatTotal) / (1 + combinedRate / 100m), 4)
                : grossLineAmount - flatTotal;
        }
        else
        {
            lineSubtotal = grossLineAmount;
        }

        decimal taxRate = 0, lineTax = 0;
        var chargeSnapshots = new List<OrderItemCharge>();
        foreach (var c in lineCharges)
        {
            var chargeAmount = c.Percentage.HasValue
                ? Math.Round(lineSubtotal * c.Percentage.Value / 100m, 4)
                : (c.Amount ?? 0);
            taxRate += c.Percentage ?? 0;
            lineTax += chargeAmount;
            chargeSnapshots.Add(new OrderItemCharge
            {
                TenantId = db.TenantId,
                Code = c.Code,
                Description = c.Description,
                Percentage = c.Percentage,
                Amount = c.Amount,
                BaseAmount = lineSubtotal,
                ChargeAmount = chargeAmount,
                CreatedAt = DateTime.UtcNow,
            });
        }

        // Stock guard: this order's committed amount for the product + this line must fit the
        // on-hand at the outlet (a serving size counts its ml). Stops overselling at the till.
        if (product.IsStocked)
        {
            var perStock = variant is { ServingQty: > 0 } ? variant.ServingQty : 1m;
            var (onHand, committed) = await StockCommitAsync(db, order, product.Id, null, ct);
            if (committed + qty * perStock > onHand)
                throw new InvalidOperationException(StockShortMessage(product.Name, onHand - committed, variant));
        }

        var newItem = new OrderItem
        {
            OrderId = order.Id,
            ProductId = product.Id,
            Sku = product.Sku,
            ProductName = product.Name,
            VariantId = variant?.Id,
            VariantName = variant?.Name,
            Quantity = qty,
            UnitPrice = unitPrice,
            DiscountAmount = lineDiscount,
            LineSubtotal = lineSubtotal,
            TaxRate = taxRate,
            TaxAmount = lineTax,
            LineTotal = lineSubtotal + lineTax,
            // Route to the product's configured station; fall back to the caller's
            // hint (POS category heuristic) then the default kitchen.
            Station = !string.IsNullOrWhiteSpace(stationCode) ? stationCode! : (input.Station ?? "kitchen"),
            Notes = input.Notes,
            IsStocked = product.IsStocked,
            IsTaxable = product.TaxClass == "standard" && product.IsTaxable,
            KotStatus = "pending",
            Modifiers = modifiers,
        };
        order.Items.Add(newItem);

        // Reference the entity object itself (not just its Id) so EF's change
        // tracker knows OrderItemCharge depends on this new OrderItem and orders
        // the inserts correctly within this one SaveChanges call.
        foreach (var cs in chargeSnapshots)
        {
            cs.OrderItem = newItem;
            db.OrderItemCharges.Add(cs);
        }

        await RecalculateAsync(db, order, ct);
        await db.SaveChangesAsync(ct);
        return order;
    }

    public async Task<Order> UpdateItemQtyAsync(Guid orderId, Guid itemId, decimal quantity, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId, ct)
            ?? throw new InvalidOperationException("Order not found");
        var item = order.Items.FirstOrDefault(i => i.Id == itemId && !i.IsDeleted)
            ?? throw new InvalidOperationException("Item not found");
        if (order.Status is "settled" or "void") throw new InvalidOperationException($"Cannot edit a {order.Status} order");

        if (quantity <= 0) { item.IsDeleted = true; }
        else
        {
            // Stock guard: the new quantity (plus the order's other lines of this product) must
            // still fit the on-hand at the outlet.
            var prod = await db.Products.AsNoTracking().Where(p => p.Id == item.ProductId)
                .Select(p => new { p.IsStocked, p.Name }).FirstOrDefaultAsync(ct);
            if (prod is { IsStocked: true })
            {
                var variant = item.VariantId is Guid vid
                    ? await db.ProductVariants.AsNoTracking().FirstOrDefaultAsync(v => v.Id == vid, ct) : null;
                var perStock = variant is { ServingQty: > 0 } ? variant.ServingQty : 1m;
                var (onHand, committed) = await StockCommitAsync(db, order, item.ProductId, item.Id, ct);
                if (committed + quantity * perStock > onHand)
                    throw new InvalidOperationException(StockShortMessage(prod.Name, onHand - committed, variant));
            }
            // Preserve the line's per-unit discount (#3b) when the quantity changes.
            var perUnitDiscount = item.Quantity > 0 ? item.DiscountAmount / item.Quantity : 0m;
            item.Quantity = quantity;
            item.DiscountAmount = Math.Round(perUnitDiscount * quantity, 4);
            item.LineSubtotal = item.UnitPrice * quantity - item.DiscountAmount;
            item.TaxAmount = Math.Round(item.LineSubtotal * item.TaxRate / 100m, 4);
            item.LineTotal = item.LineSubtotal + item.TaxAmount;
        }
        await RecalculateAsync(db, order, ct);
        await db.SaveChangesAsync(ct);
        return order;
    }

    public async Task<Order> RemoveItemAsync(Guid orderId, Guid itemId, CancellationToken ct)
        => await UpdateItemQtyAsync(orderId, itemId, 0, ct);

    /// <summary>The KOT state + current qty of a line — drives the "void after it's fired to the
    /// kitchen needs approval" gate (#71b). Null if the line isn't found.</summary>
    public async Task<(string KotStatus, decimal Quantity)?> GetItemKotAsync(Guid orderId, Guid itemId, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var it = await db.OrderItems.AsNoTracking()
            .Where(i => i.Id == itemId && i.OrderId == orderId && !i.IsDeleted)
            .Select(i => new { i.KotStatus, i.Quantity }).FirstOrDefaultAsync(ct);
        return it is null ? null : (it.KotStatus, it.Quantity);
    }

    /// <summary>On-hand for a product at the order's outlet, plus how much this order's OTHER open
    /// lines of that product already commit (qty × serving_qty). Used to stop overselling a stocked
    /// item at the till. excludeItemId lets a qty-change ignore the line being edited.</summary>
    private static async Task<(decimal onHand, decimal committed)> StockCommitAsync(
        TenantDbContext db, Order order, Guid productId, Guid? excludeItemId, CancellationToken ct)
    {
        var onHand = await db.ProductStocks.AsNoTracking()
            .Where(s => s.ProductId == productId && s.LocationId == order.LocationId)
            .Select(s => (decimal?)s.QuantityOnHand).FirstOrDefaultAsync(ct) ?? 0m;
        var lines = order.Items.Where(i => !i.IsDeleted && i.ProductId == productId && i.Id != excludeItemId).ToList();
        var vids = lines.Where(i => i.VariantId is Guid).Select(i => i.VariantId!.Value).Distinct().ToList();
        var serv = vids.Count == 0
            ? new Dictionary<Guid, decimal>()
            : await db.ProductVariants.AsNoTracking().Where(v => vids.Contains(v.Id)).ToDictionaryAsync(v => v.Id, v => v.ServingQty, ct);
        var committed = lines.Sum(i => i.Quantity * (i.VariantId is Guid vv && serv.TryGetValue(vv, out var s) && s > 0 ? s : 1m));
        return (onHand, committed);
    }

    /// <summary>Friendly "not enough stock" message; if a serving size is in play, also shows
    /// roughly how many of that size are left.</summary>
    private static string StockShortMessage(string name, decimal remaining, ProductVariant? variant)
    {
        remaining = Math.Max(0m, remaining);
        var msg = $"Not enough {name} in stock at this outlet — {remaining:0.##} left";
        if (variant is { ServingQty: > 0 })
            msg += $" (about {Math.Floor(remaining / variant.ServingQty):0} × {variant.Name})";
        return msg + ".";
    }

    /// <summary>
    /// Merge the source order's lines into the target order, then void the source
    /// (e.g. combine two tables onto one bill). Both must be open/confirmed and at
    /// the same outlet. Lines (and their modifiers) are copied onto the target.
    /// </summary>
    public async Task<Order> MergeAsync(Guid targetId, Guid sourceId, CancellationToken ct)
    {
        if (targetId == sourceId) throw new InvalidOperationException("Cannot merge an order into itself");
        await using var db = await factory.CreateForCurrentAsync(ct);
        var target = await db.Orders.Include(o => o.Items).ThenInclude(i => i.Modifiers).FirstOrDefaultAsync(o => o.Id == targetId, ct)
            ?? throw new InvalidOperationException("Target order not found");
        var source = await db.Orders.Include(o => o.Items).ThenInclude(i => i.Modifiers).FirstOrDefaultAsync(o => o.Id == sourceId, ct)
            ?? throw new InvalidOperationException("Source order not found");
        if (target.Status is "settled" or "void") throw new InvalidOperationException($"Cannot merge into a {target.Status} order");
        if (source.Status is "settled" or "void") throw new InvalidOperationException($"Cannot merge a {source.Status} order");
        if (target.LocationId != source.LocationId) throw new InvalidOperationException("Orders are at different outlets");

        foreach (var it in source.Items.Where(i => !i.IsDeleted))
        {
            target.Items.Add(new OrderItem
            {
                OrderId = target.Id, ProductId = it.ProductId, Sku = it.Sku, ProductName = it.ProductName,
                VariantId = it.VariantId, VariantName = it.VariantName, Quantity = it.Quantity, UnitPrice = it.UnitPrice,
                LineSubtotal = it.LineSubtotal, TaxRate = it.TaxRate, TaxAmount = it.TaxAmount, LineTotal = it.LineTotal,
                Station = it.Station, Notes = it.Notes, KotStatus = it.KotStatus, IsStocked = it.IsStocked, IsTaxable = it.IsTaxable,
                Modifiers = it.Modifiers.Where(m => !m.IsDeleted)
                    .Select(m => new OrderItemModifier { ModifierItemId = m.ModifierItemId, Name = m.Name, PriceDelta = m.PriceDelta }).ToList(),
            });
            it.IsDeleted = true;
        }
        source.Status = "void";
        source.VoidReason = $"Merged into {target.OrderNumber}";

        // The source's food is still being cooked — its kitchen tickets must follow the
        // items onto the target bill, not stay orphaned on a now-void order (which left
        // ghost tickets on the KOT board). Re-point active tickets to the target.
        var label = string.IsNullOrWhiteSpace(target.TableLabel) ? target.OrderNumber : target.TableLabel!;
        var srcTickets = await db.KitchenTickets.Where(t => t.OrderId == sourceId
            && (t.Status == "new" || t.Status == "preparing" || t.Status == "ready")).ToListAsync(ct);
        foreach (var t in srcTickets) { t.OrderId = target.Id; t.OrderLabel = label; }

        await RecalculateAsync(db, target, ct);
        await db.SaveChangesAsync(ct);
        return target;
    }

    /// <summary>
    /// Split selected item quantities off an open bill onto a new bill at the same
    /// table (item-level split, #69). Partial quantities are supported. Returns the
    /// new order; both bills are recalculated.
    /// </summary>
    public async Task<Order> SplitAsync(Guid orderId, IReadOnlyList<SplitMove> moves, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var source = await db.Orders.Include(o => o.Items).ThenInclude(i => i.Modifiers).FirstOrDefaultAsync(o => o.Id == orderId, ct)
            ?? throw new InvalidOperationException("Order not found");
        if (source.Status is "settled" or "void") throw new InvalidOperationException($"Cannot split a {source.Status} order");

        var dest = new Order
        {
            LocationId = source.LocationId, OrderType = source.OrderType, PriceLevelId = source.PriceLevelId,
            OrderSource = "pos", TableLabel = source.TableLabel, TableId = source.TableId, Covers = source.Covers,
            Status = "open", OrderNumber = await NextOrderNumberAsync(db, ct), OpenedAt = DateTime.UtcNow,
        };

        foreach (var mv in moves)
        {
            var it = source.Items.FirstOrDefault(i => i.Id == mv.ItemId && !i.IsDeleted);
            if (it is null) continue;
            var qty = mv.Quantity <= 0 ? it.Quantity : Math.Min(mv.Quantity, it.Quantity);
            if (qty <= 0) continue;
            var sub = it.UnitPrice * qty;
            var tax = Math.Round(sub * it.TaxRate / 100m, 4);
            dest.Items.Add(new OrderItem
            {
                ProductId = it.ProductId, Sku = it.Sku, ProductName = it.ProductName, VariantId = it.VariantId, VariantName = it.VariantName,
                Quantity = qty, UnitPrice = it.UnitPrice, LineSubtotal = sub, TaxRate = it.TaxRate, TaxAmount = tax, LineTotal = sub + tax,
                Station = it.Station, Notes = it.Notes, KotStatus = it.KotStatus, IsStocked = it.IsStocked, IsTaxable = it.IsTaxable,
                Modifiers = it.Modifiers.Where(m => !m.IsDeleted).Select(m => new OrderItemModifier { ModifierItemId = m.ModifierItemId, Name = m.Name, PriceDelta = m.PriceDelta }).ToList(),
            });
            if (qty >= it.Quantity) it.IsDeleted = true;
            else { it.Quantity -= qty; it.LineSubtotal = it.UnitPrice * it.Quantity; it.TaxAmount = Math.Round(it.LineSubtotal * it.TaxRate / 100m, 4); it.LineTotal = it.LineSubtotal + it.TaxAmount; }
        }
        if (!dest.Items.Any()) throw new InvalidOperationException("Select at least one item to split off");

        db.Orders.Add(dest);
        await RecalculateAsync(db, source, ct);
        await RecalculateAsync(db, dest, ct);
        await db.SaveChangesAsync(ct);
        return dest;
    }

    /// <summary>
    /// Set POS-depth metadata on an open/confirmed bill (#76): the head-count
    /// (covers), the steward it's attributed to, and the tour operator that
    /// brought the guests. Nulls leave the existing value untouched; pass
    /// Guid.Empty to clear the steward / operator.
    /// </summary>
    public async Task<Order> SetMetaAsync(Guid orderId, int? covers, Guid? stewardId, Guid? tourOperatorId, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == orderId, ct)
            ?? throw new InvalidOperationException("Order not found");
        if (order.Status is "settled" or "void") throw new InvalidOperationException($"Cannot edit a {order.Status} order");

        if (covers is int c) order.Covers = c <= 0 ? null : c;
        if (stewardId is Guid sid)
        {
            if (sid == Guid.Empty) order.StewardId = null;
            else
            {
                // A steward IS a user flagged is_server (#76) — not a separate master.
                if (!await db.Users.AnyAsync(u => u.Id == sid && u.IsServer && u.IsActive, ct))
                    throw new InvalidOperationException("Steward not found");
                order.StewardId = sid;
            }
        }
        if (tourOperatorId is Guid tid)
        {
            if (tid == Guid.Empty) order.TourOperatorId = null;
            else
            {
                if (!await db.TourOperators.AnyAsync(t => t.Id == tid && t.IsActive, ct))
                    throw new InvalidOperationException("Tour operator not found");
                order.TourOperatorId = tid;
            }
        }
        await db.SaveChangesAsync(ct);
        return order;
    }

    /// <summary>Set the discretionary tip on the bill (#76). Added on top of the total, not taxed.</summary>
    public async Task<Order> SetTipAsync(Guid orderId, decimal amount, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId, ct)
            ?? throw new InvalidOperationException("Order not found");
        if (order.Status is "settled" or "void") throw new InvalidOperationException($"Cannot tip a {order.Status} order");
        order.TipAmount = Math.Max(0m, amount);
        await RecalculateAsync(db, order, ct);
        await db.SaveChangesAsync(ct);
        return order;
    }

    /// <summary>Move an open order to a different table (same outlet, not already occupied).</summary>
    public async Task<Order> TransferTableAsync(Guid orderId, Guid tableId, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == orderId, ct)
            ?? throw new InvalidOperationException("Order not found");
        if (order.Status is "settled" or "void") throw new InvalidOperationException($"Cannot move a {order.Status} order");
        var table = await db.RestaurantTables.FirstOrDefaultAsync(t => t.Id == tableId, ct)
            ?? throw new InvalidOperationException("Table not found");
        if (table.LocationId != order.LocationId) throw new InvalidOperationException("Table is at a different outlet");
        var occupied = await db.Orders.AnyAsync(o => o.TableId == tableId && o.Id != orderId
            && (o.Status == "open" || o.Status == "confirmed"), ct);
        if (occupied) throw new InvalidOperationException($"Table {table.Code} is already occupied");
        order.TableId = tableId;
        order.TableLabel = table.Code;
        await db.SaveChangesAsync(ct);
        return order;
    }

    /// <summary>
    /// Decide which price level an order is priced at:
    /// explicit choice > a level that auto-applies to this order type > the default.
    /// Returns null when the tenant has no price levels (lines use base price).
    /// </summary>
    private static async Task<Guid?> ResolvePriceLevelAsync(TenantDbContext db, Guid? explicitId, string orderType, CancellationToken ct)
    {
        if (explicitId is Guid id)
        {
            var exists = await db.PriceLevels.AnyAsync(l => l.Id == id && l.IsActive, ct);
            if (exists) return id;
        }
        var byType = await db.PriceLevels.Where(l => l.IsActive && l.AppliesToOrderType == orderType)
            .OrderBy(l => l.SortOrder).Select(l => (Guid?)l.Id).FirstOrDefaultAsync(ct);
        if (byType is not null) return byType;
        return await db.PriceLevels.Where(l => l.IsActive && l.IsDefault)
            .OrderBy(l => l.SortOrder).Select(l => (Guid?)l.Id).FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Resolve the per-unit base price for a product on an order (before modifiers).
    /// Precedence: serving-size variant > customer contract price (#70) > the order's
    /// price-level override > the product base price.
    /// </summary>
    private static async Task<decimal> ResolveBasePriceAsync(
        TenantDbContext db, Order order, Guid productId, decimal productBasePrice, ProductVariant? variant, CancellationToken ct)
    {
        if (variant is not null) return variant.Price;
        if (order.CustomerId is Guid cid)
        {
            var cp = await ResolveCustomerPriceAsync(db, cid, productId, ct);
            if (cp is decimal x) return x;
        }
        if (order.PriceLevelId is Guid plid)
        {
            var ov = await db.ProductPrices.AsNoTracking()
                .FirstOrDefaultAsync(pp => pp.ProductId == productId && pp.PriceLevelId == plid, ct);
            return ov?.Price ?? productBasePrice;
        }
        return productBasePrice;
    }

    /// <summary>The highest active loyalty tier the lifetime-points total qualifies for (null = base).</summary>
    private static Task<LoyaltyTier?> ResolveTierAsync(TenantDbContext db, decimal lifetimePoints, CancellationToken ct) =>
        db.LoyaltyTiers.AsNoTracking().Where(t => t.IsActive && t.MinLifetimePoints <= lifetimePoints)
            .OrderByDescending(t => t.MinLifetimePoints).FirstOrDefaultAsync(ct);

    /// <summary>
    /// Expire a tracked customer's whole point balance if it's gone stale (no
    /// earn/redeem for the configured window). Inactivity model — no per-lot
    /// accounting. Records an "expire" ledger row. No-op when expiry is off.
    /// </summary>
    private static void ExpireIfStale(TenantDbContext db, Customer c, int expiryDays)
    {
        if (expiryDays <= 0 || c.LoyaltyPoints <= 0 || c.LoyaltyLastActivityAt is not DateTime last) return;
        if ((DateTime.UtcNow - last).TotalDays <= expiryDays) return;
        db.LoyaltyTransactions.Add(new LoyaltyTransaction
        {
            CustomerId = c.Id, TxnType = "expire", Points = -c.LoyaltyPoints, BalanceAfter = 0m,
            Note = $"Expired — {expiryDays} days inactive",
        });
        c.LoyaltyPoints = 0m;
    }

    /// <summary>A customer's contract price for a product (their own override, else their category's).</summary>
    private static async Task<decimal?> ResolveCustomerPriceAsync(TenantDbContext db, Guid customerId, Guid productId, CancellationToken ct)
    {
        var direct = await db.CustomerProductPrices.AsNoTracking()
            .Where(p => p.CustomerId == customerId && p.ProductId == productId)
            .Select(p => (decimal?)p.Price).FirstOrDefaultAsync(ct);
        if (direct is not null) return direct;

        var catId = await db.Customers.AsNoTracking().Where(c => c.Id == customerId)
            .Select(c => c.CategoryId).FirstOrDefaultAsync(ct);
        if (catId is Guid cid)
            return await db.CustomerProductPrices.AsNoTracking()
                .Where(p => p.CategoryId == cid && p.ProductId == productId)
                .Select(p => (decimal?)p.Price).FirstOrDefaultAsync(ct);
        return null;
    }

    /// <summary>
    /// Re-price every live line for the order's current customer context (used when a
    /// customer is attached/removed). Variant lines keep their size price; everything
    /// else re-resolves via the precedence, preserving each line's modifier delta.
    /// </summary>
    private static async Task RepriceLinesAsync(TenantDbContext db, Order order, CancellationToken ct)
    {
        foreach (var it in order.Items.Where(i => !i.IsDeleted))
        {
            var modDelta = it.Modifiers.Where(m => !m.IsDeleted).Sum(m => m.PriceDelta);
            decimal basePrice;
            if (it.VariantId is Guid vid)
                basePrice = await db.ProductVariants.AsNoTracking().Where(v => v.Id == vid)
                    .Select(v => v.Price).FirstOrDefaultAsync(ct);
            else
            {
                var prodBase = await db.Products.AsNoTracking().Where(p => p.Id == it.ProductId)
                    .Select(p => p.BasePrice).FirstOrDefaultAsync(ct);
                basePrice = await ResolveBasePriceAsync(db, order, it.ProductId, prodBase, null, ct);
            }
            it.UnitPrice = basePrice + modDelta;
            it.LineSubtotal = it.UnitPrice * it.Quantity;
            it.TaxAmount = Math.Round(it.LineSubtotal * it.TaxRate / 100m, 4);
            it.LineTotal = it.LineSubtotal + it.TaxAmount;
        }
    }

    /// <summary>
    /// Validate the selected modifier items against the product's attached groups
    /// (membership + each group's required/min/max) and return the snapshot lines
    /// plus the total price delta to fold into the unit price.
    /// </summary>
    private static async Task<(List<OrderItemModifier> mods, decimal delta)> ResolveModifiersAsync(
        TenantDbContext db, Guid productId, List<Guid>? selectedIds, CancellationToken ct)
    {
        var links = await db.ProductModifierGroups.AsNoTracking().Where(l => l.ProductId == productId).ToListAsync(ct);
        var groupIds = links.Select(l => l.GroupId).ToList();
        var groups = groupIds.Count == 0
            ? new List<ModifierGroup>()
            : await db.ModifierGroups.Include(g => g.Items).AsNoTracking()
                .Where(g => groupIds.Contains(g.Id) && g.IsActive).ToListAsync(ct);

        var selected = (selectedIds ?? new()).Distinct().ToList();
        var byItem = groups.SelectMany(g => g.Items.Where(i => !i.IsDeleted && i.IsActive).Select(i => (g, i)))
            .ToDictionary(x => x.i.Id, x => x);

        foreach (var id in selected)
            if (!byItem.ContainsKey(id)) throw new InvalidOperationException("Invalid modifier selection for this product");

        foreach (var g in groups)
        {
            var count = selected.Count(id => byItem[id].g.Id == g.Id);
            if (g.IsRequired && count == 0) throw new InvalidOperationException($"'{g.Name}' is required");
            if (count < g.MinSelect) throw new InvalidOperationException($"'{g.Name}' needs at least {g.MinSelect} option(s)");
            if (g.MaxSelect > 0 && count > g.MaxSelect) throw new InvalidOperationException($"'{g.Name}' allows at most {g.MaxSelect} option(s)");
        }

        var mods = new List<OrderItemModifier>();
        decimal delta = 0;
        foreach (var id in selected)
        {
            var item = byItem[id].i;
            delta += item.PriceDelta;
            mods.Add(new OrderItemModifier { ModifierItemId = item.Id, Name = item.Name, PriceDelta = item.PriceDelta });
        }
        return (mods, delta);
    }

    /// <summary>
    /// Add an OPEN / custom-priced line — a miscellaneous item where the cashier
    /// types the name and the price the customer pays (no catalogue product, no
    /// stock movement). Useful for one-off sales and price overrides.
    /// </summary>
    public async Task<Order> AddCustomItemAsync(Guid orderId, string name, decimal unitPrice, decimal quantity, string? station, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId, ct)
            ?? throw new InvalidOperationException("Order not found");
        if (order.Status is "settled" or "void")
            throw new InvalidOperationException($"Cannot add items to a {order.Status} order");
        if (string.IsNullOrWhiteSpace(name)) throw new InvalidOperationException("Item name is required");
        if (unitPrice < 0) throw new InvalidOperationException("Price cannot be negative");

        var qty = quantity <= 0 ? 1 : quantity;
        var lineSubtotal = unitPrice * qty;
        order.Items.Add(new OrderItem
        {
            OrderId = order.Id,
            ProductId = Guid.Empty,            // sentinel — not a catalogue product
            Sku = "OPEN",
            ProductName = name.Trim(),
            Quantity = qty,
            UnitPrice = unitPrice,
            LineSubtotal = lineSubtotal,
            TaxRate = 0,                        // bill-level charges still apply to subtotal
            TaxAmount = 0,
            LineTotal = lineSubtotal,
            Station = station ?? "kitchen",
            IsStocked = false,                  // no stock decrement for open items
            KotStatus = "pending",
        });
        await RecalculateAsync(db, order, ct);
        await db.SaveChangesAsync(ct);
        return order;
    }

    /// <summary>
    /// Apply a discount to the bill — either a fixed amount or a percentage of
    /// subtotal. Discount reduces the total after charges (v1: pre-tax discount
    /// on subtotal). Requires manager authority in production (RBAC later).
    /// </summary>
    public async Task<Order> SetDiscountAsync(Guid orderId, decimal? amount, decimal? percent, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId, ct)
            ?? throw new InvalidOperationException("Order not found");
        if (order.Status is "settled" or "void") throw new InvalidOperationException($"Cannot discount a {order.Status} order");

        var subtotal = order.Items.Where(i => !i.IsDeleted).Sum(i => i.LineSubtotal);
        order.DiscountAmount = percent.HasValue
            ? Math.Round(subtotal * percent.Value / 100m, 4)
            : Math.Max(0, amount ?? 0);
        await RecalculateAsync(db, order, ct);
        await db.SaveChangesAsync(ct);
        return order;
    }

    /// <summary>
    /// Attach a CRM customer to the bill: snapshots their name + tax no onto the
    /// order (for the invoice) and applies their default discount (own % overrides
    /// the category's). #70.
    /// </summary>
    public async Task<Order> AttachCustomerAsync(Guid orderId, Guid customerId, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var order = await db.Orders.Include(o => o.Items).ThenInclude(i => i.Modifiers)
            .FirstOrDefaultAsync(o => o.Id == orderId, ct)
            ?? throw new InvalidOperationException("Order not found");
        if (order.Status is "settled" or "void") throw new InvalidOperationException($"Cannot change the customer on a {order.Status} order");
        var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == customerId, ct)
            ?? throw new InvalidOperationException("Customer not found");

        order.CustomerId = customer.Id;
        order.CustomerName = customer.Name;
        order.CustomerVatNo = customer.TaxNo;

        // Re-price existing lines to the customer's contract prices (#70), then the
        // customer/category % discount applies on top of the re-priced subtotal.
        await RepriceLinesAsync(db, order, ct);
        var pct = customer.DiscountPercent;
        if (pct is null && customer.CategoryId is Guid catId)
            pct = (await db.CustomerCategories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == catId, ct))?.DiscountPercent;
        // Loyalty tier discount (#66) competes with the customer/category discount — best wins.
        var tierPct = (await ResolveTierAsync(db, customer.LoyaltyLifetimePoints, ct))?.DiscountPercent ?? 0m;
        var effPct = Math.Max(pct ?? 0m, tierPct);
        var subtotal = order.Items.Where(i => !i.IsDeleted).Sum(i => i.LineSubtotal);
        order.DiscountAmount = effPct > 0 ? Math.Round(subtotal * effPct / 100m, 4) : 0m;

        await RecalculateAsync(db, order, ct);
        await db.SaveChangesAsync(ct);
        return order;
    }

    /// <summary>Remove the CRM customer from the bill (reverts contract prices + the auto discount).</summary>
    public async Task<Order> DetachCustomerAsync(Guid orderId, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var order = await db.Orders.Include(o => o.Items).ThenInclude(i => i.Modifiers)
            .FirstOrDefaultAsync(o => o.Id == orderId, ct)
            ?? throw new InvalidOperationException("Order not found");
        if (order.Status is "settled" or "void") throw new InvalidOperationException($"Cannot change the customer on a {order.Status} order");
        order.CustomerId = null; order.CustomerName = null; order.CustomerVatNo = null;
        order.DiscountAmount = 0m;                 // drop the customer's auto discount
        await RepriceLinesAsync(db, order, ct);    // back to base / price-level
        await RecalculateAsync(db, order, ct);
        await db.SaveChangesAsync(ct);
        return order;
    }

    /// <summary>
    /// R2 — KOT/BOT generation. When an order is confirmed ("sent to kitchen"),
    /// group its pending items by station and emit one kitchen_ticket per station.
    /// Items are marked sent. In the legacy this was the Trg_GenProductionNotes
    /// trigger on PaymentDets; here it's an explicit, testable step.
    /// </summary>
    public async Task<Order> ConfirmAsync(Guid orderId, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId, ct)
            ?? throw new InvalidOperationException("Order not found");
        if (order.Status is "settled" or "void")
            throw new InvalidOperationException($"Cannot confirm a {order.Status} order");

        var pending = order.Items.Where(i => i.KotStatus == "pending").ToList();
        if (pending.Count == 0)
            throw new InvalidOperationException("No new items to send to the kitchen");

        var label = order.OrderSource switch
        {
            "ubereats" => "Uber Eats",
            "pickme"   => "PickMe",
            _ => order.OrderType == "takeaway" ? "Takeaway"
               : order.TableLabel is { } t ? $"Table {t}" : $"Order {order.OrderNumber}",
        };

        // #4b multi-kitchen: an item routes to EVERY station mapped to its product
        // (product_kitchen_stations, optionally scoped to this order's location —
        // a location-specific mapping wins over a location-wide default for that
        // product). Empty ⇒ fall back to the line's single Station. So one item
        // can land on several stations' tickets at once.
        var prodIds = pending.Select(i => i.ProductId).Distinct().ToList();
        var stationRows = await db.ProductKitchenStations.AsNoTracking()
            .Where(pk => prodIds.Contains(pk.ProductId) && !pk.IsDeleted
                && (pk.LocationId == null || pk.LocationId == order.LocationId))
            .Select(pk => new { pk.ProductId, pk.LocationId, Code = pk.KitchenStation!.Code })
            .ToListAsync(ct);
        var prodStations = stationRows
            .GroupBy(x => x.ProductId)
            .ToDictionary(
                g => g.Key,
                g => (g.Any(x => x.LocationId != null) ? g.Where(x => x.LocationId != null) : g)
                    .Select(x => x.Code).ToList());

        // station code → the items that print there
        var byStation = new Dictionary<string, List<OrderItem>>();
        foreach (var item in pending)
        {
            var stations = (prodStations.TryGetValue(item.ProductId, out var codes) && codes is { Count: > 0 })
                ? codes.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList()
                : new List<string> { item.Station };
            if (stations.Count == 0) stations.Add(item.Station);
            foreach (var st in stations)
                (byStation.TryGetValue(st, out var list) ? list : byStation[st] = new List<OrderItem>()).Add(item);
        }

        foreach (var (station, stationItems) in byStation)
        {
            var ticketNo = await NextTicketNumberAsync(db, ct);
            var items = stationItems.Select(i => new
            {
                i.Quantity,
                ProductName = i.VariantName is { } vn ? $"{i.ProductName} ({vn})" : i.ProductName,
                i.Notes,
            }).ToList();

            db.KitchenTickets.Add(new KitchenTicket
            {
                LocationId = order.LocationId,
                OrderId = order.Id,
                TicketNumber = ticketNo,
                Station = station,
                OrderLabel = label,
                OrderSource = order.OrderSource,
                Status = "new",
                ItemsJson = JsonSerializer.Serialize(items),
            });
        }

        foreach (var item in pending) item.KotStatus = "sent";

        if (order.Status == "open") { order.Status = "confirmed"; order.ConfirmedAt = DateTime.UtcNow; }
        order.PendingAcceptance = false;   // accepted (or a POS confirm, where this was already false)
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return order;
    }

    /// <summary>
    /// R1 — Stock decrement on settlement. Records payment(s), and for every
    /// stocked line decrements product_stock.quantity_on_hand at the order's
    /// location. All within one transaction so payment + stock move atomically
    /// (fixing the legacy footgun where the trigger could leave them inconsistent).
    /// </summary>
    public async Task<Order> SettleAsync(Guid orderId, SettleInput input, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId, ct)
            ?? throw new InvalidOperationException("Order not found");
        if (order.Status == "settled") throw new InvalidOperationException("Order already settled");
        if (order.Status == "void") throw new InvalidOperationException("Cannot settle a void order");
        await PeriodGuard.EnsureOpenAsync(db, DateTime.UtcNow, ct);   // #77: don't post sales into a closed period

        // Cash control: a bill can't be settled without an open shift for the outlet
        // (so the sale lands in a cash session + Z-report). Toggleable per tenant.
        var cashRules = await GetSettingsAsync(db, ct);
        if (cashRules.RequireOpenShiftForBilling
            && !await db.Shifts.AnyAsync(s => s.LocationId == order.LocationId && s.Status == "open", ct))
            throw new InvalidOperationException("Open a shift before taking payment");

        // Optional one-step tip on settle (#76) — otherwise set earlier via /tip.
        if (input.Tip is decimal tipAmt) order.TipAmount = Math.Max(0m, tipAmt);

        await RecalculateAsync(db, order, ct, persistCharges: true);

        // Multi-currency tender (#76): a payment may be in a foreign currency; its
        // base-currency value is amount × rate. Blank/base currency uses rate 1.
        // Sufficiency and reporting are always in the base currency.
        var baseCurrency = cashRules.BaseCurrency;
        var currencies = await db.Currencies.AsNoTracking().Where(c => c.IsActive).ToListAsync(ct);
        var tenders = input.Payments.Select(p =>
        {
            var code = string.IsNullOrWhiteSpace(p.CurrencyCode) ? baseCurrency : p.CurrencyCode!.Trim().ToUpperInvariant();
            if (string.Equals(code, baseCurrency, StringComparison.OrdinalIgnoreCase))
                return (p, code: baseCurrency, rate: 1m, baseAmt: Math.Round(p.Amount, 4));
            var cur = currencies.FirstOrDefault(c => string.Equals(c.Code, code, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"Currency {code} is not configured");
            return (p, code, rate: cur.RateToBase, baseAmt: Math.Round(p.Amount * cur.RateToBase, 4));
        }).ToList();

        var paid = tenders.Sum(t => t.baseAmt);
        if (Math.Round(paid, 2) < Math.Round(order.TotalAmount, 2))
            throw new InvalidOperationException(
                $"Insufficient payment: {paid:F2} {baseCurrency} tendered, {order.TotalAmount:F2} due");

        foreach (var t in tenders)
            db.Payments.Add(new Payment
            {
                OrderId = order.Id, PayType = t.p.PayType, Amount = t.p.Amount, Reference = t.p.Reference,
                CurrencyCode = t.code, FxRate = t.rate, BaseAmount = t.baseAmt,
            });

        // Credit ("charge to account") tender: the customer doesn't pay now — it
        // adds to their AR balance, bounded by their credit limit.
        var creditAmt = input.Payments.Where(p => p.PayType == "credit").Sum(p => p.Amount);
        if (creditAmt > 0.009m)
        {
            if (order.CustomerId is not Guid custId)
                throw new InvalidOperationException("Attach a credit customer before charging to account");
            var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == custId, ct)
                ?? throw new InvalidOperationException("Customer not found");
            if (!customer.IsCreditCustomer)
                throw new InvalidOperationException($"{customer.Name} is not a credit customer");
            var available = customer.CreditLimit - customer.CurrentBalance;
            if (creditAmt > available + 0.009m)
                throw new InvalidOperationException($"Credit limit exceeded — {available:F2} available, {creditAmt:F2} on account");
            customer.CurrentBalance = Math.Round(customer.CurrentBalance + creditAmt, 4);
        }

        // Loyalty redemption (#66): a "loyalty" tender spends points worth
        // redeem_value LKR each, bounded by the customer's balance.
        var loyaltyAmt = input.Payments.Where(p => p.PayType == "loyalty").Sum(p => p.Amount);
        if (loyaltyAmt > 0.009m)
        {
            if (!cashRules.LoyaltyEnabled || cashRules.LoyaltyRedeemValue <= 0)
                throw new InvalidOperationException("Loyalty redemption isn't enabled");
            if (order.CustomerId is not Guid lcid)
                throw new InvalidOperationException("Attach the loyalty customer before redeeming points");
            var lc = await db.Customers.FirstOrDefaultAsync(c => c.Id == lcid, ct)
                ?? throw new InvalidOperationException("Customer not found");
            ExpireIfStale(db, lc, cashRules.LoyaltyExpiryDays);   // stale points lapse before redemption
            var pointsNeeded = Math.Round(loyaltyAmt / cashRules.LoyaltyRedeemValue, 4);
            if (pointsNeeded > lc.LoyaltyPoints + 0.009m)
            {
                var maxRedeemable = Math.Floor(lc.LoyaltyPoints * cashRules.LoyaltyRedeemValue * 100m) / 100m;
                throw new InvalidOperationException($"These points only cover {maxRedeemable:N2} — reduce the points and take the rest on cash/card ({lc.LoyaltyPoints:F0} pts available).");
            }
            lc.LoyaltyPoints = Math.Round(lc.LoyaltyPoints - pointsNeeded, 4);
            lc.LoyaltyLastActivityAt = DateTime.UtcNow;
            db.LoyaltyTransactions.Add(new LoyaltyTransaction
            {
                CustomerId = lc.Id, OrderId = order.Id, TxnType = "redeem",
                Points = -pointsNeeded, BalanceAfter = lc.LoyaltyPoints, Note = $"Redeemed on {order.OrderNumber}",
            });
        }

        // Advance / deposit (#70): draw the bill against the customer's prepaid balance.
        var advanceAmt = input.Payments.Where(p => p.PayType == "advance").Sum(p => p.Amount);
        if (advanceAmt > 0.009m)
        {
            if (order.CustomerId is not Guid avid)
                throw new InvalidOperationException("Attach the customer before drawing on their advance");
            var av = await db.Customers.FirstOrDefaultAsync(c => c.Id == avid, ct)
                ?? throw new InvalidOperationException("Customer not found");
            if (advanceAmt > av.AdvanceBalance + 0.009m)
                throw new InvalidOperationException($"Not enough advance — {av.AdvanceBalance:F2} available, {advanceAmt:F2} requested");
            av.AdvanceBalance = Math.Round(av.AdvanceBalance - advanceAmt, 4);
        }

        // R1: decrement stock for every stocked line at this location. A serving-size variant
        // consumes serving_qty stock units per unit sold (e.g. a 50ml pour of a bottle stocked
        // in ml → 50), so a 100ml pour depletes twice a 50ml one. No variant ⇒ 1 per unit.
        var variantIds = order.Items.Where(i => i.VariantId != null).Select(i => i.VariantId!.Value).Distinct().ToList();
        var servingByVariant = variantIds.Count == 0
            ? new Dictionary<Guid, decimal>()
            : await db.ProductVariants.IgnoreQueryFilters().Where(v => variantIds.Contains(v.Id))
                .ToDictionaryAsync(v => v.Id, v => v.ServingQty, ct);
        // Hard backstop: never let settlement drive a stocked item negative (oversell).
        foreach (var grp in order.Items.Where(i => i.IsStocked).GroupBy(i => i.ProductId))
        {
            var need = grp.Sum(i => i.Quantity * (i.VariantId is Guid vid && servingByVariant.TryGetValue(vid, out var sq) && sq > 0 ? sq : 1m));
            var oh = await db.ProductStocks.AsNoTracking()
                .Where(s => s.ProductId == grp.Key && s.LocationId == order.LocationId)
                .Select(s => (decimal?)s.QuantityOnHand).FirstOrDefaultAsync(ct);
            if (oh is decimal h && need > h)
                throw new InvalidOperationException($"Can’t settle — not enough {grp.First().ProductName} in stock ({h:0.##} left, this bill needs {need:0.##}). Reduce the quantity or top up stock first.");
        }
        foreach (var item in order.Items.Where(i => i.IsStocked))
        {
            var stock = await db.ProductStocks.FirstOrDefaultAsync(
                s => s.ProductId == item.ProductId && s.LocationId == order.LocationId, ct);
            if (stock is null) continue;
            var per = (item.VariantId is Guid vid && servingByVariant.TryGetValue(vid, out var sq) && sq > 0) ? sq : 1m;
            stock.QuantityOnHand -= item.Quantity * per;
        }

        // Assign a compliant, gap-free VAT invoice number.
        var settings = await GetSettingsAsync(db, ct);
        if (settings.VatEnabled)
        {
            order.InvoiceNumber = await NextInvoiceNumberAsync(db, settings, ct);
            order.IsTaxInvoice = true;
            order.CustomerVatNo = input.CustomerVatNo;
            order.CustomerName = input.CustomerName;
        }

        order.Status = "settled";
        order.SettledAt = DateTime.UtcNow;

        // Tour-operator commission (#76): booked off the net (ex-tip) bill value
        // as a payable to the operator for the commission report. Doesn't change
        // what the guest paid.
        if (order.TourOperatorId is Guid topId)
        {
            var op = await db.TourOperators.FirstOrDefaultAsync(t => t.Id == topId, ct);
            order.TourCommissionAmount = op is null ? 0m
                : Math.Round((order.TotalAmount - order.TipAmount) * op.CommissionPercent / 100m, 4);
        }

        // Earn loyalty points on the settled bill (#66) — on the net sale, ex-tip.
        if (cashRules.LoyaltyEnabled && cashRules.LoyaltyEarnRate > 0 && order.CustomerId is Guid ecid)
        {
            var ec = await db.Customers.FirstOrDefaultAsync(c => c.Id == ecid, ct);
            // Credit / on-account customers (corporate houses) don't accrue personal
            // loyalty points — loyalty is for retail guests paying their own bill.
            if (ec is not null && !ec.IsCreditCustomer)
            {
                ExpireIfStale(db, ec, cashRules.LoyaltyExpiryDays);    // lapse stale points before earning new ones
                var tier = await ResolveTierAsync(db, ec.LoyaltyLifetimePoints, ct);   // tier from lifetime points
                var mult = tier?.EarnMultiplier ?? 1m;
                var earned = Math.Floor((order.TotalAmount - order.TipAmount) * cashRules.LoyaltyEarnRate * mult);
                if (earned > 0)
                {
                    ec.LoyaltyPoints = Math.Round(ec.LoyaltyPoints + earned, 4);
                    ec.LoyaltyLifetimePoints = Math.Round(ec.LoyaltyLifetimePoints + earned, 4);
                    ec.LoyaltyLastActivityAt = DateTime.UtcNow;
                    db.LoyaltyTransactions.Add(new LoyaltyTransaction
                    {
                        CustomerId = ec.Id, OrderId = order.Id, TxnType = "earn",
                        Points = earned, BalanceAfter = ec.LoyaltyPoints,
                        Note = tier is null ? $"Earned on {order.InvoiceNumber ?? order.OrderNumber}"
                                            : $"Earned on {order.InvoiceNumber ?? order.OrderNumber} (×{mult:0.##} {tier.Name})",
                    });
                }
            }
        }
        // NB: settlement deliberately does NOT touch this order's kitchen tickets.
        // Payment and the KDS are independent — the line still has to cook the food
        // and bump the ticket (new → preparing → ready → served). The board shows a
        // "PAID" marker so the kitchen knows the bill's closed, but clearing it stays
        // a kitchen action. (Voiding *does* cancel tickets — that food isn't coming.)
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return order;
    }

    public async Task<Order> VoidAsync(Guid orderId, string? reason, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == orderId, ct)
            ?? throw new InvalidOperationException("Order not found");
        if (order.Status == "settled") throw new InvalidOperationException("Cannot void a settled order (issue a refund instead)");
        order.Status = "void";
        order.VoidedAt = DateTime.UtcNow;
        order.VoidReason = reason;

        // Pull the order's tickets off the kitchen board (they're no longer being made).
        var tickets = await db.KitchenTickets.Where(t => t.OrderId == orderId
            && (t.Status == "new" || t.Status == "preparing" || t.Status == "ready")).ToListAsync(ct);
        foreach (var t in tickets) t.Status = "cancelled";

        await db.SaveChangesAsync(ct);
        return order;
    }

    // ---- helpers ----

    /// <summary>
    /// Recompute totals. Per-product tax (a product can carry several Tax-type
    /// charges) is already baked into each item's TaxAmount when the line was
    /// added — order.TaxAmount here is just their sum. Bill-level charges (Service
    /// Charge, Levy, ...) are tenant-wide: every active one applies to every order,
    /// computed independently off the plain subtotal and summed (no stacking order,
    /// no compounding, no order-type filtering). When persistCharges=true, writes
    /// the order_charges breakdown for the invoice + VAT return (clears any prior
    /// rows first).
    /// </summary>
    private static async Task RecalculateAsync(TenantDbContext db, Order order, CancellationToken ct, bool persistCharges = false)
    {
        var items = order.Items.Where(i => !i.IsDeleted).ToList();
        var subtotal = items.Sum(i => i.LineSubtotal);
        order.SubtotalAmount = subtotal;

        var billLevelCharges = await db.Charges.AsNoTracking()
            .Include(c => c.ChargeType)
            .Where(c => c.IsActive && !c.ChargeType!.AppliesPerProduct)
            .ToListAsync(ct);

        // Per-location VAT exemption (e.g. duty-free): drop VAT/tax-labelled bill-level
        // charges, keep the rest (service charge, levy, ...).
        var vatExempt = await db.Locations.AsNoTracking()
            .Where(l => l.Id == order.LocationId).Select(l => l.VatExempt).FirstOrDefaultAsync(ct);
        if (vatExempt)
            billLevelCharges = billLevelCharges
                .Where(c => c.ChargeType!.Code.IndexOf("vat", StringComparison.OrdinalIgnoreCase) < 0
                         && c.ChargeType.Code.IndexOf("tax", StringComparison.OrdinalIgnoreCase) < 0)
                .ToList();

        decimal serviceCharge = 0;
        var breakdown = new List<OrderCharge>();

        foreach (var c in billLevelCharges)
        {
            var amt = c.Percentage.HasValue
                ? Math.Round(subtotal * c.Percentage.Value / 100m, 4)
                : (c.Amount ?? 0);
            serviceCharge += amt;

            breakdown.Add(new OrderCharge
            {
                TenantId = db.TenantId, OrderId = order.Id,
                Code = c.Code, Name = c.Description, ChargeType = c.ChargeType!.Code,
                RatePercent = c.Percentage ?? 0, BaseAmount = subtotal, ChargeAmount = amt,
                CreatedAt = DateTime.UtcNow,
            });
        }

        // Auto-apply promotions (#65) — netted off the bill like a manual discount.
        var (promoDiscount, appliedPromos) = await Hms.Api.Features.Promotions.PromotionEngine.EvaluateAsync(db, order, ct);
        order.PromotionDiscountAmount = promoDiscount;

        var taxTotal = items.Sum(i => i.TaxAmount);

        order.ServiceChargeAmount = serviceCharge;
        order.TaxAmount = taxTotal;
        // Tip (#76) is added on top of the bill (not taxed, not discounted).
        order.TotalAmount = subtotal + serviceCharge + taxTotal - order.DiscountAmount - order.PromotionDiscountAmount + order.TipAmount;

        if (persistCharges)
        {
            var existing = db.OrderCharges.Where(oc => oc.OrderId == order.Id);
            db.OrderCharges.RemoveRange(existing);
            db.OrderCharges.AddRange(breakdown);

            db.OrderPromotions.RemoveRange(db.OrderPromotions.Where(op => op.OrderId == order.Id));
            foreach (var ap in appliedPromos) { ap.TenantId = db.TenantId; ap.CreatedAt = DateTime.UtcNow; }
            db.OrderPromotions.AddRange(appliedPromos);
        }
    }

    private static async Task<OrgSettings> GetSettingsAsync(TenantDbContext db, CancellationToken ct)
    {
        var s = await db.OrgSettings.FirstOrDefaultAsync(ct);
        if (s is null)
        {
            s = new OrgSettings { TenantId = db.TenantId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            db.OrgSettings.Add(s);
            await db.SaveChangesAsync(ct);
        }
        return s;
    }

    private static async Task<string> NextInvoiceNumberAsync(TenantDbContext db, OrgSettings settings, CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var rows = await db.Database.SqlQuery<int>($@"
            INSERT INTO invoice_series (tenant_id, series_year, last_value)
            VALUES ({db.TenantId}, {year}, 1)
            ON CONFLICT (tenant_id, series_year)
            DO UPDATE SET last_value = invoice_series.last_value + 1
            RETURNING last_value AS ""Value""").ToListAsync(ct);
        return $"{settings.InvoicePrefix}/{year}/{rows[0]:D6}";
    }

    /// <summary>
    /// Atomic per-tenant daily counter via INSERT … ON CONFLICT … RETURNING.
    /// Concurrency-safe (no find-or-create race) and avoids the duplicate-key
    /// problem of read-then-write. Order and ticket numbers share the counter
    /// space, so they interleave — which is fine.
    /// </summary>
    /// <remarks>
    /// Settled, in-period orders only. Returns headline KPIs, a per-day series, a
    /// per-source split (POS vs Uber Eats vs PickMe), and the top-selling items by
    /// revenue. Item revenue uses the line total (incl. line VAT), matching the bill.
    /// </remarks>
    /// <summary>
    /// Normalise a date to UTC for Npgsql (timestamptz rejects Unspecified-kind
    /// values). Query-string dates like "2026-06-01" bind as Kind=Unspecified;
    /// without this every report endpoint 500s the moment the client passes from/to.
    /// </summary>
    public static DateTime AsUtc(DateTime d) => d.Kind switch
    {
        DateTimeKind.Utc => d,
        DateTimeKind.Local => d.ToUniversalTime(),
        _ => DateTime.SpecifyKind(d, DateTimeKind.Utc),
    };

    private static DateTime MonthStartUtc() =>
        new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

    public async Task<SalesSummary> SalesSummaryAsync(DateTime? from, DateTime? to, Guid? locationId, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var fromD = from is { } f ? AsUtc(f) : MonthStartUtc();
        var toD = to is { } t ? AsUtc(t).AddDays(1) : fromD.AddMonths(1);   // "to" is a calendar day; make it inclusive

        var settled = db.Orders.AsNoTracking()
            .Where(o => o.Status == "settled" && o.SettledAt >= fromD && o.SettledAt < toD);
        if (locationId.HasValue) settled = settled.Where(o => o.LocationId == locationId.Value);

        var rows = await settled.Select(o => new
        {
            o.Id, o.OrderSource, o.TotalAmount, o.SubtotalAmount,
            o.DiscountAmount, o.ServiceChargeAmount, o.TaxAmount,
            Day = o.SettledAt!.Value.Date,
        }).ToListAsync(ct);

        var byDay = rows.GroupBy(r => r.Day).OrderBy(g => g.Key)
            .Select(g => new SalesByDay(DateOnly.FromDateTime(g.Key), g.Count(), g.Sum(x => x.TotalAmount)))
            .ToList();

        var bySource = rows.GroupBy(r => r.OrderSource).OrderByDescending(g => g.Sum(x => x.TotalAmount))
            .Select(g => new SalesBySource(g.Key, g.Count(), g.Sum(x => x.TotalAmount)))
            .ToList();

        var ids = rows.Select(r => r.Id).ToList();
        var topItems = new List<TopItem>();
        if (ids.Count > 0)
        {
            var agg = (await db.OrderItems.AsNoTracking()
                .Where(i => !i.IsDeleted && ids.Contains(i.OrderId))
                .GroupBy(i => new { i.ProductId, i.ProductName })
                .Select(g => new { g.Key.ProductId, g.Key.ProductName, Qty = g.Sum(x => x.Quantity), Rev = g.Sum(x => x.LineTotal) })
                .ToListAsync(ct))
              .OrderByDescending(t => t.Rev).Take(20).ToList();
            var pids = agg.Select(a => a.ProductId).ToList();
            var skus = await db.Products.AsNoTracking().Where(p => pids.Contains(p.Id))
                .Select(p => new { p.Id, p.Sku }).ToDictionaryAsync(p => p.Id, p => p.Sku, ct);
            topItems = agg.Select(a => new TopItem(a.ProductId, skus.GetValueOrDefault(a.ProductId), a.ProductName, a.Qty, a.Rev)).ToList();
        }

        return new SalesSummary(
            fromD, toD,
            rows.Count,
            rows.Sum(r => r.TotalAmount),
            rows.Sum(r => r.SubtotalAmount),
            rows.Sum(r => r.DiscountAmount),
            rows.Sum(r => r.ServiceChargeAmount),
            rows.Sum(r => r.TaxAmount),
            byDay, bySource, topItems);
    }

    /// <summary>
    /// Consolidated HQ view across all outlets for a period: per-outlet settled
    /// sales (orders/gross/net/tax) and current stock value (Σ qty × avg cost),
    /// plus group totals. The owner's "how is each branch doing" screen.
    /// </summary>
    public async Task<OutletReport> OutletSummaryAsync(DateTime? from, DateTime? to, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var fromD = from is { } f ? AsUtc(f) : MonthStartUtc();
        var toD = to is { } t ? AsUtc(t).AddDays(1) : fromD.AddMonths(1);   // "to" is a calendar day; make it inclusive

        var locs = await db.Locations.AsNoTracking().Where(l => !l.IsDeleted)
            .Select(l => new { l.Id, l.Code, l.Name }).ToListAsync(ct);

        var sales = await db.Orders.AsNoTracking()
            .Where(o => o.Status == "settled" && o.SettledAt >= fromD && o.SettledAt < toD)
            .GroupBy(o => o.LocationId)
            .Select(g => new
            {
                LocationId = g.Key,
                Orders = g.Count(),
                Gross = g.Sum(x => x.TotalAmount),
                Net = g.Sum(x => x.SubtotalAmount),
                Tax = g.Sum(x => x.TaxAmount),
            }).ToListAsync(ct);

        var stock = await db.ProductStocks.AsNoTracking()
            .GroupBy(s => s.LocationId)
            .Select(g => new { LocationId = g.Key, Value = g.Sum(x => x.QuantityOnHand * x.AverageCost) })
            .ToListAsync(ct);

        var rows = locs.Select(l =>
        {
            var s = sales.FirstOrDefault(x => x.LocationId == l.Id);
            var sv = stock.FirstOrDefault(x => x.LocationId == l.Id);
            return new OutletRow(l.Id, l.Code, l.Name,
                s?.Orders ?? 0, s?.Gross ?? 0m, s?.Net ?? 0m, s?.Tax ?? 0m, sv?.Value ?? 0m);
        }).OrderByDescending(r => r.GrossSales).ThenBy(r => r.Code).ToList();

        return new OutletReport(fromD, toD, rows,
            rows.Sum(r => r.OrderCount), rows.Sum(r => r.GrossSales), rows.Sum(r => r.NetSales),
            rows.Sum(r => r.Tax), rows.Sum(r => r.StockValue));
    }

    /// <summary>
    /// Per-steward performance over a period (#76): bills, covers served, gross
    /// sales (ex-tip) and tips collected — the basis for a tip-payout report.
    /// Unattributed bills roll up under a synthetic "(unassigned)" row.
    /// </summary>
    public async Task<List<StewardSalesRow>> StewardSalesAsync(DateTime? from, DateTime? to, Guid? locationId, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var fromD = from is { } f ? AsUtc(f) : MonthStartUtc();
        var toD = to is { } t ? AsUtc(t).AddDays(1) : fromD.AddMonths(1);   // "to" is a calendar day; make it inclusive

        var settled = db.Orders.AsNoTracking()
            .Where(o => o.Status == "settled" && o.SettledAt >= fromD && o.SettledAt < toD);
        if (locationId.HasValue) settled = settled.Where(o => o.LocationId == locationId.Value);

        var rows = await settled.Select(o => new { o.StewardId, o.Covers, o.TotalAmount, o.TipAmount }).ToListAsync(ct);
        // Steward names come from users flagged is_server (#76).
        var names = await db.Users.AsNoTracking().Select(u => new { u.Id, u.DisplayName }).ToListAsync(ct);
        var byId = names.ToDictionary(s => s.Id, s => s.DisplayName);

        return rows.GroupBy(r => r.StewardId)
            .Select(g => new StewardSalesRow(
                g.Key,
                g.Key is Guid sid && byId.TryGetValue(sid, out var n) ? n : "(unassigned)",
                g.Count(),
                g.Sum(x => x.Covers ?? 0),
                g.Sum(x => x.TotalAmount - x.TipAmount),
                g.Sum(x => x.TipAmount)))
            .OrderByDescending(r => r.GrossSales).ToList();
    }

    /// <summary>
    /// Per-tour-operator commission over a period (#76): bills, gross (ex-tip) and
    /// commission booked — what the venue owes each operator.
    /// </summary>
    public async Task<List<TourCommissionRow>> TourCommissionAsync(DateTime? from, DateTime? to, Guid? locationId, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var fromD = from is { } f ? AsUtc(f) : MonthStartUtc();
        var toD = to is { } t ? AsUtc(t).AddDays(1) : fromD.AddMonths(1);   // "to" is a calendar day; make it inclusive

        var settled = db.Orders.AsNoTracking()
            .Where(o => o.Status == "settled" && o.TourOperatorId != null
                && o.SettledAt >= fromD && o.SettledAt < toD);
        if (locationId.HasValue) settled = settled.Where(o => o.LocationId == locationId.Value);

        var rows = await settled.Select(o => new { o.TourOperatorId, o.TotalAmount, o.TipAmount, o.TourCommissionAmount }).ToListAsync(ct);
        var ops = await db.TourOperators.AsNoTracking().Select(t => new { t.Id, t.Code, t.Name }).ToListAsync(ct);
        var byId = ops.ToDictionary(o => o.Id, o => o);

        return rows.GroupBy(r => r.TourOperatorId!.Value)
            .Select(g =>
            {
                byId.TryGetValue(g.Key, out var op);
                return new TourCommissionRow(g.Key, op?.Code ?? "?", op?.Name ?? "(deleted)",
                    g.Count(), g.Sum(x => x.TotalAmount - x.TipAmount), g.Sum(x => x.TourCommissionAmount));
            })
            .OrderByDescending(r => r.Commission).ToList();
    }

    private static async Task<int> NextCounterAsync(TenantDbContext db, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var rows = await db.Database
            .SqlQuery<int>($@"
                INSERT INTO order_counters (tenant_id, counter_date, last_value)
                VALUES ({db.TenantId}, {today}, 1)
                ON CONFLICT (tenant_id, counter_date)
                DO UPDATE SET last_value = order_counters.last_value + 1
                RETURNING last_value AS ""Value""")
            .ToListAsync(ct);
        return rows[0];
    }

    // Bill/KOT numbers reset daily (a fresh #0001 each service day) but are
    // date-stamped so they stay unique across days (the counter is keyed by date).
    private static async Task<string> NextOrderNumberAsync(TenantDbContext db, CancellationToken ct)
    {
        var prefix = (await db.OrgSettings.AsNoTracking().FirstOrDefaultAsync(ct))?.OrderPrefix ?? "ORD";
        var day = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyMMdd");
        return $"{prefix}-{day}-{await NextCounterAsync(db, ct):D4}";
    }

    private static async Task<string> NextTicketNumberAsync(TenantDbContext db, CancellationToken ct)
    {
        var day = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyMMdd");
        return $"KOT-{day}-{await NextCounterAsync(db, ct):D4}";
    }
}

/// <summary>Per-tenant daily counter, keyed (tenant_id, counter_date) via RLS.</summary>
public class OrderCounter
{
    public Guid TenantId { get; set; }
    public DateTime CounterDate { get; set; }
    public int LastValue { get; set; }
}

public record CreateOrderInput(Guid LocationId, string? OrderType, string? OrderSource,
    string? ExternalOrderId, string? TableLabel, int? Covers, Guid? CashierId,
    string? CustomerName = null, string? DeliveryAddress = null, string? DeliveryPhone = null,
    string? DeliveryNotes = null, string? AggregatorPayload = null, Guid? PriceLevelId = null,
    Guid? TableId = null, Guid? StewardId = null, Guid? TourOperatorId = null, bool PendingAcceptance = false);
public record AddItemInput(Guid ProductId, decimal Quantity, string? Station, string? Notes, List<Guid>? ModifierItemIds = null, Guid? VariantId = null);
public record SettleInput(List<PaymentInput> Payments, string? CustomerName = null, string? CustomerVatNo = null, decimal? Tip = null);
// CurrencyCode null/blank ⇒ base currency (#76). Amount is in that currency.
public record PaymentInput(string PayType, decimal Amount, string? Reference, string? CurrencyCode = null);
public record SplitMove(Guid ItemId, decimal Quantity);

// ── Sales report DTOs ──
public record SalesSummary(
    DateTime PeriodFrom, DateTime PeriodTo, int OrderCount,
    decimal GrossSales, decimal NetSales, decimal Discount, decimal ServiceCharge, decimal Tax,
    List<SalesByDay> ByDay, List<SalesBySource> BySource, List<TopItem> TopItems);
public record SalesByDay(DateOnly Day, int OrderCount, decimal Total);
public record SalesBySource(string Source, int OrderCount, decimal Total);
public record TopItem(Guid ProductId, string? Sku, string ProductName, decimal Quantity, decimal Revenue);

// ── HQ / multi-outlet report DTOs ──
public record OutletReport(
    DateTime PeriodFrom, DateTime PeriodTo, List<OutletRow> Outlets,
    int TotalOrders, decimal TotalGross, decimal TotalNet, decimal TotalTax, decimal TotalStockValue);
public record OutletRow(Guid LocationId, string Code, string Name,
    int OrderCount, decimal GrossSales, decimal NetSales, decimal Tax, decimal StockValue);

// ── POS-depth report DTOs (#76) ──
public record StewardSalesRow(Guid? StewardId, string Name, int OrderCount, int Covers, decimal GrossSales, decimal Tips);
public record TourCommissionRow(Guid TourOperatorId, string Code, string Name, int OrderCount, decimal GrossSales, decimal Commission);
