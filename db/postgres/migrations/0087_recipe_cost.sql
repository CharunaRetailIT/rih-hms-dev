-- =============================================================================
-- Migration 0053: Recipe ingredient cost prices + total recipe cost.
-- Each recipe line snapshots its ingredient's cost price (per stock unit) and
-- the computed line cost; the recipe stores the sum for one batch (yield_quantity
-- output units).
-- =============================================================================

ALTER TABLE recipes ADD COLUMN IF NOT EXISTS total_cost numeric(18,4) NOT NULL DEFAULT 0;

ALTER TABLE recipe_lines ADD COLUMN IF NOT EXISTS cost_price numeric(18,4) NOT NULL DEFAULT 0;
ALTER TABLE recipe_lines ADD COLUMN IF NOT EXISTS line_cost  numeric(18,4) NOT NULL DEFAULT 0;
