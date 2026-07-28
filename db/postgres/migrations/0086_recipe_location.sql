-- =============================================================================
-- Migration 0052: Recipes can now be scoped to a location (a product may need
-- a different BOM per outlet), same pattern as the existing per-output-unit
-- keying from migration 0016.
-- =============================================================================

ALTER TABLE recipes ADD COLUMN IF NOT EXISTS location_id uuid NULL;

DROP INDEX IF EXISTS uq_recipe_tenant_product_unit;

-- one recipe per (product, output unit, location); NULL treated as a fixed sentinel
CREATE UNIQUE INDEX IF NOT EXISTS uq_recipe_tenant_product_unit_location
  ON recipes (tenant_id, product_id,
              COALESCE(output_unit_id, '00000000-0000-0000-0000-000000000000'::uuid),
              COALESCE(location_id, '00000000-0000-0000-0000-000000000000'::uuid))
  WHERE is_deleted = false;

CREATE INDEX IF NOT EXISTS ix_recipes_location ON recipes(location_id) WHERE location_id IS NOT NULL;
