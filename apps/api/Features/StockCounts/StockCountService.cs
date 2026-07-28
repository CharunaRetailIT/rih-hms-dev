using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.StockCounts;

/// <summary>
/// Physical stock count (#77). Opening a count snapshots the current on-hand per
/// stocked product at the outlet; the team enters counted quantities; posting
/// writes the variance back to product_stock (counted becomes the new on-hand)
/// and stamps last_counted_at. The count + lines are the permanent variance record.
/// </summary>
public class StockCountService(ITenantDbContextFactory factory)
{
    public async Task<StockCount> CreateAsync(Guid locationId, string? notes, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var products = await db.Products.AsNoTracking().Where(p => p.IsStocked).Select(p => p.Id).ToListAsync(ct);
        var onHand = await db.ProductStocks.AsNoTracking().Where(s => s.LocationId == locationId)
            .ToDictionaryAsync(s => s.ProductId, s => s.QuantityOnHand, ct);

        var count = new StockCount { LocationId = locationId, Status = "draft", Notes = notes, CountNumber = await NextNumberAsync(db, ct) };
        foreach (var pid in products)
        {
            var sys = onHand.GetValueOrDefault(pid, 0m);
            count.Lines.Add(new StockCountLine { ProductId = pid, SystemQty = sys, CountedQty = sys });
        }
        db.StockCounts.Add(count);
        await db.SaveChangesAsync(ct);
        return count;
    }

    public async Task<StockCount?> GetAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        return await db.StockCounts.Include(c => c.Lines).AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<List<StockCount>> ListAsync(Guid? locationId, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var q = db.StockCounts.AsNoTracking().AsQueryable();
        if (locationId.HasValue) q = q.Where(c => c.LocationId == locationId.Value);
        return await q.OrderByDescending(c => c.CreatedAt).Take(50).ToListAsync(ct);
    }

    public async Task<PagedStockCountResult> GetPagedAsync(int pageNumber, int pageSize, Guid? locationId, string? status, CancellationToken ct)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        await using var db = await factory.CreateForCurrentAsync(ct);
        var query = db.StockCounts.AsNoTracking().AsQueryable();
        if (locationId.HasValue) query = query.Where(c => c.LocationId == locationId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(c => c.Status == status);

        var totalCount = await query.CountAsync(ct);
        var data = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(c => new StockCountDto(c.Id, c.CountNumber, c.LocationId, c.Status, c.Notes, c.CreatedAt, c.PostedAt))
            .ToListAsync(ct);

        return new PagedStockCountResult(
            data,
            new PaginationMeta(totalCount, pageNumber, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize)));
    }

    public async Task<StockCount> SaveLinesAsync(Guid id, IReadOnlyDictionary<Guid, decimal> counted, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var count = await db.StockCounts.Include(c => c.Lines).FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new InvalidOperationException("Stock count not found");
        if (count.Status != "draft") throw new InvalidOperationException($"A {count.Status} count can't be edited");
        foreach (var ln in count.Lines)
            if (counted.TryGetValue(ln.ProductId, out var c)) ln.CountedQty = c;
        await db.SaveChangesAsync(ct);
        return count;
    }

    /// <summary>Discard a draft count — it never touched stock, so just void + soft-delete it.</summary>
    public async Task DiscardAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var count = await db.StockCounts.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new InvalidOperationException("Stock count not found");
        if (count.Status != "draft") throw new InvalidOperationException($"A {count.Status} count can't be discarded");
        count.Status = "void";
        count.IsDeleted = true;
        await db.SaveChangesAsync(ct);
    }

    public async Task<StockCountResult> PostAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var count = await db.StockCounts.Include(c => c.Lines).FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new InvalidOperationException("Stock count not found");
        if (count.Status != "draft") throw new InvalidOperationException($"Count is already {count.Status}");

        var now = DateTime.UtcNow;
        var varianceLines = 0;
        decimal netVarianceQty = 0;
        foreach (var ln in count.Lines.Where(l => !l.IsDeleted))
        {
            ln.Variance = ln.CountedQty - ln.SystemQty;
            if (ln.Variance != 0) { varianceLines++; netVarianceQty += ln.Variance; }

            var stock = db.ProductStocks.Local.FirstOrDefault(s => s.ProductId == ln.ProductId && s.LocationId == count.LocationId)
                ?? await db.ProductStocks.FirstOrDefaultAsync(s => s.ProductId == ln.ProductId && s.LocationId == count.LocationId, ct);
            if (stock is null)
            {
                stock = new ProductStock { ProductId = ln.ProductId, LocationId = count.LocationId, QuantityOnHand = 0m, AverageCost = 0m };
                db.ProductStocks.Add(stock);
            }
            stock.QuantityOnHand = ln.CountedQty;   // the physical count is the truth
            stock.LastCountedAt = now;
        }
        count.Status = "posted";
        count.PostedAt = now;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return new StockCountResult(count.Id, count.CountNumber, count.Lines.Count, varianceLines, netVarianceQty);
    }

    private static async Task<string> NextNumberAsync(TenantDbContext db, CancellationToken ct)
    {
        var rows = await db.Database.SqlQuery<int>($@"
            INSERT INTO document_counters (tenant_id, doc_type, last_value)
            VALUES ({db.TenantId}, 'STOCKCOUNT', 1)
            ON CONFLICT (tenant_id, doc_type)
            DO UPDATE SET last_value = document_counters.last_value + 1
            RETURNING last_value AS ""Value""").ToListAsync(ct);
        return $"SC-{rows[0]:D4}";
    }
}

public record StockCountResult(Guid Id, string? CountNumber, int LineCount, int VarianceLines, decimal NetVarianceQty);
