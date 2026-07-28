-- =============================================================================
-- Migration 0051: Catch up SQL schema with the product refactor (flags,
-- variants, location-wise pricing, multiple suppliers, departments, serving
-- units, printer types). These tables/columns were added to the EF model but
-- never got a migration, so new tenant databases were missing them entirely.
-- =============================================================================

-- -----------------------------------------------------------------------------
-- departments — groups products / reports / permissions by business area
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS departments (
    id             uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id      uuid        NOT NULL,
    location_id    uuid        NULL REFERENCES locations(id),
    code           varchar(50) NOT NULL,
    name           varchar(150) NOT NULL,
    remark         varchar(500) NULL,
    dashboard_color varchar(20) NULL,
    is_active      boolean     NOT NULL DEFAULT true,
    created_at     timestamptz NOT NULL DEFAULT now(),
    updated_at     timestamptz NOT NULL DEFAULT now(),
    created_by     uuid        NULL,
    updated_by     uuid        NULL,
    is_deleted     boolean     NOT NULL DEFAULT false
);
CREATE UNIQUE INDEX IF NOT EXISTS uq_departments_tenant_code ON departments(tenant_id, code) WHERE NOT is_deleted;
CREATE INDEX IF NOT EXISTS ix_departments_tenant ON departments(tenant_id);

-- -----------------------------------------------------------------------------
-- printer_types — KOT / BOT / None, what a kitchen station's printer prints
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS printer_types (
    id          uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id   uuid        NOT NULL,
    code        varchar(20) NOT NULL,
    name        varchar(80) NOT NULL,
    sort_order  int         NOT NULL DEFAULT 0,
    is_active   boolean     NOT NULL DEFAULT true,
    created_at  timestamptz NOT NULL DEFAULT now(),
    updated_at  timestamptz NOT NULL DEFAULT now(),
    created_by  uuid        NULL,
    updated_by  uuid        NULL,
    is_deleted  boolean     NOT NULL DEFAULT false,
    CONSTRAINT uq_printer_type_code UNIQUE (tenant_id, code)
);
CREATE INDEX IF NOT EXISTS ix_printer_types_tenant ON printer_types(tenant_id);

-- -----------------------------------------------------------------------------
-- serving_units — Cup / Pot / Glass etc. used by product variants
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS serving_units (
    id          uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id   uuid        NOT NULL,
    location_id uuid        NULL REFERENCES locations(id),
    code        varchar(20) NOT NULL,
    name        varchar(60) NOT NULL,
    sort_order  int         NOT NULL DEFAULT 0,
    is_active   boolean     NOT NULL DEFAULT true,
    created_at  timestamptz NOT NULL DEFAULT now(),
    updated_at  timestamptz NOT NULL DEFAULT now(),
    created_by  uuid        NULL,
    updated_by  uuid        NULL,
    is_deleted  boolean     NOT NULL DEFAULT false,
    CONSTRAINT uq_serving_unit_code UNIQUE (tenant_id, location_id, code)
);
CREATE INDEX IF NOT EXISTS ix_serving_units_tenant_location ON serving_units(tenant_id, location_id);

-- -----------------------------------------------------------------------------
-- product_suppliers — a product can be sourced from more than one supplier
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS product_suppliers (
    id           uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id    uuid        NOT NULL,
    product_id   uuid        NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    supplier_id  uuid        NOT NULL REFERENCES suppliers(id),
    is_preferred boolean     NOT NULL DEFAULT false,
    is_active    boolean     NOT NULL DEFAULT true,
    created_at   timestamptz NOT NULL DEFAULT now(),
    updated_at   timestamptz NOT NULL DEFAULT now(),
    created_by   uuid        NULL,
    updated_by   uuid        NULL,
    is_deleted   boolean     NOT NULL DEFAULT false,
    CONSTRAINT uq_product_supplier UNIQUE (tenant_id, product_id, supplier_id)
);
CREATE INDEX IF NOT EXISTS ix_product_suppliers_tenant   ON product_suppliers(tenant_id);
CREATE INDEX IF NOT EXISTS ix_product_suppliers_product  ON product_suppliers(product_id);
CREATE INDEX IF NOT EXISTS ix_product_suppliers_supplier ON product_suppliers(supplier_id);

-- -----------------------------------------------------------------------------
-- products — new flags, location/department scoping, discount rules, reorder,
-- kitchen station + preferred supplier links
-- -----------------------------------------------------------------------------
ALTER TABLE products ADD COLUMN IF NOT EXISTS location_id            uuid           NULL;
ALTER TABLE products ADD COLUMN IF NOT EXISTS department_id          uuid           NULL;
ALTER TABLE products ADD COLUMN IF NOT EXISTS name_on_invoice        varchar(200)   NULL;
ALTER TABLE products ADD COLUMN IF NOT EXISTS ref_code_01            varchar(80)    NULL;
ALTER TABLE products ADD COLUMN IF NOT EXISTS ref_code_02            varchar(80)    NULL;
ALTER TABLE products ADD COLUMN IF NOT EXISTS is_tax_inclusive       boolean        NOT NULL DEFAULT false;
ALTER TABLE products ADD COLUMN IF NOT EXISTS product_type           varchar(30)    NOT NULL DEFAULT 'finished';
ALTER TABLE products ADD COLUMN IF NOT EXISTS is_scale_item          boolean        NOT NULL DEFAULT false;
ALTER TABLE products ADD COLUMN IF NOT EXISTS is_open_item           boolean        NOT NULL DEFAULT false;
ALTER TABLE products ADD COLUMN IF NOT EXISTS is_locked              boolean        NOT NULL DEFAULT false;
ALTER TABLE products ADD COLUMN IF NOT EXISTS tracks_expiry          boolean        NOT NULL DEFAULT false;
ALTER TABLE products ADD COLUMN IF NOT EXISTS allow_below_cost       boolean        NOT NULL DEFAULT false;
ALTER TABLE products ADD COLUMN IF NOT EXISTS allow_discount         boolean        NOT NULL DEFAULT true;
ALTER TABLE products ADD COLUMN IF NOT EXISTS discount_type          varchar(20)    NOT NULL DEFAULT 'none';
ALTER TABLE products ADD COLUMN IF NOT EXISTS discount_value         numeric(18,4)  NOT NULL DEFAULT 0;
ALTER TABLE products ADD COLUMN IF NOT EXISTS max_discount_amount    numeric(18,4)  NULL;
ALTER TABLE products ADD COLUMN IF NOT EXISTS max_discount_percentage numeric(18,4) NULL;
ALTER TABLE products ADD COLUMN IF NOT EXISTS reorder_quantity       numeric(18,4)  NULL;
ALTER TABLE products ADD COLUMN IF NOT EXISTS par_level              numeric(18,4)  NULL;
ALTER TABLE products ADD COLUMN IF NOT EXISTS kitchen_station_id     uuid           NULL;
ALTER TABLE products ADD COLUMN IF NOT EXISTS preferred_supplier_id  uuid           NULL;

CREATE INDEX IF NOT EXISTS ix_products_location         ON products(location_id) WHERE location_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS ix_products_department        ON products(department_id) WHERE department_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS ix_products_kitchen_station   ON products(kitchen_station_id) WHERE kitchen_station_id IS NOT NULL;

-- -----------------------------------------------------------------------------
-- price_levels — per-location price lists (not just per-channel)
-- -----------------------------------------------------------------------------
ALTER TABLE price_levels ADD COLUMN IF NOT EXISTS location_id uuid NULL;

CREATE UNIQUE INDEX IF NOT EXISTS uq_price_levels_tenant_location_code
    ON price_levels(tenant_id, location_id, code);
CREATE INDEX IF NOT EXISTS ix_price_levels_tenant_location ON price_levels(tenant_id, location_id);

-- -----------------------------------------------------------------------------
-- product_variants — recipe-deduction flag, serving qty/unit, per-variant cost
-- -----------------------------------------------------------------------------
ALTER TABLE product_variants ADD COLUMN IF NOT EXISTS serving_unit_id        uuid          NULL;
ALTER TABLE product_variants ADD COLUMN IF NOT EXISTS deduct_stock_on_recipe boolean       NOT NULL DEFAULT true;
ALTER TABLE product_variants ADD COLUMN IF NOT EXISTS serving_qty            numeric(18,4) NOT NULL DEFAULT 1;
ALTER TABLE product_variants ADD COLUMN IF NOT EXISTS cost_price             numeric(18,4) NOT NULL DEFAULT 0;

CREATE INDEX IF NOT EXISTS ix_product_variants_serving_unit ON product_variants(serving_unit_id) WHERE serving_unit_id IS NOT NULL;

-- -----------------------------------------------------------------------------
-- product_prices — per-location overrides and per-variant overrides, cost price
-- -----------------------------------------------------------------------------
ALTER TABLE product_prices ADD COLUMN IF NOT EXISTS location_id        uuid          NULL;
ALTER TABLE product_prices ADD COLUMN IF NOT EXISTS product_variant_id uuid          NULL;
ALTER TABLE product_prices ADD COLUMN IF NOT EXISTS cost_price         numeric(18,4) NOT NULL DEFAULT 0;

CREATE INDEX IF NOT EXISTS ix_product_prices_location ON product_prices(location_id) WHERE location_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS ix_product_prices_variant  ON product_prices(product_variant_id) WHERE product_variant_id IS NOT NULL;

-- Old constraint didn't account for location/variant scoping — replace it.
ALTER TABLE product_prices DROP CONSTRAINT IF EXISTS uq_product_price;
ALTER TABLE product_prices ADD CONSTRAINT uq_product_price
    UNIQUE (tenant_id, location_id, product_id, product_variant_id, price_level_id);

-- -----------------------------------------------------------------------------
-- kitchen_stations — per-location scoping + which printer type it routes to
-- -----------------------------------------------------------------------------
ALTER TABLE kitchen_stations ADD COLUMN IF NOT EXISTS location_id     uuid NULL;
ALTER TABLE kitchen_stations ADD COLUMN IF NOT EXISTS printer_type_id uuid NULL;

CREATE INDEX IF NOT EXISTS ix_kitchen_stations_location      ON kitchen_stations(location_id) WHERE location_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS ix_kitchen_stations_printer_type  ON kitchen_stations(printer_type_id) WHERE printer_type_id IS NOT NULL;

-- Backfill any pre-existing stations (older, already-provisioned tenant DBs)
-- with a default "KOT" printer type before enforcing NOT NULL below. No-op on
-- a freshly provisioned (empty) tenant DB.
DO $$
DECLARE
    r RECORD;
    kot_id uuid;
BEGIN
    FOR r IN SELECT DISTINCT tenant_id FROM kitchen_stations WHERE printer_type_id IS NULL LOOP
        INSERT INTO printer_types (tenant_id, code, name, sort_order)
        VALUES (r.tenant_id, 'KOT', 'Kitchen Order Ticket', 1)
        ON CONFLICT (tenant_id, code) DO NOTHING;

        SELECT id INTO kot_id FROM printer_types WHERE tenant_id = r.tenant_id AND code = 'KOT';

        UPDATE kitchen_stations
        SET printer_type_id = kot_id
        WHERE tenant_id = r.tenant_id AND printer_type_id IS NULL;
    END LOOP;
END $$;

ALTER TABLE kitchen_stations ALTER COLUMN printer_type_id SET NOT NULL;

-- RLS on the new tables
ALTER TABLE departments        ENABLE ROW LEVEL SECURITY;
ALTER TABLE printer_types      ENABLE ROW LEVEL SECURITY;
ALTER TABLE serving_units      ENABLE ROW LEVEL SECURITY;
ALTER TABLE product_suppliers  ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS p_departments_tenant       ON departments;
DROP POLICY IF EXISTS p_printer_types_tenant     ON printer_types;
DROP POLICY IF EXISTS p_serving_units_tenant     ON serving_units;
DROP POLICY IF EXISTS p_product_suppliers_tenant ON product_suppliers;

CREATE POLICY p_departments_tenant       ON departments       USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_printer_types_tenant     ON printer_types     USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_serving_units_tenant     ON serving_units     USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_product_suppliers_tenant ON product_suppliers USING (tenant_id::text = current_setting('app.tenant_id', true));
