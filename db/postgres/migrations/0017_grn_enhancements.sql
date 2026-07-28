-- =============================================================================
-- Migration 0017: GRN receive enhancements (data integrity)
--   * purchase unit per line → converted to stock units on receive
--   * free/bonus quantity (added to stock, excluded from cost)
--   * per-line discount (off goods value, feeds landed cost)
--   * supplier invoice number (3-way match)
--   * void/reverse a posted GRN
-- =============================================================================

ALTER TABLE goods_received_notes ADD COLUMN IF NOT EXISTS supplier_invoice_no varchar(50)  NULL;
ALTER TABLE goods_received_notes ADD COLUMN IF NOT EXISTS voided_at           timestamptz  NULL;
ALTER TABLE goods_received_notes ADD COLUMN IF NOT EXISTS void_reason         varchar(200) NULL;
-- status now: draft | posted | void

ALTER TABLE grn_lines ADD COLUMN IF NOT EXISTS unit_id         uuid          NULL;             -- received (purchasing) unit
ALTER TABLE grn_lines ADD COLUMN IF NOT EXISTS unit_symbol     varchar(10)   NULL;
ALTER TABLE grn_lines ADD COLUMN IF NOT EXISTS free_quantity   numeric(15,4) NOT NULL DEFAULT 0;  -- in received unit
ALTER TABLE grn_lines ADD COLUMN IF NOT EXISTS discount_amount numeric(15,4) NOT NULL DEFAULT 0;  -- off this line's goods value
ALTER TABLE grn_lines ADD COLUMN IF NOT EXISTS stock_quantity  numeric(15,4) NOT NULL DEFAULT 0;  -- qty actually added to stock (incl free), in stock units
