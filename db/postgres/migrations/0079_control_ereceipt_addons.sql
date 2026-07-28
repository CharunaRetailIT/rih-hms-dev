-- E-receipts add-ons (#79): metered monthly message bundles, billed flat per month.
-- included_qty = the monthly message allowance (0 = unlimited). Two tiers — email-only and
-- all-channels (email + SMS + WhatsApp). Prices/quotas are RIT-editable in the admin. Target: hms_control.
SET search_path TO control, public;

ALTER TABLE control.addons ADD COLUMN IF NOT EXISTS included_qty int NOT NULL DEFAULT 0;

INSERT INTO control.addons (id, code, name, unit, unit_price, currency, included_qty, is_active, created_at, updated_at, is_deleted)
SELECT gen_random_uuid(), 'ereceipt_email', 'E-Receipts — Email', 'flat_month', 1500, 'LKR', 2000, true, now(), now(), false
WHERE NOT EXISTS (SELECT 1 FROM control.addons WHERE code = 'ereceipt_email');

INSERT INTO control.addons (id, code, name, unit, unit_price, currency, included_qty, is_active, created_at, updated_at, is_deleted)
SELECT gen_random_uuid(), 'ereceipt_all', 'E-Receipts — Email + SMS + WhatsApp', 'flat_month', 3500, 'LKR', 2000, true, now(), now(), false
WHERE NOT EXISTS (SELECT 1 FROM control.addons WHERE code = 'ereceipt_all');
