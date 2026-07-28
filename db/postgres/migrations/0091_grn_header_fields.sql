-- =============================================================================
-- Migration 0057: GRN header fields for parity with the legacy GRN screen
--   * currency + exchange rate (informational — GRN cost stays in tenant base currency)
--   * payment terms/method, recorded per-GRN like purchase_orders (0055)
--   * header-level discount and other charges (freight/handling) — recorded,
--     not allocated into the weighted-average stock cost
--   * reference number (e.g. delivery note / supplier ref)
-- =============================================================================

ALTER TABLE goods_received_notes ADD COLUMN IF NOT EXISTS currency_code    varchar(3)    NOT NULL DEFAULT 'LKR';
ALTER TABLE goods_received_notes ADD COLUMN IF NOT EXISTS currency_rate    numeric(18,6) NOT NULL DEFAULT 1;
ALTER TABLE goods_received_notes ADD COLUMN IF NOT EXISTS payment_terms_days int         NULL;
ALTER TABLE goods_received_notes ADD COLUMN IF NOT EXISTS payment_method   varchar(30)   NULL;
ALTER TABLE goods_received_notes ADD COLUMN IF NOT EXISTS discount_amount  numeric(15,4) NOT NULL DEFAULT 0;
ALTER TABLE goods_received_notes ADD COLUMN IF NOT EXISTS other_charges    numeric(15,4) NOT NULL DEFAULT 0;
ALTER TABLE goods_received_notes ADD COLUMN IF NOT EXISTS reference_no     varchar(50)   NULL;
