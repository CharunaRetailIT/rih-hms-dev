-- =============================================================================
-- Migration 0040: Loyalty tiers + points expiry (#66 follow-on)
-- Tiers grant an earn multiplier (and an optional tier discount) based on a
-- customer's lifetime points. Expiry: a balance lapses after N days of no
-- earn/redeem activity (0 = never). Inactivity-based, so no per-lot accounting.
-- =============================================================================

CREATE TABLE IF NOT EXISTS loyalty_tiers (
    id                  uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id           uuid          NOT NULL,
    name                varchar(60)   NOT NULL,
    min_lifetime_points numeric(15,4) NOT NULL DEFAULT 0,   -- threshold to reach this tier
    earn_multiplier     numeric(8,4)  NOT NULL DEFAULT 1,   -- ×points earned at this tier
    discount_percent    numeric(8,4)  NOT NULL DEFAULT 0,   -- optional tier discount at the till
    sort_order          int           NOT NULL DEFAULT 0,
    is_active           boolean       NOT NULL DEFAULT true,
    created_at          timestamptz   NOT NULL DEFAULT now(),
    updated_at          timestamptz   NOT NULL DEFAULT now(),
    created_by          uuid          NULL,
    updated_by          uuid          NULL,
    is_deleted          boolean       NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_loyalty_tiers_tenant ON loyalty_tiers(tenant_id);

ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS loyalty_expiry_days int NOT NULL DEFAULT 0;  -- 0 = points never expire
ALTER TABLE customers ADD COLUMN IF NOT EXISTS loyalty_last_activity_at timestamptz NULL;       -- last earn/redeem (for expiry)

ALTER TABLE loyalty_tiers ENABLE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS p_loyalty_tiers_tenant ON loyalty_tiers;
CREATE POLICY p_loyalty_tiers_tenant ON loyalty_tiers USING (tenant_id::text = current_setting('app.tenant_id', true));
