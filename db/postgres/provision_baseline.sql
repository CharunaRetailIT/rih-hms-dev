-- =============================================================================
-- Baseline seed for a freshly provisioned tenant DB (run AFTER all tenant
-- migrations). Minimal, clean — no demo products. The ProvisioningService
-- substitutes {{TENANT_ID}}, {{OWNER_EMAIL}}, {{OWNER_NAME}} (single quotes in
-- the email/name are escaped by doubling). org_settings is auto-created by the
-- app on first access, so it's intentionally omitted here.
-- =============================================================================

-- Scope this session to the tenant so the inserts below pass the FORCE'd
-- row-level security WITH CHECK (defaulted from the policy's USING clause) when
-- this seed runs as a non-superuser app role. No-op under a superuser connection.
SELECT set_config('app.tenant_id', '{{TENANT_ID}}', false);

-- Units of measure (mass / volume / count) so products + recipes work day one.
INSERT INTO units_of_measure (tenant_id, code, name, symbol, is_base_unit, dimension, factor_to_base) VALUES
  ('{{TENANT_ID}}','EA','Each','ea',true,'count',1),
  ('{{TENANT_ID}}','KG','Kilogram','kg',true,'mass',1000),
  ('{{TENANT_ID}}','G','Gram','g',false,'mass',1),
  ('{{TENANT_ID}}','L','Litre','L',true,'volume',1000),
  ('{{TENANT_ID}}','ML','Millilitre','mL',false,'volume',1),
  ('{{TENANT_ID}}','BOT','Bottle','bot',false,'count',1),
  ('{{TENANT_ID}}','PLT','Plate','plt',false,'count',1)
ON CONFLICT (tenant_id, code) DO NOTHING;

-- Consumption tax, named for the signup country (VAT / GST / Sales Tax), default rate 18% — owner edits in Settings.
INSERT INTO taxes (tenant_id, code, name, rate_percent, is_inclusive, apply_on) VALUES
  ('{{TENANT_ID}}','VAT','{{TAX_LABEL}} (18%)',18.0000,false,'line')
ON CONFLICT (tenant_id, code) DO NOTHING;

-- Bill-level tax charge (the tenant can add service charge / levies in Settings). charge_type 'vat' stays a stable internal code.
INSERT INTO tax_charges (tenant_id, code, name, charge_type, rate_percent, sequence, compound_on_previous, applies_to_takeaway, applies_to_delivery) VALUES
  ('{{TENANT_ID}}','VAT','{{TAX_LABEL}}','vat',18.0000,1,false,true,true)
ON CONFLICT (tenant_id, code) DO NOTHING;

-- Org settings: seed the business base currency + tax label chosen at signup (UAE → AED, India → GST, etc.).
-- All other columns take their defaults.
INSERT INTO org_settings (tenant_id, base_currency, tax_label)
SELECT '{{TENANT_ID}}','{{BASE_CURRENCY}}','{{TAX_LABEL}}'
WHERE NOT EXISTS (SELECT 1 FROM org_settings WHERE tenant_id = '{{TENANT_ID}}');

-- One default outlet.
INSERT INTO locations (tenant_id, code, name, address_line1, city, country_code, currency, location_type, can_sell, can_produce, can_stock) VALUES
  ('{{TENANT_ID}}','MAIN','Main Outlet','—','—','{{COUNTRY_CODE}}','{{BASE_CURRENCY}}','outlet',true,false,true)
ON CONFLICT (tenant_id, code) DO NOTHING;

-- The owner account (so they can sign in via magic-link immediately).
INSERT INTO users (tenant_id, email, display_name, role, is_active) VALUES
  ('{{TENANT_ID}}','{{OWNER_EMAIL}}','{{OWNER_NAME}}',0,true)
ON CONFLICT (tenant_id, email) DO NOTHING;
