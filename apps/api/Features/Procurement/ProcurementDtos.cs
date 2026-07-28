namespace Hms.Api.Features.Procurement;

public record GrnListDto(
    Guid Id, string GrnNumber, Guid SupplierId, string SupplierName,
    Guid LocationId, string LocationCode, string LocationName,
    Guid? PurchaseOrderId, string? PoNumber,
    string Status, DateOnly ReceivedDate,
    decimal TotalCost, decimal TaxAmount, decimal DiscountAmount, decimal OtherCharges, decimal NetAmount,
    string? SupplierInvoiceNo, string? ReferenceNo, DateTime? PostedAt,
    DateTime? ApprovedAt, DateTime? RejectedAt, string? RejectReason,
    List<string> FulfilledRequestNoteNumbers
);

public record PagedGrnResult(List<GrnListDto> Data, PaginationMeta Pagination);

public record PoListDto(
    Guid Id, string PoNumber, Guid SupplierId, string SupplierName,
    Guid LocationId, string LocationCode, string LocationName,
    string Status, DateOnly OrderDate, DateOnly? ExpectedDate, decimal TotalAmount
);

public record PagedPoResult(List<PoListDto> Data, PaginationMeta Pagination);

public record PaginationMeta(int TotalCount, int PageNumber, int PageSize, int TotalPages);
