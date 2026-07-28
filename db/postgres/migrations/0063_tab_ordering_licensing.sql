-- =============================================================================
-- Migration 0063: Tab Ordering & Guest QR — licensed add-ons (#106).
-- Target DB: any hms_tenant_<slug>
--
-- Entitlements (RIT platform-admin grants, MSO365-style) live on org_settings:
--   tab_device_limit  — how many waiter-handheld DEVICE seats the tenant bought.
--   guest_qr_enabled  — flat-monthly guest-QR add-on on/off.
-- tab_devices registers each handheld; an active device consumes one seat, and
-- registration is refused once active devices reach the limit.
-- RLS: tab_devices ENABLE + FORCE + tenant policy (0051 pattern).
-- =============================================================================

ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS tab_device_limit int     NOT NULL DEFAULT 0;
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS guest_qr_enabled boolean NOT NULL DEFAULT false;

CREATE TABLE IF NOT EXISTS tab_devices (
    id           uuid         PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id    uuid         NOT NULL,
    name         varchar(120) NOT NULL,
    token_hash   varchar(128) NOT NULL,      -- SHA-256 of the device token (raw token shown once at register)
    fingerprint  varchar(200) NULL,          -- device id / browser fingerprint (soft anti-share signal)
    location_id  uuid         NULL,          -- optional: pin a device to an outlet
    last_seen_at timestamptz  NULL,
    is_active    boolean      NOT NULL DEFAULT true,
    created_at   timestamptz  NOT NULL DEFAULT now(),
    updated_at   timestamptz  NOT NULL DEFAULT now(),
    created_by   uuid         NULL,
    updated_by   uuid         NULL,
    is_deleted   boolean      NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_tab_devices_tenant_active ON tab_devices (tenant_id, is_active);

DO $$
BEGIN
    EXECUTE 'ALTER TABLE public.tab_devices ENABLE ROW LEVEL SECURITY';
    EXECUTE 'ALTER TABLE public.tab_devices FORCE ROW LEVEL SECURITY';
    EXECUTE 'DROP POLICY IF EXISTS p_tab_devices_tenant ON public.tab_devices';
    EXECUTE 'CREATE POLICY p_tab_devices_tenant ON public.tab_devices USING (tenant_id::text = current_setting(''app.tenant_id'', true))';
END $$;
