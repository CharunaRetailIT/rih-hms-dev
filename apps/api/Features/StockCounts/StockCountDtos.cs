namespace Hms.Api.Features.StockCounts;

public record StockCountDto(
    Guid Id, string? CountNumber, Guid LocationId, string Status, string? Notes, DateTime CreatedAt, DateTime? PostedAt
);

public record PagedStockCountResult(
    List<StockCountDto> Data,
    PaginationMeta Pagination
);

public record PaginationMeta(
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);
