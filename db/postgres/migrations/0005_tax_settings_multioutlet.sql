-- =============================================================================
-- Migration 0005: Configurable tax engine + org settings + multi-outlet + invoices
-- Target DB: any hms_tenant_<slug>
-- Run with:  psql -d hms_tenant_demo -f db/postgres/migrations/0005_tax_settings_multioutlet.sql
-- =============================================================================

-- -----------------------------------------------------------------------------
-- org_settings — one row per tenant. The ERP dashboard edits this.
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS org_settings (
    tenant_id                  uuid          PRIMARY KEY,
    legal_name                 varchar(200)  NULL,
    vat_registration_number    varchar(40)   NULL,
    business_registration_no   varchar(40)   NULL,
    vat_enabled                boolean       NOT NULL DEFAULT true,
    invoice_prefix             varchar(10)   NOT NULL DEFAULT 'INV',
    tax_invoice_footer         text          NULL,
    base_currency              char(3)       NOT NULL DEFAULT 'LKR',
    vat_filing_frequency       varchar(20)   NOT NULL DEFAULT 'monthly', -- monthly | quarterly
    created_at                 timestamptz   NOT NULL DEFAULT now(),
    updated_at                 timestamptz   NOT NULL DEFAULT now()
);

-- -----------------------------------------------------------------------------
-- tax_charges — configurable, ordered, compounding bill-level charges/taxes.
-- This REPLACES the hardcoded service charge. Nothing is hardcoded now.
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS tax_charges (
    id                    uuid           PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id             uuid           NOT NULL,
    code                  varchar(20)    NOT NULL,
    name                  varchar(80)    NOT NULL,    -- shown on the bill, e.g. "Service Charge"
    charge_type           varchar(20)    NOT NULL,    -- service_charge | levy | vat
    rate_percent          numeric(8, 4)  NOT NULL,
    sequence              int            NOT NULL DEFAULT 0,   -- application order
    compound_on_previous  boolean        NOT NULL DEFAULT false, -- base = running total vs subtotal
    applies_to_takeaway   boolean        NOT NULL DEFAULT true,
    applies_to_delivery   boolean        NOT NULL DEFAULT true,
    is_active             boolean        NOT NULL DEFAULT true,
    created_at            timestamptz    NOT NULL DEFAULT now(),
    updated_at            timestamptz    NOT NULL DEFAULT now(),
    created_by            uuid           NULL,
    updated_by            uuid           NULL,
    is_deleted            boolean        NOT NULL DEFAULT false,
    CONSTRAINT uq_tax_charges_tenant_code UNIQUE (tenant_id, code)
);
CREATE INDEX IF NOT EXISTS ix_tax_charges_tenant ON tax_charges(tenant_id, sequence) WHERE is_active AND NOT is_deleted;

COMMENT ON TABLE tax_charges IS 'Configurable bill-level charges/taxes applied in sequence. SL stack: Service Charge → SSCL → VAT (compounding).';

-- -----------------------------------------------------------------------------
-- order_charges — computed breakdown snapshot per order (for the VAT invoice
-- and the VAT return). One row per charge applied to an order.
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS order_charges (
    id              uuid           PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id       uuid           NOT NULL,
    order_id        uuid           NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    code            varchar(20)    NOT NULL,
    name            varchar(80)    NOT NULL,
    charge_type     varchar(20)    NOT NULL,
    rate_percent    numeric(8, 4)  NOT NULL,
    base_amount     numeric(15, 4) NOT NULL,   -- what the rate was applied to
    charge_amount   numeric(15, 4) NOT NULL,
    sequence        int            NOT NULL DEFAULT 0,
    created_at      timestamptz    NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_order_charges_order ON order_charges(order_id);

-- -----------------------------------------------------------------------------
-- invoice_series — gap-free sequential invoice numbers (SL audit requirement)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS invoice_series (
    tenant_id    uuid    NOT NULL,
    series_year  int     NOT NULL,
    last_value   int     NOT NULL DEFAULT 0,
    PRIMARY KEY (tenant_id, series_year)
);

-- -----------------------------------------------------------------------------
-- orders: VAT-invoice columns
-- -----------------------------------------------------------------------------
ALTER TABLE orders ADD COLUMN IF NOT EXISTS invoice_number   varchar(30) NULL;
ALTER TABLE orders ADD COLUMN IF NOT EXISTS is_tax_invoice   boolean NOT NULL DEFAULT false;
ALTER TABLE orders ADD COLUMN IF NOT EXISTS customer_vat_no  varchar(40) NULL;
ALTER TABLE orders ADD COLUMN IF NOT EXISTS customer_name    varchar(200) NULL;
CREATE UNIQUE INDEX IF NOT EXISTS uq_orders_invoice ON orders(tenant_id, invoice_number) WHERE invoice_number IS NOT NULL;

-- -----------------------------------------------------------------------------
-- locations: multi-outlet type + capabilities
-- -----------------------------------------------------------------------------
ALTER TABLE locations ADD COLUMN IF NOT EXISTS location_type varchar(20) NOT NULL DEFAULT 'outlet';
    -- head_office | central_kitchen | warehouse | outlet
ALTER TABLE locations ADD COLUMN IF NOT EXISTS can_sell      boolean NOT NULL DEFAULT true;
ALTER TABLE locations ADD COLUMN IF NOT EXISTS can_produce   boolean NOT NULL DEFAULT false;
ALTER TABLE locations ADD COLUMN IF NOT EXISTS can_stock     boolean NOT NULL DEFAULT true;
ALTER TABLE locations ADD COLUMN IF NOT EXISTS vat_registration_number varchar(40) NULL; -- per-outlet override

-- -----------------------------------------------------------------------------
-- user_locations — which locations a user can access (org-wide if no rows)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS user_locations (
    tenant_id    uuid NOT NULL,
    user_id      uuid NOT NULL,
    location_id  uuid NOT NULL,
    PRIMARY KEY (tenant_id, user_id, location_id)
);

-- =============================================================================
-- RLS
-- =============================================================================
ALTER TABLE org_settings    ENABLE ROW LEVEL SECURITY;
ALTER TABLE tax_charges     ENABLE ROW LEVEL SECURITY;
ALTER TABLE order_charges   ENABLE ROW LEVEL SECURITY;
ALTER TABLE invoice_series  ENABLE ROW LEVEL SECURITY;
ALTER TABLE user_locations  ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS p_org_settings_tenant   ON org_settings;
DROP POLICY IF EXISTS p_tax_charges_tenant     ON tax_charges;
DROP POLICY IF EXISTS p_order_charges_tenant   ON order_charges;
DROP POLICY IF EXISTS p_invoice_series_tenant  ON invoice_series;
DROP POLICY IF EXISTS p_user_locations_tenant  ON user_locations;

CREATE POLICY p_org_settings_tenant  ON org_settings
    USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_tax_charges_tenant   ON tax_charges
    USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_order_charges_tenant ON order_charges
    USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_invoice_series_tenant ON invoice_series
    USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_user_locations_tenant ON user_locations
    USING (tenant_id::text = current_setting('app.tenant_id', true));
