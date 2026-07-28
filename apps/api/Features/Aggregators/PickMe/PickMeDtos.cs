namespace Hms.Api.Features.Aggregators.PickMe;

// =============================================================================
// PickMe POS API v1.4.7 response shapes. JSON is snake_case; we rely on
// JsonNamingPolicy.SnakeCaseLower (set on the client's serializer) so these
// PascalCase members map automatically (PickmeJobId ↔ pickme_job_id, etc.).
// Money fields are decimal? with AllowReadingFromString — PickMe returns some
// prices as numbers (outlet/items) and some as strings (joblist options).
// =============================================================================

// ── 1. /joblist ──
public sealed record PickMeJobListResponse(PickMeParams? Params, List<PickMeJob>? Data);
public sealed record PickMeParams(PickMePagination? Pagination, string? FromTimestamp, string? ToTimestamp);
public sealed record PickMePagination(int Page, int Size, int TotalRecords);

public sealed record PickMeJob(
    string PickmeJobId,
    PickMeCustomer? Customer,
    PickMeOutlet? Outlet,
    PickMeOrder? Order,
    PickMePayment? Payment,
    PickMeStatus? Status,
    string? DeliveryMode,        // "Delivery" | "PickUp"
    string? CreatedTimestamp);

public sealed record PickMeCustomer(string? ContactNumber, PickMeLocation? Location);
public sealed record PickMeLocation(string? Address);
public sealed record PickMeOutlet(string? Name, string? ContactNumber, PickMeLocation? Location);
public sealed record PickMeOrder(List<PickMeOrderItem>? Items, string? DeliveryNote);
public sealed record PickMeOrderItem(
    long Id, string? RefId, string? Name, decimal Qty, decimal Total, string? SpIns, List<PickMeOption>? Options);
public sealed record PickMeOption(string? Name, List<PickMeOptionItem>? Items);
public sealed record PickMeOptionItem(string? Name, decimal? Qty, decimal? Price, string? RefId);
public sealed record PickMePayment(decimal? Total, string? Method);
public sealed record PickMeStatus(string? Name, string? UpdatedTimestamp);

// ── 2. /outlet/items ──
public sealed record PickMeItemListResponse(PickMeItemParams? Params, List<PickMeMenuItem>? Data);
public sealed record PickMeItemParams(string? OutletName);
public sealed record PickMeMenuItem(
    long Id, string? Name, string? Description, string? Image,
    decimal? Price, string? CurrencyCode, string? Availability, string? Category, string? RefId);
