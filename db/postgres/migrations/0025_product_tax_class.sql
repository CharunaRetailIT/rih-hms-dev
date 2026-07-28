-- =============================================================================
-- Migration 0025: Per-product tax class (#55d)
-- The bill-level tax engine (service charge → SSCL → VAT, compounding) is the
-- source of truth for sales tax. Until now it VAT-ed the WHOLE subtotal — a
-- mixed basket with a zero-rated/exempt item was over-taxed. This adds a per-
-- product tax class so VAT-type charges apply only to the *taxable* subtotal.
--   standard   → in the VAT base (default)
--   zero_rated → 0% (reported separately; out of the VAT base)
--   exempt     → outside VAT entirely
-- (Org-wide stacked / compound taxes are already configurable via tax_charges.)
-- =============================================================================

ALTER TABLE products ADD COLUMN IF NOT EXISTS tax_class varchar(16) NOT NULL DEFAULT 'standard';

-- Whether a line counts toward the VAT base, snapshotted at add time.
ALTER TABLE order_items ADD COLUMN IF NOT EXISTS is_taxable boolean NOT NULL DEFAULT true;

-- Back-fill: any product already flagged non-taxable becomes 'exempt'.
UPDATE products SET tax_class = 'exempt' WHERE is_taxable = false AND tax_class = 'standard';
