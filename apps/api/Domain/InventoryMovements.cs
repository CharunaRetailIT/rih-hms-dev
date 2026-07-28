namespace Hms.Api.Domain;

public class StockTransfer : BaseEntity
{
    public string TransferNumber { get; set; } = default!;
    public Guid FromLocationId { get; set; }
    public Guid ToLocationId { get; set; }
    // draft|pending|dispatched|received|rejected — draft = editable, not yet submitted. Submit
    // resolves straight to "dispatched" (and posts the stock decrement) when no approval policy
    // applies, or "pending" when one does. Approving a pending transfer is what actually dispatches
    // it; rejecting it is terminal and never touches stock. "cancelled" is a legacy value from
    // before this approval gate existed, kept only so old soft-deleted rows still render sensibly.
    public string Status { get; set; } = "draft";
    public bool IsReturn { get; set; }
    public DateOnly TransferDate { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Notes { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectReason { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public List<StockTransferLine> Lines { get; set; } = new();
}

public class StockTransferLine : BaseEntity
{
    public Guid TransferId { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineTotal { get; set; }
}

public class WastageNote : BaseEntity
{
    public Guid LocationId { get; set; }
    public string WastageNumber { get; set; } = default!;
    public string Reason { get; set; } = "spoilage";  // spoilage|breakage|expiry|theft|other
    // "posted" is the legacy value from the old always-immediate Record Wastage
    // modal (now removed). The document workflow uses draft|pending|approved|rejected.
    public string Status { get; set; } = "posted";
    public string? Notes { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime? PostedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectReason { get; set; }
    public List<WastageLine> Lines { get; set; } = new();
}

public class WastageLine : BaseEntity
{
    public Guid WastageId { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public decimal Quantity { get; set; }
    public decimal CurrentStock { get; set; }    // on-hand at the moment this line was applied
    public decimal NewStock { get; set; }        // resulting on-hand after this line
    public decimal UnitCost { get; set; }
    public decimal LineTotal { get; set; }
}

public class StockAdjustment : BaseEntity
{
    public Guid LocationId { get; set; }
    public string AdjustmentNumber { get; set; } = default!;
    public string Reason { get; set; } = "count";  // count|correction|opening|other
    // "posted" is the legacy value written by the quick single-line adjust
    // modal (Inventory page) and the opening-stock importer — both apply
    // immediately and never touch the fields below. The document workflow
    // (this feature) uses draft|pending|approved|rejected instead.
    public string Status { get; set; } = "posted";
    public string? Notes { get; set; }
    public decimal TotalValue { get; set; }
    public DateTime? PostedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectReason { get; set; }
    public List<StockAdjustmentLine> Lines { get; set; } = new();
}

public class StockAdjustmentLine : BaseEntity
{
    public Guid AdjustmentId { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public string AdjustmentType { get; set; } = "add";  // add|reduce|override
    public decimal QuantityDelta { get; set; }   // signed — authoritative for posting
    public decimal CurrentStock { get; set; }    // on-hand at the moment this line was applied
    public decimal NewStock { get; set; }        // resulting on-hand after this line
    public decimal UnitCost { get; set; }
    public decimal LineTotal { get; set; }       // QuantityDelta * UnitCost, signed
}
