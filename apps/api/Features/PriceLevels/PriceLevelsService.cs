using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.PriceLevels;

public class PriceLevelsService(ITenantDbContextFactory factory)
{
    private static readonly string[] AllowedOrderTypes =
    [
        "dine_in",
        "takeaway",
        "delivery",
        "online",
        "third_party",
        "wholesale"
    ];

    public async Task<PagedPriceLevelResult> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search,
        Guid? locationId,
        string? appliesToOrderType,
        bool? isActive,
        CancellationToken ct)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        await using var db = await factory.CreateForCurrentAsync(ct);

        var query = db.PriceLevels
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (locationId.HasValue)
            query = query.Where(x => x.LocationId == locationId.Value || x.LocationId == null);

        if (!string.IsNullOrWhiteSpace(appliesToOrderType))
        {
            var type = appliesToOrderType.Trim().ToLowerInvariant();
            query = query.Where(x => x.AppliesToOrderType == type);
        }

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = $"%{search.Trim()}%";

            query = query.Where(x =>
                EF.Functions.ILike(x.Code, s) ||
                EF.Functions.ILike(x.Name, s) ||
                EF.Functions.ILike(x.AppliesToOrderType ?? "", s));
        }

        var totalCount = await query.CountAsync(ct);

        var data = await query
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PriceLevelDto(
                x.Id,
                x.LocationId,
                x.Code,
                x.Name,
                x.IsDefault,
                x.AppliesToOrderType,
                x.SortOrder,
                x.IsActive
            ))
            .ToListAsync(ct);

        return new PagedPriceLevelResult(
            data,
            new PaginationMeta(
                totalCount,
                pageNumber,
                pageSize,
                (int)Math.Ceiling(totalCount / (double)pageSize)
            )
        );
    }

    public async Task<List<PriceLevelDto>> ListAsync(Guid? locationId, bool? all, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        var query = db.PriceLevels
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (all != true)
            query = query.Where(x => x.IsActive);

        if (locationId.HasValue)
            query = query.Where(x => x.LocationId == locationId.Value || x.LocationId == null);

        return await query
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new PriceLevelDto(
                x.Id,
                x.LocationId,
                x.Code,
                x.Name,
                x.IsDefault,
                x.AppliesToOrderType,
                x.SortOrder,
                x.IsActive
            ))
            .ToListAsync(ct);
    }

    public async Task<PriceLevelDto?> GetAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        return await db.PriceLevels
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new PriceLevelDto(
                x.Id,
                x.LocationId,
                x.Code,
                x.Name,
                x.IsDefault,
                x.AppliesToOrderType,
                x.SortOrder,
                x.IsActive
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(bool Success, string? Error, PriceLevelDto? Data)> SaveAsync(
        SavePriceLevelRequest req,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Code))
            return (false, "Code is required.", null);

        if (string.IsNullOrWhiteSpace(req.Name))
            return (false, "Name is required.", null);

        var orderType = string.IsNullOrWhiteSpace(req.AppliesToOrderType)
            ? null
            : req.AppliesToOrderType.Trim().ToLowerInvariant();

        if (orderType is not null && !AllowedOrderTypes.Contains(orderType))
            return (false, "Invalid order type.", null);

        await using var db = await factory.CreateForCurrentAsync(ct);

        var code = req.Code.Trim().ToUpperInvariant();

        var duplicate = await db.PriceLevels.AnyAsync(x =>
            !x.IsDeleted &&
            x.Code == code &&
            x.LocationId == req.LocationId &&
            (!req.Id.HasValue || x.Id != req.Id.Value), ct);

        if (duplicate)
            return (false, "Price level code already exists for this location.", null);

        if (req.LocationId.HasValue)
        {
            var locationExists = await db.Locations.AnyAsync(x =>
                x.Id == req.LocationId.Value &&
                !x.IsDeleted &&
                x.IsActive, ct);

            if (!locationExists)
                return (false, "Selected location is invalid or inactive.", null);
        }

        var entity = req.Id.HasValue
            ? await db.PriceLevels.FirstOrDefaultAsync(x => x.Id == req.Id.Value && !x.IsDeleted, ct)
            : null;

        if (req.Id.HasValue && entity is null)
            return (false, "Price level not found.", null);

        if (entity is null)
        {
            entity = new PriceLevel
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            db.PriceLevels.Add(entity);
        }

        entity.LocationId = req.LocationId;
        entity.Code = code;
        entity.Name = req.Name.Trim();
        entity.IsDefault = req.IsDefault;
        entity.AppliesToOrderType = orderType;
        entity.SortOrder = req.SortOrder < 0 ? 0 : req.SortOrder;
        entity.IsActive = req.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        if (entity.IsDefault)
        {
            var otherDefaults = await db.PriceLevels
                .Where(x =>
                    !x.IsDeleted &&
                    x.Id != entity.Id &&
                    x.LocationId == entity.LocationId &&
                    x.IsDefault)
                .ToListAsync(ct);

            foreach (var item in otherDefaults)
            {
                item.IsDefault = false;
                item.UpdatedAt = DateTime.UtcNow;
            }
        }

        await db.SaveChangesAsync(ct);

        return (true, null, new PriceLevelDto(
            entity.Id,
            entity.LocationId,
            entity.Code,
            entity.Name,
            entity.IsDefault,
            entity.AppliesToOrderType,
            entity.SortOrder,
            entity.IsActive
        ));
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        var entity = await db.PriceLevels.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

        if (entity is null)
            return (false, "Price level not found.");

        if (entity.IsDefault)
            return (false, "Cannot delete the default price level.");

        var usedByProductPrices = await db.ProductPrices.AnyAsync(x =>
            !x.IsDeleted &&
            x.PriceLevelId == id, ct);

        if (usedByProductPrices)
            return (false, "Cannot delete this price level because product prices are linked to it.");

        entity.IsDeleted = true;
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return (true, null);
    }
}