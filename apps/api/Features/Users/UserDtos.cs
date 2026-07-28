namespace Hms.Api.Features.Users;

public record UserDto(
    Guid Id, string? Email, string? Username, string DisplayName, int Role, Guid? HomeLocationId,
    bool IsActive, bool IsServer, DateTime? LastLoginAt, string? PhoneE164, bool HasPin
);

public record PagedUserResult(
    List<UserDto> Data,
    PaginationMeta Pagination
);

public record PaginationMeta(
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);
