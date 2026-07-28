# Backlog — RIT HMS

> Prioritised, living backlog. Status as of **2026-06-04**. IDs match the working
> task tracker. See `sprint-plan.md` for the sprint grouping.

## Legend
`P0` ship-blocker for live SaaS · `P1` high value · `P2` nice-to-have · `🅿️` parked · `🔒` blocked on RIT
✅ done · 🟡 partial · ❌ not started

---

## Remaining work — done vs not done (current)

> Legacy **feature parity is complete** and the v1 follow-on tail is cleared. What's
> left is the go-live infra (P0), net-new P2 modules, small follow-on remainders on
> shipped modules, and scale hardening. Detail for each ID is in the tables below.

### 🔒 Blocked on RIT (can't action in code)
- ❌ **#14 Rotate the leaked SA passwords** on the live servers — **P0, URGENT**.
- 🟡 **#9 Aggregator access** — ✅ PickMe real API live + sandbox-validated; ❌ PickMe **LIVE key** (on UAT sign-off) + ❌ **Uber Eats sandbox**.
- ❌ **#13 Pilot** at one outlet + daily reconciliation over two live shifts — P1.
- ❌ **#7 Internal sign-off** on repo destination — P2.

### 🚀 P0 — the only go-live blocker left
- ❌ **Deploy + ops** — hosting, automated prod migration runner, backups, error tracking, health dashboards.

### 🟢 P2 — whole modules NOT built
- ❌ **#67 Gift cards / vouchers** · ✅ **#73 GL / accounting export** (chart, double-entry journals, sales/purchase auto-post, expenses, AP, trial balance, CSV) · ❌ **#74 Handheld waiter PWA** · ✅ **#75 Catering / banquet** (halls, per-head packages, bookings + function billing, deposits, own-fleet off-site) · ✅ **#76 POS depth** (covers, steward + tips, multi-currency tender, tour-agent commission) · 🟡 **#53b Product CSV bulk import** (CRUD ✅, importer ❌) · 🅿️ **#57 GRN mobile PWA** (parked) · ❌ Manager read-only mobile dashboard.

### 🟡 Follow-on remainders on shipped modules
- ❌ **#16 Print agent binary** (queue ✅) + per-agent token + POS "enqueue vs browser-print" toggle.
- ❌ **#66 expiry-reminder delivery** (needs a notifications channel; expiring-soon endpoint ✅).
- ❌ **#65 bank-BIN promos** (needs card-BIN capture at settle).
- ❌ **#72 unified `inventory_movements` ledger** (bin card works without it).
- ❌ **#77 day-end-across-shifts rollup** (month-end lock ✅).
- ❌ **#71 document state-machine / change log** (per-screen perms ✅).

### ⚙️ Scale / hardening (surfaced building v1 — fine for a single-node pilot)
- ❌ **Realtime KDS multi-instance backplane** (SSE is in-memory; add Postgres LISTEN/NOTIFY or Redis for >1 API node).
- ❌ **PickMe poller leader-election** (so a tenant isn't polled by every node against the 30/min cap).
- ❌ **Provisioning background job** (runs inline today) + **prod secret store** for JWT signing / master keys.

### ⚪ Low / backlog (won't block anything)
- ❌ Scale/weighed items (weight-per-unit) · ❌ Sinhala / secondary-language names + reference codes · ❌ reorder qty + period (reorder *level* exists) · ❌ requisition → PO merge + reorder-level PO suggestions · ❌ supplier ↔ product link (preferred supplier / agreed cost) · ❌ receipt-print short name · ❌ per-product price floor/ceiling · ❌ GRN in-place correcting edit + back-dated receipt · ❌ PO/GRN summary-detail + Excel export · ❌ sales targets · ❌ kitchen-instruction templates.

---

## ✅ Sprint A — procurement & product parity (DONE)

| ID | Item | Status |
|----|------|--------|
| #49 | PO + GRN purchasing unit → stock conversion | ✅ |
| #50 | PO line/header discounts + freight (landed cost) | ✅ |
| #51 | GRN free items + supplier invoice # | ✅ |
| #52 | PO send/cancel/edit + GRN void/reverse | ✅ |
| #56 | PO approval (maker-checker) + supplier returns/PRN + currency/delivery/terms | ✅ |

## ✅ Sprint B — customer-facing depth (DONE)

> Product master is now at full legacy-parity-plus. Next up: **Sprint C (live-SaaS P0s)**.

| ID | Item | Pri | Notes |
|----|------|-----|-------|
| #54 | **Modifiers / add-ons** (+ POS wiring) | ✅ | groups/items + attach + POS picker + line pricing. |
| #55a | **Kitchen / printer routing** | ✅ | Stations CRUD + per-product routing + KOT split per station + printer name (for #16 print agent). Menu dropdown + stations manager. |
| #55b | **Serving-size variants** (cup/pot, S/M/L) | ✅ | One product, several sellable sizes — each its own price + printed label. POS size-picker, Menu sizes editor. (v1: price/name; stock decrements by qty.) |
| #55c | **Per-location pricing / price levels** | ✅ | Named levels (Dine-in/Delivery/…), per-product overrides, auto-select by order type (delivery markup), explicit override. Menu price-by-level matrix + levels manager. |
| #55d | **Per-product tax class** | ✅ | VAT base now honours per-product taxability — standard / zero-rated / exempt drop in/out of the VAT base; mixed baskets taxed correctly. (Org-wide stacked/compound taxes already configurable via the bill-level engine, #25.) Menu tax-treatment dropdown. |
| #53b | Product **CSV bulk import** | P2 | CRUD + Menu UI ✅; bulk import for onboarding still a stub. |

## Soon (Sprint C — make it a live SaaS)

| ID | Item | Pri | Notes |
|----|------|-----|-------|
| #17/#20 | **Tenant auto-provisioning** | ✅ | Signup → `ProvisionAsync`: CREATE DATABASE → run every tenant migration → seed baseline. Runs inline; a background job is the only scale follow-on. |
| — | **Auth hardening** | ✅ | Magic link returned only in Development; tenant list `PlatformAdmin`-gated; refresh/logout + rotating refresh tokens; secrets via master key. (Prod: load signing/master keys from a real secret store.) |
| — | **Deploy + ops** | **P0** | THE remaining go-live blocker: hosting, automated migration runner in prod, backups, error tracking, health dashboards. |
| #26 | **Outlet switcher + location types** | ✅ | HQ rollup ✅ + Owner/Manager **branch switcher** in the sidebar (drives POS/floor/reports). ✅ **outlet CRUD + types/capabilities** (outlet / central_kitchen / warehouse / head_office · can-sell/produce/stock) via Settings → Outlets. |

## Later (Sprint D–E)

| ID | Item | Pri | Notes |
|----|------|-----|-------|
| #16 | **Print agent** (KOT + bills, thermal) | 🟡 | ✅ **server-side queue** built: `print_jobs` + `/api/v1/print` (enqueue · poll · ack), contract in `docs/print-agent.md`. ↳ remaining: the venue **agent binary**, a per-agent token, and a POS "enqueue instead of browser-print" toggle. |
| #57 | **GRN mobile PWA** | 🅿️ | Parked by request. Foundation kept (barcode field + viewport). Resume: `/m/grn` installable app, scan, receive vs PO + ad-hoc. |
| — | Manager mobile dashboard (read-only) | P2 | |
| #13 | **Daily reconciliation** report + **pilot** at one outlet | P1 | Aggregator settlement vs sales; two live shifts. |

## Legacy-parity modules NOT yet built (full system gap analysis, 2026-06-02)

> From a code-level inventory of the legacy `legacy/HospitalityManagement` app
> (controllers + RIT.HMS.Domain/BLL entities) diffed against the new system.
> These are **whole modules / capabilities the legacy has that the rewrite does
> not yet** — the real remaining parity surface. ✅ full · 🟡 partial · ❌ none.

| ID | Module | New status | Pri | Notes / legacy mapping |
|----|--------|-----------|-----|------------------------|
| #65 | **Promotions engine** | 🟡 v1+ | **P1** | ✅ schedulable (date/day/**happy-hour** window) + order-type-scoped **product-discount**, **bill-value (spend & save)**, **buy-X-get-Y (BOGO)** and **bundle/combo** — all auto-applied at the till, netted on the bill + snapshotted (`order_promotions`); admin UI (per-type rule editors) + POS line. ✅ **lowest-price (3-for-2)** + **customer-segment** scope. ↳ follow-on: bank-BIN (needs card-BIN capture at settle). Legacy: `InvPromotion*`, `RstPromotions`. |
| #66 | **Loyalty program** | 🟡 v1 | P2 | ✅ v1 (built on CRM #70): org **earn rate** (points per LKR) + **redeem value**; points **earned automatically on settle** for the attached customer; **redeemed at the till as a "Points" tender** (bounded by balance); per-customer balance + lifetime + **transaction ledger** in the CRM drawer; Settings config toggle. ✅ **tiers** (earn multiplier + tier discount by lifetime points) + **points expiry** (inactivity window, lazy + sweep endpoint) — tier CRUD + expiry config in Settings. ✅ **loyalty cards** (card no. + by-card lookup) + **expiring-soon** endpoint. Legacy: `InvLoyaltyTransaction`, `LoyaltyCustomer`. ↳ follow-on: expiry-reminder *delivery* (needs a notifications channel). |
| #67 | **Gift cards / vouchers** *(Phase 2)* | ❌ | P2 | Voucher master + books, purchase/GRN/transfer/cancel, **spend-get-voucher** promo, POS redemption. Legacy: `InvGiftVoucher*`, `GiftVoucher*Controller`. |
| #68 | **Table / floor management + reservations** | 🟡 v1 | **P1** | ✅ v1: per-outlet tables (code/seats/area), Floor screen grid with live **open/occupied** (derived from open orders), tap → open/resume a bill at that table; reservations list + lifecycle (book/seat/cancel/no-show). POS `?table=` deep-link + `orders.table_id`. ✅ **visual X/Y floor plan** — drag-to-arrange Plan view on the Floor screen (pos_x/pos_y, live open/occupied, tap to open/resume). ↳ follow-on: chairs, shapes. Legacy: `TableMaster`, `ChairMaster`. |
| #69 | **Split / merge bill + table transfer** | 🟡 v1 | **P1** | ✅ v1: **merge** another open tab into the current bill (source voided), **transfer** to another table (blocks occupied), POS **table picker** on "+" (billing selects a live floor table; resume occupied). ✅ **item-level split bill**: move chosen item quantities (incl. partial) onto a new bill at the same table (`/orders/{id}/split` + POS Split modal). |
| #70 | **CRM / customers** | 🟡 v1 | **P1** | ✅ v1: customer master + **categories**, **per-customer (or per-category) default discount** auto-applied when attached at the till, **credit customers (AR)** — credit limit + running balance, **charge-to-account ("Credit") tender** bounded by the limit, **AR receipts** that pay the balance down, **ledger** (visit history + receipts). POS: attach/quick-add a customer on the bill; Customers screen (CRUD + categories + ledger drawer). ✅ **per-product contract pricing** (customer/category price override resolved at the till) + **AR statements** (opening + running balance + closing for a period) — both in the CRM drawer. ✅ **advance/deposit** payments (`advance_balance` + 'advance' tender); suspended bills are covered by held/open orders + recall. Legacy: `Customer`, `CustomerCategory`, `CustomerDiscounts`, `CustomoerPreviousVisit`. |
| #71 | **Granular permissions + approvals** | 🟡 v1 | **P1** | ✅ v1: configurable **per-role POS limits** (`role_permissions`) — **max-discount %**, can-void, can-comp/no-sale — enforced server-side on the discount & void endpoints by the caller's role (Owner bypasses; unconfigured roles stay permissive so nothing breaks until tightened). Owner-only **Role permissions** matrix on the Team screen. ✅ **manager-PIN override**: an Owner/Manager PIN authorises an over-limit discount or a void at the till (verified server-side, audited as "approved by …"). ✅ **function-level (per-screen) access** (`role_screen_access`) — owners hide whole screens per role; sidebar gates + Team matrix. Legacy: `CashierPermission`, `SysUserGroupPermission`. ↳ follow-on: doc state machine (`DocStatusChangeLog`). |
| #72 | **Reports library** | 🟡 v1 | **P1** | ✅ now: VAT return · sales summary (KPIs/per-day/top items) · HQ outlet rollup · **sales register** (transaction list + tender + totals) · **item sales** (usage/best-sellers) · **stock balance** (on-hand + value, as-at) · **shift settlement** (cash-ups + variance) · **promotion usage**. All on the Reports screen with date/outlet filters. ✅ **food costing** (dish cost from production roll-up / recipe estimate vs sell price + GP%) · ✅ **bin card** (per-product movement ledger from GRN/sales/wastage/adjustment/transfer/production with running balance). ✅ **budget vs sales** (per-outlet monthly target vs actual). ↳ follow-on: a unified `inventory_movements` table to simplify the bin card. Legacy: `ReportController`, `BLL_Reports`. |
| #73 | **GL / accounting export** | ✅ | P2 | Chart of accounts (seeded), **double-entry journals** (Σdr=Σcr) with **auto-posting from settled sales** (Dr tenders/discounts, Cr revenue/svc/tax/tips) and **posted GRNs** (Dr inventory/input-VAT, Cr AP), idempotent per source doc; **expenses** (Dr expense/Cr cash) + **supplier AP payments** (Dr AP/Cr cash); **trial balance**, **AP aging**, **CSV export** of posted lines. Migration 0052 (ENABLE+FORCE RLS); `/api/v1/accounting/*` (BackOffice = Accountant role); Accounting screen (journals/trial-balance/AP/expenses/chart). Legacy: `ImportJournalDetails`, `BLL_Journal`, `PaymentTerm`. Follow-ons: cost-centre dimension, cheque/bank reconciliation, multi-currency revaluation. |
| #74 | **Handheld waiter order-taker (tab ordering)** | ❌ | P2 | Legacy STOS tablet app: waiter takes orders at table → KOT → main POS. Legacy: `HMSOrderTaker` project, `STOS_TabOrder*`. (New PWA shell could host this + #57 GRN.) **Steward auto-assign (#76 tie-in):** when a bill is created from the tab-ordering app, auto-set `order.steward_id = the logged-in waiter` so tips/sales attribute without a manual pick. Backend already supports it — `CreateOrderInput.StewardId` is honoured on create; the app just passes its own user id. **Implication:** a waiter who *uses* tab ordering must be a login user (PIN) **and** flagged `is_server` — i.e. not a login-less steward-only record. So the model has two waiter shapes: (a) login-less Stuart records (cashier picks them on the bill), (b) PIN + is_server waiters who self-serve via tab ordering and get auto-assigned. |
| #75 | **Catering / banquet** | ✅ | P2 | Halls (venues) + per-head **packages**; **event bookings** with hall double-booking prevention + enquiry→confirmed→running→completed/cancelled lifecycle; **function billing** = pax × price/head + ad-hoc extras − discount, with **deposit/advance → balance**; **own-fleet off-site** delivery (address/vehicle/driver + dispatch pending→dispatched→delivered). **Inventory tie-in:** a package links to a recipe; **Produce** posts a pax-scaled production order that consumes ingredients from stock (reuses Production) and records the event's food cost + gross margin (migration 0055). Migration 0054 (5 tables, ENABLE+FORCE RLS) + 0055; `/api/v1/catering/*`; Catering screen (Bookings/Halls/Packages + drill-down, recipe link, Produce). ✅ **booking validation** (pax ≥ 1 + title-or-customer required, client + server — a 0-pax booking is now rejected, not silently saved) + **edit an existing booking** (pencil on the detail re-opens the pre-filled form; upsert recomputes the bill; locked once produced). Follow-ons: catering tax mode, room-type nightly rates, driver/vehicle masters + dispatch board. Legacy: `RoomMaster`, `Event`, `MealType`, `DeliveryPerson`, `Vehicle`. |
| #76 | **POS depth: covers, steward, tips, multi-currency** | ✅ | P2 | Covers per bill, **steward/waiter attribution + tips/gratuity** (added on top, untaxed), **multi-currency tender** (currency master + FX rate → base-currency settlement, payment stores currency/rate/base amount), **tour-agent commission** (operator master + % booked off net at settle). Steward-sales + tips report and per-operator commission report. **A steward IS a user flagged `is_server`** (managed in Team — no parallel people-master; server-only waiters can be login-less name records); POS "served by" list = `/api/v1/servers`. Migration 0050; masters `/tour-operators` `/currencies`; POS "Details" panel + foreign-tender selector. Legacy parity: `TransactionDet.StewardID/NoOfAdults`, `PaymentDet.CurrencyCode/CurrencyRate`, `TourAgentId`. |
| #77 | **Day-end / month-end + stock count + audit log** | 🟡 v1 | P1 | ✅ now: **audit / activity log** (append-only: settle/void/discount/shift open-close/permission changes + stock-count post, with actor+role; filterable screen) · **physical stock count** (open a sheet → snapshots on-hand per stocked product → enter counts → post writes the variance back to stock + stamps last_counted_at; Stock-count screen). Have: shift Z-report. ✅ **month-end period lock** (close books through a date; settle + GRN/return/production reversals into a closed period are blocked; close=BackOffice / reopen=Owner, audited). ↳ follow-on: formal **day-end across shifts** rollup. Legacy `TransactionLog`/`DocStatusChangeLog`/`LOG*`. |
| #78 | **Recall settled bill + reprint receipt** | 🟡 | **P1** | ✅ now: **Print** on the POS prints the live bill/tax-invoice (80mm thermal receipt via `/orders/{id}/invoice`), gated on items (no more blank-bill print). ✅ **recall a settled bill** (`/orders/settled` search) + **reprint a duplicate** (`reprint_count`, marked "REPRINT · COPY N", audited) via a POS Recall modal. Legacy: duplicate-bill print + bill reprint permission. |

> **Already at full legacy parity (✅):** POS billing + multi-tender settlement, KOT
> + per-station/printer routing, shifts + Z-report, order resume/suspend-recall,
> products/categories/units + UoM conversion, modifiers/sizes/price-levels/tax-class,
> suppliers/PO/GRN/PRN, transfers/wastage/adjustments, production (BOM + parity),
> compound tax/VAT, document numbering/prefixes. **New does *more* than legacy:**
> multi-tenant SaaS + RLS + auto-provisioning, JWT auth + refresh, Uber/PickMe
> aggregator integration, weighted-avg cost roll-up into finished goods.

## Blocked on RIT 🔒

| ID | Item | Pri | Notes |
|----|------|-----|-------|
| #14 | **Rotate leaked SA passwords** on live servers | **P0** | URGENT — compromised regardless of this rewrite. |
| #9 | Apply for **Uber Eats + PickMe sandboxes** | P1 | ✅ **PickMe: real API live** (POS API v1.4.7) — per-outlet X-API-KEY (encrypted), `/joblist` poller (pull/mirror model, gated `Aggregators:PickMe:PollingEnabled`), order ingestion (items by ref_id=SKU, options/sp_ins→notes, prepaid settle, status mirror, cancel→void), menu/price/availability push (`update/item/ref`), live sandbox validated end-to-end. ↳ **Uber Eats** still mock (needs its sandbox). ↳ PickMe LIVE key on UAT sign-off. |
| #7 | Internal sign-off on repo destination | P2 | |

---

## Legacy → New parity register (detail: product / PO / GRN / production)

Complete record from the legacy-vs-new gap analyses (product master, PO, GRN,
production). High-severity items are already top-line above; this captures the
**remaining** medium/low items so none are lost.
**Legend:** ✅ = done · ↳ Sprint A/B = **delivered** (those sprints are complete) ·
↳ Sprint C / reporting / backlog = still planned.

### Product master
| Gap | Sev | Status |
|----|----|----|
| Update / delete + create UI | High | ✅ done (#53) |
| Modifiers / add-ons (✅ #54) · kitchen routing (✅ #55a) · serving sizes (✅ #55b) · per-location pricing (✅ #55c) · per-product tax class (✅ #55d) | High | ✅ Sprint B complete (#54 / #55a–d) |
| **Supplier ↔ product link** (preferred supplier, last/agreed cost) | Med | ↳ Sprint A |
| **Receipt-print name** (short bill name vs long menu name) | Med | ↳ Sprint B |
| **Per-product discount rules** + price floor/ceiling (max/fixed %, min/max) | Med | ↳ Sprint B |
| **Reorder quantity + period** (only reorder *level* today) | Med | ↳ Sprint C |
| **Pack / bundle / combo** items | Med | ↳ Sprint B |
| Scale item / weight-per-unit (weighed goods) | Low | backlog |
| Sinhala / secondary-language name · reference codes (RefCode01/02) | Low | backlog |
| Sales targets (qty/period/type) · kitchen instruction templates | Low | backlog |
| Open item · batch/expiry | — | ✅ (POS open item; GRN batch/expiry) |

### Purchase Orders
| Gap | Sev | Status |
|----|----|----|
| Unit conversion · discounts/charges · approval · cancel/edit · currency · terms | High/Med | ✅ **done** (Sprint A — #49/#50/#52/#56) |
| **Per-line / multi-tax + tax-on-tax** | Med | 🟡 product-side tax class ✅ (#55d); per-PO-line multi-tax not yet |
| **Separate delivery vs PO location** (+ delivery address) | Med | ✅ done (PO `delivery_location_id` + `delivery_address`) |
| **Reorder-level PO suggestions / requisition → PO merge** | Med | ↳ Sprint C |
| **Month-end period lock** (block posting into closed periods) | Med | ✅ done (#77 — `0041_period_lock`; settle/GRN/return/production into a closed period are blocked) |
| Free qty on PO line · validity/expiry dates · event tag · requested-by · reference no | Low | backlog (free qty exists on GRN) |
| PO summary/detail reports + Excel export | Low | ↳ reporting |

### GRN
| Gap | Sev | Status |
|----|----|----|
| Unit conversion · free items · line discount · supplier invoice # · void/reverse | High | ✅ done |
| Supplier returns / debit note (PRN) + input-VAT reversal · approval | High/Med | ↳ Sprint A (#56) |
| **Recipe/menu cost roll-up on receive** (recompute dish cost from new avg) | Med | ↳ Sprint A/B |
| **GRN in-place edit** (void exists; correcting edit doesn't) | Med | ↳ Sprint A |
| **`inventory_movements` ledger row on GRN** (transfers/wastage log; GRN mutates stock directly) | Med | ↳ Sprint A (audit consistency) |
| Month-end lock · back-dated receipt · GRN summary/detail reports · price-level on receive | Low | backlog / reporting |
| Serial / warranty tracking | — | Won't do (not F&B) |

### Production
All legacy capabilities reached: BOM + **UoM conversion**, custom/ad-hoc,
draft→post→**void**, multi-product document, multiple recipes per output unit,
inter-location. Legacy in-place "edit production" ≈ our **void + re-produce**.
New does better: weighted-avg cost roll-up into the finished good (legacy didn't).

---

## Done (highlights)

Foundation (cloud rewrite, multi-tenant + RLS) · **JWT auth enforced + RBAC** ·
POS (keypad/discount/multi-tender/open items) · KOT · **compound tax/VAT engine** ·
**shifts + cash-up Z-report** · suppliers/PO/**GRN (receive integrity)** ·
transfers/wastage/adjustments · **production (BOM + UoM conversion + full legacy
parity)** · delivery aggregators (mock) · **product CRUD + Menu screen** · reports
(VAT return, sales, **HQ outlet rollup**) · user/team management · configurable
prefixes/branding/tax-certs · **menu depth: modifiers, kitchen routing, serving
sizes, price levels, tax class** · **auth hardening (refresh/logout, prod-secret
guard, admin-gated tenants)** · **tenant auto-provisioning** (signup → DB) ·
**real PickMe POS API** (poll + menu push) · **loyalty tiers + expiry + cards** ·
**month-end period lock** · **visual floor plan** · **per-product contract pricing +
AR statements + advances** · **function-level perms** · **food costing / bin card /
budget-vs-sales** · **recall + reprint** · **print-job queue** ·
**app shell wired**: in-app **notification centre** (bell, real count, 60s
auto-poll — aggregator orders awaiting acceptance, low/out-of-stock vs reorder
level, today's reservations, upcoming catering; `GET /api/v1/notifications`) ·
**global search** (⌘K — menu items, customers, transactions) · in-app **HTML help
guide** (`/help`, searchable, module-by-module) · **account menu + sign-out
confirmation** (fixes accidental log-out from the sidebar).
Tested: 235 integration · 36 unit · ~24 E2E · CI.

### Realtime & push
✅ **(a) live web notifications (SSE)** — a general per-tenant event stream
(`RealtimeBus` + `GET /api/v1/events/stream`, the app-wide sibling of the KDS
`KitchenBroadcaster`) pushes one-line topic signals (`notifications`, `orders`).
The **notification bell** and the **delivery board** subscribe over `fetch` (JWT
on the header, auto-reconnect w/ backoff, debounced refetch) and update
**instantly** — a new Uber Eats / PickMe order pops the bell + queue with no
manual refresh; settle/void clears it. 60s poll kept as a backstop. Publish
points: aggregator ingestion, order settle, order void.
Still open, in rising effort: **(b) rider push** — needs a rider app/PWA (none
exists yet) + web-push/FCM plumbing; **(c) tab-ordering push** — depends on the
deferred **#74** handheld order-taker. *(Single-process bus today; multi-node
would move it behind Postgres LISTEN/NOTIFY or Redis.)*
