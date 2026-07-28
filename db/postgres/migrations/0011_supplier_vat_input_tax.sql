-- =============================================================================
-- Migration 0011: Supplier VAT + input-tax capture on receipts
-- VAT-registered suppliers charge input VAT; we capture it so the VAT return
-- nets output VAT (sales) against input VAT (purchases). Stock cost stays
-- ex-VAT (input VAT is reclaimable, not an inventory cost).
-- =============================================================================

ALTER TABLE suppliers ADD COLUMN IF NOT EXISTS vat_registration_number varchar(40) NULL;
ALTER TABLE suppliers ADD COLUMN IF NOT EXISTS is_vat_registered boolean NOT NULL DEFAULT false;

-- VAT on the purchase order (estimated input VAT at order time).
ALTER TABLE purchase_orders      ADD COLUMN IF NOT EXISTS tax_amount numeric(15,4) NOT NULL DEFAULT 0;
ALTER TABLE purchase_order_lines ADD COLUMN IF NOT EXISTS tax_rate   numeric(8,4)  NOT NULL DEFAULT 0;
ALTER TABLE purchase_order_lines ADD COLUMN IF NOT EXISTS tax_amount numeric(15,4) NOT NULL DEFAULT 0;

-- Input VAT captured on goods received (the reclaimable amount).
ALTER TABLE goods_received_notes ADD COLUMN IF NOT EXISTS tax_amount numeric(15,4) NOT NULL DEFAULT 0;
ALTER TABLE grn_lines ADD COLUMN IF NOT EXISTS tax_rate   numeric(8,4)  NOT NULL DEFAULT 0;
ALTER TABLE grn_lines ADD COLUMN IF NOT EXISTS tax_amount numeric(15,4) NOT NULL DEFAULT 0;
