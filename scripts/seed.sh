#!/usr/bin/env bash
# Seed the demo tenant in the control plane + an owner user in its tenant DB.
# Idempotent — safe to run multiple times.
set -euo pipefail

DB_CONTROL=${DB_CONTROL:-hms_control}
DB_TENANT=${DB_TENANT:-hms_tenant_demo}

DEMO_SLUG="demo"
DEMO_NAME="Demo Restaurant"
DEMO_OWNER="owner@demo.local"
DEMO_LOC_CODE="MAIN"
DEMO_LOC_NAME="Demo Outlet — Main"
# Stable demo tenant id — so `make reset` keeps the same id and existing logins
# (JWT tenant_id) still resolve after a reseed instead of erroring "tenant not found".
DEMO_TENANT_ID="a0000000-0000-4000-8000-000000000001"

echo "==> Seeding control plane ($DB_CONTROL)"
psql -d "$DB_CONTROL" -v ON_ERROR_STOP=1 <<SQL
INSERT INTO control.tenants
    (id, slug, display_name, database_name, database_host, status, plan, owner_email, trial_ends_at)
VALUES
    ('${DEMO_TENANT_ID}', '${DEMO_SLUG}', '${DEMO_NAME}', '${DB_TENANT}', 'localhost', 3, 'starter', '${DEMO_OWNER}', now() + interval '14 days')
ON CONFLICT (slug) DO UPDATE
    SET display_name  = EXCLUDED.display_name,
        database_name = EXCLUDED.database_name,
        status        = 3,
        updated_at    = now();
SQL

TENANT_ID=$(psql -d "$DB_CONTROL" -tAc "SELECT id FROM control.tenants WHERE slug='${DEMO_SLUG}'")
echo "    Tenant id: $TENANT_ID"

echo "==> Seeding tenant DB ($DB_TENANT)"
psql -d "$DB_TENANT" -v ON_ERROR_STOP=1 <<SQL
-- Staff users — one per role so each persona can sign in via magic link.
-- 0=Owner 1=Manager 2=Cashier 3=Kitchen 4=Accountant
INSERT INTO users (tenant_id, email, display_name, role, is_active)
VALUES
    ('${TENANT_ID}', '${DEMO_OWNER}',        'Demo Owner',    0, true),
    ('${TENANT_ID}', 'manager@demo.local',   'Maya Manager',  1, true),
    ('${TENANT_ID}', 'cashier@demo.local',   'Asela Cashier', 2, true),
    ('${TENANT_ID}', 'kitchen@demo.local',   'Kanan Kitchen', 3, true),
    ('${TENANT_ID}', 'finance@demo.local',   'Farah Finance', 4, true)
ON CONFLICT (tenant_id, email) DO NOTHING;

INSERT INTO locations (tenant_id, code, name, address_line1, city, country_code, currency)
VALUES
    ('${TENANT_ID}', '${DEMO_LOC_CODE}', '${DEMO_LOC_NAME}', 'No. 1, Galle Road', 'Colombo', 'LK', 'LKR')
ON CONFLICT (tenant_id, code) DO NOTHING;
SQL

echo "==> Seeding master data (units, taxes, categories, products)"
psql -d "$DB_TENANT" -v ON_ERROR_STOP=1 <<SQL
-- Units of measure
-- dimension groups convertible units; factor_to_base = base units per 1 (mass base = g, volume base = mL)
INSERT INTO units_of_measure (tenant_id, code, name, symbol, is_base_unit, dimension, factor_to_base) VALUES
    ('${TENANT_ID}', 'EA',  'Each',       'ea',  true,  'count',  1),
    ('${TENANT_ID}', 'KG',  'Kilogram',   'kg',  true,  'mass',   1000),
    ('${TENANT_ID}', 'G',   'Gram',       'g',   false, 'mass',   1),
    ('${TENANT_ID}', 'L',   'Litre',      'L',   true,  'volume', 1000),
    ('${TENANT_ID}', 'ML',  'Millilitre', 'mL',  false, 'volume', 1),
    ('${TENANT_ID}', 'BOT', 'Bottle',     'bot', false, 'count',  1),
    ('${TENANT_ID}', 'PLT', 'Plate',      'plt', false, 'count',  1)
ON CONFLICT (tenant_id, code) DO NOTHING;

-- Taxes (SL: 18% VAT, 10% Service Charge)
INSERT INTO taxes (tenant_id, code, name, rate_percent, is_inclusive, apply_on) VALUES
    ('${TENANT_ID}', 'VAT',  'VAT (18%)',          18.0000, false, 'line'),
    ('${TENANT_ID}', 'SVC',  'Service Charge 10%', 10.0000, false, 'service_charge'),
    ('${TENANT_ID}', 'EXMT', 'Exempt',              0.0000, false, 'line')
ON CONFLICT (tenant_id, code) DO NOTHING;

-- Categories (POS button colours echo the design system)
INSERT INTO categories (tenant_id, code, name, sort_order, color_hex, icon_name) VALUES
    ('${TENANT_ID}', 'KOTTU',   'Kottu',         10, '#F59E0B', 'lunch_dining'),
    ('${TENANT_ID}', 'RICE',    'Rice & Curry',  20, '#0F766E', 'rice_bowl'),
    ('${TENANT_ID}', 'CURRY',   'Curries',       30, '#1E40AF', 'soup_kitchen'),
    ('${TENANT_ID}', 'SIDES',   'Sides',         40, '#64748B', 'restaurant'),
    ('${TENANT_ID}', 'BEV',     'Beverages',     50, '#0EA5E9', 'local_bar'),
    ('${TENANT_ID}', 'DESSERT', 'Desserts',      60, '#EC4899', 'icecream')
ON CONFLICT (tenant_id, code) DO NOTHING;
SQL

# Resolve IDs for products
UOM_EA=$(psql -d "$DB_TENANT" -tAc "SELECT id FROM units_of_measure WHERE tenant_id='${TENANT_ID}' AND code='EA'")
UOM_PLT=$(psql -d "$DB_TENANT" -tAc "SELECT id FROM units_of_measure WHERE tenant_id='${TENANT_ID}' AND code='PLT'")
UOM_BOT=$(psql -d "$DB_TENANT" -tAc "SELECT id FROM units_of_measure WHERE tenant_id='${TENANT_ID}' AND code='BOT'")
TAX_VAT=$(psql -d "$DB_TENANT" -tAc "SELECT id FROM taxes             WHERE tenant_id='${TENANT_ID}' AND code='VAT'")
CAT_KOT=$(psql -d "$DB_TENANT" -tAc "SELECT id FROM categories        WHERE tenant_id='${TENANT_ID}' AND code='KOTTU'")
CAT_RICE=$(psql -d "$DB_TENANT" -tAc "SELECT id FROM categories       WHERE tenant_id='${TENANT_ID}' AND code='RICE'")
CAT_BEV=$(psql -d "$DB_TENANT" -tAc "SELECT id FROM categories        WHERE tenant_id='${TENANT_ID}' AND code='BEV'")
CAT_DES=$(psql -d "$DB_TENANT" -tAc "SELECT id FROM categories        WHERE tenant_id='${TENANT_ID}' AND code='DESSERT'")
CAT_SIDE=$(psql -d "$DB_TENANT" -tAc "SELECT id FROM categories       WHERE tenant_id='${TENANT_ID}' AND code='SIDES'")

psql -d "$DB_TENANT" -v ON_ERROR_STOP=1 <<SQL
-- Sample products (Sri Lankan menu)
INSERT INTO products (tenant_id, sku, name, category_id, unit_of_measure_id, base_price, cost_price, tax_id, sort_order, color_hex) VALUES
    -- Kottu
    ('${TENANT_ID}', 'KOTTU-CHK',  'Chicken Kottu',     '$CAT_KOT',  '$UOM_PLT', 900.00,  350.00, '$TAX_VAT', 10, '#F59E0B'),
    ('${TENANT_ID}', 'KOTTU-CHS',  'Cheese Kottu',      '$CAT_KOT',  '$UOM_PLT', 1050.00, 420.00, '$TAX_VAT', 20, '#F59E0B'),
    ('${TENANT_ID}', 'KOTTU-BF',   'Beef Kottu',        '$CAT_KOT',  '$UOM_PLT', 1150.00, 480.00, '$TAX_VAT', 30, '#F59E0B'),
    ('${TENANT_ID}', 'KOTTU-EGG',  'Egg Kottu',         '$CAT_KOT',  '$UOM_PLT', 750.00,  280.00, '$TAX_VAT', 40, '#F59E0B'),
    ('${TENANT_ID}', 'KOTTU-VEG',  'Vegetable Kottu',   '$CAT_KOT',  '$UOM_PLT', 650.00,  220.00, '$TAX_VAT', 50, '#F59E0B'),
    ('${TENANT_ID}', 'KOTTU-SF',   'Seafood Kottu',     '$CAT_KOT',  '$UOM_PLT', 1350.00, 580.00, '$TAX_VAT', 60, '#F59E0B'),
    -- Rice
    ('${TENANT_ID}', 'RICE-POL',   'Pol Sambol Rice',   '$CAT_RICE', '$UOM_PLT', 650.00,  220.00, '$TAX_VAT', 10, '#0F766E'),
    ('${TENANT_ID}', 'RICE-CHK',   'Chicken Rice',      '$CAT_RICE', '$UOM_PLT', 850.00,  340.00, '$TAX_VAT', 20, '#0F766E'),
    ('${TENANT_ID}', 'RICE-LAMP',  'Lamprais',          '$CAT_RICE', '$UOM_PLT', 1200.00, 520.00, '$TAX_VAT', 30, '#0F766E'),
    -- Sides
    ('${TENANT_ID}', 'SIDE-PAPAD', 'Papadam (3pcs)',    '$CAT_SIDE', '$UOM_EA',   150.00,  50.00, '$TAX_VAT', 10, '#64748B'),
    ('${TENANT_ID}', 'SIDE-MALLU', 'Gotukola Mallum',   '$CAT_SIDE', '$UOM_PLT',  280.00, 100.00, '$TAX_VAT', 20, '#64748B'),
    -- Beverages
    ('${TENANT_ID}', 'BEV-LION',   'Lion Lager 330ml',  '$CAT_BEV',  '$UOM_BOT',  450.00, 220.00, '$TAX_VAT', 10, '#0EA5E9'),
    ('${TENANT_ID}', 'BEV-COKE',   'Coca-Cola 330ml',   '$CAT_BEV',  '$UOM_BOT',  250.00,  90.00, '$TAX_VAT', 20, '#0EA5E9'),
    ('${TENANT_ID}', 'BEV-WATER',  'Mineral Water 500ml','$CAT_BEV', '$UOM_BOT',  150.00,  45.00, '$TAX_VAT', 30, '#0EA5E9'),
    ('${TENANT_ID}', 'BEV-TEA',    'Ceylon Tea',        '$CAT_BEV',  '$UOM_EA',   180.00,  40.00, '$TAX_VAT', 40, '#0EA5E9'),
    -- Desserts
    ('${TENANT_ID}', 'DES-WAT',    'Watalappam',        '$CAT_DES',  '$UOM_PLT',  450.00, 150.00, '$TAX_VAT', 10, '#EC4899'),
    ('${TENANT_ID}', 'DES-CURD',   'Curd & Treacle',    '$CAT_DES',  '$UOM_PLT',  350.00, 120.00, '$TAX_VAT', 20, '#EC4899')
ON CONFLICT (tenant_id, sku) DO NOTHING;
SQL

LOC_ID=$(psql -d "$DB_TENANT" -tAc "SELECT id FROM locations WHERE tenant_id='${TENANT_ID}' AND code='${DEMO_LOC_CODE}'")
psql -d "$DB_TENANT" -v ON_ERROR_STOP=1 <<SQL
-- Seed stock-on-hand for all products at the main location
INSERT INTO product_stock (tenant_id, product_id, location_id, quantity_on_hand, average_cost)
SELECT '${TENANT_ID}', p.id, '$LOC_ID', 50, p.cost_price
FROM products p
WHERE p.tenant_id='${TENANT_ID}'
ON CONFLICT (tenant_id, product_id, location_id) DO NOTHING;
SQL

echo "==> Seeding org settings + configurable tax charges (SL stack)"
psql -d "$DB_TENANT" -v ON_ERROR_STOP=1 <<SQL
-- Org settings (VAT reg, invoice prefix) — editable on the dashboard
INSERT INTO org_settings (tenant_id, legal_name, vat_registration_number, business_registration_no, vat_enabled, invoice_prefix, tax_invoice_footer)
VALUES ('${TENANT_ID}', 'Demo Restaurant (Pvt) Ltd', '134567890-7000', 'PV 00123456', true, 'INV', 'Thank you. This is a VAT tax invoice.')
ON CONFLICT (tenant_id) DO UPDATE SET legal_name=EXCLUDED.legal_name, vat_registration_number=EXCLUDED.vat_registration_number, updated_at=now();

-- Configurable compound tax stack: Service Charge 10% -> SSCL 2.5% (compound) -> VAT 18% (compound)
INSERT INTO tax_charges (tenant_id, code, name, charge_type, rate_percent, sequence, compound_on_previous, applies_to_takeaway, applies_to_delivery) VALUES
    ('${TENANT_ID}', 'SVC',  'Service Charge', 'service_charge', 10.0000, 1, false, false, false),
    ('${TENANT_ID}', 'SSCL', 'SSCL',           'levy',            2.5000, 2, true,  true,  true),
    ('${TENANT_ID}', 'VAT',  'VAT',            'vat',            18.0000, 3, true,  true,  true)
ON CONFLICT (tenant_id, code) DO UPDATE SET rate_percent=EXCLUDED.rate_percent, sequence=EXCLUDED.sequence, compound_on_previous=EXCLUDED.compound_on_previous;

-- Mark the main outlet as a sellable outlet; add an HQ + central kitchen for the multi-outlet demo
UPDATE locations SET location_type='outlet', can_sell=true, can_produce=false WHERE tenant_id='${TENANT_ID}' AND code='MAIN';
INSERT INTO locations (tenant_id, code, name, address_line1, city, country_code, currency, location_type, can_sell, can_produce, can_stock)
VALUES
    ('${TENANT_ID}', 'HQ',  'Head Office',     'No. 1, Galle Road', 'Colombo', 'LK', 'LKR', 'head_office',    false, false, false),
    ('${TENANT_ID}', 'CK',  'Central Kitchen', 'No. 5, Industrial Rd','Colombo','LK', 'LKR', 'central_kitchen', false, true,  true)
ON CONFLICT (tenant_id, code) DO NOTHING;

-- Category hierarchy: Beverages -> Fizzy / Alcohol / Hot
WITH bev AS (SELECT id FROM categories WHERE tenant_id='${TENANT_ID}' AND code='BEV')
INSERT INTO categories (tenant_id, parent_id, code, name, sort_order, color_hex, icon_name)
SELECT '${TENANT_ID}', bev.id, v.code, v.name, v.so, '#0EA5E9', v.icon
FROM bev, (VALUES
    ('BEV-FIZZY',   'Fizzy',   10, 'local_drink'),
    ('BEV-ALCOHOL', 'Alcohol', 20, 'sports_bar'),
    ('BEV-HOT',     'Hot',     30, 'coffee')
) AS v(code, name, so, icon)
ON CONFLICT (tenant_id, code) DO NOTHING;

-- Re-parent existing beverage products under the right sub-categories
UPDATE products SET category_id = (SELECT id FROM categories WHERE tenant_id='${TENANT_ID}' AND code='BEV-FIZZY')
    WHERE tenant_id='${TENANT_ID}' AND sku='BEV-COKE';
UPDATE products SET category_id = (SELECT id FROM categories WHERE tenant_id='${TENANT_ID}' AND code='BEV-ALCOHOL')
    WHERE tenant_id='${TENANT_ID}' AND sku='BEV-LION';
UPDATE products SET category_id = (SELECT id FROM categories WHERE tenant_id='${TENANT_ID}' AND code='BEV-HOT')
    WHERE tenant_id='${TENANT_ID}' AND sku='BEV-TEA';

-- Add Pepsi under Fizzy (the requested example)
INSERT INTO products (tenant_id, sku, name, category_id, unit_of_measure_id,
    base_price, cost_price, tax_id, sort_order, color_hex)
SELECT '${TENANT_ID}', 'BEV-PEPSI', 'Pepsi 330ml',
    (SELECT id FROM categories WHERE tenant_id='${TENANT_ID}' AND code='BEV-FIZZY'),
    (SELECT id FROM units_of_measure WHERE tenant_id='${TENANT_ID}' AND code='BOT'),
    250.00, 90.00, (SELECT id FROM taxes WHERE tenant_id='${TENANT_ID}' AND code='VAT'), 15, '#0EA5E9'
ON CONFLICT (tenant_id, sku) DO NOTHING;

-- Kitchen / printer stations: KOT lines route here per product (see migration 0022)
INSERT INTO kitchen_stations (tenant_id, code, name, printer_name, sort_order) VALUES
    ('${TENANT_ID}', 'KITCHEN', 'Hot Kitchen', 'kot-kitchen', 0),
    ('${TENANT_ID}', 'BAR',     'Bar',         'kot-bar',     1),
    ('${TENANT_ID}', 'DESSERT', 'Dessert',     'kot-dessert', 2)
ON CONFLICT (tenant_id, code) DO NOTHING;

-- Route drinks to the bar and desserts to the dessert station; everything else stays kitchen.
UPDATE products SET kitchen_station_code='BAR'
    WHERE tenant_id='${TENANT_ID}' AND category_id IN (
        SELECT id FROM categories WHERE tenant_id='${TENANT_ID}'
          AND code IN ('BEV','BEV-FIZZY','BEV-ALCOHOL','BEV-HOT'));
UPDATE products SET kitchen_station_code='DESSERT'
    WHERE tenant_id='${TENANT_ID}' AND category_id IN (
        SELECT id FROM categories WHERE tenant_id='${TENANT_ID}' AND code='DESSERT');
UPDATE products SET kitchen_station_code='KITCHEN'
    WHERE tenant_id='${TENANT_ID}' AND kitchen_station_code IS NULL AND is_sold = true;

-- Serving-size variants (#55b): Tea → Cup / Pot (one product, two price points)
INSERT INTO product_variants (tenant_id, product_id, code, name, price, sort_order)
SELECT '${TENANT_ID}', p.id, v.code, v.name, v.price, v.so
FROM products p, (VALUES ('CUP','Cup',150.0000,0),('POT','Pot',400.0000,1)) AS v(code,name,price,so)
WHERE p.tenant_id='${TENANT_ID}' AND p.sku='BEV-TEA'
ON CONFLICT (tenant_id, product_id, code) DO NOTHING;

-- Price levels (#55c): Dine-in (default) + Delivery (auto for delivery orders)
INSERT INTO price_levels (tenant_id, code, name, is_default, applies_to_order_type, sort_order) VALUES
    ('${TENANT_ID}','DINEIN','Dine-in', true,  NULL,       0),
    ('${TENANT_ID}','DELIVERY','Delivery', false,'delivery', 1)
ON CONFLICT (tenant_id, code) DO NOTHING;

-- Floor tables (#68): a small demo floor across two areas
INSERT INTO restaurant_tables (tenant_id, location_id, code, seats, area, sort_order)
SELECT '${TENANT_ID}', l.id, v.code, v.seats, v.area, v.so
FROM locations l, (VALUES
    ('T1',2,'Main',1),('T2',2,'Main',2),('T3',4,'Main',3),('T4',4,'Main',4),
    ('G1',6,'Garden',5),('G2',6,'Garden',6),('BAR1',1,'Bar',7),('BAR2',1,'Bar',8)
) AS v(code,seats,area,so)
WHERE l.tenant_id='${TENANT_ID}' AND l.code='${DEMO_LOC_CODE}'
ON CONFLICT (tenant_id, location_id, code) DO NOTHING;
SQL

# Stock for the new product
LOC_ID2=$(psql -d "$DB_TENANT" -tAc "SELECT id FROM locations WHERE tenant_id='${TENANT_ID}' AND code='${DEMO_LOC_CODE}'")
psql -d "$DB_TENANT" -v ON_ERROR_STOP=1 <<SQL
INSERT INTO product_stock (tenant_id, product_id, location_id, quantity_on_hand, average_cost)
SELECT '${TENANT_ID}', p.id, '$LOC_ID2', 50, p.cost_price
FROM products p WHERE p.tenant_id='${TENANT_ID}' AND p.sku='BEV-PEPSI'
ON CONFLICT (tenant_id, product_id, location_id) DO NOTHING;
SQL

echo "==> Done"
echo ""
echo "  Tenant slug: ${DEMO_SLUG}"
echo "  Owner email: ${DEMO_OWNER}"
echo "  Login at:    http://localhost:3000/login"
echo ""
echo "  Seeded: 7 UoMs · 3 product taxes · 9 categories (nested) · 18 products · stock"
echo "          3 tenants locations (Main outlet, HQ, Central Kitchen)"
echo "          org settings (VAT reg) + 3 configurable tax charges (SVC/SSCL/VAT)"
echo "          3 kitchen stations (Hot Kitchen / Bar / Dessert) + product routing"
echo "          serving-size variants (Tea → Cup / Pot)"
echo "          price levels (Dine-in default + Delivery)"
