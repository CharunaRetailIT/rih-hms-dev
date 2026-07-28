using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Locations;

public class LocationsService(ITenantDbContextFactory factory)
{
    private static readonly string[] AllowedTypes =
    [
        "head_office",
        "central_kitchen",
        "warehouse",
        "outlet"
    ];

    public async Task<PagedLocationResult> GetPagedAsync(int pageNumber, int pageSize, string? search, string? locationType, bool? isActive, CancellationToken ct)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        await using var db = await factory.CreateForCurrentAsync(ct);

        var query = db.Locations
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(locationType))
            query = query.Where(x => x.LocationType == locationType);

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = $"%{search.Trim()}%";

            query = query.Where(x =>
                EF.Functions.ILike(x.Code, s) ||
                EF.Functions.ILike(x.Name, s) ||
                EF.Functions.ILike(x.City, s) ||
                EF.Functions.ILike(x.PhoneE164 ?? "", s));
        }

        var totalCount = await query.CountAsync(ct);

        var data = await query
            .OrderBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new LocationDto(
                x.Id,
                x.Code,
                x.Name,
                x.AddressLine1,
                x.AddressLine2,
                x.City,
                x.CountryCode,
                x.TimeZone,
                x.Currency,
                x.PhoneE164,
                x.IsActive,
                x.LocationType,
                x.CanSell,
                x.CanProduce,
                x.CanStock,
                x.VatRegistrationNumber,
                x.DefaultPrepMinutes
            ))
            .ToListAsync(ct);

        return new PagedLocationResult(
            data,
            new PaginationMeta(
                totalCount,
                pageNumber,
                pageSize,
                (int)Math.Ceiling(totalCount / (double)pageSize)
            )
        );
    }

    public async Task<LocationDto?> GetAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        return await db.Locations
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new LocationDto(
                x.Id,
                x.Code,
                x.Name,
                x.AddressLine1,
                x.AddressLine2,
                x.City,
                x.CountryCode,
                x.TimeZone,
                x.Currency,
                x.PhoneE164,
                x.IsActive,
                x.LocationType,
                x.CanSell,
                x.CanProduce,
                x.CanStock,
                x.VatRegistrationNumber,
                x.DefaultPrepMinutes
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(bool Success, string? Error, LocationDto? Data)> SaveAsync(SaveLocationRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Code))
            return (false, "Code is required.", null);

        if (string.IsNullOrWhiteSpace(req.Name))
            return (false, "Name is required.", null);

        if (string.IsNullOrWhiteSpace(req.AddressLine1))
            return (false, "Address line 1 is required.", null);

        if (string.IsNullOrWhiteSpace(req.City))
            return (false, "City is required.", null);

        var type = req.LocationType.Trim().ToLowerInvariant();

        if (!AllowedTypes.Contains(type))
            return (false, "Invalid location type.", null);

        await using var db = await factory.CreateForCurrentAsync(ct);

        var code = req.Code.Trim().ToUpperInvariant();

        var duplicate = await db.Locations.AnyAsync(x =>
            !x.IsDeleted &&
            x.Code == code &&
            (!req.Id.HasValue || x.Id != req.Id.Value), ct);

        if (duplicate)
            return (false, "Location code already exists.", null);

        var entity = req.Id.HasValue
            ? await db.Locations.FirstOrDefaultAsync(x => x.Id == req.Id.Value && !x.IsDeleted, ct)
            : null;

        if (req.Id.HasValue && entity is null)
            return (false, "Location not found.", null);

        if (entity is null)
        {
            entity = new Location
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            db.Locations.Add(entity);
        }

        entity.Code = code;
        entity.Name = req.Name.Trim();
        entity.AddressLine1 = req.AddressLine1.Trim();
        entity.AddressLine2 = string.IsNullOrWhiteSpace(req.AddressLine2) ? null : req.AddressLine2.Trim();
        entity.City = req.City.Trim();
        entity.CountryCode = string.IsNullOrWhiteSpace(req.CountryCode) ? "LK" : req.CountryCode.Trim().ToUpperInvariant();
        entity.TimeZone = string.IsNullOrWhiteSpace(req.TimeZone) ? "Asia/Colombo" : req.TimeZone.Trim();
        entity.Currency = string.IsNullOrWhiteSpace(req.Currency) ? "LKR" : req.Currency.Trim().ToUpperInvariant();
        entity.PhoneE164 = string.IsNullOrWhiteSpace(req.PhoneE164) ? null : req.PhoneE164.Trim();
        entity.IsActive = req.IsActive;
        entity.LocationType = type;
        entity.CanSell = req.CanSell;
        entity.CanProduce = req.CanProduce;
        entity.CanStock = req.CanStock;
        entity.VatRegistrationNumber = string.IsNullOrWhiteSpace(req.VatRegistrationNumber) ? null : req.VatRegistrationNumber.Trim();
        entity.DefaultPrepMinutes = req.DefaultPrepMinutes < 0 ? 0 : req.DefaultPrepMinutes;
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return (true, null, new LocationDto(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.AddressLine1,
            entity.AddressLine2,
            entity.City,
            entity.CountryCode,
            entity.TimeZone,
            entity.Currency,
            entity.PhoneE164,
            entity.IsActive,
            entity.LocationType,
            entity.CanSell,
            entity.CanProduce,
            entity.CanStock,
            entity.VatRegistrationNumber,
            entity.DefaultPrepMinutes
        ));
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        var entity = await db.Locations.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

        if (entity is null)
            return (false, "Location not found.");

        entity.IsDeleted = true;
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return (true, null);
    }
}