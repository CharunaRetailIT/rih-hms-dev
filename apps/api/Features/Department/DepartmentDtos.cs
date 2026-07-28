namespace Hms.Api.Features.Departments;

public record DepartmentDto(
    Guid Id,
    string Code,
    string Name,
    string? Remark,
    bool IsActive,
    Guid? LocationId,
    string? LocationName,
    string? DashboardColor
);

public record SaveDepartmentRequest(
    Guid? Id,
    string Code,
    string Name,
    string? Remark,
    bool IsActive,
    Guid? LocationId,
    string? DashboardColor
);

public record PagedDepartmentResult(List<DepartmentDto> Data, PaginationMeta Pagination);

public record PaginationMeta(int TotalCount, int PageNumber, int PageSize, int TotalPages);