-- =============================================================================
-- Migration 0036: Username for PIN login (no staff roster exposure)
-- Multi-tenant: staff sign in with workspace + username + PIN, instead of picking
-- their name from a public roster (which leaked the staff list per tenant slug).
-- Username is unique per tenant (NULLs allowed for email-only users).
-- =============================================================================

ALTER TABLE users ADD COLUMN IF NOT EXISTS username varchar(60) NULL;

CREATE UNIQUE INDEX IF NOT EXISTS uq_users_username
    ON users (tenant_id, username) WHERE username IS NOT NULL;
