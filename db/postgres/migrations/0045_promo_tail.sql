-- =============================================================================
-- Migration 0045: Promotion tail (#65 follow-on)
-- • customer-segment scope: a promo that only fires for an attached customer in
--   a given category.
-- • lowest_price promo type ("3-for-2" / cheapest free) uses the existing
--   promotion_lines columns (min_qty = group size, get_qty = waved per group,
--   discount_percent = how much off the cheapest; product_id optionally scopes it).
-- =============================================================================

ALTER TABLE promotions ADD COLUMN IF NOT EXISTS applies_to_category_id uuid NULL;  -- customer category scope (null = any)
