-- E-receipts entitlement + usage meter (#79), projected from the tenant's subscription add-ons.
-- e_receipt_channels = csv of allowed channels (e.g. "email" or "email,sms,whatsapp").
-- e_receipt_quota = monthly message allowance (0 = none purchased / unlimited per projection).
-- e_receipt_used + e_receipt_period_start drive the rolling monthly meter (reset on month change).
-- Column names match EF's snake_case mapping of OrgSettings.EReceipt* (→ e_receipt_*).
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS e_receipt_channels     text;
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS e_receipt_quota        int NOT NULL DEFAULT 0;
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS e_receipt_used         int NOT NULL DEFAULT 0;
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS e_receipt_period_start timestamptz;
