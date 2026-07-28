-- Serving-size depletion (Lakshitha's "units of serving"): each product variant (serving
-- size) consumes this many STOCK units when sold. e.g. a bottle stocked in ml → a "50 ml"
-- pour variant has serving_qty = 50; selling 2 deducts 100 ml. Default 1 preserves the
-- existing behaviour (one stock unit per sale) for all current variants.
ALTER TABLE product_variants ADD COLUMN IF NOT EXISTS serving_qty numeric(18,4) NOT NULL DEFAULT 1;
