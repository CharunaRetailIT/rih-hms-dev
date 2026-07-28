# RIT HMS v2 — Multi-Tenant Architecture

This document defines how v2 handles tenancy end-to-end: signup, provisioning, isolation, authentication, billing, customisation, and migrations.

Related: [v2 Entities](./v2-entities.md) · [v2 API surface](./v2-api-surface.md) · [v2 Business rules](./v2-business-rules.md) · [v2 v1 scope](./v2-v1-scope.md).

---

## 1. Core model

**One paying customer = one tenant = one Postgres database.**

- **Why DB-per-tenant?** Hard isolation (regulatory, no leaky queries), per-tenant backup/restore, independent scaling, easy `DROP DATABASE` on cancellation.
- **Why not schema-per-tenant?** Postgres struggles past ~500 schemas in one DB. DB-per-tenant scales to thousands across an Azure Flexible Server pool.
- **Why not row-level?** Legacy used row-level via `GroupOfCompanyID + CompanyID + LocationId`. 1271 occurrences of session-based scope leaks across the codebase. Hard to audit.

In v2, **row-level filtering (RLS) is defence-in-depth, not the primary isolation**. The primary isolation is the database boundary.

---

## 2. Tenant lifecycle

```
       signup
         │
         ▼
   ┌──────────────┐
   │  pending     │ ← Subscription row exists, payment_provider intent only
   └──────┬───────┘
          │ payment confirmed (webhook)
          ▼
   ┌──────────────┐
   │ provisioning │ ← Hangfire job creating DB + Entra + seed
   └──────┬───────┘
          │ provisioning complete
          ▼
   ┌──────────────┐      ┌──────────────┐
   │   trialing   │─────▶│    active    │
   └──────┬───────┘      └──────┬───────┘
          │ trial expired              │ payment failed
          ▼                             ▼
   ┌──────────────┐              ┌──────────────┐
   │ trial_ended  │              │  past_due    │
   └──────┬───────┘              └──────┬───────┘
          │ user cancels OR             │ payment recovered OR
          │ admin cancels               │ admin cancels
          ▼                             ▼
   ┌──────────────┐              ┌──────────────┐
   │  cancelled   │◀─────────────│  cancelled   │
   └──────┬───────┘              └──────┬───────┘
          │ retention period (90 days) expires
          ▼
   ┌──────────────┐
   │   deleted    │ ← DB dropped, Entra users deleted, only invoices retained
   └──────────────┘
```

| State | Login allowed? | Data writable? | DB exists? |
|---|---|---|---|
| `pending` | no | no | no |
| `provisioning` | no | no | being created |
| `trialing` | yes | yes | yes |
| `active` | yes | yes | yes |
| `trial_ended` | read-only | no | yes |
| `past_due` | read-only after 7 days | no after 7 days | yes |
| `cancelled` | read-only for 90 days | no | yes |
| `deleted` | no | no | no |

---

## 3. Control plane vs tenant databases

| | Control plane | Tenant DB |
|---|---|---|
| Database name | `rit_control` | `rit_tenant_<tenant_id_short>` |
| Schemas | `public` | `public` |
| Tables | `tenants`, `subscriptions`, `subscription_plans`, `provisioning_jobs`, `webhook_events`, `feature_flags`, `tenant_config`, `audit_log_control` | Everything else: orders, products, customers, etc. |
| Auth | Service principal (full access) | Per-request via connection string with `app.tenant_id` GUC set |
| Backups | Hourly | Daily + monthly archive |
| Scale | One Postgres Flexible Server | Multiple Flexible Servers in elastic pool |
| Migrations | Standard EF migrations | Iterated across all tenant DBs by Hangfire job |
| Connection pool | Singleton | `IDbContextFactory<TenantDbContext>` per request |

The control plane is the only thing the client never sees directly — all customer-facing API calls route to a tenant DB after auth.

---

## 4. Auto-provisioning pipeline

When a signup completes (Stripe/PayHere webhook with `payment_intent.succeeded` or equivalent):

```
   webhook
      │
      ▼
[WebhookHandler]      writes Subscription.status = 'provisioning'
      │
      ▼
[Hangfire enqueue]    job: TenantProvisioningJob(tenant_id)
      │
      ▼
┌───────────────────────────────────────────────────┐
│ TenantProvisioningJob steps (each idempotent):    │
│                                                   │
│ 1. CreateDatabaseStep                             │
│    - CREATE DATABASE rit_tenant_<short_id>        │
│    - Skip if exists                               │
│                                                   │
│ 2. RunMigrationsStep                              │
│    - Apply all EF Core migrations                 │
│    - Check __EFMigrationsHistory                  │
│                                                   │
│ 3. SeedReferenceDataStep                          │
│    - Roles, default permissions                   │
│    - Default catering modes, payment methods      │
│    - Default tax (LK GST 18%)                     │
│    - Default outlet (1 row)                       │
│                                                   │
│ 4. CreateEntraTenantStep                          │
│    - Provision Entra External ID resource         │
│    - Create app registration                      │
│    - Configure OIDC redirect URIs                 │
│                                                   │
│ 5. InviteOwnerStep                                │
│    - Create Entra user for signup email           │
│    - Add to OWNER role group                      │
│    - Send welcome email with magic-link           │
│                                                   │
│ 6. RegisterTenantInDirectoryStep                  │
│    - Insert into Azure Front Door routing         │
│    - DNS: tenant_short_id.app.rit-hms.com         │
│                                                   │
│ 7. MarkProvisioningCompleteStep                   │
│    - Subscription.status = 'trialing'             │
│    - trial_end_at = now + 14 days                 │
│    - Emit TenantProvisioned event                 │
└───────────────────────────────────────────────────┘
      │
      ▼
Owner receives email → clicks magic-link → first login
```

**Failure handling.** Each step is idempotent. On exception, Hangfire retries with exponential backoff. After 3 attempts, the job is marked failed in `provisioning_jobs`, an alert fires (PagerDuty), and Subscription remains `provisioning` (visible in admin UI). Manual replay button retries the failed step onward.

**Expected duration.** 60-180 seconds end-to-end. Owner sees a spinner during signup, polls `GET /api/v1/provisioning/{job_id}` until complete.

---

## 5. Connection routing

On every authenticated request:

```csharp
public class TenantContextMiddleware
{
    public async Task InvokeAsync(HttpContext ctx, IDbContextFactory<TenantDbContext> factory)
    {
        var tenantId = ctx.User.FindFirst("tenant_id")?.Value
            ?? throw new UnauthorizedAccessException("Missing tenant_id claim");

        var subscription = await _controlPlane.GetSubscriptionAsync(tenantId);
        if (!subscription.AllowsAccess()) throw new SubscriptionInactiveException();

        var connStr = _connectionResolver.Resolve(tenantId); // looks up in rit_control.tenants
        var tenantContext = new TenantContext(tenantId, connStr);
        ctx.Items["TenantContext"] = tenantContext;

        // Set Postgres GUC for RLS
        await using var conn = await factory.CreateConnectionAsync(connStr);
        await conn.ExecuteAsync($"SET LOCAL app.tenant_id = '{tenantId}'");

        await _next(ctx);
    }
}
```

Key points:
- `IDbContextFactory<TenantDbContext>` is registered scoped; resolves connection string from `TenantContext`.
- Connection strings are looked up from `rit_control.tenants.database_name` once and cached for 60s.
- Postgres connection pool is per-`(server, database)` — Npgsql handles this natively.
- The `app.tenant_id` GUC is set on every checkout. RLS policies use `current_setting('app.tenant_id')::uuid` to enforce isolation.
- Background jobs (Hangfire) construct `TenantContext` from the job payload and use the same factory.

---

## 6. Migration strategy across N tenant DBs

EF Core migrations applied via a dedicated Hangfire job:

```
[TenantMigrationOrchestrator]   ← triggered manually or by CI/CD on release
    │
    ▼
For each tenant in rit_control.tenants WHERE status NOT IN (deleted, cancelled-old):
    │
    ▼
Enqueue [TenantMigrationJob(tenant_id, target_version)]
    │
    ▼
TenantMigrationJob:
    1. Set Subscription.maintenance_mode = true   (returns 503 on tenant API)
    2. Backup database to blob storage
    3. Apply migrations sequentially
    4. Run post-migration data fixups (if any)
    5. Validate schema (compare against expected)
    6. Set maintenance_mode = false
    7. Report status to dashboard
```

**Progress dashboard.** A `/admin/migrations/{release_id}` page shows: total tenants, completed, in-progress, failed. Click a failed tenant to see error log and replay button.

**Throttling.** Default concurrency = 5 tenants in parallel. Tunable per release. Migrations on small tenant DBs are typically < 10s; on large ones (1M+ orders), can take minutes.

**Forward-only.** No automatic rollback. If a migration breaks production, fix forward with a new migration. Backups are for catastrophic-failure recovery only.

**Breaking changes.** Two-step deploys: (1) deploy migration that adds new column, app reads both old + new; (2) backfill data; (3) deploy migration that drops old column. Plan ahead.

---

## 7. Subscription model

| Plan | Outlets | Users | Aggregator integrations | Reports | Loyalty | Price (LKR/mo) |
|---|---|---|---|---|---|---|
| **Starter** | 1 | 5 | – | 5 core | – | 14,900 |
| **Pro** | 3 | 25 | Uber + PickMe | 15 | yes | 39,900 |
| **Enterprise** | unlimited | unlimited | all | all + API | yes | quote |

(Prices indicative; sales decision.)

**State transitions** are driven by **Stripe / PayHere webhooks**:

| Webhook event | Subscription state change |
|---|---|
| `payment_intent.succeeded` (first) | `pending → provisioning → trialing` |
| `invoice.payment_succeeded` | `trialing|past_due → active` |
| `invoice.payment_failed` | `active → past_due` |
| 7 days in `past_due` | `past_due → cancelled` (read-only) |
| 90 days in `cancelled` | `cancelled → deleted` (DB dropped, retention period over) |
| user cancels via portal | `* → cancelled` (effective end of current period) |

All webhook events are stored in `webhook_events` table with HMAC-validated payload + idempotency key, processed via outbox.

**PayHere** is the local Sri Lankan payment processor. Used as the primary for LK tenants. Stripe is for international tenants.

---

## 8. Role & permission model

v1 ships with 5 roles. v2 adds 2 more.

| Role code | v1/v2 | Description | Default permissions |
|---|---|---|---|
| `OWNER` | v1 | Tenant owner; full control. | all |
| `MANAGER` | v1 | Outlet manager; most operations except billing + user deletion. | all minus billing, delete-user |
| `CASHIER` | v1 | POS terminal user. | open/settle/recall orders, print KOT |
| `KITCHEN` | v1 | Kitchen display / Print Agent operator. | view tickets, mark printed/voided |
| `ACCOUNTANT` | v1 | Read sales + GL exports. | view reports, export CSV, GL transfer |
| `WAITER` | v2 | Tablet waiter app. | open table orders, add lines, NOT settle |
| `VIEWER` | v2 | Read-only auditor. | view all, edit nothing |

**Permission grid.** Each role has rows in `permissions` table: `(role_id, feature_key, access_level, limit_value)`. Example:

| role | feature_key | access | limit |
|---|---|---|---|
| CASHIER | `apply_discount` | allow | 0.10 (10%) |
| MANAGER | `apply_discount` | allow | 0.50 |
| MANAGER | `void_settled_order` | allow | null |
| CASHIER | `void_settled_order` | deny | null |
| MANAGER | `approve_po` | allow | 100000 (LKR limit) |

**Permission resolution at runtime.** JWT carries `roles[]` claim. `IPermissionAuthorizer.Check(user, "void_settled_order", amount)` walks user's roles, returns true if any allows + amount within limit.

**No per-user permissions in v1.** Permissions are role-only. (Legacy supported per-user; we explicitly drop this — it caused permission drift.)

---

## 9. Authentication

**Entra External ID** is the source of truth for user identity. Each tenant has its own Entra External ID tenant resource (provisioned in step 4 of auto-provisioning).

- **No passwords stored in our DB.** Ever.
- **No machine binding.** (Legacy bound license to MAC address — gone.)
- **JWT format:** Standard JWT issued by Entra, validated by ASP.NET Core JWT middleware. Claims: `sub` (user_id), `email`, `tenant_id`, `roles[]`, `outlet_ids[]`.
- **Magic link fallback.** If Entra is unavailable, ASP.NET Identity-backed magic-link login generates a JWT directly. Same claim structure. Toggle via `FeatureFlag.fallback_login_enabled`.
- **MFA.** Available via Entra (SMS, authenticator app). Mandatory for OWNER role; optional otherwise.
- **Session lifetime.** Access token 1h, refresh token 30d. Refresh via `/auth/refresh`.

---

## 10. Per-tenant customisation: flags + config, not branches

Legacy customers often had bespoke code branches per customer. **We will not do this in v2.**

| Customisation mechanism | Use for |
|---|---|
| `FeatureFlag` (boolean per tenant) | Beta features, gradual rollout, per-customer enable/disable |
| `TenantConfig` (key-value JSON per tenant) | Tax rate overrides, receipt header text, currency symbol, default catering mode, kitchen routing rules |
| `SubscriptionPlan` features array | Plan-level capability gating |
| Theme tokens (JSON) | Logo, primary color, receipt template |

Examples of legitimate customisation:
- `FeatureFlag.aggregator_uber_eats_enabled = true` for a tenant that paid for it.
- `TenantConfig.receipt_footer_text = "Thank you - Bistro Kandy"`.
- `TenantConfig.service_charge_pct = 0.10` (default), or `0` for a tenant that doesn't charge it.
- `TenantConfig.default_catering_mode = "takeaway"` for a delivery-only tenant.

Examples that **must NOT** be per-tenant code:
- Custom tax calculation logic → use `Tax` entity layers (R4).
- Custom KOT format per tenant → use `KitchenStation.printer_config.template`.
- Custom workflow per tenant → use role + permission grid (R18).

**If a customer asks for X and we cannot do it with flags/config**, push back. Either it's a generic feature (build it into core) or it's not v2 scope (decline).

---

## 11. Data retention & deletion

| Tier | Retention |
|---|---|
| Active tenant DB | Indefinite while subscription active |
| Cancelled tenant DB | 90 days read-only |
| Deleted tenant DB | Dropped; only invoices retained 7 years per LK tax law |
| `webhook_events` | 1 year |
| `audit_log_control` | 7 years cold storage |
| `outbox_events` | 30 days after `processed_at` |
| `transaction_log` (per tenant) | 7 years |

GDPR / data-subject-access requests: per-tenant export via `POST /api/v1/admin/export-tenant-data` (returns signed blob URL). Deletion: `POST /api/v1/admin/delete-customer-data` (PII anonymised, orders retained for audit).

---

## 12. Operational concerns

- **Backup.** Tenant DBs: nightly logical backup + point-in-time recovery 7 days. Monthly snapshot to cold storage.
- **Restore.** Per-tenant restore via control plane admin UI. Tested quarterly via dry-run.
- **Monitoring.** Per-tenant dashboards (orders/min, error rate, p99 latency). Alerts on subscription state errors.
- **Cost allocation.** Each tenant's DB charges are tagged with `tenant_id` in Azure billing. Used for plan profitability analysis.
- **Capacity.** One Azure Flexible Server can host ~200 small tenant DBs. Multiple servers in an elastic pool, with tenants assigned to a server at provisioning time (round-robin + size hints).

---

## Open Questions

1. **Entra External ID per-tenant cost.** Each tenant = one Entra resource. At scale (1000+ tenants), per-tenant Azure cost may be material. **Verify** with Azure pricing before pilot scale-up. [?]
2. **DB-per-tenant vs schema-per-tenant cutoff.** At what tenant count do we hit Postgres limits per instance? Test at 100 tenants per server first. [?]
3. **Cross-tenant analytics.** Some features (industry benchmarks) need cross-tenant aggregation. **Recommend** a read-only `analytics_warehouse` populated nightly from anonymised tenant data; no live cross-tenant queries.
4. **Subscription seat counts.** Plan limits users (5/25/unlimited). Counted by `User` rows with `is_active=true`? Or by unique Entra IDs in last 30 days? **Recommend** the latter — more honest billing.
5. **Migrating legacy customers.** A `LegacyMigrationJob` reads from the old SQL Server DB and writes into a new tenant DB. Field-by-field mapping + data cleansing. Plan separately; not v1 scope.
6. **Magic-link as primary auth.** Could we ship v1 with magic-link only and defer Entra? **Yes** — simpler, faster pilot. Add Entra in v1.5. [?]

