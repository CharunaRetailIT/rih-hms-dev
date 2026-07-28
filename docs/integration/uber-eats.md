# Uber Eats — Integration Spec

> Target: Sprints 2–3 (weeks 4–6).
> API: Uber Eats Marketplace API (formerly UberEats Orders API).

## What we're integrating

| Capability | Endpoint | Direction |
|---|---|---|
| Publish menu | `PUT /v2/eats/stores/{store_id}/menus` | HMS → Uber |
| Update store status (open/closed) | `POST /v1/eats/stores/{store_id}/status` | HMS → Uber |
| Receive new orders | `order.notification` webhook | Uber → HMS |
| Accept an order | `POST /v1/eats/orders/{order_id}/accept` | HMS → Uber |
| Deny an order | `POST /v1/eats/orders/{order_id}/deny` | HMS → Uber |
| Update order status | `POST /v1/eats/orders/{order_id}` (PREPARING / READY_FOR_PICKUP / etc.) | HMS → Uber |
| Cancel an order | `POST /v1/eats/orders/{order_id}/cancel` | HMS → Uber |
| Receive cancellations | `order.cancel` webhook | Uber → HMS |
| Settlement reports | `GET /v1/eats/reports/...` | HMS pulls weekly |

## Auth

OAuth2 client credentials. Token TTL ~30 days. Cache the token in
`HMSLoginManager` DB or in `MemoryCache`; refresh proactively at 80% TTL.

## Webhook security

- Every webhook carries `X-Uber-Signature: HMAC-SHA256` of the body keyed
  by `UBEREATS_WEBHOOK_HMAC_SECRET`.
- `AggregatorWebhookController` rejects any request whose computed HMAC
  doesn't match the header (constant-time compare).
- `Replay protection`: idempotency on `ExternalOrderId` — a duplicate
  webhook for the same order is a no-op.

## Schema additions (one EF migration)

See [`docs/database.md`](../database.md) for the existing schema. We add:

```csharp
// Migration: 20260601_AddAggregatorColumns
AddColumn("dbo.SuspendHeds", "OrderSource",          c => c.String(maxLength: 50, defaultValue: "DINE_IN"));
AddColumn("dbo.SuspendHeds", "ExternalOrderId",      c => c.String(maxLength: 100));
AddColumn("dbo.SuspendHeds", "ExternalRef",          c => c.String(maxLength: 100));
AddColumn("dbo.SuspendHeds", "AggregatorPayload",    c => c.String(storeType: "nvarchar(max)"));
AddColumn("dbo.SuspendHeds", "DeliveryAddress",      c => c.String(maxLength: 500));
AddColumn("dbo.SuspendHeds", "DeliveryPhone",        c => c.String(maxLength: 50));
AddColumn("dbo.SuspendHeds", "DeliveryNotes",        c => c.String(maxLength: 500));
AddColumn("dbo.SuspendHeds", "PromisedDeliveryTime", c => c.DateTime(nullable: true));

CreateIndex("dbo.SuspendHeds", new[] { "OrderSource", "ExternalOrderId" }, unique: false);

// Mirror columns on TransactionDets so settlement carries the marker.
AddColumn("dbo.TransactionDets", "OrderSource",     c => c.String(maxLength: 50));
AddColumn("dbo.TransactionDets", "ExternalOrderId", c => c.String(maxLength: 100));

// Outbox table
CreateTable("dbo.AggregatorOutbox", c => new {
    Id              = c.Long(nullable: false, identity: true),
    Aggregator      = c.String(maxLength: 50, nullable: false),     // "UBEREATS" / "PICKME"
    ExternalOrderId = c.String(maxLength: 100),
    Operation       = c.String(maxLength: 50, nullable: false),     // "STATUS_UPDATE", "ACCEPT", "CANCEL", etc.
    Endpoint        = c.String(maxLength: 500, nullable: false),
    Method          = c.String(maxLength: 10,  nullable: false),    // "POST" / "PATCH"
    PayloadJson     = c.String(storeType: "nvarchar(max)"),
    Attempts        = c.Int(nullable: false, defaultValue: 0),
    NextRetryAt     = c.DateTime(nullable: false),
    Status          = c.String(maxLength: 20, nullable: false, defaultValue: "PENDING"),
    LastError       = c.String(maxLength: 2000),
    CreatedAt       = c.DateTime(nullable: false),
    UpdatedAt       = c.DateTime(nullable: false),
}).PrimaryKey(t => t.Id)
  .Index(t => new { t.Status, t.NextRetryAt });

// Seed pay types
Sql("INSERT INTO PayTypes (Code, Name, IsActive) VALUES ('UBEREATS_PREPAID', 'Uber Eats (pre-paid)', 1);");
```

## Order lifecycle in HMS

```
Uber sends order.notification webhook
  → HMAC verified
  → idempotency check on ExternalOrderId
  → INSERT SuspendHeds (OrderSource = 'UBEREATS', ExternalOrderId = ...)
  → INSERT SuspendDets for each item
  → POST /v1/eats/orders/{id}/accept   (via outbox)
  → KOT prints with "DELIVERY — UBER" header
  → Kitchen marks ready in HMSOrderTaker
  → outbox POSTs status PREPARING → READY_FOR_PICKUP
  → Uber driver picks up
  → outbox POSTs OUT_FOR_DELIVERY
  → Customer receives
  → outbox POSTs COMPLETED
  → Settlement runs end of day:
      INSERT TransactionDets (OrderSource = 'UBEREATS', payment method UBEREATS_PREPAID)
      → trigger decrements ProductStockMasters
      → InvSales updated for reconciliation report
```

## Reconciliation

Daily report joining `InvSales` rows where `OrderSource = 'UBEREATS'` against
the Uber Eats settlement CSV pulled from their reports API. Discrepancies
flagged for finance review. Common causes:
- Uber commission (~25–30% in SL) not stored in `InvSales` — track separately
- Uber promos / discounts shifting net amount
- Refunds processed by Uber but not yet in HMS

## Open questions for kickoff

1. Which outlet pilots first? (Raffles is the most likely.)
2. Auto-accept policy — accept everything, or queue for cashier confirmation
   (slows fulfilment)?
3. What's the menu refresh cadence — manual, scheduled, or event-driven on
   product save?
4. Stock-out handling — pause Uber items when stock = 0, or take the order
   and apologise?
5. Who fields the 1–2 daily aggregator support calls in the first month?
