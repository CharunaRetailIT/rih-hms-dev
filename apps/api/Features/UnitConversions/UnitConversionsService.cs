using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.UnitConversions;

public class UnitConversionsService(ITenantDbContextFactory factory)
{
    public async Task<List<UnitConversionDto>> ListAsync(CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        return await db.UnitConversions
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.UnitOfMeasure.Code)
            .ThenBy(x => x.SubUnitOfMeasure.Code)
            .Select(x => new UnitConversionDto(
                x.Id,
                x.UnitOfMeasureId,
                x.UnitOfMeasure.Code,
                x.UnitOfMeasure.Name,
                x.SubUnitOfMeasureId,
                x.SubUnitOfMeasure.Code,
                x.SubUnitOfMeasure.Name,
                x.SubUnitValue,
                x.BaseUnitValue
            ))
            .ToListAsync(ct);
    }

    public async Task<(bool Success, string? Error, UnitConversionDto? Data)> SaveAsync(SaveUnitConversionRequest req, CancellationToken ct)
    {
        try
        {
            if (req.UnitOfMeasureId == Guid.Empty)
                return (false, "Base unit is required.", null);

            if (req.SubUnitOfMeasureId == Guid.Empty)
                return (false, "Sub unit is required.", null);

            if (req.UnitOfMeasureId == req.SubUnitOfMeasureId)
                return (false, "Base unit and sub unit cannot be same.", null);

            if (req.SubUnitValue <= 0)
                return (false, "Sub unit value must be greater than zero.", null);

            if (req.BaseUnitValue <= 0)
                return (false, "Base unit value must be greater than zero.", null);

            await using var db = await factory.CreateForCurrentAsync(ct);

            var baseUnit = await db.UnitsOfMeasure.FirstOrDefaultAsync(x => x.Id == req.UnitOfMeasureId && !x.IsDeleted, ct);
            if (baseUnit is null)
                return (false, "Base unit not found.", null);

            var subUnit = await db.UnitsOfMeasure.FirstOrDefaultAsync(x => x.Id == req.SubUnitOfMeasureId && !x.IsDeleted, ct);
            if (subUnit is null)
                return (false, "Sub unit not found.", null);

            if (baseUnit.Dimension != subUnit.Dimension)
                return (false, "Base unit and sub unit must be in same dimension.", null);

            var duplicate = await db.UnitConversions.AnyAsync(x =>
                !x.IsDeleted &&
                x.UnitOfMeasureId == req.UnitOfMeasureId &&
                x.SubUnitOfMeasureId == req.SubUnitOfMeasureId &&
                (!req.Id.HasValue || x.Id != req.Id.Value), ct);

            if (duplicate)
                return (false, "This conversion already exists.", null);

            var entity = req.Id.HasValue
                ? await db.UnitConversions.FirstOrDefaultAsync(x => x.Id == req.Id.Value && !x.IsDeleted, ct)
                : null;

            if (req.Id.HasValue && entity is null)
                return (false, "Conversion not found.", null);

            if (entity is null)
            {
                entity = new UnitConversion
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    TenantId = baseUnit.TenantId
                };

                db.UnitConversions.Add(entity);
            }

            entity.UnitOfMeasureId = req.UnitOfMeasureId;
            entity.SubUnitOfMeasureId = req.SubUnitOfMeasureId;
            entity.SubUnitValue = req.SubUnitValue;
            entity.BaseUnitValue = req.BaseUnitValue;
            entity.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync(ct);

            return (true, null, new UnitConversionDto(
                entity.Id,
                entity.UnitOfMeasureId,
                baseUnit.Code,
                baseUnit.Name,
                entity.SubUnitOfMeasureId,
                subUnit.Code,
                subUnit.Name,
                entity.SubUnitValue,
                entity.BaseUnitValue
            ));
        }
        catch(Exception ex)
        {
            throw new Exception(
                $"Failed to save unit conversion. BaseUnitId: {req.UnitOfMeasureId}, SubUnitId: {req.SubUnitOfMeasureId}",
                ex);
        }
       
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        var entity = await db.UnitConversions.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

        if (entity is null)
            return (false, "Conversion not found.");

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return (true, null);
    }

    public async Task<(bool Success, string? Error, UnitConversionResult? Data)> ConvertAsync(UnitConversionRequest req, CancellationToken ct)
    {
        if (req.Quantity < 0)
            return (false, "Quantity cannot be negative.", null);

        await using var db = await factory.CreateForCurrentAsync(ct);

        var from = await db.UnitsOfMeasure.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == req.FromUnitId && !x.IsDeleted, ct);

        var to = await db.UnitsOfMeasure.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == req.ToUnitId && !x.IsDeleted, ct);

        if (from is null || to is null)
            return (false, "Invalid unit selected.", null);

        if (from.Dimension != to.Dimension)
            return (false, "Cannot convert between different dimensions.", null);

        if (from.FactorToBase <= 0 || to.FactorToBase <= 0)
            return (false, "Invalid conversion factor.", null);

        var converted = req.Quantity * from.FactorToBase / to.FactorToBase;

        return (true, null, new UnitConversionResult(
            from.Id,
            from.Code,
            to.Id,
            to.Code,
            req.Quantity,
            converted,
            from.Dimension
        ));
    }

    public async Task<PagedUnitConversionResult> GetPagedAsync(int pageNumber, int pageSize, string? search, Guid? unitOfMeasureId, CancellationToken ct)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        await using var db = await factory.CreateForCurrentAsync(ct);

        var query = db.UnitConversions
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (unitOfMeasureId.HasValue)
            query = query.Where(x => x.UnitOfMeasureId == unitOfMeasureId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = $"%{search.Trim()}%";

            query = query.Where(x =>
                EF.Functions.ILike(x.UnitOfMeasure.Code, s) ||
                EF.Functions.ILike(x.UnitOfMeasure.Name, s) ||
                EF.Functions.ILike(x.SubUnitOfMeasure.Code, s) ||
                EF.Functions.ILike(x.SubUnitOfMeasure.Name, s));
        }

        var totalCount = await query.CountAsync(ct);

        var data = await query
            .OrderBy(x => x.UnitOfMeasure.Code)
            .ThenBy(x => x.SubUnitOfMeasure.Code)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UnitConversionDto(
                x.Id,
                x.UnitOfMeasureId,
                x.UnitOfMeasure.Code,
                x.UnitOfMeasure.Name,
                x.SubUnitOfMeasureId,
                x.SubUnitOfMeasure.Code,
                x.SubUnitOfMeasure.Name,
                x.SubUnitValue,
                x.BaseUnitValue
            ))
            .ToListAsync(ct);

        return new PagedUnitConversionResult(
            data,
            new PaginationMeta(totalCount, pageNumber, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize))
        );
    }
}