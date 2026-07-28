# Reports — RIT HMS

> Tracks every report in the Reports screen (`apps/web/app/(tenant)/reports/page.tsx`),
> grouped by section as they appear in the UI. Update the status here whenever a
> report is added. Backend lives in `apps/api/Features/Reports/ReportsEndpoint.cs`
> and `apps/api/Features/Orders/OrderService.cs` (a few of the older ones).

✅ done · 🟡 backend only, no UI tab yet · ❌ not started

## Pagination component sweep (2026-07-22)
Audited every list page under `apps/web/app/(tenant)/*/page.tsx`. 15 pages were already
server-paginated on the backend (real `Skip`/`Take` + `totalCount`) but hand-rolled their
own "Showing X-Y of Z / Page N of M / Previous / Next" footer instead of the shared
`<Pagination>` component (`@/components/ui/Pagination`) — all 15 swapped over:
Locations, Departments, Suppliers, Wastage (notes), Stock Adjustments, Transfers, GRN,
Request Notes, Tax Types, Tax/Service Charge, UOM Conversions, Serving Units, Price
Levels, Kitchen Stations, Products (menu). Verified via `tsc --noEmit` (clean) and live
click-through on 4 representative pages (Locations, Suppliers, Wastage, Serving Units,
Products) — all render correctly, no console errors.

**Bucket B — now fixed too**: the remaining pages that had NO server-side pagination
each got a new backend `GET .../paged` endpoint (real `Skip`/`Take` + `totalCount`,
per-feature `PaginationMeta` record) and a frontend rework onto the shared
`<Pagination>` component: Unit of Measure, Modifiers, Promotions, Team, Tour Operators
(both the Agents and Companies tabs), Stock Count, Transactions, Purchase Orders, and
Audit Log. Verified live against real data (or, where the demo tenant had no rows for
that master, via a direct `curl` against the new endpoint) — all return correct
`totalCount`/`totalPages` and the UI renders "X–Y of Z / Page N of M" correctly.

**Deliberately left unpaginated**: Product Categories — it renders as a genuine
one-level parent→child tree (not a flat list; children are separate rows nested under
their parent), so row-level `Skip`/`Take` would risk splitting a parent from its
children across a page boundary. The dataset is also small and curated (a dozen or so
rows in the demo tenant), so this was a deliberate judgment call to leave it as a
single unpaginated list, the same way `replenishment/page.tsx` was excluded from the
original audit as "not a plain record table".

Transactions note: the existing "split payment" / "combo:x+y" tender filters are
still computed client-side and now only see the current page's rows (previously they
saw up to 1000 rows for the selected date range) — a deliberate trade-off to get real
pagination; the single-tender filter (`payType=cash` etc) is unaffected since it was
already pushed server-side.

**Reports screen — every report endpoint is now server-paginated**: the Reports screen
(`reports/page.tsx`) has ~19 report endpoints under `apps/api/Features/Reports/ReportsEndpoint.cs`.
Six genuinely grow without bound — either one row per settled bill/shift (time-cumulative,
same shape as Transactions) or one row per catalog item (grows with menu/product-catalog
size):
- **Sales Register**, **Daily Summary Detail** — one row per settled bill.
- **Shift Settlement** — one row per shift opened; missed in the first pass, added after
  a follow-up review since it accumulates over time exactly like Register.
- **Stock Balance** — one row per stocked product x outlet; converted from an in-memory
  dictionary join to a proper `IQueryable` join so `Skip`/`Take` runs in SQL.
- **Food Costing** — one row per active recipe; scales with menu size, same class as
  Stock Balance.
- **Bin Card** — the running balance is inherently sequential (each line depends on the
  cumulative sum of every prior line), so it can't use a plain SQL `Skip`/`Take`. The
  full ledger is still built once per request; only the returned page of `lines` is
  sliced afterward, so opening/closing/totalIn/totalOut stay whole-period.

The remaining seven group by a small, roughly-fixed dimension (menu category, steward,
table, tour operator, promo code, budget month) so their row count doesn't grow with a
wider date range — but were paginated anyway for UI consistency across every report list
view, per explicit request: **Sales by Category**, **Promotion Usage**, **Discount &
Complimentary**, **Table Turnover**, **Steward Sales**, **Tour Commission**, **Budget vs
Sales**. These slice an in-memory grouped list (`all.Skip(...).Take(...)`) rather than a
SQL query, since the grouping itself already requires materializing the full period's
rows; `steward-sales`/`tour-commission` slice at the endpoint layer since their
`OrderService` methods are also used nowhere else and didn't need a signature change.

All thirteen now have real `Skip`/`Take` pagination + the shared `<Pagination>` footer,
with totals aggregated over the whole filtered period, not just the visible page. On the
frontend, every one of these was previously fetched inline as part of one giant
`Promise.all` shared across the whole Reports page (refetched on every "Run" click, page
1 only, unbounded row count) — each was extracted into its own self-contained component
(`SalesRegisterReport`, `DailySummaryDetailReport`, `StockBalanceReport`, `ShiftsReport`,
`CategorySalesReportView`, `PromotionsUsageReport`, `DiscountsReportView`,
`TableTurnoverReportView`, `StewardSalesReportView`, `TourCommissionReportView`),
matching the pattern already used by Wastage/Low Stock/Bin Card: each owns its own page
state, fetch, and CSV/PDF export (which loops all pages to export the full period, not
just the visible page). `FoodCostingReport` and `BudgetVsSalesReport` were already
separate manually-triggered ("Run" button) components — pagination was added in place
rather than a full extraction.

On the frontend, Sales Register, Daily Summary Detail, Stock Balance, and Shift
Settlement were previously fetched inline as part of one giant `Promise.all` shared
across the whole Reports page (every report refetched on every "Run" click, page 1
only) — extracted each into a self-contained component (`SalesRegisterReport`,
`DailySummaryDetailReport`, `StockBalanceReport`, `ShiftsReport`), matching the existing
pattern already used by Wastage/Low Stock/Bin Card: each owns its own page state, fetch,
and CSV/PDF export (which loops all pages to export the full period, not just the
visible page).

Every report in this screen has both **CSV** and **PDF** export buttons (client-side,
via `downloadCsv`/`downloadPdf` in `reports/page.tsx`, using `jspdf` + `jspdf-autotable`).
The PDF template is a shared letterhead design: green accent bar, tenant name +
"RIT HMS" masthead, report title/period, KPI stat-card grids for summary figures,
branded green table headers with zebra striping and auto right-aligned numeric
columns, bold highlighted total rows, and a page-numbered footer. Only ASCII
punctuation is used in PDF-rendered text (no en/em dashes, arrows, or middle dots) —
jsPDF's built-in Helvetica font only supports WinAnsi/Latin-1, and non-ASCII
characters silently corrupt the whole text run's spacing.

---

## Master Data
- ❌ Product / menu master list (active/inactive, missing price or recipe)
- ❌ Customer master export
- ❌ Supplier master list
- ❌ Price list (products × price levels)

## Sales
- ✅ Overview (KPIs, by source, by day, top items)
- ✅ Daily Summary (date × outlet)
- ✅ Daily Summary Detail (per-receipt drill-down)
- ✅ Register (per-bill list + tenders)
- ✅ Item Sales
- ✅ By Outlet (HQ rollup)
- ✅ Promotions (usage + discount given)
- ✅ **Steward Sales** — bills/covers/gross/tips per waiter.
- ✅ **Tour Commission** — bills/gross/commission per tour operator.
- ✅ **Sales by Category** — units/revenue/tax rolled up by menu category (not the
  operational "Department" concept — those are two distinct entities in this system)
- ✅ **Void / Cancellation** — voided bills with reason + "voided by" (audit-log lookup), server-side paginated
- ✅ **Discount & Complimentary (lite)** — manual discount totals per steward. No reason/approver/comp
  tracking exists in the system yet (only `Order.DiscountAmount`, no `DiscountReason` or comp flag) —
  this reports amounts only. Revisit if/when that tracking gets added to the POS discount flow.
- ✅ **Table Turnover** — bills, covers and avg occupancy duration per dine-in table
- ❌ Delivery & aggregator performance (Uber Eats / PickMe split + commission)
- ❌ Top / bottom customers by spend

## Tax
- ✅ Return (output/input, net payable, charge breakdown)

## Inventory
- ✅ Stock Balance
- ✅ Bin Card
- ✅ Wastage — server-side paginated (bottom pagination control), reason filter
- ✅ **Low Stock / Reorder** — as-at-now snapshot, effective reorder/par per product×outlet (same rule as Replenishment), server-side paginated
- ✅ **Slow-Moving Stock (aging)** — on-hand value with no sale in N+ days (or never sold),
  valued at cost, server-side paginated. Aging signal is "days since last SALE" (settled
  orders only) — not a full any-movement ledger (BinCard is per-product across 7 source
  tables with no shared schema; too heavy to run for every product at once).
- ✅ **Purchases by Supplier** — GRN rollup per supplier (approved only), server-side paginated
- ❌ Stock count variance (system vs. physical)

## Operations
- ✅ Shift Settlement

## Costing
- ✅ Food Costing
- ✅ Budget vs Sales
- ❌ Menu engineering (profitability × popularity)

## Finance (section not built yet)
- ❌ AR aging (credit customers)
- ❌ AP aging (supplier payables)
- ❌ P&L summary

## Loyalty (section not built yet)
- ❌ Points issued / redeemed over time
- ❌ Enrollment growth / tier distribution

---

## Build order (agreed)
1. ✅ Steward Sales
2. ✅ Tour Commission
3. ✅ Wastage report (Inventory) — server-side paginated list, first of its kind in Reports
4. ✅ Purchases by Supplier (Inventory) — server-side paginated, matches the Wastage-report template
5. ✅ Void / Cancellation (Sales) — server-side paginated, "voided by" resolved from the audit log
6. ✅ Low Stock / Reorder (Inventory) — as-at-now snapshot, reuses Replenishment's effective-level rule
7. ✅ Discount & Complimentary — lite version (Sales), no reason/approver/comp tracking exists yet
8. ✅ Table Turnover (Sales) — bills/covers/avg duration per dine-in table
9. ✅ Sales by Category (Sales) — units/revenue/tax by menu category
10. ✅ Slow-Moving Stock (Inventory) — days-since-last-sale aging, server-side paginated
11. Next up — pick one:
    - Delivery & aggregator performance (Sales)
    - Top / bottom customers by spend (Sales)
    - Menu engineering (Costing)

## Pagination convention (added after Wastage report)
New report **lists** (not KPI/summary views) should be server-side paginated: a
`GET .../reports/...` endpoint taking `pageNumber`/`pageSize` and returning
`{ data, pagination: { totalCount, pageNumber, pageSize, totalPages } }`, rendered
with the shared `<Pagination>` component (`@/components/ui/Pagination`) at the
bottom of the table, same as the master-data list pages (Suppliers, Customers, …).
Existing non-paginated report tables (Register, Item Sales, Stock Balance, etc.)
have NOT been retrofitted yet — only new reports follow this convention so far.
