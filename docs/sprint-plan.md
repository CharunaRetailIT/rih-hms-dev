# Sprint Plan — RIT HMS (cloud rewrite)

> **Status: 2026-06-02.** The original 10-week *strangler-fig* plan (modernise the
> legacy .NET Framework + SQL Server app in place) was **superseded** by a clean
> **cloud-native rewrite**: ASP.NET Core 8 Minimal API + EF Core + **PostgreSQL
> (DB-per-tenant + RLS)** + Next.js 15, as a **multi-tenant SaaS**. The original
> plan is preserved in git history. This document tracks the rewrite.

## Where we are

A working multi-tenant HMS: POS, kitchen, full supply chain, production, delivery
aggregators (mock), reporting, and role-based users — **build-green and tested**
(154 integration + 36 unit + ~22 E2E, GitHub Actions CI). **Sprints A & B are
done** (full procurement + product/menu parity). **Not yet live as a paid SaaS**:
tenant auto-provisioning, real aggregator sandboxes, a print agent, and
production hardening/deploy remain (Sprint C+).

Tests: `154 integration · 36 unit · ~22 E2E`. Repo: `github.com/mubs62/rit-hms`.

---

## Delivered ✅

**Platform**
- Cloud rewrite foundation: ASP.NET Core 8 + EF Core + Npgsql + PostgreSQL, snake_case.
- Multi-tenancy: control DB + per-tenant DBs, tenant resolved from JWT, **RLS** defence-in-depth.
- **Auth enforced**: magic-link → JWT, validated (`AddJwtBearer`), every endpoint requires auth; **RBAC** (Owner/Manager/Cashier/Kitchen/Accountant) enforced at the API + gated in the nav; dev-only `X-Tenant-Id` scheme.
- AES-GCM secret encryption for aggregator keys; configurable document prefixes, bill branding, tax certs.

**Front of house**
- POS terminal: keypad, qty ±, discount, multi-tender + change, open/custom-price items, real settlement.
- KOT display; configurable **compound tax/VAT engine** (Service Charge → SSCL → VAT).
- **Shifts**: open with float → cash-up **Z-report** (by-tender, expected vs counted, variance).
- **Delivery (Uber Eats / PickMe — mock)**: incoming queue, accept + prep time, ready → pickup, item 86, per-merchant/per-location credentials (encrypted), outbox, dev simulator.

**Supply chain**
- Suppliers; **PO**; **GRN** receive with weighted-average cost, supplier **input-VAT** capture, **unit conversion on receive**, free items, line discount, supplier invoice #, **void/reverse**.
- Transfers / returns / wastage / stock adjustments (movement-logged).
- **Production**: recipes/BOM with **UoM conversion** (500 g ← kg) + full legacy parity (custom/ad-hoc, draft→post→void, multi-product document, multiple recipes per output unit, inter-location).

**Master data & back office**
- Products with **create/edit/delete** + Menu screen; categories (hierarchy); units (dimension + factor); taxes.
- Reports: **VAT return** (output − input), **sales summary**, **consolidated HQ outlet rollup**.
- User/team management screen.

**UI screens**: dashboard, POS, KOT, menu, inventory, suppliers, purchasing, production, transfers, reports, delivery, settings, team, login/signup.

---

## Forward plan

### ✅ Sprint A — Procurement & product parity (DONE)
- PO line **purchasing unit** + conversion to stock unit. *(#49 ✅)*
- PO **discounts + header charges** → landed cost. *(#50 ✅)*
- PO lifecycle: **send / cancel / edit** + GRN void/reverse. *(#52 ✅)*
- Procurement **approval** (maker-checker), **supplier returns / debit note (PRN)** + input-VAT reversal, currency + payment terms on PO. *(#56 ✅)*

### ✅ Sprint B — Customer-facing depth (DONE)
- **Modifiers / add-ons** + POS wiring. *(#54 ✅)*
- **Kitchen / printer routing**, **serving-size variants**, **per-location pricing / price levels**, **per-product tax class**. *(#55a–d ✅)*
- Product **CSV bulk import** — *still a P2 stub* *(part of #53)*.

### QA / hardening (this session, not originally planned)
- Systematic runtime audit: screen-load smoke, HTTP flow smoke, POS money-path E2E.
- Fixed: Reports 500 (timestamptz), POS open-order resume + tabs + dashboard deep-link, KOT recall, Settings tax-charge UI, KOT hydration. *(#58–63)*
- **In progress:** per-screen interaction sweep (purchasing/production/transfers/delivery buttons).

### Sprint C — Make it a live SaaS (NEXT, P0)
- **Tenant auto-provisioning**: signup → background job creates + migrates + seeds the tenant DB, activates subscription. *(#17/#20)*
- **Auth hardening**: no magic-link in prod responses, admin-gate the tenant list, JWT signing key + master key from a secret store, refresh/logout.
- **Deploy**: hosting, automated migration runner, backups, error tracking / health dashboards.
- **Multi-outlet switcher** + location types (rollup reporting already done). *(#26)*

### Sprint D — Printing & mobile
- **Print agent** for KOT + bills (thermal printers). *(#16)*
- **GRN mobile PWA** (parked): installable Android web app — receive vs PO + ad-hoc, barcode scan, recent GRNs. *(#57)*
- Manager mobile dashboard (read-only).

### Sprint E — Pilot & real aggregators
- **Daily reconciliation** report (aggregator settlement vs sales). *(part of #13)*
- Swap the aggregator mock for **real Uber Eats + PickMe sandboxes** (once approved). *(#9)*
- **Pilot** one outlet for two live service shifts; fix what reality breaks. *(#13)*

---

## Only RIT can do (blocked on you)
- **URGENT — rotate the leaked SA passwords** on the live servers (independent of this rewrite). *(#14)*
- Apply for **Uber Eats + PickMe partner sandboxes** — the integration is built to swap in. *(#9)*
- Internal sign-off on the repo destination. *(#7)*

## Honest expectations
- **Procurement/product parity (Sprint A–B):** ~2–3 weeks.
- **Live-SaaS readiness (Sprint C):** ~3–4 weeks (auto-provisioning + hardening + deploy is the gap between "great demo" and "paying tenants").
- **Print + mobile (Sprint D):** ~2 weeks.
- **Pilot (Sprint E):** 1–2 weeks once an outlet + sandboxes are ready.
