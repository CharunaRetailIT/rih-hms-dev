using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Charges;

public class ChargesService(ITenantDbContextFactory factory)
{
    public async Task<PagedChargeResult> GetPagedAsync(int pageNumber, int pageSize, string? search, Guid? chargeTypeId, bool? isActive, CancellationToken ct)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        await using var db = await factory.CreateForCurrentAsync(ct);

        var query = db.Charges
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (chargeTypeId.HasValue)
            query = query.Where(x => x.ChargeTypeId == chargeTypeId.Value);

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = $"%{search.Trim()}%";

            query = query.Where(x =>
                EF.Functions.ILike(x.Code, s) ||
                EF.Functions.ILike(x.Description, s));
        }

        var totalCount = await query.CountAsync(ct);

        var data = await query
            .OrderBy(x => x.ChargeType!.SortOrder)
            .ThenBy(x => x.Description)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ChargeDto(
                x.Id,
                x.ChargeTypeId,
                x.ChargeType!.Name,
                x.ChargeType.AppliesPerProduct,
                x.Code,
                x.Description,
                x.Percentage,
                x.Amount,
                x.IsActive
            ))
            .ToListAsync(ct);

        return new PagedChargeResult(
            data,
            new PaginationMeta(
                totalCount,
                pageNumber,
                pageSize,
                (int)Math.Ceiling(totalCount / (double)pageSize)
            )
        );
    }

    public async Task<ChargeDto?> GetAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        return await db.Charges
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new ChargeDto(
                x.Id,
                x.ChargeTypeId,
                x.ChargeType!.Name,
                x.ChargeType.AppliesPerProduct,
                x.Code,
                x.Description,
                x.Percentage,
                x.Amount,
                x.IsActive
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(bool Success, string? Error, ChargeDto? Data)> SaveAsync(SaveChargeRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Code))
            return (false, "Code is required.", null);

        if (string.IsNullOrWhiteSpace(req.Description))
            return (false, "Description is required.", null);

        var hasPercentage = req.Percentage.HasValue && req.Percentage.Value != 0;
        var hasAmount = req.Amount.HasValue && req.Amount.Value != 0;

        if (hasPercentage == hasAmount)
            return (false, "Set either a percentage or a flat amount, not both.", null);

        await using var db = await factory.CreateForCurrentAsync(ct);

        var chargeTypeExists = await db.ChargeTypes.AnyAsync(x => x.Id == req.ChargeTypeId && !x.IsDeleted, ct);

        if (!chargeTypeExists)
            return (false, "Invalid charge type.", null);

        var code = req.Code.Trim().ToUpperInvariant();

        var duplicate = await db.Charges.AnyAsync(x =>
            !x.IsDeleted &&
            x.Code == code &&
            (!req.Id.HasValue || x.Id != req.Id.Value), ct);

        if (duplicate)
            return (false, "Charge code already exists.", null);

        var entity = req.Id.HasValue
            ? await db.Charges.FirstOrDefaultAsync(x => x.Id == req.Id.Value && !x.IsDeleted, ct)
            : null;

        if (req.Id.HasValue && entity is null)
            return (false, "Charge not found.", null);

        if (entity is null)
        {
            entity = new Charge();
            db.Charges.Add(entity);
        }

        entity.ChargeTypeId = req.ChargeTypeId;
        entity.Code = code;
        entity.Description = req.Description.Trim();
        entity.Percentage = hasPercentage ? req.Percentage : null;
        entity.Amount = hasAmount ? req.Amount : null;
        entity.IsActive = req.IsActive;

        await db.SaveChangesAsync(ct);

        var chargeType = await db.ChargeTypes.AsNoTracking().FirstAsync(x => x.Id == entity.ChargeTypeId, ct);

        return (true, null, new ChargeDto(
            entity.Id,
            entity.ChargeTypeId,
            chargeType.Name,
            chargeType.AppliesPerProduct,
            entity.Code,
            entity.Description,
            entity.Percentage,
            entity.Amount,
            entity.IsActive
        ));
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateForCurrentAsync(ct);

        var entity = await db.Charges.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

        if (entity is null)
            return (false, "Charge not found.");

        var usedByProducts = await db.ProductCharges.AnyAsync(x => !x.IsDeleted && x.ChargeId == entity.Id, ct);

        if (usedByProducts)
            return (false, "Cannot delete this charge because products are linked to it.");

        entity.IsDeleted = true;
        entity.IsActive = false;

        await db.SaveChangesAsync(ct);

        return (true, null);
    }
}
