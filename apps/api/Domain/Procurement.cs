namespace Hms.Api.Domain;

public class PurchaseOrder : BaseEntity
{
    public Guid LocationId { get; set; }
    public Guid SupplierId { get; set; }
    public string PoNumber { get; set; } = default!;
    // draft|pending|approved|rejected|partially_received|received
    // draft = editable, not yet submitted. Submit resolves straight to "approved" when no
    // approval policy applies, or "pending" when one does. Receiving only ever moves a PO
    // forward from approved/partially_received.
    public string Status { get; set; } = "draft";
    public DateOnly OrderDate { get; set; }
    public DateOnly? ExpectedDate { get; set; }
    public decimal SubtotalAmount { get; set; }    // goods, net of line discounts
    public decimal DiscountAmount { get; set; }    // header-level discount
    public decimal Deductions { get; set; }        // header-level deduction, separate from discount
    public decimal OtherCharges { get; set; }      // freight / handling → landed cost
    public decimal TaxAmount { get; set; }
    public bool ExcludeTax { get; set; }           // skip per-product tax entirely for this order
    public decimal TotalAmount { get; set; }
    public string CurrencyCode { get; set; } = "LKR";
    public decimal CurrencyRate { get; set; } = 1;
    public Guid? DeliveryLocationId { get; set; }  // ship-to (null = LocationId)
    public string? DeliveryAddress { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Notes { get; set; }
    public int? PaymentTermsDays { get; set; }     // override; defaults from Supplier.PaymentTermsDays
    public string? PaymentMethod { get; set; }     // cash|bank_transfer|cheque|credit — record-keeping only
    // lifecycle audit (maker-checker)
    public DateTime? SubmittedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectReason { get; set; }
    public List<PurchaseOrderLine> Lines { get; set; } = new();
}

public class PurchaseOrderLine : BaseEntity
{
    public Guid PurchaseOrderId { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public decimal QuantityOrdered { get; set; }   // in the purchasing unit
    public decimal QuantityReceived { get; set; }
    public decimal UnitCost { get; set; }          // per purchasing unit
    public decimal LineTotal { get; set; }         // goods value ex-VAT, net of line discount
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public Guid? UnitId { get; set; }              // purchasing unit (null = product stock unit)
    public string? UnitSymbol { get; set; }
    public decimal DiscountAmount { get; set; }
}

public class GoodsReceivedNote : BaseEntity
{
    public Guid LocationId { get; set; }
    public Guid SupplierId { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public string GrnNumber { get; set; } = default!;
    // draft|pending|approved|rejected|void — draft = not yet submitted, no stock impact.
    // Submit resolves straight to "approved" (which posts stock + weighted-avg cost) when
    // no approval policy applies, or "pending" when one does.
    public string Status { get; set; } = "draft";
    public DateOnly ReceivedDate { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TaxAmount { get; set; }   // input VAT (reclaimable)
    public string? SupplierInvoiceNo { get; set; }
    public string? Notes { get; set; }
    public DateTime? PostedAt { get; set; }
    public DateTime? VoidedAt { get; set; }
    public string? VoidReason { get; set; }
    public string CurrencyCode { get; set; } = "LKR";
    public decimal CurrencyRate { get; set; } = 1;
    public int? PaymentTermsDays { get; set; }     // override; defaults from Supplier.PaymentTermsDays
    public string? PaymentMethod { get; set; }     // cash|bank_transfer|cheque|credit — record-keeping only
    public decimal DiscountAmount { get; set; }    // header-level discount, on top of line discounts
    public decimal OtherCharges { get; set; }      // freight / handling — recorded, not allocated into stock cost
    public string? ReferenceNo { get; set; }
    // lifecycle audit (maker-checker, mirrors PurchaseOrder)
    public DateTime? SubmittedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectReason { get; set; }
    public List<GrnLine> Lines { get; set; } = new();
}

public class GrnLine : BaseEntity
{
    public Guid GrnId { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public decimal QuantityReceived { get; set; }   // in the received (purchasing) unit
    public decimal UnitCost { get; set; }            // per received unit
    public decimal LineTotal { get; set; }           // goods value ex-VAT, net of discount
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public string? BatchNo { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public Guid? UnitId { get; set; }                // received (purchasing) unit
    public string? UnitSymbol { get; set; }
    public decimal FreeQuantity { get; set; }        // bonus units (received unit), no cost
    public decimal DiscountAmount { get; set; }
    public decimal StockQuantity { get; set; }       // qty added to stock incl free, in stock units
}

public class PurchaseReturn : BaseEntity
{
    public Guid LocationId { get; set; }
    public Guid SupplierId { get; set; }
    public Guid? GrnId { get; set; }
    public string PrnNumber { get; set; } = default!;
    public string Status { get; set; } = "posted";   // posted|void
    public DateOnly ReturnDate { get; set; }
    public string? Reason { get; set; }
    public string? SupplierCreditNo { get; set; }
    public decimal TotalCost { get; set; }            // goods value returned (ex-VAT)
    public decimal TaxAmount { get; set; }            // input VAT reversed
    public string? Notes { get; set; }
    public DateTime? PostedAt { get; set; }
    public DateTime? VoidedAt { get; set; }
    public string? VoidReason { get; set; }
    public List<PurchaseReturnLine> Lines { get; set; } = new();
}

public class PurchaseReturnLine : BaseEntity
{
    public Guid ReturnId { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public decimal Quantity { get; set; }            // in the return unit
    public Guid? UnitId { get; set; }
    public string? UnitSymbol { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineTotal { get; set; }           // goods value ex-VAT
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal StockQuantity { get; set; }       // qty removed from stock (converted)
    public string? BatchNo { get; set; }
}

/// <summary>Per-document-type running counter (PO-0001, GRN-0001, …).</summary>
public class DocumentCounter
{
    public Guid TenantId { get; set; }
    public string DocType { get; set; } = default!;
    public int LastValue { get; set; }
}
