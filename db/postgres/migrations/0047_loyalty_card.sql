-- =============================================================================
-- Migration 0047: Loyalty card number (#66 follow-on)
-- A scannable card/membership number per customer for quick attach at the till.
-- =============================================================================

ALTER TABLE customers ADD COLUMN IF NOT EXISTS loyalty_card_no varchar(40) NULL;
CREATE UNIQUE INDEX IF NOT EXISTS uq_customers_card ON customers (tenant_id, loyalty_card_no) WHERE loyalty_card_no IS NOT NULL AND is_deleted = false;
