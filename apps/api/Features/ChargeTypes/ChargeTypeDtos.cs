namespace Hms.Api.Features.ChargeTypes;

public record ChargeTypeDto(
    Guid Id,
    string Code,
    string Name,
    bool AppliesPerProduct,
    int SortOrder,
    bool IsActive
);

public record SaveChargeTypeRequest(
    Guid? Id,
    string Code,
    string Name,
    bool AppliesPerProduct,
    int SortOrder,
    bool IsActive
);

public record PagedChargeTypeResult(List<ChargeTypeDto> Data, PaginationMeta Pagination);

public record PaginationMeta(int TotalCount, int PageNumber, int PageSize, int TotalPages);
