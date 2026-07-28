# RIT HMS v2 — v1 Scope (Opinionated)

This document defines the v1 release: the minimum viable rewrite that can replace the legacy system for a single pilot customer (one outlet, restaurant POS use case). Everything else is deferred to v2.

Related: [v2 Entities](./v2-entities.md) · [v2 API surface](./v2-api-surface.md) · [v2 Business rules](./v2-business-rules.md) · [v2 Multi-tenancy](./v2-multi-tenancy.md).

---

## TL;DR

> **v1 is a pure restaurant POS with cloud tenancy, KOT printing, basic inventory visibility, master data, 5 core reports, and tenant signup. No loyalty, no gift vouchers, no full PO/GRN, no hotel rooms, no advanced permissions, no multi-outlet. Aggregator integration schema exists but no live Uber/PickMe wiring.**

**Target.** 14-18 weeks of multi-agent effort to pilot.
**Pilot definition.** One restaurant uses the system end-to-end for 30 days without falling back to legacy.

---

## 1. What v1 includes

### 1.1 POS core
- Open table / open order
- Add lines with serving units, modifiers (addons), custom instructions
- Multi-tender payment (cash + card + cheque + voucher)
- Suspend / recall orders (multi-cashier pickup)
- Settle order → atomic insert of `Order` + `OrderLine` + `OrderPayment` rows; emits `OrderSettled` outbox event
- Void / recall with stock reversal
- Receipt print (thermal printer)
- POS keyboard shortcuts (numeric, F-keys)
- Location-based tax (one tax tier per outlet in v1)
- Order audit log (`TransactionLog`)

### 1.2 KOT / BOT
- Kitchen station master + product → station routing
- KOT ticket generation on payment confirmed
- Print Agent contract: long-poll `/kitchen-tickets/pending`, `PATCH /status`
- Modifiers + instructions on KOT
- Manual reprint
- Ticket audit (who printed, when, voided?)

### 1.3 Master data
- Customer + addresses + categories + per-customer discounts
- Employee (unified — no separate steward/delivery driver entities)
- Unit of measure + conversions (shared reference data)
- Department / Category / SubCategory hierarchy
- Currency + current rates (no history API)
- Tax definitions + `ProductTax` junction (simple — no full composition engine yet)
- Bank + bank bins (lookup only, not promotion rules)
- Payment methods (unified `PayType` + `PaymentMethod` table)
- Catering modes (dine-in / takeaway / room-service / delivery)
- Tables + chairs (read-only for waiter tablet)
- Catering events + event menus (defer if not needed by pilot)

### 1.4 Inventory (visibility only)
- Product master (raw + finished)
- ProductServingUnit per product
- Stock-on-hand per outlet (initialized via bulk import; auto-decremented on settled sales)
- Supplier master (CRUD only)
- Recipe (BOM) — required so finished-good sales decrement ingredients
- **No** PO / GRN / Transfer / Adjustment write flows in v1. Stock initialization is a one-time bulk CSV.

### 1.5 Reports (5 core)
1. Daily sales (CSV): receipt, location, food/bev/other, payment breakdown, tax, net, GP %.
2. Payment-method summary (cash-up sheet by date + outlet).
3. KOT log (tickets by date / outlet / station).
4. Sales-by-category (aggregated revenue by menu category).
5. Top-selling items (top N by revenue and by quantity).

Reports are CSV-export only in v1. Dashboard UI (Grafana) is v2.

### 1.6 Finance basics
- Month-end open / close (period lock)
- Journal entry generation from settled orders (writes to `JournalEntry` table)
- **No** external GL transfer (push to SAP/Tally) in v1 — accountant exports CSV manually
- **No** commission accrual

### 1.7 Tenancy & auth
- Tenant signup via Stripe (international) or PayHere (LK)
- Auto-provisioning Hangfire job: create DB → run migrations → seed → invite owner
- Entra External ID SSO **OR** magic-link fallback (see [v2-multi-tenancy.md §6](./v2-multi-tenancy.md))
- 5 roles (OWNER, MANAGER, CASHIER, KITCHEN, ACCOUNTANT) with basic permissions
- Subscription state machine (trialing → active → past_due → cancelled)
- One outlet per tenant
- `tenant_id` on every table + EF query filter + Postgres RLS

### 1.8 Aggregator integration (schema only)
- `Order.order_source` enum with `pos` | `uber_eats` | `pickme`
- `OrderType` and document logic designed to support deferred stock decrement
- **No** live Uber Eats / PickMe webhook integration in v1
- v1 customer accepts aggregator orders via the aggregator's own tablet; reconciles manually

---

## 2. What v1 explicitly defers to v2

| Capability | Why deferred |
|---|---|
| Loyalty (cards, points, redemption, expiry) | 46 code refs, 8 tables, zero pilot revenue |
| Gift vouchers (sale, transfer, redeem, expire) | 47 code refs, 10 tables; optional |
| Promotions engine (7 detail tables, rule engine) | High complexity, needs rule designer UI |
| Hotel rooms + charge-to-room | Requires PMS integration scope |
| Full PO / GRN workflow + approval | High-effort backend; pilot can survive without |
| Inter-outlet stock transfers | Single-outlet pilot |
| Stock adjustment write + approval | Single-outlet pilot |
| Request notes | Inter-outlet feature |
| Batch / expiry / serial tracking enforcement | Complex; v1 captures columns, v2 enforces |
| Advanced approval workflows + escalation | Use role permission limits in v1 |
| External GL transfer (SpTransferToGL replacement) | Accountant CSV export sufficient |
| Commission accrual + payout | Manual in v1 |
| Loan payments | Niche legacy feature |
| Multi-outlet | One outlet per tenant for v1 |
| Multi-currency settlement + revaluation | LKR-only pilot |
| Tour-agent commission tracking | Defer until needed |
| Live Uber Eats / PickMe integration | Schema designed; integration is v2 |
| Advanced kitchen features (meal-type windows, chair-level split, recipe variance per outlet) | Niche |
| Dashboards (Grafana) | CSV export in v1 |
| Cross-tenant analytics warehouse | v2 |
| Per-user permission overrides | Role-only in v1 |
| Mobile waiter app (Next.js on tablet) | v1 ships with desktop POS; tablet waiter is v1.5 |

---

## 3. Per-bounded-context v1 entity list

### Tenancy & Identity
| Entity | v1? |
|---|---|
| `Tenant` | yes |
| `Subscription` | yes |
| `SubscriptionPlan` | yes |
| `Outlet` | yes (1 per tenant) |
| `User` | yes |
| `Role` | yes (5 roles) |
| `Permission` | yes |
| `FeatureFlag` | yes |
| `TenantConfig` | yes |

### Master Data
| Entity | v1? |
|---|---|
| `Customer` | yes |
| `CustomerAddress` | yes |
| `CustomerCategory` | yes |
| `CustomerDiscount` | yes |
| `Employee` (unified) | yes |
| `Department` | yes |
| `Category` | yes |
| `SubCategory` | yes |
| `MealType` | no |
| `Currency` | yes |
| `CurrencyHistory` | no (data captured, no API) |
| `Tax`, `ProductTax`, `OutletTax` | yes |
| `PaymentMethodTax`, `CateringModeTax` | no |
| `Bank`, `BankBin` | yes (lookup only) |
| `Vehicle` | no |
| `CateringEvent` | no |
| `Partner` (tour agents) | no |

### Inventory
| Entity | v1? |
|---|---|
| `Product` | yes |
| `ProductServingUnit` | yes |
| `ProductStock` | yes (read + decrement) |
| `Supplier` | yes (master only) |
| `SupplierGroup`, `SupplierType` | yes |
| `UnitOfMeasure`, `UnitConversion` | yes |
| `PurchaseOrder`, `PurchaseOrderLine` | no |
| `GoodsReceiptNote`, `GoodsReceiptLine` | no |
| `Recipe`, `RecipeIngredient` | yes |
| `StockAdjustment` | no |
| `StockTransfer` | no |
| `RequestNote` | no |

### POS / Sales
| Entity | v1? |
|---|---|
| `Order` | yes |
| `OrderLine` | yes |
| `OrderPayment` | yes |
| `SuspendOrder` | yes |
| `AdvancePayment` | no |
| `OrderDiscount` | yes |
| `OrderPromotionLine` | no (schema only) |
| `TransactionLog` | yes |
| `CateringMode` | yes |
| `Table` | yes |
| `Chair` | no |

### Kitchen
| Entity | v1? |
|---|---|
| `KitchenStation` | yes |
| `ProductStationMapping` | yes |
| `KitchenTicket`, `KitchenTicketLine` | yes |
| `ServingUnit` | yes |
| `Addon`, `AddonCategory` | yes |
| `ProductInstruction` | yes |

### Finance
| Entity | v1? |
|---|---|
| `PaymentMethod` | yes |
| `PaymentTerm` | yes |
| `PaidInType`, `PaidOutType` | yes |
| `MonthEnd` | yes |
| `JournalEntry`, `JournalEntryBatch` | yes |
| `GLTransferLog` | no |
| `OutboxEvent` | yes |

---

## 4. Per-bounded-context v1 endpoint list

See [v2-api-surface.md](./v2-api-surface.md) for full details. v1 endpoints, by count:

| Context | v1 endpoint count | Notes |
|---|---|---|
| Auth + Tenancy | 11 | signup, login, webhooks, subscription status |
| Identity & Access | 8 | users, roles (read), outlets (read) |
| Master Data | 22 | customer, employee, category trees, tax, currency, payment method |
| Inventory | 12 | product, stock (read), supplier, recipe |
| POS / Orders | 16 | open/add/suspend/recall/pay/settle/void/print, transaction log |
| Kitchen | 7 | stations, ticket pending/status, reprint |
| Finance / Reports | 9 | 5 reports + month-end + audit |

**Total v1 endpoints: ~85.** Within the 60-80 target with a small overrun for control plane + reports.

---

## 5. Effort estimate per bounded context

Assumes multi-agent assisted development, opinionated stack (.NET 8, EF Core, Postgres, Next.js, Hangfire), and one human reviewer per agent.

| Context | Effort (person-weeks) | Notes |
|---|---|---|
| Tenancy + Auth + Provisioning | 3.0 | Hangfire pipeline, Entra setup, magic-link fallback, control plane |
| Master Data | 2.0 | CRUD-heavy; mostly Cursor-able once schema is fixed |
| Inventory (v1 — visibility only) | 1.5 | Product + stock cache + recipe + supplier master |
| POS / Sales / Settlement | 4.0 | Highest-risk; saga settlement, suspend/recall, multi-tender, void semantics |
| Kitchen / KOT | 2.0 | Print Agent contract + ticket service + station routing |
| Finance / Reports | 1.5 | 5 reports + month-end + journal generation |
| Aggregator schema readiness | 0.5 | Just enums + types; no live integration |
| Cross-cutting (outbox, RLS, audit, idempotency, observability) | 2.0 | Lift; pays dividends in every other context |
| QA, pilot onboarding, data migration script | 1.5 | Real customer onboarding |

**Sum: 17.5 person-weeks.** Target band 14-18 weeks ✓ (just inside upper bound; tight).

**Risks that push over 18 weeks.**
1. **Settlement saga complexity.** If the outbox + Hangfire + idempotency pattern requires 2-3 redesigns, +2 weeks.
2. **Print Agent.** ESC/POS printer drivers vary; one customer's printer may eat 1 week.
3. **Entra External ID setup automation.** First-time Entra provisioning automation may take longer than estimated. **Mitigation**: ship v1 with magic-link only; add Entra in v1.5.
4. **Aggregator schema vs reality.** When the real Uber/PickMe API contracts arrive, schema may need rework.

**Risk-adjusted estimate: 18-20 weeks.** Plan for 20 weeks, declare success at 18.

---

## 6. Pilot acceptance criteria

The pilot customer must be able to do the following without falling back to legacy or manual workarounds.

### 6.1 Daily operations
- [ ] Cashier opens a table, adds 5+ items with modifiers + custom instructions
- [ ] KOT prints automatically at the correct kitchen station(s)
- [ ] Multiple tables are open simultaneously without lock conflicts
- [ ] Suspend an order, switch terminals, recall on different terminal
- [ ] Settle with split payment (e.g. LKR 4000 cash + LKR 2000 card)
- [ ] Print receipt to thermal printer
- [ ] Void a settled order; supervisor approval required; stock and GL reversed
- [ ] System runs 12 hours continuous service with no crashes or data loss

### 6.2 Master data
- [ ] Admin creates 100+ products, 20+ categories, 5+ kitchen stations
- [ ] Admin creates 50+ customers with categories, discounts, addresses
- [ ] Admin creates 10+ employees with role assignments
- [ ] Admin uploads opening stock balances via CSV

### 6.3 Reports
- [ ] Daily sales CSV downloaded matches the legacy report (within 0.5% tolerance)
- [ ] Payment-method summary tallies with cash drawer count
- [ ] Top-selling items report ranks correctly by revenue and qty
- [ ] Month-end close locks the period; no further writes accepted

### 6.4 Tenancy & auth
- [ ] Owner signs up via Stripe/PayHere, receives invite email, logs in, completes setup in < 30 minutes
- [ ] Owner invites 5 staff users; each receives magic-link email, logs in successfully
- [ ] Role-based access enforced (cashier cannot void; manager can)
- [ ] Subscription enters trialing for 14 days, transitions to active on first payment
- [ ] Subscription `past_due` after failed payment → read-only mode triggered

### 6.5 Performance
- [ ] Open-order latency p99 < 300ms
- [ ] Settle latency p99 < 800ms
- [ ] KOT pending poll latency p95 < 100ms
- [ ] System sustains 50 orders/hour at a single outlet without degradation

### 6.6 Reliability
- [ ] Network outage during settle: order reaches consistent state on recovery (no double-decrement, no lost payment)
- [ ] Print Agent crash + restart: pending tickets are still picked up and printed
- [ ] Database failover (Azure Flexible Server zone-redundant): < 60s downtime; no data loss
- [ ] Outbox event handler retry recovers from any handler exception

### 6.7 Stretch (nice-to-have but not blocking)
- [ ] Real-time order dashboard (instead of CSV download)
- [ ] Customer search by partial NIC / mobile
- [ ] Bulk import of products from legacy CSV export

---

## 7. What v1 is NOT (anti-scope)

To prevent scope creep, here is what we are explicitly **not** building in v1, even if a stakeholder asks:

- A loyalty program (any form).
- A promotions / discounts rule engine beyond customer-product overrides.
- Gift voucher purchase + redemption.
- Multi-outlet centralised management.
- A waiter tablet app distinct from the cashier POS (waiter can use the cashier POS in v1).
- Live integration with Uber Eats / PickMe Food. (Aggregator orders are entered manually for v1.)
- A general ledger system (we generate journal entries; the customer's accountant handles GL).
- Email / SMS marketing.
- Customer self-service portal.
- Kiosk / self-order tablet for diners.
- Inventory forecasting / demand planning.
- Recipe cost optimization.
- Multi-currency settlement (LKR only).
- Reports beyond the 5 listed.
- A dashboard UI.
- Custom code per customer.
- Migration tooling from legacy SQL Server. (One-time manual data load.)

---

## Open Questions

1. **Pilot customer identity.** Is the pilot a specific named customer with known requirements, or a hypothetical generic restaurant? **Recommend** identify before kickoff so we can interview them.
2. **Aggregator manual entry in v1.** Is the pilot customer comfortable using Uber Eats / PickMe tablets in parallel and reconciling manually? Or do they need automation from day 1? If day 1 → push aggregator into v1, add 3 weeks. [?]
3. **Magic-link vs Entra for v1.** Strong recommendation to ship magic-link only and add Entra in v1.5. Decision needed by week 1.
4. **Multi-tenant in v1 or single-tenant?** v1 includes tenant signup + auto-provisioning. If the pilot is the only customer, we could ship single-tenant and add multi-tenancy later. **Recommend** keep multi-tenant — provisioning is the hard part; better to do it once correctly.
5. **Data migration script.** Is the pilot customer migrating from legacy or starting fresh? Migration tooling is +1 week.
6. **Receipt printing format.** Locked to one thermal printer model, or flexible driver? **Recommend** lock to one model (Epson TM-T82) for v1; add driver flexibility in v2.
7. **Backup / disaster recovery testing.** Is a 60s RTO acceptable for the pilot, or do we need < 10s? Affects DB topology choice (zone-redundant vs cross-region). **Recommend** zone-redundant for v1.

