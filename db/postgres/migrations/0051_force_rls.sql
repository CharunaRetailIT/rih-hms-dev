-- =============================================================================
-- Migration 0051: FORCE Row-Level Security on every tenant table
-- Target DB: every hms_tenant_<slug>
--
-- RLS was enabled on the tenant tables (0002+) but never actually enforced for
-- the application, for two reasons:
--   1. The app role OWNS its tables, and a table owner BYPASSES RLS unless the
--      table is FORCE'd — so the owner saw every row regardless of the policy.
--   2. The app never set app.tenant_id, so even where RLS did apply the policy
--      (tenant_id::text = current_setting('app.tenant_id', true)) matched nothing.
--
-- (2) is fixed in the application: TenantGucConnectionInterceptor sets the GUC on
-- every tenant connection as it opens. This migration fixes (1): FORCE RLS so the
-- owning app role is subject to the same policies as everyone else.
--
-- Superusers and BYPASSRLS roles still bypass RLS — that is expected and
-- unavoidable (local dev and the test/CI suite connect as the postgres
-- superuser, so RLS is inert there by design; the integration test uses a
-- dedicated non-superuser role to exercise it).
--
-- Iterates over every public table that already has RLS enabled, so it covers
-- all current tables and stays correct as the set grows. Idempotent: the
-- relforcerowsecurity guard makes re-runs a no-op.
-- =============================================================================

DO $$
DECLARE t text;
BEGIN
    FOR t IN
        SELECT c.relname
        FROM pg_class c
        JOIN pg_namespace n ON n.oid = c.relnamespace
        WHERE n.nspname = 'public'
          AND c.relkind = 'r'
          AND c.relrowsecurity            -- RLS enabled
          AND NOT c.relforcerowsecurity   -- not already forced
    LOOP
        EXECUTE format('ALTER TABLE public.%I FORCE ROW LEVEL SECURITY', t);
    END LOOP;
END $$;
