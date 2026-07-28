-- Per-product discount (#3b): an auto-applied discount on a product — percentage or fixed amount.
-- Floored at the product's cost price (can't sell below cost) unless an admin enables the override.
ALTER TABLE products      ADD COLUMN IF NOT EXISTS discount_type  text          NOT NULL DEFAULT 'none';   -- none | percent | fixed
ALTER TABLE products      ADD COLUMN IF NOT EXISTS discount_value numeric(18,4) NOT NULL DEFAULT 0;
ALTER TABLE org_settings  ADD COLUMN IF NOT EXISTS allow_discount_below_cost boolean NOT NULL DEFAULT false;
-- the applied per-line discount (so the bill shows it and recalcs preserve it)
ALTER TABLE order_items   ADD COLUMN IF NOT EXISTS discount_amount numeric(18,4) NOT NULL DEFAULT 0;
