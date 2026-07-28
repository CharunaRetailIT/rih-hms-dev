-- =============================================================================
-- Migration 0065: Projected entitlements on org_settings (#109 Phase B).
-- Target DB: any hms_tenant_<slug>
--
-- The control-plane subscription is PROJECTED into these columns so the app can
-- enforce limits at runtime without a cross-DB call. plan_code is informational
-- (which plan the projection came from). A limit of 0 means "not projected yet /
-- unlimited" — enforcement treats <=0 as no cap, so existing tenants are unaffected
-- until their subscription is synced. (tab_device_limit + guest_qr_enabled already
-- exist from 0063.)
-- =============================================================================
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS location_limit int  NOT NULL DEFAULT 0;
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS user_limit     int  NOT NULL DEFAULT 0;
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS plan_code      text;
