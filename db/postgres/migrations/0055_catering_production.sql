-- =============================================================================
-- Migration 0055: connect catering to inventory/production (#75 follow-on).
-- A package can reference a recipe (by its output product); producing an event
-- generates a pax-scaled production order that consumes ingredients from stock
-- (via the Production module) and records the event's food cost. Additive columns
-- on existing (already RLS-forced) tables.
-- =============================================================================

ALTER TABLE catering_packages ADD COLUMN IF NOT EXISTS recipe_product_id  uuid           NULL;
ALTER TABLE catering_events   ADD COLUMN IF NOT EXISTS production_order_id uuid           NULL;
ALTER TABLE catering_events   ADD COLUMN IF NOT EXISTS food_cost           numeric(15, 4) NOT NULL DEFAULT 0;
ALTER TABLE catering_events   ADD COLUMN IF NOT EXISTS produced_at         timestamptz    NULL;
