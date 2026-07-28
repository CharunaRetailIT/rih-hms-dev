using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.ServingUnits;

public class ServingUnitsService(ITenantDbContextFactory factory)
{
    public async Task<PagedServingUnitResult> GetPagedAsync(int pageNumber, int pageSize, string? search, bool? isActive, CancellationToken ct)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        await using var db = await factory.CreateForCurrentAsync(ct);

        var query = db.ServingUnits
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
            .Select(x => new ServingUnitDto(
                x.Id,
                x.Code,
                x.Name,
                x.SortOrder,
                x.IsActive
            ))
            .ToListAsync(ct);

        return new PagedServingUnitResult(
            data,
            new PaginationMeta(
                totalCount,
                pageNumber,
                pageSize,
                (int)Math.Ceiling(totalCount / (double)pageSize)
            )
        );
    }

    public async Task<List<ServingUnitDto>> ListAsync(bool? all, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        var query = db.ServingUnits
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (all != true)
            query = query.Where(x => x.IsActive);

        return await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new ServingUnitDto(
                x.Id,
                x.Code,
                x.Name,
                x.SortOrder,
                x.IsActive
            ))
            .ToListAsync(ct);
    }

    public async Task<ServingUnitDto?> GetAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        return await db.ServingUnits
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new ServingUnitDto(
                x.Id,
                x.Code,
                x.Name,
                x.SortOrder,
                x.IsActive
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(bool Success, string? Error, ServingUnitDto? Data)> SaveAsync(SaveServingUnitRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Code))
            return (false, "Code is required.", null);

        if (string.IsNullOrWhiteSpace(req.Name))
            return (false, "Name is required.", null);

        await using var db = await factory.CreateForCurrentAsync(ct);

        var code = req.Code.Trim().ToUpperInvariant();

        var duplicate = await db.ServingUnits.AnyAsync(x =>
            !x.IsDeleted &&
            x.Code == code &&
            (!req.Id.HasValue || x.Id != req.Id.Value), ct);

        if (duplicate)
            return (false, "Serving unit code already exists.", null);

        var entity = req.Id.HasValue
            ? await db.ServingUnits.FirstOrDefaultAsync(x => x.Id == req.Id.Value && !x.IsDeleted, ct)
            : null;

        if (req.Id.HasValue && entity is null)
            return (false, "Serving unit not found.", null);

        if (entity is null)
        {
            entity = new ServingUnit
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            db.ServingUnits.Add(entity);
        }

        entity.Code = code;
        entity.Name = req.Name.Trim();
        entity.SortOrder = req.SortOrder < 0 ? 0 : req.SortOrder;
        entity.IsActive = req.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return (true, null, new ServingUnitDto(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.SortOrder,
            entity.IsActive
        ));
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        var entity = await db.ServingUnits.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

        if (entity is null)
            return (false, "Serving unit not found.");

        var usedByVariant = await db.ProductVariants.AnyAsync(x =>
            !x.IsDeleted &&
            x.ServingUnitId == id, ct);

        if (usedByVariant)
            return (false, "Cannot delete this serving unit because it is used by product variants.");

        entity.IsDeleted = true;
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return (true, null);
    }
}