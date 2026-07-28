namespace Hms.Api.Features.Suppliers;

public record SupplierDto(
    Guid Id,
    string Code,
    string Name,
    string? ContactName,
    string? Phone,
    string? Email,
    string? Address,
    int PaymentTermsDays,
    bool IsActive,
    bool IsVatRegistered,
    string? VatRegistrationNumber,
    string? CountryCode,
    string? Province,
    string? District,
    string? PostalCode,
    Guid? SupplierGroupId,
    string? SupplierGroupName,
    Guid? SupplierTypeId,
    string? SupplierTypeName
);

public record SaveSupplierRequest(
    Guid? Id,
    string Code,
    string Name,
    string? ContactName,
    string? Phone,
    string? Email,
    string? Address,
    int PaymentTermsDays,
    bool IsActive,
    bool IsVatRegistered,
    string? VatRegistrationNumber,
    string? CountryCode,
    string? Province,
    string? District,
    string? PostalCode,
    Guid? SupplierGroupId,
    Guid? SupplierTypeId
);

public record SupplierLookupDto(Guid Id, string Code, string Name);

public record SupplierGroupDto(
    Guid Id,
    string Code,
    string Name,
    string? Remark,
    bool IsActive
);

public record SaveSupplierGroupRequest(
    Guid? Id,
    string Code,
    string Name,
    string? Remark,
    bool IsActive
);

public record SupplierTypeDto(
    Guid Id,
    string Code,
    string Name,
    string? Remark,
    bool IsActive
);

public record SaveSupplierTypeRequest(
    Guid? Id,
    string Code,
    string Name,
    string? Remark,
    bool IsActive
);

public record PagedSupplierResult(
    List<SupplierDto> Data,
    PaginationMeta Pagination
);

public record PagedSupplierGroupResult(
    List<SupplierGroupDto> Data,
    PaginationMeta Pagination
);

public record PagedSupplierTypeResult(
    List<SupplierTypeDto> Data,
    PaginationMeta Pagination
);

public record PaginationMeta(
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);