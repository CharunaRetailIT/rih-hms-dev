-- =============================================================================
-- Migration 0058: User home location (location scoping P1)
-- A user pinned to a home outlet only sees that outlet's transactional data and
-- the branch switcher is hidden. NULL = head-office / all-access (Owner is always
-- all-access regardless). Drives per-location scoping across the app.
-- =============================================================================

ALTER TABLE users ADD COLUMN IF NOT EXISTS home_location_id uuid;
