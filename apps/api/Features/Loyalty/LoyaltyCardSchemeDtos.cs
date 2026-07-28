namespace Hms.Api.Features.Loyalty;

public record LoyaltyCardSchemeTierDto(Guid? Id, decimal BillFromValue, decimal BillToValue, decimal Increment, decimal Points, int SortOrder);

public record LoyaltyCardSchemeDto(
    Guid Id, string Code, string Name, string? Description, string Type,
    decimal DiscountPercent, Guid? PromotionId, string? PromotionName, bool IsActive,
    List<LoyaltyCardSchemeTierDto> Tiers);

public record LoyaltyCardSchemeInput(
    Guid? Id, string? Code, string? Name, string? Description, string? Type,
    decimal? DiscountPercent, Guid? PromotionId, bool? IsActive,
    List<LoyaltyCardSchemeTierDto>? Tiers);

public record PagedLoyaltyCardSchemeResult(List<LoyaltyCardSchemeDto> Data, PaginationMeta Pagination);

public record PaginationMeta(int TotalCount, int PageNumber, int PageSize, int TotalPages);
