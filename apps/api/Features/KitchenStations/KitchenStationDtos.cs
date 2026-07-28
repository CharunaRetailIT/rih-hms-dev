namespace Hms.Api.Features.KitchenStations;

public record PrinterTypeDto(
    Guid Id,
    string Code,
    string Name,
    int SortOrder,
    bool IsActive
);

public record SavePrinterTypeRequest(
    Guid? Id,
    string Code,
    string Name,
    int SortOrder = 0,
    bool IsActive = true
);

public record PagedPrinterTypeResult(
    List<PrinterTypeDto> Data,
    PaginationMeta Pagination
);

public record KitchenStationDto(
    Guid Id,
    Guid? LocationId,
    Guid PrinterTypeId,
    string PrinterTypeCode,
    string PrinterTypeName,
    string Code,
    string Name,
    string? PrinterName,
    int SortOrder,
    bool IsActive
);

public record SaveKitchenStationRequest(
    Guid? Id,
    Guid? LocationId,
    Guid PrinterTypeId,
    string Code,
    string Name,
    string? PrinterName = null,
    int SortOrder = 0,
    bool IsActive = true
);

public record PagedKitchenStationResult(
    List<KitchenStationDto> Data,
    PaginationMeta Pagination
);

public record PaginationMeta(
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);