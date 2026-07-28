-- =============================================================================
-- Migration 0034: Staff PIN login (no email required for POS staff)
-- Email becomes optional so emailless POS staff can exist. A hashed passcode
-- (PBKDF2 — never plaintext) lets them sign in by tapping their name + PIN.
-- Brute-force is contained by a failed-attempt counter + a temporary lockout.
-- Magic-link still works for any user that has an email.
-- =============================================================================

ALTER TABLE users ALTER COLUMN email DROP NOT NULL;
ALTER TABLE users ADD COLUMN IF NOT EXISTS passcode_hash    text        NULL;
ALTER TABLE users ADD COLUMN IF NOT EXISTS pin_failed_count int         NOT NULL DEFAULT 0;
ALTER TABLE users ADD COLUMN IF NOT EXISTS pin_locked_until timestamptz NULL;
