-- =============================================================================
-- Migration 0037: KOT workflow modes (#KOT)
-- Clients run the kitchen ticket flow differently:
--   • KDS display (on/off) — some opt out of a kitchen screen entirely
--   • auto-print KOT when an order is sent/accepted — for venues with a KOT
--     printer (or printing from the POS and handing it over by hand)
-- Both default to the happy path (KDS on, manual print) so nothing changes
-- until a tenant configures it.
-- =============================================================================

ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS kds_enabled    boolean NOT NULL DEFAULT true;
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS kot_auto_print boolean NOT NULL DEFAULT false;
