-- =============================================================================
-- Migration 0056: Customer date of birth + phone lookup index (#70 follow-on)
-- Optional birthday on the customer master — powers birthday greetings / offers
-- and richer CRM. Captured at POS onboarding or on the Customers screen.
-- The phone index backs "find-or-create by phone" so the till stops creating a
-- duplicate customer for the same number on every visit.
-- =============================================================================

ALTER TABLE customers ADD COLUMN IF NOT EXISTS date_of_birth date;
CREATE INDEX IF NOT EXISTS ix_customers_tenant_phone ON customers (tenant_id, phone);
