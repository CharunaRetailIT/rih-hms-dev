namespace Hms.Api.Domain;

/// <summary>
/// A bill / tab. The transactional centre of the POS.
/// order_source is schema-ready for Uber Eats / PickMe (wired in Sprint 4).
/// </summary>
public class Order : BaseEntity
{
    public Guid LocationId { get; set; }
    public string OrderNumber { get; set; } = default!;
    public string OrderType { get; set; } = "dine_in";    // dine_in | takeaway | delivery
    public Guid? PriceLevelId { get; set; }                // price list the order was priced at
    public string OrderSource { get; set; } = "pos";       // pos | ubereats | pickme
    public string? ExternalOrderId { get; set; }
    public string? TableLabel { get; set; }
    public Guid? TableId { get; set; }                     // link to a real RestaurantTable (#68)
    public int? Covers { get; set; }
    public string Status { get; set; } = "open";           // open | confirmed | settled | void
    // Guest QR orders (#108) land here awaiting a steward's accept — not auto-sent to the
    // kitchen. Always false for pos/ubereats/pickme orders. Cleared by ConfirmAsync.
    public bool PendingAcceptance { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal PromotionDiscountAmount { get; set; }   // auto-applied promotions (#65)
    public decimal ServiceChargeAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TipAmount { get; set; }                 // discretionary staff tip, added to the bill (#76)
    public decimal TotalAmount { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? CashierId { get; set; }

    // POS depth (#76): steward attribution + tour-operator commission
    public Guid? StewardId { get; set; }                   // waiter the dine-in bill is attributed to
    public Guid? TourOperatorId { get; set; }              // travel agent that brought the guests
    public decimal TourCommissionAmount { get; set; }      // commission booked to the operator at settle
    public DateTime OpenedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? SettledAt { get; set; }
    public DateTime? VoidedAt { get; set; }
    public string? VoidReason { get; set; }

    // VAT invoice (assigned at settle)
    public string? InvoiceNumber { get; set; }
    public bool IsTaxInvoice { get; set; }
    public int ReprintCount { get; set; }                  // duplicate-bill copies printed (#78)
    public string? CustomerVatNo { get; set; }
    public string? CustomerName { get; set; }

    // Delivery / aggregator (Uber Eats / PickMe)
    public string? DeliveryAddress { get; set; }
    public string? DeliveryPhone { get; set; }
    public string? DeliveryNotes { get; set; }
    public DateTime? PromisedTime { get; set; }
    public string? AggregatorPayload { get; set; }
    public string? AggregatorStatus { get; set; }   // pending | preparing | ready | picked_up | rejected | cancelled
    public int? PrepMinutes { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? ReadyAt { get; set; }
    public DateTime? PickedUpAt { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public Guid? VariantId { get; set; }                   // chosen serving size, if any
    public string? VariantName { get; set; }               // snapshot label (Cup / Large)
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }            // per-product auto discount applied to this line (#3b)
    public decimal LineSubtotal { get; set; }              // = UnitPrice*Quantity − DiscountAmount
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public string Station { get; set; } = "kitchen";       // kitchen | bar
    public string? Notes { get; set; }
    public string KotStatus { get; set; } = "pending";     // pending | sent | preparing | ready
    public bool IsStocked { get; set; } = true;
    public bool IsTaxable { get; set; } = true;            // counts toward the VAT base
    public List<OrderItemModifier> Modifiers { get; set; } = new();
}

public class KitchenTicket : BaseEntity
{
    public Guid LocationId { get; set; }
    public Guid OrderId { get; set; }
    public string TicketNumber { get; set; } = default!;
    public string Station { get; set; } = "kitchen";
    public string OrderLabel { get; set; } = default!;
    public string OrderSource { get; set; } = "pos";
    public string Status { get; set; } = "new";            // new | preparing | ready | served
    public string ItemsJson { get; set; } = "[]";
    public DateTime? ReadyAt { get; set; }
    public DateTime? ServedAt { get; set; }
}

public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public string PayType { get; set; } = "cash";          // cash | card | ubereats_prepaid | pickme_prepaid | credit | loyalty | advance
    public decimal Amount { get; set; }                    // amount in the tender's CurrencyCode
    public string? Reference { get; set; }

    // Multi-currency tender (#76). For base-currency / non-cash tenders FxRate=1
    // so BaseAmount == Amount. BaseAmount is what counts toward settling the bill.
    public string? CurrencyCode { get; set; }
    public decimal FxRate { get; set; } = 1m;
    public decimal BaseAmount { get; set; }
}
