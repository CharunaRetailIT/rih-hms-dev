-- =============================================================================
-- Migration 0003: Master data — categories, products, units, taxes
-- Target DB: any hms_tenant_<slug>
-- Run with:  psql -d hms_tenant_demo -f db/postgres/migrations/0003_master_data.sql
--
-- Foundation for the Sprint 3 POS work. v1 keeps the model deliberately
-- narrow — full PO/GRN/stock-batch workflow is deferred to v2.
-- =============================================================================

-- -----------------------------------------------------------------------------
-- units_of_measure — reference data (could live in the control plane and be
-- replicated, but at v1 scale we just seed every tenant DB)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS units_of_measure (
    id            uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id     uuid          NOT NULL,
    code          varchar(20)   NOT NULL,
    name          varchar(80)   NOT NULL,
    symbol        varchar(10)   NULL,         -- "kg", "L", "ea"
    is_base_unit  boolean       NOT NULL DEFAULT false,
    created_at    timestamptz   NOT NULL DEFAULT now(),
    updated_at    timestamptz   NOT NULL DEFAULT now(),
    created_by    uuid          NULL,
    updated_by    uuid          NULL,
    is_deleted    boolean       NOT NULL DEFAULT false,
    CONSTRAINT uq_uom_tenant_code UNIQUE (tenant_id, code)
);
CREATE INDEX IF NOT EXISTS ix_uom_tenant ON units_of_measure(tenant_id);

COMMENT ON TABLE units_of_measure IS 'Units used for stock and recipe quantities (kg, L, ea, etc.).';

-- -----------------------------------------------------------------------------
-- taxes — VAT, GST, service charge, etc. (Sri Lanka has compound stacking;
-- v1 supports a single tax per product; compound engine lands in v2)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS taxes (
    id              uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id       uuid          NOT NULL,
    code            varchar(20)   NOT NULL,
    name            varchar(80)   NOT NULL,
    rate_percent    numeric(8, 4) NOT NULL,    -- 18.0000 = 18%
    is_active       boolean       NOT NULL DEFAULT true,
    is_inclusive    boolean       NOT NULL DEFAULT false,  -- if true, price already includes this tax
    apply_on        varchar(20)   NOT NULL DEFAULT 'line', -- 'line' | 'bill' | 'service_charge'
    created_at      timestamptz   NOT NULL DEFAULT now(),
    updated_at      timestamptz   NOT NULL DEFAULT now(),
    created_by      uuid          NULL,
    updated_by      uuid          NULL,
    is_deleted      boolean       NOT NULL DEFAULT false,
    CONSTRAINT uq_taxes_tenant_code UNIQUE (tenant_id, code)
);
CREATE INDEX IF NOT EXISTS ix_taxes_tenant ON taxes(tenant_id);

COMMENT ON TABLE taxes IS 'v1: a product references at most one tax. v2 introduces compound tax stacking.';

-- -----------------------------------------------------------------------------
-- categories — menu/product categories with simple hierarchy (parent_id)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS categories (
    id              uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id       uuid          NOT NULL,
    parent_id       uuid          NULL REFERENCES categories(id),
    code            varchar(40)   NOT NULL,
    name            varchar(120)  NOT NULL,
    description     text          NULL,
    sort_order      int           NOT NULL DEFAULT 0,
    is_active       boolean       NOT NULL DEFAULT true,
    color_hex       char(7)       NULL,        -- POS button colour, e.g. "#0F766E"
    icon_name       varchar(60)   NULL,        -- material symbol name, e.g. "lunch_dining"
    created_at      timestamptz   NOT NULL DEFAULT now(),
    updated_at      timestamptz   NOT NULL DEFAULT now(),
    created_by      uuid          NULL,
    updated_by      uuid          NULL,
    is_deleted      boolean       NOT NULL DEFAULT false,
    CONSTRAINT uq_categories_tenant_code UNIQUE (tenant_id, code)
);
CREATE INDEX IF NOT EXISTS ix_categories_tenant   ON categories(tenant_id);
CREATE INDEX IF NOT EXISTS ix_categories_parent   ON categories(parent_id) WHERE parent_id IS NOT NULL;

-- -----------------------------------------------------------------------------
-- products — menu items
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS products (
    id                  uuid           PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id           uuid           NOT NULL,
    category_id         uuid           NULL REFERENCES categories(id),
    sku                 varchar(40)    NOT NULL,
    barcode             varchar(60)    NULL,
    name                varchar(200)   NOT NULL,
    description         text           NULL,
    unit_of_measure_id  uuid           NOT NULL REFERENCES units_of_measure(id),
    base_price          numeric(15, 4) NOT NULL DEFAULT 0,
    cost_price          numeric(15, 4) NOT NULL DEFAULT 0,
    tax_id              uuid           NULL REFERENCES taxes(id),
    is_taxable          boolean        NOT NULL DEFAULT true,
    is_sold             boolean        NOT NULL DEFAULT true,    -- can be sold from POS
    is_purchased        boolean        NOT NULL DEFAULT true,    -- can be purchased from suppliers
    is_stocked          boolean        NOT NULL DEFAULT true,    -- track stock-on-hand
    is_active           boolean        NOT NULL DEFAULT true,
    reorder_level       numeric(15, 4) NULL,
    color_hex           char(7)        NULL,                     -- POS button colour
    image_url           varchar(500)   NULL,
    sort_order          int            NOT NULL DEFAULT 0,
    created_at          timestamptz    NOT NULL DEFAULT now(),
    updated_at          timestamptz    NOT NULL DEFAULT now(),
    created_by          uuid           NULL,
    updated_by          uuid           NULL,
    is_deleted          boolean        NOT NULL DEFAULT false,
    CONSTRAINT uq_products_tenant_sku UNIQUE (tenant_id, sku)
);
CREATE INDEX IF NOT EXISTS ix_products_tenant            ON products(tenant_id);
CREATE INDEX IF NOT EXISTS ix_products_category          ON products(category_id) WHERE category_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS ix_products_active_sold       ON products(tenant_id) WHERE is_active AND is_sold AND NOT is_deleted;
CREATE INDEX IF NOT EXISTS ix_products_barcode           ON products(tenant_id, barcode) WHERE barcode IS NOT NULL;

COMMENT ON TABLE products IS 'Menu items / sellable + stockable products. v1 narrow model — modifiers, recipes, batch tracking are Sprint 3+.';

-- -----------------------------------------------------------------------------
-- product_stock — per-location stock-on-hand
-- A product can have one stock row per location. v1 uses simple stock count;
-- v2 adds batch/lot tracking, expiry, serial numbers.
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS product_stock (
    id                  uuid           PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id           uuid           NOT NULL,
    product_id          uuid           NOT NULL REFERENCES products(id),
    location_id         uuid           NOT NULL REFERENCES locations(id),
    quantity_on_hand    numeric(15, 4) NOT NULL DEFAULT 0,
    average_cost        numeric(15, 4) NOT NULL DEFAULT 0,
    last_received_at    timestamptz    NULL,
    last_counted_at     timestamptz    NULL,
    created_at          timestamptz    NOT NULL DEFAULT now(),
    updated_at          timestamptz    NOT NULL DEFAULT now(),
    created_by          uuid           NULL,
    updated_by          uuid           NULL,
    is_deleted          boolean        NOT NULL DEFAULT false,
    CONSTRAINT uq_product_stock_unique UNIQUE (tenant_id, product_id, location_id)
);
CREATE INDEX IF NOT EXISTS ix_product_stock_product  ON product_stock(product_id);
CREATE INDEX IF NOT EXISTS ix_product_stock_location ON product_stock(location_id);

-- =============================================================================
-- Row-Level Security (defence-in-depth)
-- =============================================================================
ALTER TABLE units_of_measure ENABLE ROW LEVEL SECURITY;
ALTER TABLE taxes             ENABLE ROW LEVEL SECURITY;
ALTER TABLE categories        ENABLE ROW LEVEL SECURITY;
ALTER TABLE products          ENABLE ROW LEVEL SECURITY;
ALTER TABLE product_stock     ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS p_uom_tenant            ON units_of_measure;
DROP POLICY IF EXISTS p_taxes_tenant          ON taxes;
DROP POLICY IF EXISTS p_categories_tenant     ON categories;
DROP POLICY IF EXISTS p_products_tenant       ON products;
DROP POLICY IF EXISTS p_product_stock_tenant  ON product_stock;

CREATE POLICY p_uom_tenant           ON units_of_measure
    USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_taxes_tenant         ON taxes
    USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_categories_tenant    ON categories
    USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_products_tenant      ON products
    USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_product_stock_tenant ON product_stock
    USING (tenant_id::text = current_setting('app.tenant_id', true));
