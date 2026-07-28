namespace Hms.Api.Features.Modifiers;

public record ModifierItemDto(Guid Id, string Name, decimal PriceDelta, bool IsDefault, int SortOrder);

public record ModifierGroupDto(
    Guid Id,
    string Name,
    int MinSelect,
    int MaxSelect,
    bool IsRequired,
    int SortOrder,
    bool IsActive,
    List<ModifierItemDto> Items
);

public record PagedModifierGroupResult(
    List<ModifierGroupDto> Data,
    PaginationMeta Pagination
);

public record PaginationMeta(
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);
