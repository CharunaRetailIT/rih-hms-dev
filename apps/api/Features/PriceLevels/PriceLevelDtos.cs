namespace Hms.Api.Features.PriceLevels;

public record PriceLevelDto(
    Guid Id,
    Guid? LocationId,
    string Code,
    string Name,
    bool IsDefault,
    string? AppliesToOrderType,
    int SortOrder,
    bool IsActive
);

public record SavePriceLevelRequest(
    Guid? Id,
    Guid? LocationId,
    string Code,
    string Name,
    bool IsDefault = false,
    string? AppliesToOrderType = null,
    int SortOrder = 0,
    bool IsActive = true
);

public record PagedPriceLevelResult(
    List<PriceLevelDto> Data,
    PaginationMeta Pagination
);

public record PaginationMeta(
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);