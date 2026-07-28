using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Suppliers;

public class SuppliersService(ITenantDbContextFactory factory)
{
    public async Task<List<SupplierLookupDto>> LookupAsync(CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        return await db.Suppliers
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new SupplierLookupDto(x.Id, x.Code, x.Name))
            .ToListAsync(ct);
    }

    public async Task<PagedSupplierResult> GetPagedAsync(int pageNumber, int pageSize, string? search, Guid? supplierGroupId, Guid? supplierTypeId, bool? isActive, CancellationToken ct)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        await using var db = await factory.CreateForCurrentAsync(ct);

        var query = db.Suppliers
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (supplierGroupId.HasValue)
            query = query.Where(x => x.SupplierGroupId == supplierGroupId.Value);

        if (supplierTypeId.HasValue)
            query = query.Where(x => x.SupplierTypeId == supplierTypeId.Value);

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = $"%{search.Trim()}%";

            query = query.Where(x =>
                EF.Functions.ILike(x.Code, s) ||
                EF.Functions.ILike(x.Name, s) ||
                EF.Functions.ILike(x.ContactName ?? "", s) ||
                EF.Functions.ILike(x.Phone ?? "", s) ||
                EF.Functions.ILike(x.Email ?? "", s) ||
                EF.Functions.ILike(x.SupplierGroup != null ? x.SupplierGroup.Name : "", s) ||
                EF.Functions.ILike(x.SupplierType != null ? x.SupplierType.Name : "", s));
        }

        var totalCount = await query.CountAsync(ct);

        var data = await query
            .OrderBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new SupplierDto(
                x.Id,
                x.Code,
                x.Name,
                x.ContactName,
                x.Phone,
                x.Email,
                x.Address,
                x.PaymentTermsDays,
                x.IsActive,
                x.IsVatRegistered,
                x.VatRegistrationNumber,
                x.CountryCode,
                x.Province,
                x.District,
                x.PostalCode,
                x.SupplierGroupId,
                x.SupplierGroup != null ? x.SupplierGroup.Name : null,
                x.SupplierTypeId,
                x.SupplierType != null ? x.SupplierType.Name : null
            ))
            .ToListAsync(ct);

        return new PagedSupplierResult(
            data,
            new PaginationMeta(
                totalCount,
                pageNumber,
                pageSize,
                (int)Math.Ceiling(totalCount / (double)pageSize)
            )
        );
    }

    public async Task<SupplierDto?> GetAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        return await db.Suppliers
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new SupplierDto(
                x.Id,
                x.Code,
                x.Name,
                x.ContactName,
                x.Phone,
                x.Email,
                x.Address,
                x.PaymentTermsDays,
                x.IsActive,
                x.IsVatRegistered,
                x.VatRegistrationNumber,
                x.CountryCode,
                x.Province,
                x.District,
                x.PostalCode,
                x.SupplierGroupId,
                x.SupplierGroup != null ? x.SupplierGroup.Name : null,
                x.SupplierTypeId,
                x.SupplierType != null ? x.SupplierType.Name : null
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(bool Success, string? Error, SupplierDto? Data)> SaveAsync(SaveSupplierRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Code))
            return (false, "Supplier code is required.", null);

        if (string.IsNullOrWhiteSpace(req.Name))
            return (false, "Supplier name is required.", null);

        if (req.PaymentTermsDays < 0)
            return (false, "Payment terms cannot be negative.", null);

        await using var db = await factory.CreateForCurrentAsync(ct);

        var code = req.Code.Trim().ToUpperInvariant();

        var duplicate = await db.Suppliers.AnyAsync(x =>
            !x.IsDeleted &&
            x.Code == code &&
            (!req.Id.HasValue || x.Id != req.Id.Value), ct);

        if (duplicate)
            return (false, "Supplier code already exists.", null);

        SupplierGroup? group = null;
        SupplierType? type = null;

        if (req.SupplierGroupId.HasValue)
        {
            group = await db.SupplierGroups.FirstOrDefaultAsync(x =>
                x.Id == req.SupplierGroupId.Value && !x.IsDeleted, ct);

            if (group is null)
                return (false, "Supplier group not found.", null);
        }

        if (req.SupplierTypeId.HasValue)
        {
            type = await db.SupplierTypes.FirstOrDefaultAsync(x =>
                x.Id == req.SupplierTypeId.Value && !x.IsDeleted, ct);

            if (type is null)
                return (false, "Supplier type not found.", null);
        }

        var entity = req.Id.HasValue
            ? await db.Suppliers.FirstOrDefaultAsync(x => x.Id == req.Id.Value && !x.IsDeleted, ct)
            : null;

        if (req.Id.HasValue && entity is null)
            return (false, "Supplier not found.", null);

        if (entity is null)
        {
            entity = new Supplier
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            db.Suppliers.Add(entity);
        }

        entity.Code = code;
        entity.Name = req.Name.Trim();
        entity.ContactName = string.IsNullOrWhiteSpace(req.ContactName) ? null : req.ContactName.Trim();
        entity.Phone = string.IsNullOrWhiteSpace(req.Phone) ? null : req.Phone.Trim();
        entity.Email = string.IsNullOrWhiteSpace(req.Email) ? null : req.Email.Trim();
        entity.Address = string.IsNullOrWhiteSpace(req.Address) ? null : req.Address.Trim();
        entity.PaymentTermsDays = req.PaymentTermsDays;
        entity.IsActive = req.IsActive;
        entity.IsVatRegistered = req.IsVatRegistered;
        entity.VatRegistrationNumber = string.IsNullOrWhiteSpace(req.VatRegistrationNumber) ? null : req.VatRegistrationNumber.Trim();
        entity.CountryCode = string.IsNullOrWhiteSpace(req.CountryCode) ? null : req.CountryCode.Trim().ToUpperInvariant();
        entity.Province = string.IsNullOrWhiteSpace(req.Province) ? null : req.Province.Trim();
        entity.District = string.IsNullOrWhiteSpace(req.District) ? null : req.District.Trim();
        entity.PostalCode = string.IsNullOrWhiteSpace(req.PostalCode) ? null : req.PostalCode.Trim();
        entity.SupplierGroupId = req.SupplierGroupId;
        entity.SupplierTypeId = req.SupplierTypeId;
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return (true, null, new SupplierDto(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.ContactName,
            entity.Phone,
            entity.Email,
            entity.Address,
            entity.PaymentTermsDays,
            entity.IsActive,
            entity.IsVatRegistered,
            entity.VatRegistrationNumber,
            entity.CountryCode,
            entity.Province,
            entity.District,
            entity.PostalCode,
            entity.SupplierGroupId,
            group?.Name,
            entity.SupplierTypeId,
            type?.Name
        ));
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        var entity = await db.Suppliers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

        if (entity is null)
            return (false, "Supplier not found.");

        entity.IsDeleted = true;
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return (true, null);
    }

    public async Task<List<SupplierGroupDto>> GetGroupsAsync(CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        return await db.SupplierGroups
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .Select(x => new SupplierGroupDto(
                x.Id,
                x.Code,
                x.Name,
                x.Remark,
                x.IsActive
            ))
            .ToListAsync(ct);
    }

    public async Task<PagedSupplierGroupResult> GetGroupsPagedAsync(int pageNumber, int pageSize, string? search, bool? isActive, CancellationToken ct)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        await using var db = await factory.CreateForCurrentAsync(ct);

        var query = db.SupplierGroups.AsNoTracking().Where(x => !x.IsDeleted);

        if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = $"%{search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Code, s) || EF.Functions.ILike(x.Name, s));
        }

        var totalCount = await query.CountAsync(ct);

        var data = await query
            .OrderBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(x => new SupplierGroupDto(x.Id, x.Code, x.Name, x.Remark, x.IsActive))
            .ToListAsync(ct);

        return new PagedSupplierGroupResult(
            data,
            new PaginationMeta(totalCount, pageNumber, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize)));
    }

    public async Task<(bool Success, string? Error, SupplierGroupDto? Data)> SaveGroupAsync(SaveSupplierGroupRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Code))
            return (false, "Supplier group code is required.", null);

        if (string.IsNullOrWhiteSpace(req.Name))
            return (false, "Supplier group name is required.", null);

        await using var db = await factory.CreateForCurrentAsync(ct);

        var code = req.Code.Trim().ToUpperInvariant();

        var duplicate = await db.SupplierGroups.AnyAsync(x =>
            !x.IsDeleted &&
            x.Code == code &&
            (!req.Id.HasValue || x.Id != req.Id.Value), ct);

        if (duplicate)
            return (false, "Supplier group code already exists.", null);

        var entity = req.Id.HasValue
            ? await db.SupplierGroups.FirstOrDefaultAsync(x => x.Id == req.Id.Value && !x.IsDeleted, ct)
            : null;

        if (req.Id.HasValue && entity is null)
            return (false, "Supplier group not found.", null);

        if (entity is null)
        {
            entity = new SupplierGroup
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            db.SupplierGroups.Add(entity);
        }

        entity.Code = code;
        entity.Name = req.Name.Trim();
        entity.Remark = string.IsNullOrWhiteSpace(req.Remark) ? null : req.Remark.Trim();
        entity.IsActive = req.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return (true, null, new SupplierGroupDto(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.Remark,
            entity.IsActive
        ));
    }

    public async Task<(bool Success, string? Error)> DeleteGroupAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        var entity = await db.SupplierGroups.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

        if (entity is null)
            return (false, "Supplier group not found.");

        var used = await db.Suppliers.AnyAsync(x =>
            x.SupplierGroupId == id && !x.IsDeleted, ct);

        if (used)
            return (false, "Cannot delete supplier group because suppliers are using it.");

        entity.IsDeleted = true;
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return (true, null);
    }

    public async Task<List<SupplierTypeDto>> GetTypesAsync(CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        return await db.SupplierTypes
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .Select(x => new SupplierTypeDto(
                x.Id,
                x.Code,
                x.Name,
                x.Remark,
                x.IsActive
            ))
            .ToListAsync(ct);
    }

    public async Task<PagedSupplierTypeResult> GetTypesPagedAsync(int pageNumber, int pageSize, string? search, bool? isActive, CancellationToken ct)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        await using var db = await factory.CreateForCurrentAsync(ct);

        var query = db.SupplierTypes.AsNoTracking().Where(x => !x.IsDeleted);

        if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = $"%{search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Code, s) || EF.Functions.ILike(x.Name, s));
        }

        var totalCount = await query.CountAsync(ct);

        var data = await query
            .OrderBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(x => new SupplierTypeDto(x.Id, x.Code, x.Name, x.Remark, x.IsActive))
            .ToListAsync(ct);

        return new PagedSupplierTypeResult(
            data,
            new PaginationMeta(totalCount, pageNumber, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize)));
    }

    public async Task<(bool Success, string? Error, SupplierTypeDto? Data)> SaveTypeAsync(SaveSupplierTypeRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Code))
            return (false, "Supplier type code is required.", null);

        if (string.IsNullOrWhiteSpace(req.Name))
            return (false, "Supplier type name is required.", null);

        await using var db = await factory.CreateForCurrentAsync(ct);

        var code = req.Code.Trim().ToUpperInvariant();

        var duplicate = await db.SupplierTypes.AnyAsync(x =>
            !x.IsDeleted &&
            x.Code == code &&
            (!req.Id.HasValue || x.Id != req.Id.Value), ct);

        if (duplicate)
            return (false, "Supplier type code already exists.", null);

        var entity = req.Id.HasValue
            ? await db.SupplierTypes.FirstOrDefaultAsync(x => x.Id == req.Id.Value && !x.IsDeleted, ct)
            : null;

        if (req.Id.HasValue && entity is null)
            return (false, "Supplier type not found.", null);

        if (entity is null)
        {
            entity = new SupplierType
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            db.SupplierTypes.Add(entity);
        }

        entity.Code = code;
        entity.Name = req.Name.Trim();
        entity.Remark = string.IsNullOrWhiteSpace(req.Remark) ? null : req.Remark.Trim();
        entity.IsActive = req.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return (true, null, new SupplierTypeDto(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.Remark,
            entity.IsActive
        ));
    }

    public async Task<(bool Success, string? Error)> DeleteTypeAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        var entity = await db.SupplierTypes.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

        if (entity is null)
            return (false, "Supplier type not found.");

        var used = await db.Suppliers.AnyAsync(x =>
            x.SupplierTypeId == id && !x.IsDeleted, ct);

        if (used)
            return (false, "Cannot delete supplier type because suppliers are using it.");

        entity.IsDeleted = true;
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return (true, null);
    }
}