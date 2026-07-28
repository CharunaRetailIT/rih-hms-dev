using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Loyalty;

public class LoyaltyCardSchemeService(ITenantDbContextFactory factory)
{
    public async Task<List<LoyaltyCardSchemeDto>> ListAsync(bool all, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var query = db.LoyaltyCardSchemes.AsNoTracking().Include(x => x.Tiers).AsQueryable();
        if (!all) query = query.Where(x => x.IsActive);
        var rows = await query.OrderBy(x => x.Name).ToListAsync(ct);
        var promos = await db.Promotions.AsNoTracking().ToDictionaryAsync(p => p.Id, p => p.Name, ct);
        return rows.Select(x => ToDto(x, promos)).ToList();
    }

    public async Task<PagedLoyaltyCardSchemeResult> GetPagedAsync(int pageNumber, int pageSize, string? search, string? type, bool? isActive, CancellationToken ct)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        await using var db = await factory.CreateForCurrentAsync(ct);
        var query = db.LoyaltyCardSchemes.AsNoTracking().Include(x => x.Tiers).AsQueryable();

        if (!string.IsNullOrWhiteSpace(type)) query = query.Where(x => x.Type == type);
        if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = $"%{search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Code, s) || EF.Functions.ILike(x.Name, s));
        }

        var totalCount = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Name).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        var promos = await db.Promotions.AsNoTracking().ToDictionaryAsync(p => p.Id, p => p.Name, ct);

        return new PagedLoyaltyCardSchemeResult(
            rows.Select(x => ToDto(x, promos)).ToList(),
            new PaginationMeta(totalCount, pageNumber, pageSize, (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize))));
    }

    public async Task<LoyaltyCardSchemeDto> UpsertAsync(LoyaltyCardSchemeInput input, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        var code = (input.Code ?? "").Trim();
        var name = (input.Name ?? "").Trim();
        var type = input.Type is "discount" or "promotion" ? input.Type : "points";
        if (code == "" || name == "") throw new InvalidOperationException("Code and name are required.");
        if (type == "promotion" && input.PromotionId is null) throw new InvalidOperationException("Select a promotion for a Promotion-type scheme.");

        LoyaltyCardScheme scheme;
        if (input.Id is Guid id)
            scheme = await db.LoyaltyCardSchemes.Include(x => x.Tiers).FirstOrDefaultAsync(x => x.Id == id, ct)
                     ?? throw new InvalidOperationException("Loyalty card scheme not found.");
        else
        {
            scheme = new LoyaltyCardScheme();
            db.LoyaltyCardSchemes.Add(scheme);
        }

        scheme.Code = code;
        scheme.Name = name;
        scheme.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
        scheme.Type = type;
        scheme.DiscountPercent = type == "discount" ? Math.Clamp(input.DiscountPercent ?? 0m, 0m, 100m) : 0m;
        scheme.PromotionId = type == "promotion" ? input.PromotionId : null;
        scheme.IsActive = input.IsActive ?? true;

        // Points tiers: replace-all-on-save — the list is small and always fully resent by the form.
        scheme.Tiers.Clear();
        if (type == "points" && input.Tiers is { Count: > 0 })
        {
            var order = 0;
            foreach (var t in input.Tiers)
                scheme.Tiers.Add(new LoyaltyCardSchemeTier
                {
                    BillFromValue = Math.Max(0, t.BillFromValue),
                    BillToValue = Math.Max(0, t.BillToValue),
                    Increment = Math.Max(0, t.Increment),
                    Points = Math.Max(0, t.Points),
                    SortOrder = order++,
                });
        }

        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) { throw new InvalidOperationException("A loyalty card scheme with that code already exists."); }

        var promos = await db.Promotions.AsNoTracking().ToDictionaryAsync(p => p.Id, p => p.Name, ct);
        return ToDto(scheme, promos);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var scheme = await db.LoyaltyCardSchemes.FirstOrDefaultAsync(x => x.Id == id, ct)
                     ?? throw new InvalidOperationException("Loyalty card scheme not found.");
        if (await db.Customers.AnyAsync(x => x.LoyaltyCardSchemeId == id, ct))
            throw new InvalidOperationException("Cannot remove a scheme that still has customers enrolled in it.");
        scheme.IsDeleted = true;
        await db.SaveChangesAsync(ct);
    }

    private static LoyaltyCardSchemeDto ToDto(LoyaltyCardScheme x, Dictionary<Guid, string> promos) => new(
        x.Id, x.Code, x.Name, x.Description, x.Type,
        x.DiscountPercent, x.PromotionId, x.PromotionId is { } pid && promos.TryGetValue(pid, out var pn) ? pn : null, x.IsActive,
        x.Tiers.OrderBy(t => t.SortOrder)
            .Select(t => new LoyaltyCardSchemeTierDto(t.Id, t.BillFromValue, t.BillToValue, t.Increment, t.Points, t.SortOrder))
            .ToList());
}
