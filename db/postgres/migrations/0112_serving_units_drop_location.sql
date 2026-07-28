-- =============================================================================
-- Migration 0112: Drop location from serving_units — the Serving Units screen
-- no longer manages per-location serving units; every serving unit is now
-- tenant-wide (matches product variants, which already reference serving
-- units without any location scoping of their own).
-- =============================================================================

DROP INDEX IF EXISTS ix_serving_units_tenant_location;
ALTER TABLE serving_units DROP CONSTRAINT IF EXISTS uq_serving_unit_code;

ALTER TABLE serving_units DROP COLUMN IF EXISTS location_id;

ALTER TABLE serving_units ADD CONSTRAINT uq_serving_unit_code UNIQUE (tenant_id, code);
