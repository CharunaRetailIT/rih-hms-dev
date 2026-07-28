using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Departments;

public class DepartmentsService(ITenantDbContextFactory factory)
{
    public async Task<PagedDepartmentResult> GetPagedAsync(int pageNumber, int pageSize, string? search, Guid? locationId, bool? isActive, CancellationToken ct)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        await using var db = await factory.CreateForCurrentAsync(ct);

        var query = db.Departments
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (locationId.HasValue)
            query = query.Where(x => x.LocationId == locationId.Value);

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = $"%{search.Trim()}%";

            query = query.Where(x =>
                EF.Functions.ILike(x.Code, s) ||
                EF.Functions.ILike(x.Name, s) ||
                EF.Functions.ILike(x.Remark ?? "", s) ||
                EF.Functions.ILike(x.Location!.Name, s));
        }

        var totalCount = await query.CountAsync(ct);

        var data = await query
            .OrderBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DepartmentDto(
                x.Id,
                x.Code,
                x.Name,
                x.Remark,
                x.IsActive,
                x.LocationId,
                x.Location != null ? x.Location.Name : null,
                x.DashboardColor
            ))
            .ToListAsync(ct);

        return new PagedDepartmentResult(
            data,
            new PaginationMeta(
                totalCount,
                pageNumber,
                pageSize,
                (int)Math.Ceiling(totalCount / (double)pageSize)
            )
        );
    }

    public async Task<DepartmentDto?> GetAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        return await db.Departments
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new DepartmentDto(
                x.Id,
                x.Code,
                x.Name,
                x.Remark,
                x.IsActive,
                x.LocationId,
                x.Location != null ? x.Location.Name : null,
                x.DashboardColor
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(bool Success, string? Error, DepartmentDto? Data)> SaveAsync(SaveDepartmentRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Code))
            return (false, "Code is required.", null);

        if (string.IsNullOrWhiteSpace(req.Name))
            return (false, "Name is required.", null);

        await using var db = await factory.CreateForCurrentAsync(ct);

        var code = req.Code.Trim().ToUpperInvariant();

        var duplicate = await db.Departments.AnyAsync(x =>
            !x.IsDeleted &&
            x.Code == code &&
            (!req.Id.HasValue || x.Id != req.Id.Value), ct);

        if (duplicate)
            return (false, "Department code already exists.", null);

        Location? location = null;

        if (req.LocationId.HasValue)
        {
            location = await db.Locations.FirstOrDefaultAsync(x => x.Id == req.LocationId.Value && !x.IsDeleted, ct);

            if (location is null)
                return (false, "Location not found.", null);
        }

        var entity = req.Id.HasValue
            ? await db.Departments.FirstOrDefaultAsync(x => x.Id == req.Id.Value && !x.IsDeleted, ct)
            : null;

        if (req.Id.HasValue && entity is null)
            return (false, "Department not found.", null);

        if (entity is null)
        {
            entity = new Department
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                TenantId = location?.TenantId ?? Guid.Empty
            };

            db.Departments.Add(entity);
        }

        entity.Code = code;
        entity.Name = req.Name.Trim();
        entity.Remark = string.IsNullOrWhiteSpace(req.Remark) ? null : req.Remark.Trim();
        entity.IsActive = req.IsActive;
        entity.LocationId = req.LocationId;
        entity.DashboardColor = string.IsNullOrWhiteSpace(req.DashboardColor) ? null : req.DashboardColor.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return (true, null, new DepartmentDto(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.Remark,
            entity.IsActive,
            entity.LocationId,
            location?.Name,
            entity.DashboardColor
        ));
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        var entity = await db.Departments.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

        if (entity is null)
            return (false, "Department not found.");

        entity.IsDeleted = true;
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return (true, null);
    }
}