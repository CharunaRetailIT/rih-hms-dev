# SMART HMS — Architecture & Audit

## 1. Executive summary

SMART HMS is a Sri Lankan hospitality (restaurant + hotel + inventory + loyalty)
management platform deployed across multiple chains. The product works and earns
revenue, but architecturally it carries the typical debt of an organically-grown
.NET Framework 4 application from the 2016-era.

| Dimension | Score | Verdict |
|---|---|---|
| Architecture clarity | 4 / 10 | Layered names (Domain/Data/BLL) but logic leaks everywhere — controllers do DB work, business rules live in SQL triggers, two web apps share one DB directly with no API boundary |
| Domain model health | 4 / 10 | 105 entities, ~50% inherit a `BaseEntity` audit base; God entities (Customer = 68 props), string-typed enums, public setters everywhere |
| Database hygiene | 3 / 10 | 207 tables, only ~10 foreign keys declared (4% coverage), duplicate/legacy tables, 783 EF migrations with developer-named files |
| API readiness | 2 / 10 | One stub `ApiController`. No webhook framework, no `HttpClient` usage, no queueing, no outbox |
| Auth / multi-tenancy | 3 / 10 | Dual identity (ASP.NET Identity + custom `SysUserMaster`); tenant isolation is `Session["loggedusercompanyId"]` read directly in every controller — not enforced by a filter |
| Security posture | 2 / 10 | SA credentials in clear text in `Web.config`; TripleDES license check with zero IV + hardcoded key; `debug="true"`; no HTTPS enforcement; multiple EOL CVEs |
| Frontend | 3 / 10 | Gentelella admin theme (2016), Bootstrap 3, jQuery 1.10, 110 MB of Bower vendors |
| Tests / CI | 1 / 10 | No test project found, no unit tests. Pipelines only packages a `.zip` |
| Documentation | 0 / 10 | `README.md` in main project was the Gentelella template's stock README |
| **Overall** | **24 / 100** | **Functional and clearly making money — but architecturally legacy and risky to extend** |

## 2. Application layers

```
Browser
  └─► IIS / Owin pipeline
        └─► SessionTimeoutAttribute        (checks Session["loggeduser"] exists — that's all)
              └─► MVC Controller action
                    ├─► Reads Session["loggedusercompanyId"], Session["loggeduserlocId"]   ← tenancy lives here
                    ├─► Talks to BLL service     (sometimes — many controllers hit EF directly)
                    │       └─► UnitOfWork → GenericRepository<T> → EF6 → SQL Server
                    └─► Returns Razor view (Bootstrap 3 / Gentelella)
                          └─► Inline jQuery does $.ajax back to other actions for partial refresh
```

## 3. Auth flow (today)

OWIN cookie auth with ASP.NET Identity 2.2.1 lives in `App_Start/Startup.Auth.cs`.
After Identity validates, the app loads the parallel `SysUserMaster` row and
stuffs `loggedusercompanyId`, `loggeduserlocId`, `loggeduser` into `Session` —
**every multi-tenant security check after that is just developers remembering
to read those session keys**. There is no global authorization filter that
enforces tenant scope; one wrong `where` clause leaks data across companies.

## 4. The POS sale flow

```
                       HMSOrderTaker app                 HospitalityManagement app
                       (waiter on tablet)                  (cashier on desktop)
┌──────────────────┐    ┌──────────────────┐         ┌──────────────────────┐
│ SaveTableHead    │───►│ STOS_TabOrder    │         │                      │
│  (open table)    │    │   Header (Pending)│         │                      │
└──────────────────┘    └────────┬─────────┘         │                      │
                                 │                    │                      │
┌──────────────────┐    ┌────────▼─────────┐         │                      │
│ SaveTabOrder     │───►│ STOS_TabOrder    │         │                      │
│ ItemsToDb        │    │   Detail (KOT/BOT)│         │                      │
└──────────────────┘    └────────┬─────────┘         │                      │
                                 │  cashier recalls   │                      │
                                 │  the order ───────►│                      │
                                 │                    ▼                      │
                                 │           ┌─────────────────┐             │
                                 │           │ SuspendHeds /   │             │
                                 │           │ SuspendDets     │             │
                                 │           └────────┬────────┘             │
                                 │                    │ on settle            │
                                 │                    ▼                      │
                                 │           ┌─────────────────┐             │
                                 │           │ PaymentDets     │── TRIGGER ──┤ → genProductionNotes SP (kitchen)
                                 │           └────────┬────────┘             │
                                 │                    │                      │
                                 │                    ▼                      │
                                 │           ┌─────────────────┐             │
                                 │           │ TransactionDets │── TRIGGER ──┤ → Trigger_UpdateStockInHeadOffice
                                 │           └────────┬────────┘             │    decrements ProductStockMasters.Stock
                                 │                    │                      │    when DocumentID IN (1,3) AND TransStatus=1
                                 │                    ▼                      │
                                 │           ┌─────────────────┐             │
                                 │           │ InvSales        │             │
                                 │           └─────────────────┘             │
```

**Important: stock decrement is trigger-driven.** Any new code path that
inserts into `TransactionDets` decrements stock automatically — and any
path that bypasses it leaves stock incorrect. This is the single most
important fact about the system for anyone planning extensions.

## 5. Top architectural risks

1. **Trigger-driven business logic.** Stock decrement, kitchen ticket generation,
   GL transfer — all in T-SQL triggers and stored procs. Difficult to test,
   reason about, or refactor.
2. **Two apps, one DB, no API.** `HMSOrderTaker` writes to the same SQL Server
   as `HospitalityManagement`. No abstraction. Schema changes break both.
3. **Session-based tenancy.** A single missed `where` clause leaks data
   across companies. Discovered: no global tenant filter.
4. **~10 FKs across 207 tables.** Referential integrity is application-level.
5. **No tests.** Any rewrite or substantial refactor has no safety net.
6. **Plaintext SA passwords in source** (since fixed in this fork — must be
   rotated on the live servers regardless).
7. **TripleDES + zero-IV + hardcoded key** licence check. Trivially bypassable.
8. **EOL packages.** Newtonsoft 6 (2014), jQuery 1.10 (2013), Bootstrap 3,
   ReportViewerForMvc4. Multiple CVEs.

## 6. Where this fork is going

See [`sprint-plan.md`](./sprint-plan.md). The short version:

- Sprint 1: Foundation work that makes everything else safer (secrets, FKs,
  tenant filter, CI green).
- Sprint 2–6: Build Uber Eats + PickMe integrations on the existing stack.
  Revenue first.
- Sprint 7+: Strangler-fig modernisation — new Next.js front-end and
  ASP.NET Core 8 API in front of the same SQL DB, retiring MVC views one
  workflow at a time.
