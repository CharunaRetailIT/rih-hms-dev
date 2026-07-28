namespace Hms.Api.Domain;

/// <summary>
/// A requisition raised by one location against another, upstream of actual fulfillment.
/// "po" mode is fulfilled by raising a Purchase Order; "transfer" mode is fulfilled by
/// raising a Transfer of Goods that moves existing stock from FromLocationId to
/// ToLocationId. The request note itself never touches stock — approving it only
/// authorises it to be picked up by whichever downstream document fulfills it.
/// </summary>
public class RequestNote : BaseEntity
{
    public string RequestNumber { get; set; } = default!;
    public string Mode { get; set; } = "po";  // po|transfer
    public Guid FromLocationId { get; set; }  // expected to fulfill the request
    public Guid ToLocationId { get; set; }    // requesting location
    public DateOnly ExpectedDeliveryDate { get; set; }
    // draft|pending|approved|rejected|fulfilled — draft = editable, not yet submitted. Submit
    // resolves straight to "approved" when no approval policy applies, or "pending" when one
    // does. "fulfilled" is terminal, set once an approved note has been bundled into whichever
    // document fulfills its mode — a "po"-mode note into a Purchase Order (PurchaseOrderId), a
    // "transfer"-mode note into a Transfer of Goods (TransferId). A TOG's separate "GRN based"
    // line mode (pulling in an already-received GRN's lines) is unrelated to this — that traces
    // back through GRN -> PurchaseOrderId here, purely for display.
    public string Status { get; set; } = "draft";
    public string? CommonRemark { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectReason { get; set; }
    public Guid? PurchaseOrderId { get; set; }   // set when a "po"-mode note is bundled into a PO
    public Guid? TransferId { get; set; }        // set when a "transfer"-mode note is bundled into a TOG
    public List<RequestNoteLine> Lines { get; set; } = new();
}

public class RequestNoteLine : BaseEntity
{
    public Guid RequestId { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public decimal Sih { get; set; }        // stock-in-hand at FromLocationId, snapshotted when the line was added
    public decimal Quantity { get; set; }
    public string? Remark { get; set; }
}
