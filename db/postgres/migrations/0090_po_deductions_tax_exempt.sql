-- Purchase orders: a separate "deductions" bucket (distinct from header discount)
-- and a per-order flag to exclude tax entirely.
ALTER TABLE purchase_orders ADD COLUMN IF NOT EXISTS deductions numeric(15,4) NOT NULL DEFAULT 0;
ALTER TABLE purchase_orders ADD COLUMN IF NOT EXISTS exclude_tax boolean NOT NULL DEFAULT false;
