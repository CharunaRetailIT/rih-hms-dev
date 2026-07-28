-- =============================================================================
-- Migration 0057: Tenant logo (#82a)
-- Stores the business logo as a data-URL (base64) or hosted URL on org settings,
-- shown in the sidebar and on printed bills/receipts. Nullable; set in Settings.
-- =============================================================================

ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS logo_url text;
