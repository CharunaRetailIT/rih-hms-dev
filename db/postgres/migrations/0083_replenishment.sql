-- =============================================================================
-- Migration 0083: inventory replenishment (#replenishment).
-- Target DB: any hms_tenant_<slug> (filename has no "control" → tenant classifier).
--
-- Two-layer reorder model:
--   • product defaults — reorder_level (already on products) + par_level + a
--     preferred supplier, applied everywhere unless overridden;
--   • per-location overrides — product_replenishment_levels lets one outlet carry
--     its own min (reorder) / max (par) / preferred supplier; an absent row falls
--     back to the product default.
-- Each location may name a replenish_source (a central kitchen / warehouse) that
-- tops it up by transfer; the worksheet suggests a transfer for what the hub can
-- spare and a PO for the rest. RLS: ENABLE+FORCE+tenant policy (0051/0082 pattern).
-- =============================================================================

ALTER TABLE products  ADD COLUMN IF NOT EXISTS par_level            numeric(18,4) NULL;
ALTER TABLE products  ADD COLUMN IF NOT EXISTS preferred_supplier_id uuid         NULL;
ALTER TABLE locations ADD COLUMN IF NOT EXISTS replenish_source_location_id uuid  NULL;

CREATE TABLE IF NOT EXISTS product_replenishment_levels (
    id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id             uuid NOT NULL,
    product_id            uuid NOT NULL,
    location_id           uuid NOT NULL,
    reorder_level         numeric(18,4) NULL,    -- min / trigger point (null → use product default)
    par_level             numeric(18,4) NULL,    -- max / order-up-to target (null → product default, else reorder)
    preferred_supplier_id uuid NULL,
    created_at  timestamptz NOT NULL DEFAULT now(),
    updated_at  timestamptz NOT NULL DEFAULT now(),
    created_by  uuid NULL, updated_by uuid NULL,
    is_deleted  boolean NOT NULL DEFAULT false
);
CREATE UNIQUE INDEX IF NOT EXISTS ux_repl_levels_prod_loc ON product_replenishment_levels (tenant_id, product_id, location_id) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS ix_repl_levels_loc ON product_replenishment_levels (tenant_id, location_id);

DO $$
DECLARE app_role text := 'hms_app';
BEGIN
    EXECUTE 'ALTER TABLE public.product_replenishment_levels ENABLE ROW LEVEL SECURITY';
    EXECUTE 'ALTER TABLE public.product_replenishment_levels FORCE ROW LEVEL SECURITY';
    EXECUTE 'DROP POLICY IF EXISTS p_repl_levels_tenant ON public.product_replenishment_levels';
    EXECUTE 'CREATE POLICY p_repl_levels_tenant ON public.product_replenishment_levels USING (tenant_id::text = current_setting(''app.tenant_id'', true))';
    -- If applied as a superuser the table is postgres-owned → the app role hits
    -- "permission denied". Hand ownership to the runtime role like every other table.
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = app_role) THEN
        EXECUTE 'ALTER TABLE public.product_replenishment_levels OWNER TO hms_app';
    END IF;
END $$;
