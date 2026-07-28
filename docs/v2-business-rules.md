# RIT HMS v2 — Business Rules to Preserve

This document captures non-obvious business rules from the legacy system that must survive into v2. Each rule includes legacy source path:line and an opinionated v2 implementation strategy.

Related: [v2 Entities](./v2-entities.md) · [v2 API surface](./v2-api-surface.md) · [v2 Multi-tenancy](./v2-multi-tenancy.md) · [v2 v1 scope](./v2-v1-scope.md).

---

## The two critical triggers (resolve first)

### R1. Stock decrement on settlement
**Rule.** When a row is inserted into `TransactionDets` with `DocumentID IN (1, 3)` AND `BillTypeID = 1` AND `SaleTypeID = 1` AND `TransStatus = 1` AND `Status = 1`, the trigger `Trigger_UpdateStockInHeadOffice` decrements `ProductStockMasters.Stock` by the line quantity, scoped to that location. **This is the single most critical rule in the entire legacy system.**

**Source.** `RIT.HMS.Domain/Common/StructureChangesTriggers.cs:89-92`, `docs/database.md:39-44`.

**v2 implementation.**
1. `OrderSettlementService.SettleAsync(orderId)` writes settled `Order` + `OrderLine` rows.
2. Inside the same `SaveChangesAsync` transaction, write an `OutboxEvent` row of type `OrderSettled` with the order payload and `idempotency_key = order_id + ':settled'`.
3. A Hangfire job polls `outbox_events WHERE processed_at IS NULL`, dispatches to handlers, marks processed.
4. `StockDecrementHandler` reads `OrderSettled`, builds a list of `(product_id, outlet_id, quantity_to_deduct)`, and executes the decrement in a single transaction. For products with recipes, it expands lines into ingredient deductions via `RecipeIngredientResolver`.
5. **Aggregator orders (`order_source IN (uber_eats, pickme)`)**: handler defers the decrement until `OrderAggregatorStatusUpdated` event with status `delivered` or `picked_up`. Until then, stock is `reserved` not `decremented` (separate column on `ProductStock`).
6. **No DB triggers**. Outbox + handler == reliable, observable, testable, and works across reboots.

### R2. Kitchen ticket on payment
**Rule.** When a row is inserted into `PaymentDets`, the trigger `Trg_GenProductionNotes` calls the stored proc `genProductionNotes`, which reads `TransactionDets` (joined on receipt/location/z-no/unit) and produces `ProductionNoteHeaders/Details` rows that drive thermal-printer KOT/BOT output.

**Source.** `RIT.HMS.Domain/Common/StructureChangesTriggers.cs:28-54`, `docs/database.md:33-34`.

**v2 implementation.**
1. When `Order.order_status` transitions to `confirmed` (typically on first `POST /orders/{id}/payments`), `KitchenTicketService.GenerateAsync(orderId)` runs:
   - For each `OrderLine`, resolve all `KitchenStation` via `ProductStationMapping` (one product → N stations is normal).
   - Group lines by station, create one `KitchenTicket` per station with `KitchenTicketLine` children, copy `item_comment`, `modifiers`, `serving_unit`.
   - Emit `KitchenTicketCreated` outbox event.
2. **Print Agent** (small .NET 8 service running on a venue PC) polls `GET /kitchen-tickets/pending?station_id=...` every 2-5s (or SSE in v2). On receiving a ticket, it formats ESC/POS bytes and pushes to the configured printer.
3. Print Agent reports back via `PATCH /kitchen-tickets/{id}/status` (`printed`, `reprinted`, `voided`).
4. Reprints are explicit — `POST /kitchen-tickets/{id}/reprint` creates a new ticket row with `status=reprinted` and links to original.

---

## POS / Sales / Settlement

### R3. Multi-tender payment must sum to net amount
Payment lines on an order may use multiple methods (cash + card + voucher). Last payment must bring `balance` to zero before `settle` is allowed.
- **Source.** `PaymentDets.Balance` column.
- **v2.** `OrderSettlementService` precondition: `SUM(order_payments.amount) = order.net_amount`. Throw `PAYMENT_INCOMPLETE` otherwise.

### R4. Tax composition (4 layers, sequenced)
Tax applies in 4 layers — outlet base, catering mode override, product-specific, payment-method-specific. Each layer has `tax_sequence` and `is_tax_on_tax`.
- **Source.** `docs/database.md:22`, `LocationTaxes`, `CateringModeTaxes`, `ProductTaxes`, `PayTypeTaxes`.
- **v2.** `TaxCompositionService.Compute(orderLine, outlet, paymentMethod, cateringMode)` walks layers in `tax_sequence` order. For each tax, if `is_tax_on_tax`, applies to running total; else to base net. Returns `tax_lines[]` with breakdown for receipt + audit.

### R5. Recall and void semantics
Settled orders can be recalled by a supervisor. Recall = re-open for edit. Void = reverse settlement. Voided lines must NOT re-decrement stock (and a void on an already-settled order must REVERSE the decrement).
- **Source.** `SuspendHeds.IsRecall/RecallNo`, `TransactionDets.IsRecall/RecallNO`, `PaymentDets.IsRecallAdv`.
- **v2.** `OrderRecallService.Recall(orderId, supervisorUserId)` transitions `order_status: posted → draft`, writes `TransactionLog`. `OrderVoidService.Void(orderId, reason)` transitions to `void`, emits `OrderVoided` event. Handler emits compensating `StockReplenishEvent` for the lines that were originally deducted. Never modify settled rows in place.

### R6. Document type controls stock logic
Legacy `DocumentID` enum: 1=Sale, 2=Sale-Return, 3=Sale-with-Tax, 4=Sale-Return-with-Tax, 9=Aggregator (custom, no auto-decrement). Stock decrement fires only on (1, 3).
- **Source.** `Trigger_UpdateStockInHeadOffice:89-101`.
- **v2.** Replace with `OrderType` enum + explicit handler logic. `StockDecrementHandler` checks `order.order_type IN (sale, sale_with_tax)` AND `order.order_source = pos`. Aggregator orders use `order_type = sale` but `order_source != pos` and are handled separately.

### R7. Transaction status as state machine
Legacy `TransStatus` + `Status` int flags — opaque. Stock decrements only when both = 1.
- **Source.** `Trigger_UpdateStockInHeadOffice:89`.
- **v2.** Single `order_status` enum: `draft → confirmed → posted → void`. Stock decrement fires only on `posted`. Use a domain state machine (e.g. Stateless library or custom). Document forbidden transitions (`void → posted` is illegal).

### R8. Order source determines aggregator settlement
Aggregator orders (Uber Eats, PickMe Food) settle differently from POS — payment is collected by aggregator, commission is netted out, stock decrement is deferred to delivery confirmation.
- **Source.** Project scope (not in legacy schema).
- **v2.** `Order.order_source` enum drives `OrderSourceStrategy`. POS strategy: immediate stock decrement, normal GL post. Uber/PickMe strategy: stock reserved on creation, decremented on `delivered`, GL post nets commission. See [v2-multi-tenancy.md](./v2-multi-tenancy.md) for tenant-level aggregator API key storage.

### R9. Currency rate locked at settlement
Multi-currency settlement locks the exchange rate at the moment of settlement, not at query time.
- **Source.** Implicit in legacy `PaymentDets.CurrencyRate`.
- **v2.** `Order.currency_id` + `Order.currency_rate` are snapshot fields, never updated after `closed_at`. Currency master can change rates freely; historical orders show original.

---

## Master Data

### R10. Customer credit limit enforcement
Customer with `outstanding > credit_limit` cannot create new credit orders without manager override.
- **Source.** `Customers.CreditLimit` / `Customers.Outstanding`, `Customer.cs:103-106`.
- **v2.** `OrderValidator` runs precondition. `current_outstanding_amount` is computed nightly from AR aging (Hangfire job), or on-demand for premium tenants. Override is a `Permission` (`override_credit_limit`).

### R11. Customer category default discount + GST relief
`CustomerCategory.is_vat` flag drives whether GST applies; `default_discount_pct` is auto-applied unless overridden per line.
- **Source.** `CustomerCategories.IsVat`, `DiscountPrc`.
- **v2.** `OrderLine` discount resolution order: (1) `CustomerDiscount(customer, product)` lookup; (2) `Customer.category.default_discount_pct`; (3) manual cashier override (within `Permission.limit_value`).

### R12. Customer pictures / employee pictures never in DB
Legacy stores varbinary(max) blobs in `Customers.CustomerPicture`, `Employees.EmployeePicture`.
- **Source.** `Customers`, `Employees` schema.
- **v2.** Azure Blob Storage. Store only `asset_id` UUID + `asset_type` enum. CDN-cache public assets.

### R13. ReferenceTypes is an anti-pattern
String-typed soft-enum lookup with `LookupType`/`LookupKey`/`LookupValue` rows for Gender, Religion, CivilStatus, etc.
- **Source.** `ReferenceTypes` table.
- **v2.** Drop the table. Replace with Postgres `ENUM` types + C# enums. Compile-time safety.

---

## Inventory

### R14. ProductStockMasters is a cache, not source of truth
Legacy treats `ProductStockMasters.Stock` as live, updated by trigger. Sources of truth are: opening balance + GRNs - sales (TransactionDets) ± adjustments ± transfers.
- **Source.** `architecture.md:75-77`, `database.md:51-53`.
- **v2.** `ProductStock` table is a **cache** updated by event handlers (`StockDecrementHandler`, `GRNAcceptedHandler`, `StockAdjustmentPostedHandler`, `StockTransferConfirmedHandler`). Nightly reconciliation Hangfire job recomputes from event history and alerts on drift > 0.01%. Annual snapshot at year-close.

### R15. Recipe-driven ingredient deduction
Selling a finished-good product with `is_recipe = true` and `ProductServingUnits.DeductStockOnRecipe = true` decrements the ingredient stock, not the finished-good stock.
- **Source.** `ReceipeController.cs:60-115`, `ProductController.cs:42`.
- **v2.** `RecipeIngredientResolver.Expand(orderLine)` returns list of `(raw_material_id, qty_to_deduct)` from the active recipe. Called inside `StockDecrementHandler`. Supports wastage % (`Product.wastage_prc`) → `qty_deducted = recipe_qty * (1 + wastage_prc / 100)`.

### R16. Unit-of-measure normalisation
Products may be purchased in one UOM (Case of 24) but stocked / sold in another (Unit). `UnitConversions` defines the factor.
- **Source.** `UnitConversions`, `ProductStockMasters.Unit`.
- **v2.** `ProductStock.stock_on_hand` is **always** in the product's base UOM. PO/GRN qty is in `purchasing_unit_id`; the handler converts via `UnitConversion` before posting to stock.

### R17. Batch/expiry tracking conditional
If `Product.is_expiry = true`, GRN must capture `batch_no` + `expiry_date` per line. Sale FIFO/LIFO at v2.
- **Source.** `Products.IsExpiry`, `PurchaseDetails.BatchNo`.
- **v2 (v1).** Columns exist but optional; logged for audit, not enforced. **v2 (v2)**: required for `is_expiry=true` products; `ProductStockBatch` sub-table; FIFO deduction; `BatchNearExpiry` event 30 days before.

### R18. Approval workflows for PO / GRN / Stock-Adjustment
Legacy uses config flags (APPPO, APPGRN, BDSA) gating who can post.
- **Source.** `POController.cs:76-77`, `GRNController.cs:64-80`, `StockAdjustmentController.cs:116-150`.
- **v2.** `WorkflowAuthorizer` service checks `Permission(role, 'approve_po')` + `Permission.limit_value` (e.g. only approve POs under LKR 100k). Hangfire job sends escalation if pending > 3 days.

### R19. Pricing override per outlet
`ProductStock.selling_price` overrides `Product.default_price` for that outlet. Falls back to product default.
- **Source.** `Products.SellingPrice`, `ProductStockMasters.SellingPrice`.
- **v2.** `PricingService.Resolve(product_id, outlet_id, customer_id, datetime)` walks: customer-product override → outlet override → product default. Returns `applied_pricing_rule` for audit.

---

## Kitchen / KOT

### R20. One product can route to multiple kitchen stations
A combo like 'Sizzler' fires tickets at Kitchen + Sauce Station + Plating Station.
- **Source.** `BLL_ProductKitchenMapper.cs:44-57`.
- **v2.** `ProductStationMapping` many-to-many. `KitchenTicketService` groups order lines by station and emits N tickets per order, one per station.

### R21. Recipe deduction independent of KOT print
Kitchen ticket generation does **not** decrement ingredient stock. That's a separate event (R1 + R15). KOT is a printing concern.
- **Source.** Trigger separation in legacy.
- **v2.** `KitchenTicketCreated` event has no stock side-effects. `OrderSettled` event drives stock. Keep them isolated.

### R22. Catering mode affects routing + tax + service charge
`CateringMode` (dine_in/takeaway/room_service/delivery) controls: which stations to route to (some only for dine-in), whether service charge applies, which tax band.
- **Source.** `CateringMood.cs`, `database.md:21`.
- **v2.** `Order.catering_mode` enum drives: `KitchenTicketService` skips dine-in-only stations for takeaway; `TaxCompositionService` includes `CateringModeTax` layer; `ServiceChargeCalculator` checks if mode is service-charge-eligible.

### R23. Custom kitchen instructions
Free-text notes per line ('No onion', 'Extra spicy') appear on KOT.
- **Source.** `ProductInstruction.cs`.
- **v2.** `OrderLine.item_comment` text field; copied to `KitchenTicketLine.instructions`; printed on ticket.

---

## Finance / GL

### R24. GL transfer via outbox (replaces OPENROWSET)
Legacy `SpTransferToGL` uses dynamic SQL `OPENROWSET` to push journal rows to a remote SQL Server. Not idempotent — re-runs can double-post.
- **Source.** `/tmp/hms.sql:17968-18100`.
- **v2.** `GLTransferJob` reads `journal_entries WHERE gl_posted = false`, batches, POSTs to external GL via authenticated REST API with `Idempotency-Key`. On 2xx, marks `gl_posted = true`. Retries via Hangfire with exponential backoff. No dynamic SQL, no OPENROWSET.

### R25. Daily journal entry generation
`spImportJournalDetails` aggregates `PaymentDets` (excluding `PayTypeID IN (9, 12, 13)`) by location + payment type, inserts journal rows with `TRANTYPE='08'`.
- **Source.** `/tmp/hms.sql:16438-16600`.
- **v2.** Hangfire job runs at outlet end-of-day. Reads `OrderPayment` rows for the period, groups by `payment_method`, posts double-entry journal rows: DR Cash/Card AR, CR Sales Revenue, CR Tax Payable. Emits `JournalEntryGenerated` event for downstream consumers.

### R26. Month-end period lock
`MonthEnds.LocStatus` + `LocIsClose` flags prevent transactions after a period is closed. Logic enforced in UI, not DB.
- **Source.** `MonthEndController.cs:87-148`.
- **v2.** `MonthEnd.state` enum (`open` / `locking` / `locked` / `closed`). `OrderSettlementService` precondition: reject if `order.transaction_date.month` is in a `locked` period for that outlet. EF Core interceptor enforces this on every settled write — defence-in-depth.

### R27. Payment method commission accrual (v2)
`PayType.Rate` / `PaymentMethod.CommissionRate` should generate a commission payable entry per payment. Legacy doesn't post this; assumed manual.
- **Source.** `PayTypes.Rate`, `PaymentMethods.CommissionRate`.
- **v2.** On `OrderSettled`, `CommissionAccrualHandler` posts: DR Commission Expense, CR Commission Payable. Monthly batch payout.

### R28. Currency historical snapshot
`CurrencyHistories` is the rate audit. Invoices lock the rate at creation.
- **Source.** `CurrencyHistory.cs`.
- **v2.** Nightly job ingests Central Bank rates into `CurrencyHistory`. Invoice creation reads latest-before-now from `CurrencyHistory` and stores on `Order.currency_rate`.

---

## Tenancy / Auth

### R29. Tenant isolation on every query
Legacy uses `GroupOfCompanyID + CompanyID + LocationId` in every `WHERE` clause. Missing filter = data leak.
- **Source.** Every entity in legacy schema.
- **v2.** Three-layer defence:
  1. **EF Core global query filter**: `HasQueryFilter(e => e.tenant_id == _tenantContext.TenantId)` on every entity.
  2. **TenantContext middleware** extracts `tenant_id` from JWT and sets `SET LOCAL app.tenant_id = ...` on the Postgres session.
  3. **Postgres RLS policy** on every table: `tenant_id = current_setting('app.tenant_id')::uuid`. Fails closed if middleware forgets to set it.

### R30. License key removed
Legacy `ACTIVATIONKEY.dbf` file with TripleDES + MAC binding is gone.
- **Source.** `AccountController.cs:544-690`.
- **v2.** Subscription status check at login. Trial countdown via `Subscription.trial_end_at`. No machine binding.

### R31. No password storage
Entra External ID is source of truth. Magic-link for fallback.
- **Source.** `ApplicationUser` + `SysUserMasters` dual-identity in legacy.
- **v2.** Single `User` entity. No `password`, `confirm_password`, `is_user_must_change_password` columns. Login routes through Entra OIDC.

---

## Cross-cutting

### R32. Append-only audit log
Legacy `TransactionLogs` has `ModifiedUser` + `ModifiedDate` — mutable audit (oxymoron).
- **Source.** `TransactionLogs` table.
- **v2.** `TransactionLog` is append-only. No update, no delete. Insert via outbox handler on every domain event. Stored in tenant DB; archived to cold storage after 7 years.

### R33. Idempotency on every money-affecting write
Network retries and webhook re-deliveries are inevitable.
- **Source.** Implied by aggregator integration scope.
- **v2.** Mandatory `Idempotency-Key` header on `POST /orders`, `/orders/{id}/payments`, `/orders/{id}/settle`, `/orders/{id}/void`, `/grns`, `/stock-adjustments`, all webhooks. Server stores `(tenant_id, idempotency_key) → response_body` for 24h in Redis.

### R34. Document numbering via sequence service
Legacy `DocumentNumbers` lookup table + UI counter logic.
- **Source.** `DocumentNumbers`.
- **v2.** Per-tenant Postgres sequence per document type: `seq_purchase_order`, `seq_grn`, `seq_order_receipt`. Format: `PO-2026-000123`. Generated atomically on insert.

### R35. Soft delete via `is_deleted` boolean
Legacy uses both `IsDelete` (per-entity, sporadic) and hard delete.
- **Source.** Various.
- **v2.** Every entity has `is_deleted boolean DEFAULT false`. Partial index `WHERE is_deleted = false`. EF Core global filter excludes deleted. Hard delete only at year-end archive (and only after `SubscriptionCancelled + retention_period`).

---

## Open Questions

1. **R6 / R8 — aggregator stock semantics.** Does Uber confirm `picked_up` reliably enough to gate stock decrement, or do we need a backup timer? [?]
2. **R10 — credit-limit override frequency.** Is `current_outstanding_amount` recomputed in real-time, nightly, or on-demand? **Recommend** nightly + on-demand for orders > LKR 50k.
3. **R15 — recipe ingredient deduction timing.** Decrement at `order_status = confirmed` (when KOT prints) or `order_status = posted` (settlement)? Legacy fires on settlement. **Recommend** keep on `posted` for consistency. [?]
4. **R20 — multi-station tickets ordering.** If Plating waits for Kitchen, does the system enforce that, or is it operational discipline? **Recommend** operational for v1; visual indicator in Print Agent for v2.
5. **R24 — external GL system identity.** What does the pilot customer use? SAP / QuickBooks / Tally / custom? Adapter pattern can wait until we know. [?]
6. **R29 — RLS policy performance.** Adding RLS on every table may hurt OLAP queries. Test under load; if needed, exempt read-only reporting role.

