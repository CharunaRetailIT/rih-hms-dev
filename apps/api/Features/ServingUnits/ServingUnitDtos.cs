namespace Hms.Api.Features.ServingUnits;

public record ServingUnitDto(
    Guid Id,
    string Code,
    string Name,
    int SortOrder,
    bool IsActive
);

public record SaveServingUnitRequest(
    Guid? Id,
    string Code,
    string Name,
    int SortOrder = 0,
    bool IsActive = true
);

public record PagedServingUnitResult(
    List<ServingUnitDto> Data,
    PaginationMeta Pagination
);

public record PaginationMeta(
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);