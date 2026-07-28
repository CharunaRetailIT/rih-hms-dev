-- Purchase orders: per-PO payment terms/method (was previously only on the supplier master).
ALTER TABLE purchase_orders ADD COLUMN IF NOT EXISTS payment_terms_days int NULL;
ALTER TABLE purchase_orders ADD COLUMN IF NOT EXISTS payment_method varchar(30) NULL;
