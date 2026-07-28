using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.KitchenStations;

public class KitchenStationsService(ITenantDbContextFactory factory)
{
    public async Task<PagedPrinterTypeResult> GetPrinterTypesPagedAsync(
        int pageNumber,
        int pageSize,
        string? search,
        bool? isActive,
        CancellationToken ct)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        await using var db = await factory.CreateForCurrentAsync(ct);

        var query = db.PrinterTypes
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
            .Select(x => new PrinterTypeDto(
                x.Id,
                x.Code,
                x.Name,
                x.SortOrder,
                x.IsActive
            ))
            .ToListAsync(ct);

        return new PagedPrinterTypeResult(
            data,
            new PaginationMeta(
                totalCount,
                pageNumber,
                pageSize,
                (int)Math.Ceiling(totalCount / (double)pageSize)
            )
        );
    }

    public async Task<List<PrinterTypeDto>> ListPrinterTypesAsync(bool? all, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        var query = db.PrinterTypes
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (all != true)
            query = query.Where(x => x.IsActive);

        return await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new PrinterTypeDto(
                x.Id,
                x.Code,
                x.Name,
                x.SortOrder,
                x.IsActive
            ))
            .ToListAsync(ct);
    }

    public async Task<PrinterTypeDto?> GetPrinterTypeAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        return await db.PrinterTypes
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new PrinterTypeDto(
                x.Id,
                x.Code,
                x.Name,
                x.SortOrder,
                x.IsActive
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(bool Success, string? Error, PrinterTypeDto? Data)> SavePrinterTypeAsync(
        SavePrinterTypeRequest req,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Code))
            return (false, "Code is required.", null);

        if (string.IsNullOrWhiteSpace(req.Name))
            return (false, "Name is required.", null);

        await using var db = await factory.CreateForCurrentAsync(ct);

        var code = req.Code.Trim().ToUpperInvariant();

        var duplicate = await db.PrinterTypes.AnyAsync(x =>
            !x.IsDeleted &&
            x.Code == code &&
            (!req.Id.HasValue || x.Id != req.Id.Value), ct);

        if (duplicate)
            return (false, "Printer type code already exists.", null);

        var entity = req.Id.HasValue
            ? await db.PrinterTypes.FirstOrDefaultAsync(x => x.Id == req.Id.Value && !x.IsDeleted, ct)
            : null;

        if (req.Id.HasValue && entity is null)
            return (false, "Printer type not found.", null);

        if (entity is null)
        {
            entity = new PrinterType
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            db.PrinterTypes.Add(entity);
        }

        entity.Code = code;
        entity.Name = req.Name.Trim();
        entity.SortOrder = req.SortOrder < 0 ? 0 : req.SortOrder;
        entity.IsActive = req.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return (true, null, new PrinterTypeDto(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.SortOrder,
            entity.IsActive
        ));
    }

    public async Task<(bool Success, string? Error)> DeletePrinterTypeAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        var entity = await db.PrinterTypes.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

        if (entity is null)
            return (false, "Printer type not found.");

        var usedByStation = await db.KitchenStations.AnyAsync(x =>
            !x.IsDeleted &&
            x.PrinterTypeId == id, ct);

        if (usedByStation)
            return (false, "Cannot delete this printer type because kitchen stations are using it.");

        entity.IsDeleted = true;
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return (true, null);
    }

    public async Task<PagedKitchenStationResult> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search,
        Guid? locationId,
        Guid? printerTypeId,
        bool? isActive,
        CancellationToken ct)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        await using var db = await factory.CreateForCurrentAsync(ct);

        var query = db.KitchenStations
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (locationId.HasValue)
            query = query.Where(x => x.LocationId == locationId.Value || x.LocationId == null);

        if (printerTypeId.HasValue)
            query = query.Where(x => x.PrinterTypeId == printerTypeId.Value);

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = $"%{search.Trim()}%";

            query = query.Where(x =>
                EF.Functions.ILike(x.Code, s) ||
                EF.Functions.ILike(x.Name, s) ||
                EF.Functions.ILike(x.PrinterName ?? "", s));
        }

        var totalCount = await query.CountAsync(ct);

        var data = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new KitchenStationDto(
                x.Id,
                x.LocationId,
                x.PrinterTypeId,
                x.PrinterType!.Code,
                x.PrinterType!.Name,
                x.Code,
                x.Name,
                x.PrinterName,
                x.SortOrder,
                x.IsActive
            ))
            .ToListAsync(ct);

        return new PagedKitchenStationResult(
            data,
            new PaginationMeta(
                totalCount,
                pageNumber,
                pageSize,
                (int)Math.Ceiling(totalCount / (double)pageSize)
            )
        );
    }

    public async Task<List<KitchenStationDto>> ListAsync(Guid? locationId, bool? all, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        var query = db.KitchenStations
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (all != true)
            query = query.Where(x => x.IsActive);

        if (locationId.HasValue)
            query = query.Where(x => x.LocationId == locationId.Value || x.LocationId == null);

        return await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new KitchenStationDto(
                x.Id,
                x.LocationId,
                x.PrinterTypeId,
                x.PrinterType!.Code,
                x.PrinterType!.Name,
                x.Code,
                x.Name,
                x.PrinterName,
                x.SortOrder,
                x.IsActive
            ))
            .ToListAsync(ct);
    }

    public async Task<KitchenStationDto?> GetAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        return await db.KitchenStations
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new KitchenStationDto(
                x.Id,
                x.LocationId,
                x.PrinterTypeId,
                x.PrinterType!.Code,
                x.PrinterType!.Name,
                x.Code,
                x.Name,
                x.PrinterName,
                x.SortOrder,
                x.IsActive
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(bool Success, string? Error, KitchenStationDto? Data)> SaveAsync(
        SaveKitchenStationRequest req,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Code))
            return (false, "Code is required.", null);

        if (string.IsNullOrWhiteSpace(req.Name))
            return (false, "Name is required.", null);

        await using var db = await factory.CreateForCurrentAsync(ct);

        var printerTypeExists = await db.PrinterTypes.AnyAsync(x =>
            x.Id == req.PrinterTypeId &&
            !x.IsDeleted &&
            x.IsActive, ct);

        if (!printerTypeExists)
            return (false, "Selected printer type is invalid or inactive.", null);

        if (req.LocationId.HasValue)
        {
            var locationExists = await db.Locations.AnyAsync(x =>
                x.Id == req.LocationId.Value &&
                !x.IsDeleted &&
                x.IsActive, ct);

            if (!locationExists)
                return (false, "Selected location is invalid or inactive.", null);
        }

        var code = req.Code.Trim().ToUpperInvariant();

        var duplicate = await db.KitchenStations.AnyAsync(x =>
            !x.IsDeleted &&
            x.Code == code &&
            x.LocationId == req.LocationId &&
            (!req.Id.HasValue || x.Id != req.Id.Value), ct);

        if (duplicate)
            return (false, "Kitchen station code already exists for this location.", null);

        var entity = req.Id.HasValue
            ? await db.KitchenStations.FirstOrDefaultAsync(x => x.Id == req.Id.Value && !x.IsDeleted, ct)
            : null;

        if (req.Id.HasValue && entity is null)
            return (false, "Kitchen station not found.", null);

        if (entity is null)
        {
            entity = new KitchenStation
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            db.KitchenStations.Add(entity);
        }

        entity.LocationId = req.LocationId;
        entity.PrinterTypeId = req.PrinterTypeId;
        entity.Code = code;
        entity.Name = req.Name.Trim();
        entity.PrinterName = string.IsNullOrWhiteSpace(req.PrinterName) ? null : req.PrinterName.Trim();
        entity.SortOrder = req.SortOrder < 0 ? 0 : req.SortOrder;
        entity.IsActive = req.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        var saved = await GetAsync(entity.Id, ct);

        return (true, null, saved);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        var entity = await db.KitchenStations.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

        if (entity is null)
            return (false, "Kitchen station not found.");

        var usedByProducts = await db.ProductKitchenStations.AnyAsync(x =>
            !x.IsDeleted &&
            x.KitchenStationId == entity.Id, ct);

        if (usedByProducts)
            return (false, "Cannot delete this kitchen station because products are linked to it.");

        entity.IsDeleted = true;
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return (true, null);
    }
}