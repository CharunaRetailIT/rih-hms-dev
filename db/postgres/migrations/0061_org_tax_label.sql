-- Country-derived display name for the consumption tax (VAT / GST / Sales Tax).
-- Set at signup from the chosen country; surfaced across the UI and on bills.
-- Existing tenants keep "VAT" (the prior hardcoded label).
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS tax_label text NOT NULL DEFAULT 'VAT';
