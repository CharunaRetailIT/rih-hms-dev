namespace Hms.Api.Features.Charges;

public record ChargeDto(
    Guid Id,
    Guid ChargeTypeId,
    string ChargeTypeName,
    bool AppliesPerProduct,
    string Code,
    string Description,
    decimal? Percentage,
    decimal? Amount,
    bool IsActive
);

public record SaveChargeRequest(
    Guid? Id,
    Guid ChargeTypeId,
    string Code,
    string Description,
    decimal? Percentage,
    decimal? Amount,
    bool IsActive
);

public record PagedChargeResult(List<ChargeDto> Data, PaginationMeta Pagination);

public record PaginationMeta(int TotalCount, int PageNumber, int PageSize, int TotalPages);
