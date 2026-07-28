namespace Hms.Api.Features.UnitsOfMeasure
{
    public record UnitOfMeasureDto(
     Guid Id,
     string Code,
     string Name,
     string? Symbol,
     bool IsBaseUnit,
     string Dimension,
     decimal FactorToBase
 );

    public record SaveUnitOfMeasureRequest(
        Guid? Id,
        string Code,
        string Name,
        string? Symbol,
        bool IsBaseUnit,
        string Dimension,
        decimal FactorToBase
    );

    public record PagedUnitOfMeasureResult(
        List<UnitOfMeasureDto> Data,
        PaginationMeta Pagination
    );

    public record PaginationMeta(
        int TotalCount,
        int PageNumber,
        int PageSize,
        int TotalPages
    );
}
