-- =============================================================================
-- Migration 0103: Customer Category gains IsVat — matches the old system's
-- Customer Category screen (Code, Name, Discount %, Remark, IsVAT), which this
-- app's customer_categories table already covers except this one flag.
-- =============================================================================

ALTER TABLE customer_categories ADD COLUMN IF NOT EXISTS is_vat boolean NOT NULL DEFAULT false;
