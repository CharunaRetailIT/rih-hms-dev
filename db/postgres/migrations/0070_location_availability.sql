-- =============================================================================
-- Migration 0070: per-location manual availability override (#112 / #100).
-- Target DB: any hms_tenant_<slug> (filename has no "control" → tenant classifier).
--
-- Absent row  → availability is auto-computed from stock + recipe sellability.
-- Present row → forces the item available/unavailable at that outlet (manual 86 / un-86).
-- RLS: ENABLE + FORCE + tenant policy (0051 pattern, mirrors tab_devices 0063).
-- =============================================================================

CREATE TABLE IF NOT EXISTS product_availability_overrides (
    id          uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id   uuid        NOT NULL,
    location_id uuid        NOT NULL,
    product_id  uuid        NOT NULL,
    available   boolean     NOT NULL,
    created_at  timestamptz NOT NULL DEFAULT now(),
    updated_at  timestamptz NOT NULL DEFAULT now(),
    created_by  uuid        NULL,
    updated_by  uuid        NULL,
    is_deleted  boolean     NOT NULL DEFAULT false
);
CREATE UNIQUE INDEX IF NOT EXISTS ux_avail_override_tenant_loc_prod
    ON product_availability_overrides (tenant_id, location_id, product_id);

DO $$
BEGIN
    EXECUTE 'ALTER TABLE public.product_availability_overrides ENABLE ROW LEVEL SECURITY';
    EXECUTE 'ALTER TABLE public.product_availability_overrides FORCE ROW LEVEL SECURITY';
    EXECUTE 'DROP POLICY IF EXISTS p_avail_override_tenant ON public.product_availability_overrides';
    EXECUTE 'CREATE POLICY p_avail_override_tenant ON public.product_availability_overrides USING (tenant_id::text = current_setting(''app.tenant_id'', true))';
END $$;
