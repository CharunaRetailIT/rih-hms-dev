-- =============================================================================
-- Migration 0016: Production parity with the legacy module
--   * multiple recipes per product, keyed by output unit
--   * production-order lifecycle (draft → posted → void)
--   * custom/ad-hoc production (no recipe)
--   * multiple-product production documents (shared document_no)
--   * inter-location production (consume here, deliver finished elsewhere)
-- All columns are added up front; features are layered on in code.
-- =============================================================================

-- ── multiple recipes per product (per output unit) ──
ALTER TABLE recipes ADD COLUMN IF NOT EXISTS output_unit_id uuid NULL;
ALTER TABLE recipes DROP CONSTRAINT IF EXISTS uq_recipe_tenant_product;
-- one recipe per (product, output unit); NULL output unit treated as a fixed sentinel
CREATE UNIQUE INDEX IF NOT EXISTS uq_recipe_tenant_product_unit
  ON recipes (tenant_id, product_id, COALESCE(output_unit_id, '00000000-0000-0000-0000-000000000000'::uuid))
  WHERE is_deleted = false;

-- ── production-order lifecycle + inter-location + batch + custom ──
ALTER TABLE production_orders ADD COLUMN IF NOT EXISTS document_no         varchar(20) NULL;   -- groups a multi-product batch
ALTER TABLE production_orders ADD COLUMN IF NOT EXISTS receipt_location_id uuid        NULL;   -- where finished goods land (default = location_id)
ALTER TABLE production_orders ADD COLUMN IF NOT EXISTS request_id          uuid        NULL;   -- requisition/transfer fulfilled
ALTER TABLE production_orders ADD COLUMN IF NOT EXISTS is_custom           boolean     NOT NULL DEFAULT false;
ALTER TABLE production_orders ADD COLUMN IF NOT EXISTS posted_at           timestamptz NULL;
ALTER TABLE production_orders ADD COLUMN IF NOT EXISTS voided_at           timestamptz NULL;
ALTER TABLE production_orders ADD COLUMN IF NOT EXISTS void_reason         varchar(200) NULL;

-- Existing rows were posted immediately under status 'completed'; normalise to 'posted'.
UPDATE production_orders SET status = 'posted', posted_at = COALESCE(posted_at, completed_at)
  WHERE status = 'completed';
CREATE INDEX IF NOT EXISTS ix_production_document ON production_orders(tenant_id, document_no);
