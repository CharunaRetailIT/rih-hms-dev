namespace Hms.Api.Features.Locations;

public record LocationDto(
    Guid Id,
    string Code,
    string Name,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string CountryCode,
    string TimeZone,
    string Currency,
    string? PhoneE164,
    bool IsActive,
    string LocationType,
    bool CanSell,
    bool CanProduce,
    bool CanStock,
    string? VatRegistrationNumber,
    int DefaultPrepMinutes
);

public record SaveLocationRequest(
    Guid? Id,
    string Code,
    string Name,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string CountryCode,
    string TimeZone,
    string Currency,
    string? PhoneE164,
    bool IsActive,
    string LocationType,
    bool CanSell,
    bool CanProduce,
    bool CanStock,
    string? VatRegistrationNumber,
    int DefaultPrepMinutes
);

public record PagedLocationResult(
    List<LocationDto> Data,
    PaginationMeta Pagination
);

public record PaginationMeta(
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);