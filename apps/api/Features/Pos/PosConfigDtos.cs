namespace Hms.Api.Features.Pos;

public record TourOperatorCompanyDto(
    Guid Id, string Code, string Name, string? Address1, string? Address2, string? CountryCode, string? Mobile, string? Telephone,
    string? FaxNo, string? Email, string? WebAddress, string? ContactPerson, decimal CommissionPercent, decimal CommissionAmount, bool IsActive
);

public record TourOperatorDto(
    Guid Id, string Code, string Name, decimal CommissionPercent, string Kind, bool IsActive,
    Guid? CompanyId, string? Title, string? Nic, string? Address1, string? Address2, string? Address3, string? CountryCode,
    string? Mobile, string? Email, decimal Amount, string? Remarks
);

public record PagedTourOperatorCompanyResult(List<TourOperatorCompanyDto> Data, PaginationMeta Pagination);
public record PagedTourOperatorResult(List<TourOperatorDto> Data, PaginationMeta Pagination);

public record PaginationMeta(int TotalCount, int PageNumber, int PageSize, int TotalPages);
