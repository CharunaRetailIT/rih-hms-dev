using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Modifiers;

/// <summary>
/// Modifier groups + items (add-ons / sizes / options), and attaching reusable
/// groups to products. The order-line pricing of selections lives in OrderService.
/// </summary>
public class ModifierService(ITenantDbContextFactory factory)
{
    public async Task<List<ModifierGroup>> ListGroupsAsync(CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        return await db.ModifierGroups.Include(g => g.Items).AsNoTracking()
            .OrderBy(g => g.SortOrder).ThenBy(g => g.Name).ToListAsync(ct);
    }

    public async Task<PagedModifierGroupResult> GetPagedAsync(int pageNumber, int pageSize, string? search, CancellationToken ct)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        await using var db = await factory.CreateForCurrentAsync(ct);

        var query = db.ModifierGroups.AsNoTracking().Where(g => !g.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = $"%{search.Trim()}%";
            query = query.Where(g => EF.Functions.ILike(g.Name, s));
        }

        var totalCount = await query.CountAsync(ct);

        var groups = await query
            .Include(g => g.Items)
            .OrderBy(g => g.SortOrder).ThenBy(g => g.Name)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);

        var data = groups.Select(g => new ModifierGroupDto(
            g.Id, g.Name, g.MinSelect, g.MaxSelect, g.IsRequired, g.SortOrder, g.IsActive,
            g.Items.Where(i => !i.IsDeleted && i.IsActive).OrderBy(i => i.SortOrder)
                .Select(i => new ModifierItemDto(i.Id, i.Name, i.PriceDelta, i.IsDefault, i.SortOrder)).ToList()
        )).ToList();

        return new PagedModifierGroupResult(
            data,
            new PaginationMeta(totalCount, pageNumber, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize)));
    }

    public async Task<ModifierGroup> SetGroupAsync(SetModifierGroupInput i, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(i.Name)) throw new InvalidOperationException("Group name is required");
        if (i.Items is null || i.Items.Count == 0) throw new InvalidOperationException("A group needs at least one item");
        if (i.MaxSelect < 0 || i.MinSelect < 0) throw new InvalidOperationException("Min/max must be ≥ 0");
        if (i.MaxSelect > 0 && i.MinSelect > i.MaxSelect) throw new InvalidOperationException("Min cannot exceed max");

        await using var db = await factory.CreateForCurrentAsync(ct);
        ModifierGroup group;
        if (i.Id is Guid id)
        {
            group = await db.ModifierGroups.Include(g => g.Items).FirstOrDefaultAsync(g => g.Id == id, ct)
                ?? throw new InvalidOperationException("Modifier group not found");
            db.ModifierItems.RemoveRange(group.Items);
            group.Items.Clear();
        }
        else
        {
            group = new ModifierGroup();
            db.ModifierGroups.Add(group);
        }
        group.Name = i.Name.Trim();
        group.MinSelect = i.MinSelect; group.MaxSelect = i.MaxSelect;
        group.IsRequired = i.IsRequired; group.SortOrder = i.SortOrder; group.IsActive = true;
        var order = 0;
        foreach (var it in i.Items)
        {
            if (string.IsNullOrWhiteSpace(it.Name)) throw new InvalidOperationException("Item name is required");
            group.Items.Add(new ModifierItem
            {
                Name = it.Name.Trim(), PriceDelta = it.PriceDelta, IsDefault = it.IsDefault,
                SortOrder = it.SortOrder == 0 ? order : it.SortOrder, IsActive = true,
            });
            order++;
        }
        await db.SaveChangesAsync(ct);
        return group;
    }

    public async Task DeleteGroupAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var group = await db.ModifierGroups.Include(g => g.Items).FirstOrDefaultAsync(g => g.Id == id, ct)
            ?? throw new InvalidOperationException("Modifier group not found");
        group.IsDeleted = true; group.IsActive = false;
        foreach (var it in group.Items) it.IsDeleted = true;
        // detach from any products
        var links = await db.ProductModifierGroups.Where(l => l.GroupId == id).ToListAsync(ct);
        db.ProductModifierGroups.RemoveRange(links);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>The modifier groups (with items) attached to a product — for the POS.</summary>
    public async Task<List<ModifierGroup>> GetProductModifiersAsync(Guid productId, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var links = await db.ProductModifierGroups.AsNoTracking()
            .Where(l => l.ProductId == productId).OrderBy(l => l.SortOrder).ToListAsync(ct);
        if (links.Count == 0) return new();
        var ids = links.Select(l => l.GroupId).ToList();
        var groups = await db.ModifierGroups.Include(g => g.Items).AsNoTracking()
            .Where(g => ids.Contains(g.Id) && g.IsActive).ToListAsync(ct);
        return links
            .Select(l => groups.FirstOrDefault(g => g.Id == l.GroupId))
            .Where(g => g is not null).Select(g => g!).ToList();
    }

    /// <summary>Replace the set of modifier groups attached to a product.</summary>
    public async Task SetProductGroupsAsync(Guid productId, List<Guid> groupIds, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        if (!await db.Products.AnyAsync(p => p.Id == productId, ct))
            throw new InvalidOperationException("Product not found");
        var existing = await db.ProductModifierGroups.Where(l => l.ProductId == productId).ToListAsync(ct);
        db.ProductModifierGroups.RemoveRange(existing);
        var order = 0;
        foreach (var gid in (groupIds ?? new()).Distinct())
            db.ProductModifierGroups.Add(new ProductModifierGroup { ProductId = productId, GroupId = gid, SortOrder = order++ });
        await db.SaveChangesAsync(ct);
    }
}

public record SetModifierGroupInput(Guid? Id, string Name, int MinSelect, int MaxSelect, bool IsRequired,
    int SortOrder, List<ModifierItemInput> Items);
public record ModifierItemInput(string Name, decimal PriceDelta, bool IsDefault = false, int SortOrder = 0);
public record SetProductModifiersInput(List<Guid> GroupIds);
