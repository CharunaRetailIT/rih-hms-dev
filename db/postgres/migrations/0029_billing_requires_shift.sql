-- =============================================================================
-- Migration 0029: Billing requires an open shift (cash accountability)
-- A bill can't be settled unless a shift is open for that outlet, so every sale
-- is attributed to a cash session and shows up in the Z-report. On by default;
-- can be relaxed per tenant in Settings.
-- =============================================================================

ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS require_open_shift_for_billing boolean NOT NULL DEFAULT true;
