using Hms.Api.Domain;
using Hms.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Hms.Api.Features.Promotions;

public static class PromotionsEndpoint
{
    public static IEndpointRouteBuilder MapPromotionEndpoints(this IEndpointRouteBuilder app)
    {
        // Reads: any authenticated user (POS shows live promos). Writes: BackOffice.
        var g = app.MapGroup("/api/v1/promotions").WithTags("Promotions")
            .AddEndpointFilter(new Hms.Api.Infrastructure.RequireFeatureFilter("promotions"));

        g.MapGet("", async (ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var list = await db.Promotions.AsNoTracking().Include(p => p.Lines)
                .OrderBy(p => p.Priority).ThenBy(p => p.Code)
                .Select(p => new
                {
                    p.Id, p.Code, p.Name, p.PromoType, p.IsActive, p.AutoApply, p.Priority,
                    p.StartsOn, p.EndsOn, p.DaysMask, p.StartTime, p.EndTime, p.AppliesToOrderType, p.AppliesToCategoryId, p.DisplayMessage,
                    Lines = p.Lines.Where(l => !l.IsDeleted).Select(l => new
                    {
                        l.Id, l.ProductId, l.CategoryId, l.MinQty, l.BillFrom, l.BillTo, l.GetProductId, l.GetQty,
                        l.DiscountPercent, l.DiscountAmount, l.BundlePrice,
                    }),
                })
                .ToListAsync(ct);
            return Results.Ok(list);
        }).WithName("Promotions.List");

        g.MapGet("/paged", async (ITenantDbContextFactory factory, CancellationToken ct,
            int pageNumber = 1, int pageSize = 10, string? search = null, string? status = null, string? promoType = null) =>
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            await using var db = await factory.CreateForCurrentAsync(ct);
            var query = db.Promotions.AsNoTracking().Where(p => !p.IsDeleted);

            if (status == "active") query = query.Where(p => p.IsActive);
            else if (status == "inactive") query = query.Where(p => !p.IsActive);

            if (!string.IsNullOrWhiteSpace(promoType))
                query = query.Where(p => p.PromoType == promoType);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = $"%{search.Trim()}%";
                query = query.Where(p => EF.Functions.ILike(p.Code, s) || EF.Functions.ILike(p.Name, s));
            }

            var totalCount = await query.CountAsync(ct);

            var promos = await query
                .Include(p => p.Lines)
                .OrderBy(p => p.Priority).ThenBy(p => p.Code)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .ToListAsync(ct);

            var data = promos.Select(p => new PromotionDto(
                p.Id, p.Code, p.Name, p.PromoType, p.IsActive, p.AutoApply, p.Priority,
                p.StartsOn, p.EndsOn, p.DaysMask, p.StartTime, p.EndTime, p.AppliesToOrderType, p.AppliesToCategoryId, p.DisplayMessage,
                p.Lines.Where(l => !l.IsDeleted).Select(l => new PromotionLineDto(
                    l.Id, l.ProductId, l.CategoryId, l.MinQty, l.BillFrom, l.BillTo,
                    l.GetProductId, l.GetQty, l.DiscountPercent, l.DiscountAmount, l.BundlePrice)).ToList()
            )).ToList();

            return Results.Ok(new PagedPromotionResult(
                data,
                new PaginationMeta(totalCount, pageNumber, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize))));
        }).WithName("Promotions.ListPaged");

        g.MapPut("", async (PromotionInput i, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(i.Code) || string.IsNullOrWhiteSpace(i.Name))
                return Results.BadRequest(new { error = "Code and name are required" });
            if (i.PromoType is not ("product_discount" or "time_based" or "bill_value" or "buy_x_get_y" or "bundle" or "lowest_price"))
                return Results.BadRequest(new { error = "Unknown promotion type" });

            var inLines = i.Lines ?? new();
            if (i.PromoType == "time_based")
            {
                if (i.StartTime is null || i.EndTime is null)
                    return Results.BadRequest(new { error = "A time-based (happy hour) promotion needs a start and end time" });
                if (!inLines.Any(l => l.DiscountPercent > 0 || l.DiscountAmount > 0))
                    return Results.BadRequest(new { error = "A time-based promotion needs a discount (% or amount)" });
            }
            if (i.PromoType == "buy_x_get_y" &&
                !inLines.Any(l => l.ProductId is not null && l.GetProductId is not null && l.MinQty > 0 && l.GetQty > 0))
                return Results.BadRequest(new { error = "Buy-X-get-Y needs a line with a buy product + buy qty and a get product + get qty" });
            if (i.PromoType == "bundle" &&
                (!inLines.Any(l => l.ProductId is not null && l.MinQty > 0) || !inLines.Any(l => l.BundlePrice is > 0)))
                return Results.BadRequest(new { error = "A bundle needs component products (each with a qty) and a bundle price" });
            if (i.PromoType == "product_discount" &&
                !inLines.Any(l => (l.ProductId is not null || l.CategoryId is not null) && (l.DiscountPercent > 0 || l.DiscountAmount > 0)))
                return Results.BadRequest(new { error = "An item discount needs at least one line with a product or category and a discount" });

            await using var db = await factory.CreateForCurrentAsync(ct);
            var code = i.Code.Trim().ToUpperInvariant();
            var p = i.Id is Guid id
                ? await db.Promotions.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id, ct)
                : await db.Promotions.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Code == code, ct);
            if (p is null) { p = new Promotion { Code = code }; db.Promotions.Add(p); }

            p.Code = code; p.Name = i.Name.Trim(); p.PromoType = i.PromoType;
            p.IsActive = i.IsActive; p.AutoApply = i.AutoApply; p.Priority = i.Priority;
            p.StartsOn = i.StartsOn; p.EndsOn = i.EndsOn; p.DaysMask = i.DaysMask == 0 ? 127 : i.DaysMask;
            p.StartTime = i.StartTime; p.EndTime = i.EndTime;
            p.AppliesToOrderType = string.IsNullOrWhiteSpace(i.AppliesToOrderType) ? null : i.AppliesToOrderType.Trim();
            p.AppliesToCategoryId = i.AppliesToCategoryId;
            p.DisplayMessage = i.DisplayMessage;

            // replace-set the lines
            foreach (var old in p.Lines.Where(l => !l.IsDeleted)) old.IsDeleted = true;
            foreach (var ln in i.Lines ?? new())
                p.Lines.Add(new PromotionLine
                {
                    ProductId = ln.ProductId, CategoryId = ln.CategoryId, MinQty = ln.MinQty, BillFrom = ln.BillFrom, BillTo = ln.BillTo,
                    GetProductId = ln.GetProductId, GetQty = ln.GetQty,
                    DiscountPercent = ln.DiscountPercent, DiscountAmount = ln.DiscountAmount, BundlePrice = ln.BundlePrice,
                });

            await db.SaveChangesAsync(ct);
            return Results.Ok(new { p.Id, p.Code, p.Name });
        }).WithName("Promotions.Set").RequireAuthorization("BackOffice");

        g.MapDelete("/{id:guid}", async (Guid id, ITenantDbContextFactory factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateForCurrentAsync(ct);
            var p = await db.Promotions.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (p is null) return Results.NotFound();
            p.IsDeleted = true; p.IsActive = false;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).WithName("Promotions.Delete").RequireAuthorization("BackOffice");

        return app;
    }
}

public record PromotionInput(
    Guid? Id, string Code, string Name, string PromoType,
    bool IsActive = true, bool AutoApply = true, int Priority = 0,
    DateOnly? StartsOn = null, DateOnly? EndsOn = null, int DaysMask = 127,
    TimeOnly? StartTime = null, TimeOnly? EndTime = null,
    string? AppliesToOrderType = null, Guid? AppliesToCategoryId = null, string? DisplayMessage = null,
    List<PromotionLineInput>? Lines = null);

public record PromotionLineInput(
    Guid? ProductId = null, Guid? CategoryId = null, decimal MinQty = 0, decimal BillFrom = 0, decimal? BillTo = null,
    Guid? GetProductId = null, decimal GetQty = 0,
    decimal DiscountPercent = 0, decimal DiscountAmount = 0, decimal? BundlePrice = null);
