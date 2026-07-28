using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.ChargeTypes;

public class ChargeTypesService(ITenantDbContextFactory factory)
{
    public async Task<PagedChargeTypeResult> GetPagedAsync(int pageNumber, int pageSize, string? search, bool? isActive, CancellationToken ct)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        await using var db = await factory.CreateForCurrentAsync(ct);

        var query = db.ChargeTypes
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = $"%{search.Trim()}%";

            query = query.Where(x =>
                EF.Functions.ILike(x.Code, s) ||
                EF.Functions.ILike(x.Name, s));
        }

        var totalCount = await query.CountAsync(ct);

        var data = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ChargeTypeDto(
                x.Id,
                x.Code,
                x.Name,
                x.AppliesPerProduct,
                x.SortOrder,
                x.IsActive
            ))
            .ToListAsync(ct);

        return new PagedChargeTypeResult(
            data,
            new PaginationMeta(
                totalCount,
                pageNumber,
                pageSize,
                (int)Math.Ceiling(totalCount / (double)pageSize)
            )
        );
    }

    public async Task<ChargeTypeDto?> GetAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        return await db.ChargeTypes
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new ChargeTypeDto(
                x.Id,
                x.Code,
                x.Name,
                x.AppliesPerProduct,
                x.SortOrder,
                x.IsActive
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(bool Success, string? Error, ChargeTypeDto? Data)> SaveAsync(SaveChargeTypeRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Code))
            return (false, "Code is required.", null);

        if (string.IsNullOrWhiteSpace(req.Name))
            return (false, "Name is required.", null);

        await using var db = await factory.CreateForCurrentAsync(ct);

        var code = req.Code.Trim().ToUpperInvariant();

        var duplicate = await db.ChargeTypes.AnyAsync(x =>
            !x.IsDeleted &&
            x.Code == code &&
            (!req.Id.HasValue || x.Id != req.Id.Value), ct);

        if (duplicate)
            return (false, "Charge type code already exists.", null);

        var entity = req.Id.HasValue
            ? await db.ChargeTypes.FirstOrDefaultAsync(x => x.Id == req.Id.Value && !x.IsDeleted, ct)
            : null;

        if (req.Id.HasValue && entity is null)
            return (false, "Charge type not found.", null);

        if (entity is null)
        {
            entity = new ChargeType();
            db.ChargeTypes.Add(entity);
        }

        entity.Code = code;
        entity.Name = req.Name.Trim();
        entity.AppliesPerProduct = req.AppliesPerProduct;
        entity.SortOrder = req.SortOrder;
        entity.IsActive = req.IsActive;

        await db.SaveChangesAsync(ct);

        return (true, null, new ChargeTypeDto(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.AppliesPerProduct,
            entity.SortOrder,
            entity.IsActive
        ));
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        var entity = await db.ChargeTypes.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

        if (entity is null)
            return (false, "Charge type not found.");

        var usedByCharges = await db.Charges.AnyAsync(x => !x.IsDeleted && x.ChargeTypeId == entity.Id, ct);

        if (usedByCharges)
            return (false, "Cannot delete this charge type because charges are linked to it.");

        entity.IsDeleted = true;
        entity.IsActive = false;

        await db.SaveChangesAsync(ct);

        return (true, null);
    }
}
