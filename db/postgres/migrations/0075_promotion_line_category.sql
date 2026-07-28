-- #4 product-category discount: a product_discount promotion line can target a whole
-- product CATEGORY (every item in it, including sub-categories) instead of a single
-- product. null category_id = the existing per-product behaviour.
ALTER TABLE promotion_lines ADD COLUMN IF NOT EXISTS category_id uuid;
