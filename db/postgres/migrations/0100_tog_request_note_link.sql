-- =============================================================================
-- Migration 0100: Transfer of Goods (TOG) — adds the explicit transfer date /
-- reference number fields the refactored create page needs, and lets a
-- "transfer"-mode request note link back to the TOG that fulfilled it
-- (mirrors purchase_order_id from 0099, same "fulfilled" status).
-- =============================================================================

ALTER TABLE stock_transfers ADD COLUMN IF NOT EXISTS transfer_date date NULL;
ALTER TABLE stock_transfers ADD COLUMN IF NOT EXISTS reference_no  varchar(100) NULL;
UPDATE stock_transfers SET transfer_date = created_at::date WHERE transfer_date IS NULL;
ALTER TABLE stock_transfers ALTER COLUMN transfer_date SET NOT NULL;
ALTER TABLE stock_transfers ALTER COLUMN transfer_date SET DEFAULT CURRENT_DATE;

ALTER TABLE request_notes ADD COLUMN IF NOT EXISTS transfer_id uuid NULL;
