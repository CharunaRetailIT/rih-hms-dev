-- =============================================================================
-- Migration 0095: Stock Adjustment document workflow — draft -> submit ->
-- pending/approved -> approve/reject, mirroring purchase_orders/GRN (0093/0094).
-- The existing quick single-line adjust modal (Inventory page) and the
-- opening-stock importer keep writing status='posted' directly and are
-- untouched by this migration; the new columns are additive only.
-- =============================================================================

ALTER TABLE stock_adjustments ADD COLUMN IF NOT EXISTS total_value    numeric(15,4) NOT NULL DEFAULT 0;
ALTER TABLE stock_adjustments ADD COLUMN IF NOT EXISTS submitted_at   timestamptz NULL;
ALTER TABLE stock_adjustments ADD COLUMN IF NOT EXISTS approved_by    uuid NULL;
ALTER TABLE stock_adjustments ADD COLUMN IF NOT EXISTS approved_at    timestamptz NULL;
ALTER TABLE stock_adjustments ADD COLUMN IF NOT EXISTS rejected_by    uuid NULL;
ALTER TABLE stock_adjustments ADD COLUMN IF NOT EXISTS rejected_at    timestamptz NULL;
ALTER TABLE stock_adjustments ADD COLUMN IF NOT EXISTS reject_reason  varchar(200) NULL;

ALTER TABLE stock_adjustment_lines ADD COLUMN IF NOT EXISTS adjustment_type varchar(10)   NOT NULL DEFAULT 'add';
ALTER TABLE stock_adjustment_lines ADD COLUMN IF NOT EXISTS current_stock   numeric(15,4) NOT NULL DEFAULT 0;
ALTER TABLE stock_adjustment_lines ADD COLUMN IF NOT EXISTS new_stock       numeric(15,4) NOT NULL DEFAULT 0;
ALTER TABLE stock_adjustment_lines ADD COLUMN IF NOT EXISTS line_total      numeric(15,4) NOT NULL DEFAULT 0;

ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS require_stock_adjustment_approval   boolean       NOT NULL DEFAULT false;
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS stock_adjustment_approval_threshold numeric(15,4) NOT NULL DEFAULT 0;
