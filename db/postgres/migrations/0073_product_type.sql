-- Raw Material vs Finished Goods (#3a): explicit classification on a product, so the system can
-- tell ingredients/raw stock apart from sellable finished items (default 'finished' = today's behaviour).
ALTER TABLE products ADD COLUMN IF NOT EXISTS product_type text NOT NULL DEFAULT 'finished';   -- finished | raw
