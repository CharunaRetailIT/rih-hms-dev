namespace Hms.Api.Features.Promotions;

public record PromotionLineDto(
    Guid Id, Guid? ProductId, Guid? CategoryId, decimal MinQty, decimal BillFrom, decimal? BillTo,
    Guid? GetProductId, decimal GetQty, decimal DiscountPercent, decimal DiscountAmount, decimal? BundlePrice
);

public record PromotionDto(
    Guid Id, string Code, string Name, string PromoType, bool IsActive, bool AutoApply, int Priority,
    DateOnly? StartsOn, DateOnly? EndsOn, int DaysMask, TimeOnly? StartTime, TimeOnly? EndTime,
    string? AppliesToOrderType, Guid? AppliesToCategoryId, string? DisplayMessage, List<PromotionLineDto> Lines
);

public record PagedPromotionResult(
    List<PromotionDto> Data,
    PaginationMeta Pagination
);

public record PaginationMeta(
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);
