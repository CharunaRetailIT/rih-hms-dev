using Hms.Api.Domain;
using Hms.Api.Features.Inventory;
using Hms.Api.Features.Procurement;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Replenishment;

/// <summary>
/// Inventory replenishment (#replenishment). A location is "below par" for a product
/// when on-hand ≤ its effective reorder level; the worksheet suggests bringing it back
/// up to the effective par. Effective levels resolve per-location override → product
/// default. The suggested top-up is split: take what a nominated hub (central kitchen /
/// warehouse) can spare via transfer, buy the rest from the preferred supplier via PO.
/// Generation produces DRAFTS only — the user reviews and sends (PO send still routes
/// through the approval engine).
/// </summary>
public class ReplenishmentService(ITenantDbContextFactory factory, ProcurementService procurement, InventoryMovementService inventory)
{
    /// <summary>One computed worksheet line for a below-par product at a location.</summary>
    private record Line(Guid ProductId, string Sku, string Name, decimal UnitCost, decimal OnHand,
        decimal Reorder, decimal Par, decimal Need, decimal TransferQty, decimal PoQty,
        Guid? HubLocationId, string? HubName, decimal HubOnHand, Guid? SupplierId, string? SupplierName);

    private async Task<(Location loc, Location? hub, List<Line> lines)> ComputeAsync(TenantDbContext db, Guid locationId, CancellationToken ct)
    {
        var loc = await db.Locations.AsNoTracking().FirstOrDefaultAsync(l => l.Id == locationId, ct)
            ?? throw new InvalidOperationException("Location not found");
        Location? hub = loc.ReplenishSourceLocationId is Guid hid
            ? await db.Locations.AsNoTracking().FirstOrDefaultAsync(l => l.Id == hid, ct) : null;

        var products = await db.Products.AsNoTracking().Where(p => p.IsActive && p.IsStocked).ToListAsync(ct);
        var overrides = (await db.ProductReplenishmentLevels.AsNoTracking().Where(r => r.LocationId == locationId).ToListAsync(ct))
            .ToDictionary(r => r.ProductId, r => r);
        var here = (await db.ProductStocks.AsNoTracking().Where(s => s.LocationId == locationId).ToListAsync(ct))
            .ToDictionary(s => s.ProductId, s => s.QuantityOnHand);
        var atHub = hub is null ? new Dictionary<Guid, decimal>()
            : (await db.ProductStocks.AsNoTracking().Where(s => s.LocationId == hub.Id).ToListAsync(ct))
                .ToDictionary(s => s.ProductId, s => s.QuantityOnHand);
        var suppliers = await db.Suppliers.AsNoTracking().ToDictionaryAsync(s => s.Id, s => s.Name, ct);

        var lines = new List<Line>();
        foreach (var p in products)
        {
            var ov = overrides.GetValueOrDefault(p.Id);
            var reorder = ov?.ReorderLevel ?? p.ReorderLevel ?? 0m;
            if (reorder <= 0) continue;                                   // not monitored
            var onHand = here.GetValueOrDefault(p.Id, 0m);
            if (onHand > reorder) continue;                              // above trigger → fine
            var par = ov?.ParLevel ?? p.ParLevel ?? reorder;            // order-up-to (default to reorder)
            if (par < reorder) par = reorder;
            var need = par - onHand;
            if (need <= 0) continue;
            var hubOnHand = hub is null ? 0m : atHub.GetValueOrDefault(p.Id, 0m);
            var transferQty = hub is not null && hubOnHand > 0 ? Math.Min(need, hubOnHand) : 0m;
            var poQty = need - transferQty;
            var supplierId = ov?.PreferredSupplierId ?? p.PreferredSupplierId;
            lines.Add(new Line(p.Id, p.Sku, p.Name, p.CostPrice, onHand, reorder, par, need,
                transferQty, poQty, hub?.Id, hub?.Name, hubOnHand,
                supplierId, supplierId is Guid sid ? suppliers.GetValueOrDefault(sid) : null));
        }
        return (loc, hub, lines.OrderByDescending(l => l.Need).ToList());
    }

    /// <summary>The replenishment worksheet for a location: below-par items + the transfer/PO split.</summary>
    public async Task<object> WorksheetAsync(Guid locationId, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var (loc, hub, lines) = await ComputeAsync(db, locationId, ct);
        return new
        {
            location = new { loc.Id, loc.Name },
            hub = hub is null ? null : new { hub.Id, hub.Name },
            rows = lines.Select(l => new
            {
                l.ProductId, l.Sku, l.Name, unitCost = l.UnitCost, onHand = l.OnHand,
                reorder = l.Reorder, par = l.Par, need = l.Need,
                transferQty = l.TransferQty, poQty = l.PoQty, hubOnHand = l.HubOnHand,
                supplierId = l.SupplierId, supplierName = l.SupplierName, hasSupplier = l.SupplierId is not null,
            }),
            summary = new
            {
                items = lines.Count,
                transferLines = lines.Count(l => l.TransferQty > 0),
                poLines = lines.Count(l => l.PoQty > 0),
                poValue = lines.Sum(l => l.PoQty * l.UnitCost),
                missingSupplier = lines.Count(l => l.PoQty > 0 && l.SupplierId is null),
                hasHub = hub is not null,
            },
        };
    }

    /// <summary>Turn the worksheet into draft transfers (from the hub) and/or draft POs (by supplier).</summary>
    public async Task<object> GenerateAsync(Guid locationId, bool transfers, bool pos, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var (loc, hub, lines) = await ComputeAsync(db, locationId, ct);

        var madeTransfers = new List<string>();
        var madePos = new List<object>();
        var skipped = new List<object>();

        if (transfers && hub is not null)
        {
            var tl = lines.Where(l => l.TransferQty > 0)
                .Select(l => new TransferLineInput(l.ProductId, l.TransferQty)).ToList();
            if (tl.Count > 0)
            {
                var t = await inventory.CreateTransferAsync(
                    new CreateTransferInput(hub.Id, loc.Id, false, "Auto-replenishment", tl), ct);
                madeTransfers.Add(t.TransferNumber);
            }
        }

        if (pos)
        {
            var poLines = lines.Where(l => l.PoQty > 0).ToList();
            foreach (var l in poLines.Where(l => l.SupplierId is null))
                skipped.Add(new { l.Sku, l.Name, reason = "No preferred supplier — set one to auto-PO" });
            foreach (var grp in poLines.Where(l => l.SupplierId is not null).GroupBy(l => l.SupplierId!.Value))
            {
                var input = new CreatePoInput(loc.Id, grp.Key, null, "Auto-replenishment",
                    grp.Select(l => new PoLineInput(l.ProductId, l.PoQty, l.UnitCost)).ToList());
                var po = await procurement.CreatePoAsync(input, ct);
                madePos.Add(new { supplier = grp.First().SupplierName, number = po.PoNumber });
            }
        }

        return new { transfers = madeTransfers, purchaseOrders = madePos, skipped };
    }

    /// <summary>All stocked products with their effective + override levels for a location (config grid).</summary>
    public async Task<object> ListLevelsAsync(Guid locationId, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var loc = await db.Locations.AsNoTracking().FirstOrDefaultAsync(l => l.Id == locationId, ct)
            ?? throw new InvalidOperationException("Location not found");
        var products = await db.Products.AsNoTracking().Where(p => p.IsActive && p.IsStocked).OrderBy(p => p.Name).ToListAsync(ct);
        var overrides = (await db.ProductReplenishmentLevels.AsNoTracking().Where(r => r.LocationId == locationId).ToListAsync(ct))
            .ToDictionary(r => r.ProductId, r => r);
        var here = (await db.ProductStocks.AsNoTracking().Where(s => s.LocationId == locationId).ToListAsync(ct))
            .ToDictionary(s => s.ProductId, s => s.QuantityOnHand);
        return new
        {
            location = new { loc.Id, loc.Name },
            products = products.Select(p =>
            {
                var ov = overrides.GetValueOrDefault(p.Id);
                return new
                {
                    p.Id, p.Sku, p.Name,
                    onHand = here.GetValueOrDefault(p.Id, 0m),
                    productReorder = p.ReorderLevel, productPar = p.ParLevel,
                    overrideReorder = ov?.ReorderLevel, overridePar = ov?.ParLevel,
                    overrideSupplierId = ov?.PreferredSupplierId,
                    effectiveReorder = ov?.ReorderLevel ?? p.ReorderLevel ?? 0m,
                    effectivePar = ov?.ParLevel ?? p.ParLevel ?? (ov?.ReorderLevel ?? p.ReorderLevel ?? 0m),
                };
            }),
        };
    }

    /// <summary>Upsert per-location level overrides. A row with all-null fields clears the override (inherit default).</summary>
    public async Task SetLevelsAsync(Guid locationId, List<SetLevelInput> rows, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var ids = rows.Select(r => r.ProductId).ToList();
        var existing = (await db.ProductReplenishmentLevels.Where(r => r.LocationId == locationId && ids.Contains(r.ProductId)).ToListAsync(ct))
            .ToDictionary(r => r.ProductId, r => r);
        foreach (var r in rows)
        {
            var cleared = r.ReorderLevel is null && r.ParLevel is null && r.PreferredSupplierId is null;
            if (existing.TryGetValue(r.ProductId, out var row))
            {
                if (cleared) { db.ProductReplenishmentLevels.Remove(row); continue; }
                row.ReorderLevel = r.ReorderLevel; row.ParLevel = r.ParLevel; row.PreferredSupplierId = r.PreferredSupplierId;
            }
            else if (!cleared)
            {
                db.ProductReplenishmentLevels.Add(new ProductReplenishmentLevel
                {
                    ProductId = r.ProductId, LocationId = locationId,
                    ReorderLevel = r.ReorderLevel, ParLevel = r.ParLevel, PreferredSupplierId = r.PreferredSupplierId,
                });
            }
        }
        await db.SaveChangesAsync(ct);
    }
}

public record SetLevelInput(Guid ProductId, decimal? ReorderLevel, decimal? ParLevel, Guid? PreferredSupplierId);
