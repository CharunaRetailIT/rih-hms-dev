namespace Hms.Api.Features.UnitsOfMeasure;
using global::Hms.Api.Domain;
using global::Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

public class UnitsOfMeasureService(ITenantDbContextFactory factory)
{
    private static readonly string[] AllowedDimensions = ["mass", "volume", "count"];

    public async Task<List<UnitOfMeasureDto>> ListAsync(CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        return await db.UnitsOfMeasure
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Dimension)
            .ThenBy(x => x.Code)
            .Select(x => new UnitOfMeasureDto(
                x.Id,
                x.Code,
                x.Name,
                x.Symbol,
                x.IsBaseUnit,
                x.Dimension,
                x.FactorToBase
            ))
            .ToListAsync(ct);
    }

    public async Task<PagedUnitOfMeasureResult> GetPagedAsync(int pageNumber, int pageSize, string? search, string? dimension, CancellationToken ct)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        await using var db = await factory.CreateForCurrentAsync(ct);

        var query = db.UnitsOfMeasure.AsNoTracking().Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(dimension))
            query = query.Where(x => x.Dimension == dimension);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = $"%{search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Code, s) || EF.Functions.ILike(x.Name, s));
        }

        var totalCount = await query.CountAsync(ct);

        var data = await query
            .OrderBy(x => x.Dimension).ThenBy(x => x.Code)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(x => new UnitOfMeasureDto(x.Id, x.Code, x.Name, x.Symbol, x.IsBaseUnit, x.Dimension, x.FactorToBase))
            .ToListAsync(ct);

        return new PagedUnitOfMeasureResult(
            data,
            new PaginationMeta(totalCount, pageNumber, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize)));
    }

    public async Task<UnitOfMeasureDto?> GetAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        return await db.UnitsOfMeasure
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new UnitOfMeasureDto(
                x.Id,
                x.Code,
                x.Name,
                x.Symbol,
                x.IsBaseUnit,
                x.Dimension,
                x.FactorToBase
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(bool Success, string? Error, UnitOfMeasureDto? Data)> SaveAsync(SaveUnitOfMeasureRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Code))
            return (false, "Code is required.", null);

        if (string.IsNullOrWhiteSpace(req.Name))
            return (false, "Name is required.", null);

        var dimension = req.Dimension.Trim().ToLowerInvariant();

        if (!AllowedDimensions.Contains(dimension))
            return (false, "Dimension must be mass, volume, or count.", null);

        if (req.FactorToBase <= 0)
            return (false, "Factor to base must be greater than zero.", null);

        await using var db = await factory.CreateForCurrentAsync(ct);

        var code = req.Code.Trim().ToUpperInvariant();

        var duplicate = await db.UnitsOfMeasure.AnyAsync(x =>
            !x.IsDeleted &&
            x.Code == code &&
            (!req.Id.HasValue || x.Id != req.Id.Value), ct);

        if (duplicate)
            return (false, "UOM code already exists.", null);

        var entity = req.Id.HasValue
            ? await db.UnitsOfMeasure.FirstOrDefaultAsync(x => x.Id == req.Id.Value && !x.IsDeleted, ct)
            : null;

        if (req.Id.HasValue && entity is null)
            return (false, "Unit of measure not found.", null);

        if (entity is null)
        {
            entity = new UnitOfMeasure();
            db.UnitsOfMeasure.Add(entity);
        }

        entity.Code = code;
        entity.Name = req.Name.Trim();
        entity.Symbol = string.IsNullOrWhiteSpace(req.Symbol) ? null : req.Symbol.Trim();
        entity.Dimension = dimension;
        entity.IsBaseUnit = req.IsBaseUnit;
        entity.FactorToBase = req.IsBaseUnit ? 1 : req.FactorToBase;

        if (entity.IsBaseUnit)
        {
            var others = await db.UnitsOfMeasure
                .Where(x => !x.IsDeleted && x.Dimension == dimension && x.Id != entity.Id && x.IsBaseUnit)
                .ToListAsync(ct);

            foreach (var other in others)
                other.IsBaseUnit = false;
        }

        await db.SaveChangesAsync(ct);

        return (true, null, new UnitOfMeasureDto(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.Symbol,
            entity.IsBaseUnit,
            entity.Dimension,
            entity.FactorToBase
        ));
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        var entity = await db.UnitsOfMeasure.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

        if (entity is null)
            return (false, "Unit of measure not found.");

        var usedByProducts = await db.Products.AnyAsync(x => x.UnitOfMeasureId == id && !x.IsDeleted, ct);

        if (usedByProducts)
            return (false, "Cannot delete this UOM because products are using it.");

        entity.IsDeleted = true;
        await db.SaveChangesAsync(ct);

        return (true, null);
    }
}
