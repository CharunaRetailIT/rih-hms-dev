using Hms.Api.Features.Orders;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Reports;

/// <summary>
/// Operational reports library (#72). Read-only period queries on settled data:
/// sales register (transaction list), item sales, stock balance (as-at), shift
/// settlement, and promotion usage. KPIs/per-day/top-items already live under
/// /reports/sales/summary; the VAT return + HQ outlet rollup are separate.
/// All dates are coerced to UTC (timestamptz) via OrderService.AsUtc.
/// </summary>
public static class ReportsEndpoint
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/reports").WithTags("Reports").RequireAuthorization("BackOffice");

        // ── Sales register — every settled bill in the period + tender + totals ──
        // Server-paginated: totals/tenders are computed over the WHOLE period, only the
        // `orders` list itself is a page at a time (this can run to thousands of bills
        // over a wide date range).
        g.MapGet("/sales/register", async (
            ITenantDbContextFactory f, CancellationToken ct, DateTime? from, DateTime? to, Guid? locationId,
            int pageNumber = 1, int pageSize = 25) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            await using var db = await f.CreateForCurrentAsync(ct);
            var (fromD, toD) = DayRange(from, to);
            var settled = Settled(db, fromD, toD, locationId);

            var totalCount = await settled.CountAsync(ct);
            var totalsRow = totalCount == 0 ? null : await settled
                .GroupBy(o => 1)
                .Select(grp => new
                {
                    subtotal = grp.Sum(o => o.SubtotalAmount),
                    discount = grp.Sum(o => o.DiscountAmount + o.PromotionDiscountAmount),
                    serviceCharge = grp.Sum(o => o.ServiceChargeAmount),
                    tax = grp.Sum(o => o.TaxAmount),
                    total = grp.Sum(o => o.TotalAmount),
                }).FirstOrDefaultAsync(ct);

            var orders = await settled.OrderBy(o => o.SettledAt)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(o => new
                {
                    o.Id, o.SettledAt, number = o.InvoiceNumber ?? o.OrderNumber,
                    o.OrderType, o.OrderSource, o.TableLabel, o.CustomerName,
                    o.SubtotalAmount, o.DiscountAmount, o.PromotionDiscountAmount, o.ServiceChargeAmount, o.TaxAmount, o.TotalAmount,
                }).ToListAsync(ct);

            var ids = orders.Select(o => o.Id).ToList();
            var tenders = ids.Count == 0 ? new List<TenderRow>() : await db.Payments.AsNoTracking()
                .Where(p => ids.Contains(p.OrderId))
                .GroupBy(p => p.PayType)
                .Select(grp => new TenderRow(grp.Key, grp.Sum(x => x.Amount)))
                .ToListAsync(ct);

            return Results.Ok(new
            {
                periodFrom = fromD, periodTo = toD, count = totalCount,
                totals = new
                {
                    subtotal = totalsRow?.subtotal ?? 0m,
                    discount = totalsRow?.discount ?? 0m,
                    serviceCharge = totalsRow?.serviceCharge ?? 0m,
                    tax = totalsRow?.tax ?? 0m,
                    total = totalsRow?.total ?? 0m,
                },
                tenders, orders,
                pagination = new { totalCount, pageNumber, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)) },
            });
        }).WithName("Reports.SalesRegister").WithSummary("Server-paginated transaction-level list of settled bills + tender + period totals.");

        // ── Daily sales summary — one row per Date × Outlet ─────────────────────
        g.MapGet("/sales/daily-summary", async (ITenantDbContextFactory f, CancellationToken ct, DateTime? from, DateTime? to, Guid? locationId) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var (fromD, toD) = DayRange(from, to, defaultDays: 30);
            var raw = await Settled(db, fromD, toD, locationId).Select(o => new
            {
                o.LocationId, Day = o.SettledAt!.Value.Date,
                o.TotalAmount, o.SubtotalAmount, o.DiscountAmount, o.PromotionDiscountAmount, o.ServiceChargeAmount, o.TaxAmount,
            }).ToListAsync(ct);

            var locs = await db.Locations.AsNoTracking().Select(l => new { l.Id, l.Code, l.Name }).ToDictionaryAsync(l => l.Id, ct);

            var rows = raw.GroupBy(r => new { r.LocationId, r.Day })
                .Select(g =>
                {
                    locs.TryGetValue(g.Key.LocationId, out var l);
                    return new DailySalesRow(
                        DateOnly.FromDateTime(g.Key.Day), g.Key.LocationId,
                        l?.Code ?? "—", l?.Name ?? "(deleted)",
                        g.Count(), g.Sum(x => x.TotalAmount), g.Sum(x => x.DiscountAmount + x.PromotionDiscountAmount),
                        g.Sum(x => x.ServiceChargeAmount), g.Sum(x => x.TaxAmount), g.Sum(x => x.SubtotalAmount));
                })
                .OrderBy(r => r.Date).ThenBy(r => r.LocationCode)
                .ToList();

            return Results.Ok(new
            {
                periodFrom = fromD, periodTo = toD, rows,
                totals = new
                {
                    receipts = rows.Sum(r => r.Receipts), gross = rows.Sum(r => r.Gross),
                    discount = rows.Sum(r => r.Discount), serviceCharge = rows.Sum(r => r.ServiceCharge),
                    tax = rows.Sum(r => r.Tax), net = rows.Sum(r => r.Net),
                },
            });
        }).WithName("Reports.DailySalesSummary").WithSummary("Receipts + gross/discount/service charge/tax/net, one row per day x outlet.");

        // ── Daily sales summary detail — the individual receipts behind each Daily Summary row ──
        // Server-paginated for the same reason as the register: this can be one row per
        // settled bill across the whole period. Totals are aggregated in SQL over the
        // whole period; only `rows` is a page at a time.
        g.MapGet("/sales/daily-summary/detail", async (
            ITenantDbContextFactory f, CancellationToken ct, DateTime? from, DateTime? to, Guid? locationId,
            int pageNumber = 1, int pageSize = 25) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            await using var db = await f.CreateForCurrentAsync(ct);
            var (fromD, toD) = DayRange(from, to, defaultDays: 30);
            var settled = Settled(db, fromD, toD, locationId);

            var totalCount = await settled.CountAsync(ct);
            var totalsRow = totalCount == 0 ? null : await settled
                .GroupBy(o => 1)
                .Select(grp => new
                {
                    gross = grp.Sum(o => o.TotalAmount),
                    discount = grp.Sum(o => o.DiscountAmount + o.PromotionDiscountAmount),
                    serviceCharge = grp.Sum(o => o.ServiceChargeAmount),
                    tax = grp.Sum(o => o.TaxAmount),
                    net = grp.Sum(o => o.SubtotalAmount),
                }).FirstOrDefaultAsync(ct);

            var orders = await settled.OrderBy(o => o.SettledAt)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(o => new
                {
                    o.Id, o.LocationId, o.SettledAt, number = o.InvoiceNumber ?? o.OrderNumber,
                    o.OrderType, o.TableLabel, o.CustomerName,
                    o.SubtotalAmount, o.DiscountAmount, o.PromotionDiscountAmount, o.ServiceChargeAmount, o.TaxAmount, o.TotalAmount,
                }).ToListAsync(ct);

            var locs = await db.Locations.AsNoTracking().Select(l => new { l.Id, l.Code, l.Name }).ToDictionaryAsync(l => l.Id, ct);

            var rows = orders.Select(o =>
            {
                locs.TryGetValue(o.LocationId, out var l);
                return new DailySalesDetailRow(
                    o.Id, DateOnly.FromDateTime(o.SettledAt!.Value.Date), o.SettledAt.Value,
                    o.LocationId, l?.Code ?? "—", l?.Name ?? "(deleted)",
                    o.number, o.OrderType, o.TableLabel, o.CustomerName,
                    o.TotalAmount, o.DiscountAmount + o.PromotionDiscountAmount, o.ServiceChargeAmount, o.TaxAmount, o.SubtotalAmount);
            }).ToList();

            return Results.Ok(new
            {
                periodFrom = fromD, periodTo = toD, rows,
                totals = new
                {
                    receipts = totalCount, gross = totalsRow?.gross ?? 0m,
                    discount = totalsRow?.discount ?? 0m, serviceCharge = totalsRow?.serviceCharge ?? 0m,
                    tax = totalsRow?.tax ?? 0m, net = totalsRow?.net ?? 0m,
                },
                pagination = new { totalCount, pageNumber, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)) },
            });
        }).WithName("Reports.DailySalesSummaryDetail").WithSummary("Server-paginated per-receipt detail behind the Daily Sales Summary.");

        // ── Item sales — quantity + revenue per product over the period ──────────
        g.MapGet("/sales/items", async (ITenantDbContextFactory f, CancellationToken ct, DateTime? from, DateTime? to, Guid? locationId) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var (fromD, toD) = DayRange(from, to);
            var ids = await Settled(db, fromD, toD, locationId).Select(o => o.Id).ToListAsync(ct);
            var rows = ids.Count == 0 ? new List<ItemRow>() : (await db.OrderItems.AsNoTracking()
                .Where(i => !i.IsDeleted && ids.Contains(i.OrderId))
                .GroupBy(i => new { i.ProductId, i.Sku, i.ProductName })
                .Select(grp => new ItemRow(grp.Key.ProductId, grp.Key.Sku, grp.Key.ProductName,
                    grp.Sum(x => x.Quantity), grp.Sum(x => x.LineSubtotal), grp.Sum(x => x.TaxAmount)))
                .ToListAsync(ct)).OrderByDescending(r => r.Revenue).ToList();
            return Results.Ok(new { periodFrom = fromD, periodTo = toD, items = rows, totalQty = rows.Sum(r => r.Quantity), totalRevenue = rows.Sum(r => r.Revenue) });
        }).WithName("Reports.ItemSales").WithSummary("Units sold + revenue per product (item usage / best-sellers).");

        // ── Sales by category — item sales rolled up by menu category (not the
        // operational "Department" concept — Category is the Beverages/Mains/Desserts taxonomy).
        g.MapGet("/sales/by-category", async (
            ITenantDbContextFactory f, CancellationToken ct, DateTime? from, DateTime? to, Guid? locationId,
            int pageNumber = 1, int pageSize = 25) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            await using var db = await f.CreateForCurrentAsync(ct);
            var (fromD, toD) = DayRange(from, to);
            var ids = await Settled(db, fromD, toD, locationId).Select(o => o.Id).ToListAsync(ct);
            var items = ids.Count == 0 ? new List<(Guid ProductId, decimal Quantity, decimal LineSubtotal, decimal TaxAmount)>()
                : (await db.OrderItems.AsNoTracking()
                    .Where(i => !i.IsDeleted && ids.Contains(i.OrderId))
                    .Select(i => new { i.ProductId, i.Quantity, i.LineSubtotal, i.TaxAmount })
                    .ToListAsync(ct))
                    .Select(i => (i.ProductId, i.Quantity, i.LineSubtotal, i.TaxAmount)).ToList();

            var prodCategory = await db.Products.AsNoTracking().Select(p => new { p.Id, p.CategoryId }).ToDictionaryAsync(p => p.Id, p => p.CategoryId, ct);
            var categories = await db.Categories.AsNoTracking().Select(c => new { c.Id, c.Code, c.Name }).ToDictionaryAsync(c => c.Id, ct);

            var all = items.GroupBy(i => prodCategory.GetValueOrDefault(i.ProductId))
                .Select(grp =>
                {
                    categories.TryGetValue(grp.Key ?? Guid.Empty, out var cat);
                    return new
                    {
                        categoryId = grp.Key,
                        categoryCode = cat?.Code ?? "—",
                        categoryName = cat?.Name ?? "(uncategorized)",
                        quantity = grp.Sum(x => x.Quantity),
                        revenue = grp.Sum(x => x.LineSubtotal),
                        tax = grp.Sum(x => x.TaxAmount),
                    };
                })
                .OrderByDescending(r => r.revenue)
                .ToList();

            var totalCount = all.Count;
            var rows = all.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return Results.Ok(new
            {
                periodFrom = fromD, periodTo = toD, rows, totalQty = all.Sum(r => r.quantity), totalRevenue = all.Sum(r => r.revenue),
                pagination = new { totalCount, pageNumber, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)) },
            });
        }).WithName("Reports.SalesByCategory").WithSummary("Server-paginated units sold + revenue rolled up by menu category over a period.");

        // ── Stock balance (as-at now) — on-hand qty + value per product/outlet ───
        // Server-paginated: `lines` can be one row per stocked-product x outlet, which
        // grows with the catalog. `totalValue` is summed over the WHOLE filtered set.
        g.MapGet("/stock/balance", async (
            ITenantDbContextFactory f, CancellationToken ct, Guid? locationId,
            int pageNumber = 1, int pageSize = 25) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            await using var db = await f.CreateForCurrentAsync(ct);

            var stocksQ = db.ProductStocks.AsNoTracking().AsQueryable();
            if (locationId.HasValue) stocksQ = stocksQ.Where(s => s.LocationId == locationId.Value);

            var query =
                from s in stocksQ
                join p in db.Products.AsNoTracking().Where(p => p.IsStocked) on s.ProductId equals p.Id
                join l in db.Locations.AsNoTracking() on s.LocationId equals l.Id
                select new
                {
                    productId = s.ProductId, sku = p.Sku, name = p.Name, location = l.Code,
                    onHand = s.QuantityOnHand, avgCost = s.AverageCost,
                    value = Math.Round(s.QuantityOnHand * s.AverageCost, 2),
                };

            var totalCount = await query.CountAsync(ct);
            var totalValue = totalCount == 0 ? 0m : await query.SumAsync(x => x.value, ct);
            var rows = await query.OrderByDescending(x => x.value)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            return Results.Ok(new
            {
                asAt = DateTime.UtcNow, lines = rows, totalValue,
                pagination = new { totalCount, pageNumber, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)) },
            });
        }).WithName("Reports.StockBalance").WithSummary("Server-paginated current on-hand quantity + stock value per product / outlet.");

        // ── Wastage — server-paginated, one row per wasted product line. "approved" is the
        // workflow's terminal, stock-affecting status (ApplyWastageToStockAsync runs right
        // before it's set); the old "posted" value is legacy-only and never assigned by the
        // current draft→submit→approve flow, so it isn't a valid filter here.
        g.MapGet("/wastage", async (
            ITenantDbContextFactory f, CancellationToken ct,
            int pageNumber = 1, int pageSize = 10, DateTime? from = null, DateTime? to = null, Guid? locationId = null, string? reason = null) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            await using var db = await f.CreateForCurrentAsync(ct);
            var (fromD, toD) = DayRange(from, to, defaultDays: 30);

            var notes = db.WastageNotes.AsNoTracking()
                .Where(w => w.Status == "approved" && w.ApprovedAt != null && w.ApprovedAt >= fromD && w.ApprovedAt < toD);
            if (locationId.HasValue) notes = notes.Where(w => w.LocationId == locationId.Value);
            if (!string.IsNullOrWhiteSpace(reason)) notes = notes.Where(w => w.Reason == reason);

            var query =
                from l in db.WastageLines.AsNoTracking()
                join w in notes on l.WastageId equals w.Id
                select new { w.Id, w.WastageNumber, w.LocationId, w.Reason, w.ApprovedAt, l.Sku, l.ProductName, l.Quantity, l.UnitCost, l.LineTotal };

            var totalCount = await query.CountAsync(ct);
            var totalCost = totalCount == 0 ? 0m : await query.SumAsync(x => x.LineTotal, ct);

            var page = await query.OrderByDescending(x => x.ApprovedAt)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            var locs = await db.Locations.AsNoTracking().Select(l => new { l.Id, l.Code, l.Name }).ToDictionaryAsync(l => l.Id, ct);

            return Results.Ok(new
            {
                data = page.Select(x =>
                {
                    locs.TryGetValue(x.LocationId, out var l);
                    return new
                    {
                        wastageNumber = x.WastageNumber, approvedAt = x.ApprovedAt, reason = x.Reason,
                        locationCode = l?.Code ?? "—", locationName = l?.Name ?? "(deleted)",
                        sku = x.Sku, productName = x.ProductName, quantity = x.Quantity, unitCost = x.UnitCost, lineTotal = x.LineTotal,
                    };
                }),
                pagination = new { totalCount, pageNumber, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)) },
                totals = new { cost = totalCost },
            });
        }).WithName("Reports.Wastage").WithSummary("Server-paginated wastage line detail (approved/stock-posted only), by period/outlet/reason.");

        // ── Purchases by supplier — server-paginated rollup of approved GRNs (same
        // status/date basis as the VAT return: approved + PostedAt in period).
        g.MapGet("/purchases/by-supplier", async (
            ITenantDbContextFactory f, CancellationToken ct,
            int pageNumber = 1, int pageSize = 10, DateTime? from = null, DateTime? to = null, Guid? locationId = null) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            await using var db = await f.CreateForCurrentAsync(ct);
            var (fromD, toD) = DayRange(from, to, defaultDays: 30);

            var grns = db.GoodsReceivedNotes.AsNoTracking()
                .Where(x => x.Status == "approved" && x.PostedAt != null && x.PostedAt >= fromD && x.PostedAt < toD);
            if (locationId.HasValue) grns = grns.Where(x => x.LocationId == locationId.Value);

            var grouped = grns.GroupBy(x => x.SupplierId)
                .Select(grp => new
                {
                    SupplierId = grp.Key,
                    GrnCount = grp.Count(),
                    Purchases = grp.Sum(x => x.TotalCost),          // ex-VAT (matches the VAT return)
                    Tax = grp.Sum(x => x.TaxAmount),
                    Discount = grp.Sum(x => x.DiscountAmount),
                    OtherCharges = grp.Sum(x => x.OtherCharges),
                });

            var totalCount = await grouped.CountAsync(ct);
            var totals = await grns.GroupBy(_ => 1).Select(grp => new
            {
                GrnCount = grp.Count(),
                Purchases = grp.Sum(x => x.TotalCost),
                Tax = grp.Sum(x => x.TaxAmount),
                Discount = grp.Sum(x => x.DiscountAmount),
                OtherCharges = grp.Sum(x => x.OtherCharges),
            }).FirstOrDefaultAsync(ct);

            var page = await grouped.OrderByDescending(x => x.Purchases)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            var supIds = page.Select(x => x.SupplierId).ToList();
            var sups = await db.Suppliers.AsNoTracking().IgnoreQueryFilters()
                .Where(s => supIds.Contains(s.Id))
                .Select(s => new { s.Id, s.Code, s.Name }).ToDictionaryAsync(s => s.Id, ct);

            return Results.Ok(new
            {
                data = page.Select(x =>
                {
                    sups.TryGetValue(x.SupplierId, out var s);
                    return new
                    {
                        supplierId = x.SupplierId, supplierCode = s?.Code ?? "—", supplierName = s?.Name ?? "(deleted)",
                        grnCount = x.GrnCount, purchases = x.Purchases, tax = x.Tax,
                        discount = x.Discount, otherCharges = x.OtherCharges,
                        total = x.Purchases + x.Tax,
                    };
                }),
                pagination = new { totalCount, pageNumber, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)) },
                totals = new
                {
                    grnCount = totals?.GrnCount ?? 0, purchases = totals?.Purchases ?? 0m, tax = totals?.Tax ?? 0m,
                    discount = totals?.Discount ?? 0m, otherCharges = totals?.OtherCharges ?? 0m,
                    total = (totals?.Purchases ?? 0m) + (totals?.Tax ?? 0m),
                },
            });
        }).WithName("Reports.PurchasesBySupplier").WithSummary("Server-paginated purchase rollup per supplier (approved GRNs).");

        // ── Low stock / reorder — server-paginated, as-at-now snapshot of every product/outlet
        // combination at or below its effective reorder level. Same effective-level resolution
        // (per-location override → product default) as ReplenishmentService.ComputeAsync, but
        // read-only and cross-location — this is a report, not the replenishment worksheet.
        g.MapGet("/low-stock", async (
            ITenantDbContextFactory f, CancellationToken ct,
            int pageNumber = 1, int pageSize = 10, Guid? locationId = null) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            await using var db = await f.CreateForCurrentAsync(ct);

            var products = await db.Products.AsNoTracking().Where(p => p.IsActive && p.IsStocked).ToListAsync(ct);
            var locsQ = db.Locations.AsNoTracking().AsQueryable();
            if (locationId.HasValue) locsQ = locsQ.Where(l => l.Id == locationId.Value);
            var locsList = await locsQ.Select(l => new { l.Id, l.Code, l.Name }).ToListAsync(ct);

            var overridesQ = db.ProductReplenishmentLevels.AsNoTracking().AsQueryable();
            if (locationId.HasValue) overridesQ = overridesQ.Where(r => r.LocationId == locationId.Value);
            var overrides = (await overridesQ.ToListAsync(ct)).ToDictionary(r => (r.LocationId, r.ProductId), r => r);

            var stocksQ = db.ProductStocks.AsNoTracking().AsQueryable();
            if (locationId.HasValue) stocksQ = stocksQ.Where(s => s.LocationId == locationId.Value);
            var stocks = (await stocksQ.ToListAsync(ct)).ToDictionary(s => (s.LocationId, s.ProductId), s => s.QuantityOnHand);

            var suppliers = await db.Suppliers.AsNoTracking().Select(s => new { s.Id, s.Name }).ToDictionaryAsync(s => s.Id, s => s.Name, ct);

            var rows = new List<(Guid LocationId, string LocationCode, string LocationName, Guid ProductId, string Sku, string Name, decimal OnHand, decimal Reorder, decimal Par, decimal Need, decimal UnitCost, Guid? SupplierId)>();
            foreach (var loc in locsList)
            {
                foreach (var p in products)
                {
                    var ov = overrides.GetValueOrDefault((loc.Id, p.Id));
                    var reorder = ov?.ReorderLevel ?? p.ReorderLevel ?? 0m;
                    if (reorder <= 0) continue;                                   // not monitored here
                    var onHand = stocks.GetValueOrDefault((loc.Id, p.Id), 0m);
                    if (onHand > reorder) continue;                               // above trigger → fine
                    var par = ov?.ParLevel ?? p.ParLevel ?? reorder;
                    if (par < reorder) par = reorder;
                    var need = par - onHand;
                    if (need <= 0) continue;
                    var supplierId = ov?.PreferredSupplierId ?? p.PreferredSupplierId;
                    rows.Add((loc.Id, loc.Code, loc.Name, p.Id, p.Sku, p.Name, onHand, reorder, par, need, p.CostPrice, supplierId));
                }
            }

            var totalCount = rows.Count;
            var totalPoValue = rows.Sum(r => r.Need * r.UnitCost);
            var page = rows.OrderByDescending(r => r.Need * r.UnitCost)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return Results.Ok(new
            {
                data = page.Select(r => new
                {
                    locationCode = r.LocationCode, locationName = r.LocationName,
                    sku = r.Sku, productName = r.Name,
                    onHand = r.OnHand, reorderLevel = r.Reorder, parLevel = r.Par, needQty = r.Need,
                    unitCost = r.UnitCost, poValue = r.Need * r.UnitCost,
                    supplierName = r.SupplierId is Guid sid ? suppliers.GetValueOrDefault(sid) : null,
                }),
                pagination = new { totalCount, pageNumber, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)) },
                totals = new { itemCount = totalCount, poValue = totalPoValue },
            });
        }).WithName("Reports.LowStock").WithSummary("Server-paginated as-at-now snapshot of products at/below their effective reorder level.");

        // ── Slow-moving / dead stock — on-hand value that hasn't SOLD in a while (or
        // ever). "Days since last sale" is the aging signal, not a full any-movement
        // ledger (that's BinCard, and it's per-product across 7 source tables that
        // don't share a schema — too heavy to run for every product at once).
        g.MapGet("/slow-moving", async (
            ITenantDbContextFactory f, CancellationToken ct,
            int pageNumber = 1, int pageSize = 10, Guid? locationId = null, int minDays = 30) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;
            if (minDays < 1) minDays = 30;

            await using var db = await f.CreateForCurrentAsync(ct);
            var now = DateTime.UtcNow;

            var stocksQ = db.ProductStocks.AsNoTracking().Where(s => s.QuantityOnHand > 0);
            if (locationId.HasValue) stocksQ = stocksQ.Where(s => s.LocationId == locationId.Value);
            var stocks = await stocksQ.ToListAsync(ct);

            var products = await db.Products.AsNoTracking().Where(p => p.IsActive && p.IsStocked)
                .Select(p => new { p.Id, p.Sku, p.Name, p.CostPrice }).ToDictionaryAsync(p => p.Id, ct);
            var locs = await db.Locations.AsNoTracking().Select(l => new { l.Id, l.Code, l.Name }).ToDictionaryAsync(l => l.Id, ct);

            var lastSales = await db.OrderItems.AsNoTracking()
                .Where(i => i.IsStocked && !i.IsDeleted)
                .Join(db.Orders.AsNoTracking().Where(o => o.Status == "settled"),
                    i => i.OrderId, o => o.Id, (i, o) => new { i.ProductId, o.LocationId, o.SettledAt })
                .GroupBy(x => new { x.ProductId, x.LocationId })
                .Select(g => new { g.Key.ProductId, g.Key.LocationId, LastSale = g.Max(x => x.SettledAt) })
                .ToListAsync(ct);
            var lastSaleMap = lastSales.ToDictionary(x => (x.ProductId, x.LocationId), x => x.LastSale);

            var rows = new List<(Guid LocationId, string LocationCode, string LocationName, Guid ProductId, string Sku, string Name, decimal OnHand, decimal UnitCost, decimal Value, DateTime? LastSaleAt, int? DaysSinceLastSale)>();
            foreach (var s in stocks)
            {
                if (!products.TryGetValue(s.ProductId, out var p)) continue;
                if (!locs.TryGetValue(s.LocationId, out var l)) continue;
                lastSaleMap.TryGetValue((s.ProductId, s.LocationId), out var lastSale);
                int? daysSince = lastSale.HasValue ? (int)(now - lastSale.Value).TotalDays : null;
                if (daysSince.HasValue && daysSince.Value < minDays) continue;   // sold recently enough → not slow
                var unitCost = s.AverageCost > 0 ? s.AverageCost : p.CostPrice;
                rows.Add((s.LocationId, l.Code, l.Name, p.Id, p.Sku, p.Name, s.QuantityOnHand, unitCost, s.QuantityOnHand * unitCost, lastSale, daysSince));
            }

            var totalCount = rows.Count;
            var totalValue = rows.Sum(r => r.Value);
            var page = rows.OrderByDescending(r => r.Value)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return Results.Ok(new
            {
                data = page.Select(r => new
                {
                    locationCode = r.LocationCode, locationName = r.LocationName,
                    sku = r.Sku, productName = r.Name, onHand = r.OnHand, unitCost = r.UnitCost, value = r.Value,
                    lastSaleAt = r.LastSaleAt, daysSinceLastSale = r.DaysSinceLastSale,
                }),
                pagination = new { totalCount, pageNumber, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)) },
                totals = new { itemCount = totalCount, value = totalValue },
                minDays,
            });
        }).WithName("Reports.SlowMovingStock").WithSummary("Server-paginated stock that hasn't sold in minDays (default 30) or has never sold, valued at cost.");

        // ── Void / cancellation report — server-paginated list of voided orders.
        // "Voided by" is resolved from the audit log (order.void has no dedicated
        // actor column on Order — VoidReason is the only field stored there).
        g.MapGet("/void-orders", async (
            ITenantDbContextFactory f, CancellationToken ct,
            int pageNumber = 1, int pageSize = 10, DateTime? from = null, DateTime? to = null, Guid? locationId = null) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            await using var db = await f.CreateForCurrentAsync(ct);
            var (fromD, toD) = DayRange(from, to, defaultDays: 30);

            var voids = db.Orders.AsNoTracking()
                .Where(o => o.Status == "void" && o.VoidedAt != null && o.VoidedAt >= fromD && o.VoidedAt < toD);
            if (locationId.HasValue) voids = voids.Where(o => o.LocationId == locationId.Value);

            var totalCount = await voids.CountAsync(ct);
            var totalAmount = totalCount == 0 ? 0m : await voids.SumAsync(o => o.TotalAmount, ct);

            var page = await voids.OrderByDescending(o => o.VoidedAt)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(o => new
                {
                    o.Id, o.LocationId, VoidedAt = o.VoidedAt!.Value, Number = o.InvoiceNumber ?? o.OrderNumber,
                    o.OrderType, o.TableLabel, o.CustomerName, o.VoidReason,
                    o.SubtotalAmount, o.DiscountAmount, o.PromotionDiscountAmount, o.ServiceChargeAmount, o.TaxAmount, o.TotalAmount,
                })
                .ToListAsync(ct);

            var locs = await db.Locations.AsNoTracking().Select(l => new { l.Id, l.Code, l.Name }).ToDictionaryAsync(l => l.Id, ct);

            var ids = page.Select(x => x.Id).ToList();
            var actors = ids.Count == 0 ? new Dictionary<Guid, string>() : (await db.ActivityLogs.AsNoTracking()
                .Where(a => a.EntityType == "order" && a.Action == "order.void" && a.EntityId != null && ids.Contains(a.EntityId.Value))
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new { a.EntityId, a.ActorName })
                .ToListAsync(ct))
                .GroupBy(a => a.EntityId!.Value)
                .ToDictionary(grp => grp.Key, grp => string.IsNullOrWhiteSpace(grp.First().ActorName) ? "—" : grp.First().ActorName!);

            return Results.Ok(new
            {
                data = page.Select(x =>
                {
                    locs.TryGetValue(x.LocationId, out var l);
                    return new
                    {
                        orderNumber = x.Number, voidedAt = x.VoidedAt,
                        locationCode = l?.Code ?? "—", locationName = l?.Name ?? "(deleted)",
                        orderType = x.OrderType, tableLabel = x.TableLabel, customerName = x.CustomerName,
                        subtotalAmount = x.SubtotalAmount, discountAmount = x.DiscountAmount + x.PromotionDiscountAmount,
                        serviceChargeAmount = x.ServiceChargeAmount, taxAmount = x.TaxAmount, totalAmount = x.TotalAmount,
                        voidReason = x.VoidReason, voidedBy = actors.GetValueOrDefault(x.Id, "—"),
                    };
                }),
                pagination = new { totalCount, pageNumber, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)) },
                totals = new { count = totalCount, amount = totalAmount },
            });
        }).WithName("Reports.VoidOrders").WithSummary("Server-paginated list of voided/cancelled orders, with reason and who voided them.");

        // ── Shift settlement — cash-up history with variance ─────────────────────
        // Server-paginated: one row per shift opened in the period, which accumulates over
        // time (every outlet, every day) same as the register. Totals are aggregated in
        // SQL over the whole period; only `shifts` is a page at a time.
        g.MapGet("/shifts", async (
            ITenantDbContextFactory f, CancellationToken ct, DateTime? from, DateTime? to, Guid? locationId,
            int pageNumber = 1, int pageSize = 25) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            await using var db = await f.CreateForCurrentAsync(ct);
            var (fromD, toD) = DayRange(from, to, defaultDays: 7);
            var q = db.Shifts.AsNoTracking().Where(s => s.OpenedAt >= fromD && s.OpenedAt < toD);
            if (locationId.HasValue) q = q.Where(s => s.LocationId == locationId.Value);

            var totalCount = await q.CountAsync(ct);
            var totalsRow = totalCount == 0 ? null : await q
                .GroupBy(s => 1)
                .Select(grp => new { totalSales = grp.Sum(s => s.TotalSales), totalVariance = grp.Sum(s => s.CashVariance ?? 0m) })
                .FirstOrDefaultAsync(ct);

            var rows = await q.OrderByDescending(s => s.OpenedAt)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(s => new
                {
                    s.ShiftNumber, s.OpenedByName, s.OpenedAt, s.ClosedAt, s.Status,
                    s.OpeningFloat, s.TotalSales, s.CashSales, s.CardSales, s.OtherSales, s.OrderCount,
                    s.ExpectedCash, s.DeclaredCash, s.CashVariance,
                }).ToListAsync(ct);

            return Results.Ok(new
            {
                periodFrom = fromD, periodTo = toD, shifts = rows,
                totalSales = totalsRow?.totalSales ?? 0m,
                totalVariance = totalsRow?.totalVariance ?? 0m,
                pagination = new { totalCount, pageNumber, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)) },
            });
        }).WithName("Reports.ShiftSettlement").WithSummary("Server-paginated cashier shift cash-ups in the period, with cash variance.");

        // ── Promotion usage — times applied + discount given per promo ───────────
        g.MapGet("/promotions", async (
            ITenantDbContextFactory f, CancellationToken ct, DateTime? from, DateTime? to, Guid? locationId,
            int pageNumber = 1, int pageSize = 25) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            await using var db = await f.CreateForCurrentAsync(ct);
            var (fromD, toD) = DayRange(from, to);
            var ids = await Settled(db, fromD, toD, locationId).Select(o => o.Id).ToListAsync(ct);
            var all = ids.Count == 0 ? new List<PromoRow>() : (await db.OrderPromotions.AsNoTracking()
                .Where(op => ids.Contains(op.OrderId))
                .GroupBy(op => new { op.Code, op.Name })
                .Select(grp => new PromoRow(grp.Key.Code, grp.Key.Name, grp.Count(), grp.Sum(x => x.DiscountAmount)))
                .ToListAsync(ct)).OrderByDescending(r => r.Discount).ToList();

            var totalCount = all.Count;
            var rows = all.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return Results.Ok(new
            {
                periodFrom = fromD, periodTo = toD, promotions = rows, totalDiscount = all.Sum(r => r.Discount),
                pagination = new { totalCount, pageNumber, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)) },
            });
        }).WithName("Reports.PromotionUsage").WithSummary("Server-paginated: how often each promotion fired and the discount it gave.");

        // ── Discount & complimentary (lite) — manual discount totals per steward/waiter.
        // "Lite" because Order has no DiscountReason/approver field and no complimentary
        // concept exists anywhere yet (unlike void, which has VoidReason) — this only
        // reports the aggregate DiscountAmount already on Order, grouped by steward.
        g.MapGet("/discounts", async (
            ITenantDbContextFactory f, CancellationToken ct, DateTime? from, DateTime? to, Guid? locationId,
            int pageNumber = 1, int pageSize = 25) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            await using var db = await f.CreateForCurrentAsync(ct);
            var (fromD, toD) = DayRange(from, to);
            var raw = await Settled(db, fromD, toD, locationId).Where(o => o.DiscountAmount > 0)
                .Select(o => new { o.StewardId, o.DiscountAmount, o.SubtotalAmount }).ToListAsync(ct);

            var names = await db.Users.AsNoTracking().Select(u => new { u.Id, u.DisplayName }).ToDictionaryAsync(u => u.Id, u => u.DisplayName, ct);

            var all = raw.GroupBy(r => r.StewardId)
                .Select(grp => new
                {
                    stewardId = grp.Key,
                    stewardName = grp.Key is Guid sid && names.TryGetValue(sid, out var n) ? n : "(unassigned)",
                    billCount = grp.Count(),
                    grossSales = grp.Sum(x => x.SubtotalAmount),
                    discountTotal = grp.Sum(x => x.DiscountAmount),
                })
                .OrderByDescending(r => r.discountTotal)
                .ToList();

            var totalCount = all.Count;
            var page = all.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return Results.Ok(new
            {
                periodFrom = fromD, periodTo = toD,
                rows = page.Select(r => new
                {
                    r.stewardId, r.stewardName, r.billCount, r.grossSales, r.discountTotal,
                    discountPercent = r.grossSales > 0 ? Math.Round(r.discountTotal / r.grossSales * 100, 1) : 0,
                }),
                totals = new
                {
                    billCount = all.Sum(r => r.billCount),
                    grossSales = all.Sum(r => r.grossSales),
                    discountTotal = all.Sum(r => r.discountTotal),
                },
                pagination = new { totalCount, pageNumber, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)) },
            });
        }).WithName("Reports.Discounts").WithSummary("Server-paginated manual discount totals per steward/waiter over a period (lite — no reason/approver/comp tracking yet).");

        // ── Table turnover — bills, covers and occupancy duration per dine-in table.
        g.MapGet("/table-turnover", async (
            ITenantDbContextFactory f, CancellationToken ct, DateTime? from, DateTime? to, Guid? locationId,
            int pageNumber = 1, int pageSize = 25) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            await using var db = await f.CreateForCurrentAsync(ct);
            var (fromD, toD) = DayRange(from, to);
            var raw = await Settled(db, fromD, toD, locationId)
                .Where(o => o.OrderType == "dine_in" && o.TableLabel != null)
                .Select(o => new { o.TableLabel, o.Covers, o.OpenedAt, o.SettledAt, o.TotalAmount })
                .ToListAsync(ct);

            var all = raw.GroupBy(r => r.TableLabel!)
                .Select(grp => new
                {
                    tableLabel = grp.Key,
                    billCount = grp.Count(),
                    totalCovers = grp.Sum(x => x.Covers ?? 0),
                    avgDurationMinutes = Math.Round(grp.Average(x => (x.SettledAt!.Value - x.OpenedAt).TotalMinutes), 1),
                    grossSales = grp.Sum(x => x.TotalAmount),
                })
                .OrderByDescending(r => r.billCount)
                .ToList();

            var totalCount = all.Count;
            var rows = all.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return Results.Ok(new
            {
                periodFrom = fromD, periodTo = toD, rows,
                totals = new
                {
                    billCount = all.Sum(r => r.billCount),
                    totalCovers = all.Sum(r => r.totalCovers),
                    avgDurationMinutes = raw.Count == 0 ? 0 : Math.Round(raw.Average(x => (x.SettledAt!.Value - x.OpenedAt).TotalMinutes), 1),
                    grossSales = all.Sum(r => r.grossSales),
                },
                pagination = new { totalCount, pageNumber, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)) },
            });
        }).WithName("Reports.TableTurnover").WithSummary("Server-paginated bills, covers and average occupancy duration per dine-in table over a period.");

        // ── Food costing — dish cost vs sell price + gross-profit % (#72) ──
        // Dish cost is the latest posted production unit cost (conversion-correct,
        // weighted-avg roll-up); for never-produced recipes it falls back to a
        // recipe estimate (Σ ingredient avg-cost × qty ÷ yield).
        // Server-paginated: one row per active recipe, which scales with menu size (same
        // class of risk as Stock Balance). The full costed list is still built once here
        // since gross-profit sorting needs the whole set; only the returned page is sliced.
        g.MapGet("/food-costing", async (
            ITenantDbContextFactory f, CancellationToken ct, Guid? locationId,
            int pageNumber = 1, int pageSize = 25) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;
            await using var db = await f.CreateForCurrentAsync(ct);
            var recipes = (await db.Recipes.AsNoTracking().Include(r => r.Lines).Where(r => r.IsActive).ToListAsync(ct))
                .GroupBy(r => r.ProductId).Select(grp => grp.First()).ToList();   // one row per product
            var prodIds = recipes.Select(r => r.ProductId).ToList();
            var ingIds = recipes.SelectMany(r => r.Lines.Where(l => !l.IsDeleted)).Select(l => l.IngredientProductId).Distinct().ToList();

            var avgCost = (await db.ProductStocks.AsNoTracking()
                .Where(s => ingIds.Contains(s.ProductId) && (locationId == null || s.LocationId == locationId))
                .Select(s => new { s.ProductId, s.AverageCost }).ToListAsync(ct))
                .GroupBy(x => x.ProductId).ToDictionary(grp => grp.Key, grp => grp.Average(x => x.AverageCost));
            var costFallback = await db.Products.AsNoTracking().Where(p => ingIds.Contains(p.Id))
                .Select(p => new { p.Id, p.CostPrice }).ToDictionaryAsync(x => x.Id, x => x.CostPrice, ct);
            var products = await db.Products.AsNoTracking().Where(p => prodIds.Contains(p.Id))
                .Select(p => new { p.Id, p.Name, p.Sku, p.BasePrice }).ToDictionaryAsync(x => x.Id, ct);
            var latestProd = (await db.ProductionOrders.AsNoTracking()
                .Where(po => po.Status == "posted" && prodIds.Contains(po.ProductId) && po.PostedAt != null)
                .Select(po => new { po.ProductId, po.UnitCost, po.PostedAt }).ToListAsync(ct))
                .GroupBy(x => x.ProductId).ToDictionary(grp => grp.Key, grp => grp.OrderByDescending(x => x.PostedAt).First().UnitCost);

            decimal IngCost(Guid id) => avgCost.TryGetValue(id, out var a) && a > 0 ? a : costFallback.GetValueOrDefault(id, 0m);
            var rows = recipes.Where(r => products.ContainsKey(r.ProductId)).Select(r =>
            {
                var p = products[r.ProductId];
                var estimate = r.YieldQuantity > 0
                    ? r.Lines.Where(l => !l.IsDeleted).Sum(l => l.Quantity * IngCost(l.IngredientProductId)) / r.YieldQuantity : 0m;
                var fromProduction = latestProd.TryGetValue(r.ProductId, out var uc) && uc > 0;
                var dishCost = Math.Round(fromProduction ? uc : estimate, 2);
                var sell = p.BasePrice;
                var gp = sell - dishCost;
                return new
                {
                    productId = r.ProductId, name = p.Name, sku = p.Sku,
                    dishCost, sellPrice = sell, grossProfit = Math.Round(gp, 2),
                    gpPercent = sell > 0 ? Math.Round(gp / sell * 100m, 1) : 0m,
                    foodCostPercent = sell > 0 ? Math.Round(dishCost / sell * 100m, 1) : 0m,
                    costSource = fromProduction ? "production" : "recipe-estimate",
                };
            }).OrderByDescending(x => x.foodCostPercent).ToList();

            var totalCount = rows.Count;
            var items = rows.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return Results.Ok(new
            {
                locationId, items,
                pagination = new { totalCount, pageNumber, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)) },
            });
        }).WithName("Reports.FoodCosting").WithSummary("Server-paginated recipe dish cost vs sell price + gross-profit %.");

        // ── Bin card — every stock movement for a product at a location (#72) ──
        // Assembled from the source documents (no unified movement ledger yet):
        // GRN, sales, wastage, adjustments, transfers, production. Opening is
        // derived by walking back from current on-hand over the same event set.
        // Note: the running balance is inherently sequential (each line depends on the
        // cumulative sum of every prior line), so it can't be computed with a plain SQL
        // Skip/Take. The full ledger is still built once here; only the returned page of
        // `lines` is sliced afterwards — opening/closing/totalIn/totalOut stay whole-period.
        g.MapGet("/bin-card", async (
            ITenantDbContextFactory f, CancellationToken ct, Guid productId, Guid? locationId, DateTime? from, DateTime? to,
            int pageNumber = 1, int pageSize = 25) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;
            if (productId == default) return Results.BadRequest(new { error = "productId is required" });
            await using var db = await f.CreateForCurrentAsync(ct);
            var (fromD, toD) = DayRange(from, to, 30);
            var loc = locationId ?? await db.Locations.AsNoTracking().OrderBy(l => l.Code).Select(l => (Guid?)l.Id).FirstOrDefaultAsync(ct);
            if (loc is not Guid locId) return Results.BadRequest(new { error = "No location to report on" });

            var current = await db.ProductStocks.AsNoTracking().Where(s => s.ProductId == productId && s.LocationId == locId)
                .Select(s => (decimal?)s.QuantityOnHand).FirstOrDefaultAsync(ct) ?? 0m;

            var ev = new List<Mv>();
            ev.AddRange((await (from l in db.GrnLines.AsNoTracking()
                                join gn in db.GoodsReceivedNotes.AsNoTracking() on l.GrnId equals gn.Id
                                where l.ProductId == productId && gn.Status == "posted" && gn.LocationId == locId && gn.PostedAt != null && gn.PostedAt >= fromD
                                select new { When = gn.PostedAt, gn.GrnNumber, l.StockQuantity, l.QuantityReceived }).ToListAsync(ct))
                .Select(x => new Mv(x.When!.Value, "GRN", x.GrnNumber, x.StockQuantity != 0 ? x.StockQuantity : x.QuantityReceived)));
            ev.AddRange((await (from i in db.OrderItems.AsNoTracking()
                                join o in db.Orders.AsNoTracking() on i.OrderId equals o.Id
                                where i.ProductId == productId && i.IsStocked && o.Status == "settled" && o.LocationId == locId && o.SettledAt != null && o.SettledAt >= fromD
                                select new { When = o.SettledAt, o.InvoiceNumber, o.OrderNumber, i.Quantity }).ToListAsync(ct))
                .Select(x => new Mv(x.When!.Value, "Sale", x.InvoiceNumber ?? x.OrderNumber, -x.Quantity)));
            ev.AddRange((await (from l in db.WastageLines.AsNoTracking()
                                join w in db.WastageNotes.AsNoTracking() on l.WastageId equals w.Id
                                where l.ProductId == productId && w.Status == "posted" && w.LocationId == locId && w.PostedAt != null && w.PostedAt >= fromD
                                select new { When = w.PostedAt, w.WastageNumber, l.Quantity }).ToListAsync(ct))
                .Select(x => new Mv(x.When!.Value, "Wastage", x.WastageNumber, -x.Quantity)));
            ev.AddRange((await (from l in db.StockAdjustmentLines.AsNoTracking()
                                join a in db.StockAdjustments.AsNoTracking() on l.AdjustmentId equals a.Id
                                where l.ProductId == productId && a.Status == "posted" && a.LocationId == locId && a.PostedAt != null && a.PostedAt >= fromD
                                select new { When = a.PostedAt, a.AdjustmentNumber, l.QuantityDelta }).ToListAsync(ct))
                .Select(x => new Mv(x.When!.Value, "Adjustment", x.AdjustmentNumber, x.QuantityDelta)));
            ev.AddRange((await (from l in db.StockTransferLines.AsNoTracking()
                                join t in db.StockTransfers.AsNoTracking() on l.TransferId equals t.Id
                                where l.ProductId == productId && t.FromLocationId == locId && t.DispatchedAt != null && t.DispatchedAt >= fromD && t.Status != "cancelled"
                                select new { When = t.DispatchedAt, t.TransferNumber, l.Quantity }).ToListAsync(ct))
                .Select(x => new Mv(x.When!.Value, "Transfer out", x.TransferNumber, -x.Quantity)));
            ev.AddRange((await (from l in db.StockTransferLines.AsNoTracking()
                                join t in db.StockTransfers.AsNoTracking() on l.TransferId equals t.Id
                                where l.ProductId == productId && t.ToLocationId == locId && t.Status == "received" && t.ReceivedAt != null && t.ReceivedAt >= fromD
                                select new { When = t.ReceivedAt, t.TransferNumber, l.Quantity }).ToListAsync(ct))
                .Select(x => new Mv(x.When!.Value, "Transfer in", x.TransferNumber, x.Quantity)));
            ev.AddRange((await (from c in db.ProductionConsumptions.AsNoTracking()
                                join po in db.ProductionOrders.AsNoTracking() on c.ProductionOrderId equals po.Id
                                where c.IngredientProductId == productId && po.Status == "posted" && po.LocationId == locId && po.PostedAt != null && po.PostedAt >= fromD
                                select new { When = po.PostedAt, po.ProductionNumber, c.QuantityConsumed }).ToListAsync(ct))
                .Select(x => new Mv(x.When!.Value, "Production use", x.ProductionNumber, -x.QuantityConsumed)));
            ev.AddRange((await db.ProductionOrders.AsNoTracking()
                .Where(po => po.ProductId == productId && po.Status == "posted" && (po.ReceiptLocationId ?? po.LocationId) == locId && po.PostedAt != null && po.PostedAt >= fromD)
                .Select(po => new { When = po.PostedAt, po.ProductionNumber, po.Quantity }).ToListAsync(ct))
                .Select(x => new Mv(x.When!.Value, "Production made", x.ProductionNumber, x.Quantity)));

            var opening = current - ev.Sum(e => e.Qty);                 // walk back from current on-hand
            var shown = ev.Where(e => e.When < toD).OrderBy(e => e.When).ToList();
            var running = opening;
            var allLines = shown.Select(e => { running += e.Qty; return new { date = e.When, type = e.Type, doc = e.Doc, qty = e.Qty, balance = running }; }).ToList();

            var totalCount = allLines.Count;
            var lines = allLines.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return Results.Ok(new
            {
                productId, locationId = locId, periodFrom = fromD, periodTo = toD,
                opening, closing = running,
                totalIn = shown.Where(e => e.Qty > 0).Sum(e => e.Qty),
                totalOut = shown.Where(e => e.Qty < 0).Sum(e => -e.Qty),
                lines,
                pagination = new { totalCount, pageNumber, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)) },
            });
        }).WithName("Reports.BinCard").WithSummary("Per-product stock movement ledger with a running balance.");

        // ── Steward sales + tips (#76) — per-waiter performance + tip payout basis ──
        g.MapGet("/steward-sales", async (
            OrderService svc, CancellationToken ct, DateTime? from, DateTime? to, Guid? locationId,
            int pageNumber = 1, int pageSize = 25) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            var all = await svc.StewardSalesAsync(from, to, locationId, ct);
            var totalCount = all.Count;
            var data = all.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return Results.Ok(new
            {
                data,
                pagination = new { totalCount, pageNumber, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)) },
            });
        }).WithName("Reports.StewardSales").WithSummary("Server-paginated per-steward bills, covers, gross sales and tips collected.");

        // ── Tour-operator commission (#76) — what the venue owes each operator ──
        g.MapGet("/tour-commission", async (
            OrderService svc, CancellationToken ct, DateTime? from, DateTime? to, Guid? locationId,
            int pageNumber = 1, int pageSize = 25) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            var all = await svc.TourCommissionAsync(from, to, locationId, ct);
            var totalCount = all.Count;
            var data = all.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return Results.Ok(new
            {
                data,
                pagination = new { totalCount, pageNumber, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)) },
            });
        }).WithName("Reports.TourCommission").WithSummary("Server-paginated per-operator settled bills, gross sales and commission booked.");

        // ── Sales budgets (#72) — config + budget-vs-actual report ──
        var bg = app.MapGroup("/api/v1/budgets").WithTags("Budgets").RequireAuthorization("BackOffice");
        bg.MapGet("", async (ITenantDbContextFactory f, CancellationToken ct, int? year) =>
        {
            await using var db = await f.CreateForCurrentAsync(ct);
            var q = db.SalesBudgets.AsNoTracking();
            if (year is int y) { var lo = new DateOnly(y, 1, 1); var hi = new DateOnly(y, 12, 31); q = q.Where(b => b.PeriodMonth >= lo && b.PeriodMonth <= hi); }
            return Results.Ok(await q.OrderBy(b => b.PeriodMonth).Select(b => new { b.Id, b.LocationId, b.PeriodMonth, b.Amount }).ToListAsync(ct));
        }).WithName("Budgets.List");

        bg.MapPut("", async (BudgetInput i, ITenantDbContextFactory f, CancellationToken ct) =>
        {
            // null / empty location ⇒ company-wide (all-outlets) target.
            Guid? loc = i.LocationId is { } l && l != Guid.Empty ? l : null;
            var month = new DateOnly(i.Month.Year, i.Month.Month, 1);
            await using var db = await f.CreateForCurrentAsync(ct);
            var b = loc is null
                ? await db.SalesBudgets.FirstOrDefaultAsync(x => x.LocationId == null && x.PeriodMonth == month, ct)
                : await db.SalesBudgets.FirstOrDefaultAsync(x => x.LocationId == loc && x.PeriodMonth == month, ct);
            if (b is null) { b = new Hms.Api.Domain.SalesBudget { LocationId = loc, PeriodMonth = month }; db.SalesBudgets.Add(b); }
            b.Amount = Math.Max(0, i.Amount); b.IsDeleted = false;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { b.Id, b.LocationId, b.PeriodMonth, b.Amount });
        }).WithName("Budgets.Upsert");

        g.MapGet("/budget-vs-sales", async (
            ITenantDbContextFactory f, CancellationToken ct, DateTime? from, DateTime? to, Guid? locationId,
            int pageNumber = 1, int pageSize = 25) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;
            await using var db = await f.CreateForCurrentAsync(ct);
            var now = DateTime.UtcNow;
            var fromD = from is { } ff ? OrderService.AsUtc(ff) : new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var toD = to is { } tt ? OrderService.AsUtc(tt).AddDays(1) : OrderService.AsUtc(now.Date.AddDays(1));

            var sales = await db.Orders.AsNoTracking()
                .Where(o => o.Status == "settled" && o.SettledAt >= fromD && o.SettledAt < toD && (locationId == null || o.LocationId == locationId))
                .Select(o => new { o.SettledAt, o.TotalAmount }).ToListAsync(ct);
            // "All outlets" view compares total sales to the company-wide (null-location)
            // target; a specific outlet compares to that outlet's own target.
            var budgets = await db.SalesBudgets.AsNoTracking()
                .Where(b => locationId == null ? b.LocationId == null : b.LocationId == locationId)
                .Select(b => new { b.PeriodMonth, b.Amount }).ToListAsync(ct);

            var salesByMonth = sales.GroupBy(s => new DateOnly(s.SettledAt!.Value.Year, s.SettledAt.Value.Month, 1))
                .ToDictionary(grp => grp.Key, grp => grp.Sum(x => x.TotalAmount));
            var budgetByMonth = budgets.GroupBy(b => b.PeriodMonth).ToDictionary(grp => grp.Key, grp => grp.Sum(x => x.Amount));

            var all = new List<object>();
            for (var m = new DateOnly(fromD.Year, fromD.Month, 1); m <= new DateOnly(toD.Year, toD.Month, 1); m = m.AddMonths(1))
            {
                var actual = salesByMonth.GetValueOrDefault(m, 0m);
                var budget = budgetByMonth.GetValueOrDefault(m, 0m);
                if (actual == 0 && budget == 0) continue;
                all.Add(new { month = m, budget, actual, variance = actual - budget, pct = budget > 0 ? Math.Round(actual / budget * 100m, 1) : (decimal?)null });
            }

            var totalCount = all.Count;
            var rows = all.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return Results.Ok(new
            {
                periodFrom = fromD, periodTo = toD, rows,
                pagination = new { totalCount, pageNumber, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)) },
            });
        }).WithName("Reports.BudgetVsSales").WithSummary("Server-paginated monthly sales budget vs actual settled sales.");

        // ── Master data reports — read-only, server-paginated directories of the core setup
        // entities. Not transactional data: just what's configured, for audit/handover/export.
        g.MapGet("/master-data/products", async (
            ITenantDbContextFactory f, CancellationToken ct,
            int pageNumber = 1, int pageSize = 25, string? search = null, Guid? categoryId = null, bool? isActive = null) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 200) pageSize = 200;

            await using var db = await f.CreateForCurrentAsync(ct);
            var q = db.Products.AsNoTracking().AsQueryable();
            if (categoryId.HasValue) q = q.Where(p => p.CategoryId == categoryId.Value);
            if (isActive.HasValue) q = q.Where(p => p.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = $"%{search.Trim()}%";
                q = q.Where(p => EF.Functions.ILike(p.Sku, s) || EF.Functions.ILike(p.Name, s) || (p.Barcode != null && EF.Functions.ILike(p.Barcode, s)));
            }

            var totalCount = await q.CountAsync(ct);
            var page = await q.OrderBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            var categoryIds = page.Where(p => p.CategoryId.HasValue).Select(p => p.CategoryId!.Value).Distinct().ToList();
            var categories = await db.Categories.AsNoTracking().Where(c => categoryIds.Contains(c.Id)).Select(c => new { c.Id, c.Name }).ToDictionaryAsync(c => c.Id, c => c.Name, ct);
            var departmentIds = page.Where(p => p.DepartmentId.HasValue).Select(p => p.DepartmentId!.Value).Distinct().ToList();
            var departments = await db.Departments.AsNoTracking().Where(d => departmentIds.Contains(d.Id)).Select(d => new { d.Id, d.Name }).ToDictionaryAsync(d => d.Id, d => d.Name, ct);
            var uomIds = page.Select(p => p.UnitOfMeasureId).Distinct().ToList();
            var uoms = await db.UnitsOfMeasure.AsNoTracking().Where(u => uomIds.Contains(u.Id)).Select(u => new { u.Id, u.Name }).ToDictionaryAsync(u => u.Id, u => u.Name, ct);
            var supplierIds = page.Where(p => p.PreferredSupplierId.HasValue).Select(p => p.PreferredSupplierId!.Value).Distinct().ToList();
            var suppliers = await db.Suppliers.AsNoTracking().Where(s => supplierIds.Contains(s.Id)).Select(s => new { s.Id, s.Name }).ToDictionaryAsync(s => s.Id, s => s.Name, ct);

            return Results.Ok(new
            {
                data = page.Select(p => new
                {
                    p.Id, p.Sku, p.Barcode, p.Name, p.ProductType,
                    categoryName = p.CategoryId is Guid cid ? categories.GetValueOrDefault(cid) : null,
                    departmentName = p.DepartmentId is Guid did ? departments.GetValueOrDefault(did) : null,
                    unitOfMeasure = uoms.GetValueOrDefault(p.UnitOfMeasureId),
                    p.BasePrice, p.CostPrice, p.TaxClass, p.IsTaxable,
                    p.IsSold, p.IsPurchased, p.IsStocked, p.IsActive,
                    p.ReorderLevel, p.ParLevel,
                    preferredSupplierName = p.PreferredSupplierId is Guid sid ? suppliers.GetValueOrDefault(sid) : null,
                }),
                pagination = new { totalCount, pageNumber, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)) },
            });
        }).WithName("Reports.MasterData.Products").WithSummary("Server-paginated product master directory.");

        g.MapGet("/master-data/suppliers", async (
            ITenantDbContextFactory f, CancellationToken ct,
            int pageNumber = 1, int pageSize = 25, string? search = null, bool? isActive = null) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 200) pageSize = 200;

            await using var db = await f.CreateForCurrentAsync(ct);
            var q = db.Suppliers.AsNoTracking().AsQueryable();
            if (isActive.HasValue) q = q.Where(s => s.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s2 = $"%{search.Trim()}%";
                q = q.Where(s => EF.Functions.ILike(s.Code, s2) || EF.Functions.ILike(s.Name, s2)
                    || (s.Phone != null && EF.Functions.ILike(s.Phone, s2)) || (s.Email != null && EF.Functions.ILike(s.Email, s2)));
            }

            var totalCount = await q.CountAsync(ct);
            var page = await q.OrderBy(s => s.Name)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            var groupIds = page.Where(s => s.SupplierGroupId.HasValue).Select(s => s.SupplierGroupId!.Value).Distinct().ToList();
            var groups = await db.SupplierGroups.AsNoTracking().Where(g2 => groupIds.Contains(g2.Id)).Select(g2 => new { g2.Id, g2.Name }).ToDictionaryAsync(g2 => g2.Id, g2 => g2.Name, ct);
            var typeIds = page.Where(s => s.SupplierTypeId.HasValue).Select(s => s.SupplierTypeId!.Value).Distinct().ToList();
            var types = await db.SupplierTypes.AsNoTracking().Where(t => typeIds.Contains(t.Id)).Select(t => new { t.Id, t.Name }).ToDictionaryAsync(t => t.Id, t => t.Name, ct);

            return Results.Ok(new
            {
                data = page.Select(s => new
                {
                    s.Id, s.Code, s.Name, s.ContactName, s.Phone, s.Email, s.Address,
                    s.PaymentTermsDays, s.IsVatRegistered, s.VatRegistrationNumber, s.IsActive,
                    groupName = s.SupplierGroupId is Guid gid ? groups.GetValueOrDefault(gid) : null,
                    typeName = s.SupplierTypeId is Guid tid ? types.GetValueOrDefault(tid) : null,
                }),
                pagination = new { totalCount, pageNumber, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)) },
            });
        }).WithName("Reports.MasterData.Suppliers").WithSummary("Server-paginated supplier master directory.");

        g.MapGet("/master-data/customers", async (
            ITenantDbContextFactory f, CancellationToken ct,
            int pageNumber = 1, int pageSize = 25, string? search = null, bool? isActive = null) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 200) pageSize = 200;

            await using var db = await f.CreateForCurrentAsync(ct);
            var q = db.Customers.AsNoTracking().AsQueryable();
            if (isActive.HasValue) q = q.Where(c => c.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = $"%{search.Trim()}%";
                q = q.Where(c => EF.Functions.ILike(c.Code, s) || EF.Functions.ILike(c.Name, s)
                    || (c.Phone != null && EF.Functions.ILike(c.Phone, s)) || (c.Email != null && EF.Functions.ILike(c.Email, s)));
            }

            var totalCount = await q.CountAsync(ct);
            var page = await q.OrderBy(c => c.Name)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            var categoryIds = page.Where(c => c.CategoryId.HasValue).Select(c => c.CategoryId!.Value).Distinct().ToList();
            var categories = await db.CustomerCategories.AsNoTracking().Where(cc => categoryIds.Contains(cc.Id))
                .Select(cc => new { cc.Id, cc.Name, cc.DiscountPercent }).ToDictionaryAsync(cc => cc.Id, ct);

            return Results.Ok(new
            {
                data = page.Select(c => new
                {
                    c.Id, c.Code, c.Name, c.Phone, c.Email, c.Address, c.TaxNo,
                    categoryName = c.CategoryId is Guid catId && categories.TryGetValue(catId, out var cat) ? cat.Name : null,
                    discountPercent = c.DiscountPercent ?? (c.CategoryId is Guid catId2 && categories.TryGetValue(catId2, out var cat2) ? cat2.DiscountPercent : 0m),
                    c.IsCreditCustomer, c.CreditLimit, c.CurrentBalance, c.IsActive,
                }),
                pagination = new { totalCount, pageNumber, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)) },
            });
        }).WithName("Reports.MasterData.Customers").WithSummary("Server-paginated customer master directory.");

        g.MapGet("/master-data/locations", async (
            ITenantDbContextFactory f, CancellationToken ct,
            int pageNumber = 1, int pageSize = 25, string? search = null, bool? isActive = null) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 200) pageSize = 200;

            await using var db = await f.CreateForCurrentAsync(ct);
            var q = db.Locations.AsNoTracking().AsQueryable();
            if (isActive.HasValue) q = q.Where(l => l.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = $"%{search.Trim()}%";
                q = q.Where(l => EF.Functions.ILike(l.Code, s) || EF.Functions.ILike(l.Name, s) || EF.Functions.ILike(l.City, s));
            }

            var totalCount = await q.CountAsync(ct);
            var page = await q.OrderBy(l => l.Name)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            return Results.Ok(new
            {
                data = page.Select(l => new
                {
                    l.Id, l.Code, l.Name, l.City, l.LocationType, l.Currency,
                    l.VatExempt, l.CanSell, l.CanProduce, l.CanStock, l.IsActive,
                }),
                pagination = new { totalCount, pageNumber, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)) },
            });
        }).WithName("Reports.MasterData.Locations").WithSummary("Server-paginated location master directory.");

        return app;
    }

    private record Mv(DateTime When, string Type, string Doc, decimal Qty);

    private static IQueryable<Hms.Api.Domain.Order> Settled(TenantDbContext db, DateTime fromD, DateTime toD, Guid? locationId)
    {
        var q = db.Orders.AsNoTracking().Where(o => o.Status == "settled" && o.SettledAt >= fromD && o.SettledAt < toD);
        return locationId.HasValue ? q.Where(o => o.LocationId == locationId.Value) : q;
    }

    // Default window = today (or the last N days), with provided dates coerced to UTC.
    // An explicit "to" is a calendar day picked in the UI, so it's made inclusive (+1 day
    // as the exclusive upper bound) — otherwise today's settled orders (settled after UTC
    // midnight, i.e. any time in a tenant ahead of UTC) silently drop out of the report.
    private static (DateTime from, DateTime to) DayRange(DateTime? from, DateTime? to, int defaultDays = 1)
    {
        var toD = to is { } tt ? OrderService.AsUtc(tt).AddDays(1) : OrderService.AsUtc(DateTime.UtcNow.Date.AddDays(1));
        var fromD = from is { } ff ? OrderService.AsUtc(ff) : OrderService.AsUtc(DateTime.UtcNow.Date.AddDays(1 - defaultDays));
        return (fromD, toD);
    }

    private record DailySalesRow(
        DateOnly Date, Guid LocationId, string LocationCode, string LocationName,
        int Receipts, decimal Gross, decimal Discount, decimal ServiceCharge, decimal Tax, decimal Net);

    private record DailySalesDetailRow(
        Guid Id, DateOnly Date, DateTime SettledAt, Guid LocationId, string LocationCode, string LocationName,
        string Number, string OrderType, string? TableLabel, string? CustomerName,
        decimal Gross, decimal Discount, decimal ServiceCharge, decimal Tax, decimal Net);

    private record TenderRow(string PayType, decimal Amount);
    public record BudgetInput(Guid? LocationId, DateOnly Month, decimal Amount);
    private record ItemRow(Guid ProductId, string? Sku, string ProductName, decimal Quantity, decimal Revenue, decimal Tax);
    private record PromoRow(string Code, string Name, int Times, decimal Discount);
}
