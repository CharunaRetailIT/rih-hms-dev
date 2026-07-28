namespace Hms.Api.Features.UnitConversions;

public record UnitConversionDto(
    Guid Id,
    Guid UnitOfMeasureId,
    string UnitCode,
    string UnitName,
    Guid SubUnitOfMeasureId,
    string SubUnitCode,
    string SubUnitName,
    decimal SubUnitValue,
    decimal BaseUnitValue
);

public record SaveUnitConversionRequest(
    Guid? Id,
    Guid UnitOfMeasureId,
    Guid SubUnitOfMeasureId,
    decimal SubUnitValue,
    decimal BaseUnitValue
);

public record UnitConversionRequest(Guid FromUnitId, Guid ToUnitId, decimal Quantity);

public record UnitConversionResult(
    Guid FromUnitId,
    string FromCode,
    Guid ToUnitId,
    string ToCode,
    decimal Quantity,
    decimal ConvertedQuantity,
    string Dimension
);

public record PagedUnitConversionResult(List<UnitConversionDto> Data, PaginationMeta Pagination);

public record PaginationMeta(int TotalCount, int PageNumber, int PageSize, int TotalPages);