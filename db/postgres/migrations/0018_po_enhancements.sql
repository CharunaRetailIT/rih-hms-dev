-- =============================================================================
-- Migration 0018: Purchase Order parity with the legacy module
--   * per-line purchasing unit + line discount
--   * header discount, other charges (freight) → landed cost
--   * currency + rate, delivery (ship-to) location + address, reference no
--   * lifecycle (sent/cancelled) + approval (maker-checker) columns
-- Columns added up front; send/cancel/edit + approval wired in later tranches.
-- =============================================================================

ALTER TABLE purchase_orders ADD COLUMN IF NOT EXISTS discount_amount      numeric(15,4) NOT NULL DEFAULT 0;
ALTER TABLE purchase_orders ADD COLUMN IF NOT EXISTS other_charges        numeric(15,4) NOT NULL DEFAULT 0;
ALTER TABLE purchase_orders ADD COLUMN IF NOT EXISTS currency_code        varchar(3)    NOT NULL DEFAULT 'LKR';
ALTER TABLE purchase_orders ADD COLUMN IF NOT EXISTS currency_rate        numeric(18,6) NOT NULL DEFAULT 1;
ALTER TABLE purchase_orders ADD COLUMN IF NOT EXISTS delivery_location_id uuid          NULL;   -- ship-to (default = location_id)
ALTER TABLE purchase_orders ADD COLUMN IF NOT EXISTS delivery_address     varchar(500)  NULL;
ALTER TABLE purchase_orders ADD COLUMN IF NOT EXISTS reference_no         varchar(50)   NULL;
ALTER TABLE purchase_orders ADD COLUMN IF NOT EXISTS sent_at              timestamptz   NULL;
ALTER TABLE purchase_orders ADD COLUMN IF NOT EXISTS cancelled_at         timestamptz   NULL;
ALTER TABLE purchase_orders ADD COLUMN IF NOT EXISTS cancel_reason        varchar(200)  NULL;
ALTER TABLE purchase_orders ADD COLUMN IF NOT EXISTS approval_status      varchar(20)   NOT NULL DEFAULT 'not_required';  -- not_required|pending|approved|rejected
ALTER TABLE purchase_orders ADD COLUMN IF NOT EXISTS approved_by          uuid          NULL;
ALTER TABLE purchase_orders ADD COLUMN IF NOT EXISTS approved_at          timestamptz   NULL;
ALTER TABLE purchase_orders ADD COLUMN IF NOT EXISTS reject_reason        varchar(200)  NULL;

ALTER TABLE purchase_order_lines ADD COLUMN IF NOT EXISTS unit_id         uuid          NULL;   -- purchasing unit
ALTER TABLE purchase_order_lines ADD COLUMN IF NOT EXISTS unit_symbol     varchar(10)   NULL;
ALTER TABLE purchase_order_lines ADD COLUMN IF NOT EXISTS discount_amount numeric(15,4) NOT NULL DEFAULT 0;
