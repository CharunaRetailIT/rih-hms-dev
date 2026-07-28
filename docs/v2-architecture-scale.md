# v2 Architecture at Scale — HQ + Multi-Outlet, Supply Chain, Tax/VAT

> Answers the scale & completeness questions: 1 HQ + 14 outlets, the full
> GRN/transfer/returns/wastage/production→packing→delivery flow, category
> hierarchy, and configurable tax + Sri Lanka VAT compliance.

## 1. Multi-outlet model — 1 HQ + 14 locations

**One restaurant group = one tenant = one Postgres database.** All 15 locations
(1 HQ + 14 outlets) live as rows in that one tenant DB. DB-per-tenant isolates
*groups* from each other; within a group, locations are rows. This scales to
hundreds of outlets per group without schema change.

```
Tenant: "Spice Group" → database hms_tenant_spice
  ├── Location: HQ / Head Office        (location_type = head_office)
  ├── Location: Central Kitchen         (location_type = central_kitchen)
  ├── Location: Outlet 1 — Colombo      (location_type = outlet)
  ├── Location: Outlet 2 — Kandy        (location_type = outlet)
  └── … 12 more outlets
```

### Location types & capabilities
| Type | can_sell | can_produce | can_stock | Purpose |
|---|---|---|---|---|
| `head_office` | ✗ | ✗ | ✗ | Consolidated reporting, config, no operations |
| `central_kitchen` | ✗ | ✓ | ✓ | Produces finished goods, distributes to outlets |
| `warehouse` | ✗ | ✗ | ✓ | Holds raw stock, distributes |
| `outlet` | ✓ | optional | ✓ | Sells via POS, receives transfers |

### Roles at scale
- **Org-wide roles** (Owner, Group Manager, Group Accountant) — see all locations.
- **Location-scoped roles** (Outlet Manager, Cashier, Kitchen) — scoped to one or more locations via a `user_locations` join.
- HQ dashboards roll up all 14 outlets; an outlet manager sees only theirs.

### Consolidated reporting
HQ reports aggregate across `location_id`. Because every transactional row
carries `location_id`, group-level rollups are `GROUP BY location_id` queries —
no cross-database joins needed (all in one tenant DB).

## 2. Supply chain — production → packing → delivery

The full commissary/central-kitchen flow. This is the biggest module and the
core of why a 14-outlet group needs an ERP, not just a POS.

```
                        ┌─────────────── SUPPLIERS ───────────────┐
                        │                                          │
                   Purchase Order (PO)                             │
                        │                                          │
                        ▼                                          │
              Goods Received Note (GRN)  ◄── receive raw materials │
                        │                                          │
                        ▼                                          │
              ┌──────── CENTRAL KITCHEN / WAREHOUSE ───────────┐   │
              │  raw stock on hand                              │   │
              │     │                                           │   │
              │     ▼                                           │   │
              │  Production Order (recipe/BOM: raw → finished)  │   │
              │     │   consumes raw, yields finished goods     │   │
              │     ▼                                           │   │
              │  Packing (finished goods → packed units)        │   │
              │     │                                           │   │
              └─────┼───────────────────────────────────────────┘   │
                    ▼                                                 │
            Transfer / Delivery Note  (central → outlet)              │
                    │                                                 │
                    ▼                                                 │
            Outlet GRN (receive against transfer)                     │
                    │                                                 │
                    ▼                                                 │
            Outlet stock on hand                                      │
                    │                                                 │
                    ▼                                                 │
            POS sale → stock decrement (R1, already built)            │
                                                                      │
   Wastage / Stock Adjustment  ◄── recorded at ANY stage ────────────┘
   Purchase Return  ──────────► back to supplier
   Transfer Return  ──────────► outlet back to central
```

### Documents (each a header + detail pair, with status workflow)
| Document | Direction | Effect on stock |
|---|---|---|
| **Purchase Order (PO)** | HQ/outlet → supplier | none (commitment only) |
| **Goods Received Note (GRN)** | supplier → location | + raw stock, sets avg cost |
| **Purchase Return** | location → supplier | − stock |
| **Production Order** | within central kitchen | − raw (per recipe), + finished |
| **Packing Note** | within central kitchen | finished → packed SKU |
| **Transfer / Delivery Note** | central → outlet | − sender, in-transit |
| **Transfer GRN (receipt)** | confirms delivery | + receiver stock |
| **Transfer Return** | outlet → central | reverse transfer |
| **Wastage Note** | any location | − stock, reason-coded |
| **Stock Adjustment** | any location | ± stock, reason-coded, approval |
| **Stock Count (cycle/full)** | any location | sets stock to counted qty |

### Recipes / Bill of Materials (BOM)
- A **finished product** has a recipe: a list of `(raw_product, quantity, uom)`.
- A Production Order for N finished units consumes N × recipe raw quantities.
- Yields and wastage % per recipe supported.
- Multi-level: a sub-assembly (e.g. "kottu base") can be an ingredient in a dish.

### Costing
- GRN sets/updates **weighted average cost** per raw product per location.
- Production rolls raw cost into finished-goods cost.
- POS gross-profit reports use the finished-goods cost vs. sell price.

## 3. Category hierarchy

`categories.parent_id` supports arbitrary depth. Convention for hospitality:

```
Department (optional)
  └── Category            e.g. Beverages
        └── Sub-category   e.g. Fizzy / Alcohol / Hot
              └── Product  e.g. Coca-Cola / Pepsi / Lion Lager / Ceylon Tea
```

Example requested:
```
Beverages
 ├── Fizzy
 │     ├── Coca-Cola
 │     └── Pepsi
 ├── Alcohol
 │     └── Lion Lager
 └── Hot
       └── Ceylon Tea
```
Reporting rolls product sales up the tree (sales by sub-category, by category,
by department). KOT routing and POS button colour can be set at any level and
inherited by children.

## 4. Configurable tax + Sri Lanka VAT compliance

**Nothing hardcoded.** All charges/taxes are rows in `tax_charges`, editable on
the ERP dashboard, applied in a configurable **sequence** with optional
**compounding** — which is exactly how Sri Lankan hospitality bills stack:

```
Subtotal (taxable items)                         10,000.00
  + Service Charge 10%   (on subtotal)            1,000.00
  + SSCL 2.5%            (on subtotal + SC)          275.00   ← compounds
  + VAT 18%             (on subtotal + SC + SSCL)  2,029.50   ← compounds
  ─────────────────────────────────────────────────────────
  TOTAL                                           13,304.50
```

Each `tax_charge` row: `code, name, rate_percent, charge_type
(service_charge|levy|vat), sequence, compound_on_previous, applies_to_takeaway,
is_active`. Add/disable/reorder any charge from the dashboard. Add a new levy
(e.g. a future tourism levy) without code changes.

### SL VAT tax-invoice compliance
On settle, an order that is a **tax invoice** gets:
- A **sequential, gap-free invoice number** (audit requirement) from a dedicated
  per-tenant invoice series.
- The supplier's **VAT registration number** (from `org_settings`).
- Optional **customer VAT number** (B2B invoices).
- A printed **charge breakdown** (each charge line: description, rate, amount) —
  stored in `order_charges` so the invoice is reproducible.

### VAT return / filing
- A **VAT summary report** aggregates output tax (VAT collected) over a period,
  grouped by rate — the data needed for the periodic VAT return.
- Exportable to CSV/spreadsheet for submission.
- `org_settings.vat_registration_number`, `vat_enabled`, filing frequency stored
  per tenant.

## Build sequence
1. ✅ POS core + KOT + settlement (done)
2. ▶ Configurable tax engine + SL VAT compliance + org settings (this pass)
3. ▶ Multi-level categories (this pass)
4. ▶ Location types / HQ readiness (this pass)
5. Supply chain — suppliers, PO, GRN (next)
6. Supply chain — transfers, returns, wastage, stock adjustment (next)
7. Supply chain — production (recipe/BOM), packing (next)
8. Consolidated HQ reporting across outlets
9. Aggregators (Uber Eats + PickMe) — schema-ready
