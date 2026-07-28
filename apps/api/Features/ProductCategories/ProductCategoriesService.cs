using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Products;

/// <summary>
/// Menu/product categories: simple one-level parent-child hierarchy used to group
/// products on the POS screen and in reporting. Activation status is managed
/// separately from create/edit so a category can be retired without losing its
/// configuration (color, icon, sort order, etc).
/// </summary>
public class ProductCategoriesService(ITenantDbContextFactory factory)
{
    public async Task<List<Category>> ListAsync(CancellationToken ct, bool includeInactive = false)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var q = db.Categories.AsNoTracking();
        if (!includeInactive) q = q.Where(c => c.IsActive);
        return await q.OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync(ct);
    }

    public async Task<Category?> GetAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        return await db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    /// <summary>Create a new category. Always starts active.</summary>
    public async Task<Category> CreateAsync(CategoryInput input, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var code = NormalizeCode(input.Code);
        if (await db.Categories.AnyAsync(c => c.Code == code, ct))
            throw new InvalidOperationException($"Category code '{code}' already exists");

        var parent = await ValidateParentAsync(db, input.ParentId, selfId: null, ct);

        var c = new Category
        {
            Code = code,
            Name = NormalizeName(input.Name),
            Description = NormalizeText(input.Description),
            ParentId = parent?.Id,
            ColorHex = input.ColorHex,
            IconName = input.IconName,
            SortOrder = input.SortOrder,
            IsActive = true,
        };
        db.Categories.Add(c);
        await db.SaveChangesAsync(ct);
        return c;
    }

    /// <summary>Edit an existing category's details. Does not touch IsActive — see ActivateAsync/DeactivateAsync.</summary>
    public async Task<Category> EditAsync(Guid id, CategoryInput input, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var c = await db.Categories.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new InvalidOperationException("Category not found");

        var code = NormalizeCode(input.Code);
        if (await db.Categories.AnyAsync(x => x.Code == code && x.Id != id, ct))
            throw new InvalidOperationException($"Category code '{code}' already exists");

        var parent = await ValidateParentAsync(db, input.ParentId, selfId: id, ct);

        c.Code = code;
        c.Name = NormalizeName(input.Name);
        c.Description = NormalizeText(input.Description);
        c.ParentId = parent?.Id;
        c.ColorHex = input.ColorHex;
        c.IconName = input.IconName;
        c.SortOrder = input.SortOrder;

        await db.SaveChangesAsync(ct);
        return c;
    }

    /// <summary>Reactivate a category. Blocked if its parent is currently inactive.</summary>
    public async Task<Category> ActivateAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var c = await db.Categories.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new InvalidOperationException("Category not found");

        if (c.ParentId is Guid parentId)
        {
            var parent = await db.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == parentId, ct);
            if (parent is not null && !parent.IsActive)
                throw new InvalidOperationException("Activate the parent category first");
        }

        c.IsActive = true;
        await db.SaveChangesAsync(ct);
        return c;
    }

    /// <summary>Deactivate a category. Blocked while it still has active children or active products attached.</summary>
    public async Task<Category> DeactivateAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);
        var c = await db.Categories.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new InvalidOperationException("Category not found");

        if (await db.Categories.AnyAsync(x => x.ParentId == id && x.IsActive, ct))
            throw new InvalidOperationException("Deactivate or reassign child categories first");
        if (await db.Products.AnyAsync(p => p.CategoryId == id && p.IsActive, ct))
            throw new InvalidOperationException("Reassign products out of this category first");

        c.IsActive = false;
        await db.SaveChangesAsync(ct);
        return c;
    }

    /// <summary>Shared parent validation: must exist, must be active, can't be self, only one level deep.</summary>
    private static async Task<Category?> ValidateParentAsync(TenantDbContext db, Guid? parentId, Guid? selfId, CancellationToken ct)
    {
        if (parentId is not Guid pid) return null;
        if (selfId is Guid sid && pid == sid)
            throw new InvalidOperationException("A category cannot be its own parent");

        var parent = await db.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == pid, ct)
            ?? throw new InvalidOperationException("Parent category not found");
        if (parent.ParentId is not null)
            throw new InvalidOperationException("Categories only support one level of nesting");
        if (!parent.IsActive)
            throw new InvalidOperationException("Cannot assign a child to an inactive parent");
        return parent;
    }

    private static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new InvalidOperationException("Code is required");
        return code.Trim().ToUpperInvariant();
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new InvalidOperationException("Name is required");
        return name.Trim();
    }

    private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public record CategoryInput(string Code, string Name, Guid? ParentId = null, string? Description = null,
    string? ColorHex = null, string? IconName = null, int SortOrder = 0);