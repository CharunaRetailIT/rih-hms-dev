-- =============================================================================
-- Migration 0054: Charge refactor — replaces `taxes` + `tax_charges` with a
-- unified ChargeType (category: Tax, Service Charge, Levy, A/C Cost, ...) +
-- Charge (a % rate or flat amount) model. Charges whose type is flagged
-- applies_per_product get assigned to specific products (a product may carry
-- several, e.g. VAT + a luxury tax) via product_charges; all other charge
-- types stay tenant-wide and apply to every order, replacing the old
-- tax_charges bill-level stack (now without sequencing/compounding — every
-- active tenant-wide charge just sums independently off the subtotal).
--
-- Pre-launch system — no live tenant data to preserve, so the old tables are
-- dropped outright rather than migrated.
-- =============================================================================

CREATE TABLE IF NOT EXISTS charge_types (
    id                  uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id           uuid        NOT NULL,
    code                varchar(20) NOT NULL,
    name                varchar(80) NOT NULL,
    applies_per_product boolean     NOT NULL DEFAULT false,
    sort_order          int         NOT NULL DEFAULT 0,
    is_active           boolean     NOT NULL DEFAULT true,
    created_at          timestamptz NOT NULL DEFAULT now(),
    updated_at          timestamptz NOT NULL DEFAULT now(),
    created_by          uuid        NULL,
    updated_by          uuid        NULL,
    is_deleted          boolean     NOT NULL DEFAULT false,
    CONSTRAINT uq_charge_types_tenant_code UNIQUE (tenant_id, code)
);
CREATE INDEX IF NOT EXISTS ix_charge_types_tenant ON charge_types(tenant_id);

CREATE TABLE IF NOT EXISTS charges (
    id             uuid           PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id      uuid           NOT NULL,
    charge_type_id uuid           NOT NULL REFERENCES charge_types(id),
    code           varchar(20)    NOT NULL,
    description    varchar(200)   NOT NULL,
    percentage     numeric(8, 4)  NULL,
    amount         numeric(15, 4) NULL,
    is_active      boolean        NOT NULL DEFAULT true,
    created_at     timestamptz    NOT NULL DEFAULT now(),
    updated_at     timestamptz    NOT NULL DEFAULT now(),
    created_by     uuid           NULL,
    updated_by     uuid           NULL,
    is_deleted     boolean        NOT NULL DEFAULT false,
    CONSTRAINT uq_charges_tenant_code UNIQUE (tenant_id, code)
);
CREATE INDEX IF NOT EXISTS ix_charges_tenant      ON charges(tenant_id);
CREATE INDEX IF NOT EXISTS ix_charges_charge_type ON charges(charge_type_id);

CREATE TABLE IF NOT EXISTS product_charges (
    id          uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id   uuid        NOT NULL,
    product_id  uuid        NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    charge_id   uuid        NOT NULL REFERENCES charges(id) ON DELETE CASCADE,
    created_at  timestamptz NOT NULL DEFAULT now(),
    updated_at  timestamptz NOT NULL DEFAULT now(),
    created_by  uuid        NULL,
    updated_by  uuid        NULL,
    is_deleted  boolean     NOT NULL DEFAULT false,
    CONSTRAINT uq_product_charge UNIQUE (tenant_id, product_id, charge_id)
);
CREATE INDEX IF NOT EXISTS ix_product_charges_tenant  ON product_charges(tenant_id);
CREATE INDEX IF NOT EXISTS ix_product_charges_product ON product_charges(product_id);
CREATE INDEX IF NOT EXISTS ix_product_charges_charge  ON product_charges(charge_id);

CREATE TABLE IF NOT EXISTS order_item_charges (
    id             uuid           PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id      uuid           NOT NULL,
    order_item_id  uuid           NOT NULL REFERENCES order_items(id) ON DELETE CASCADE,
    code           varchar(20)    NOT NULL,
    description    varchar(200)   NOT NULL,
    percentage     numeric(8, 4)  NULL,
    amount         numeric(15, 4) NULL,
    base_amount    numeric(15, 4) NOT NULL,
    charge_amount  numeric(15, 4) NOT NULL,
    created_at     timestamptz    NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_order_item_charges_order_item ON order_item_charges(order_item_id);

-- RLS on the new tables
ALTER TABLE charge_types      ENABLE ROW LEVEL SECURITY;
ALTER TABLE charges           ENABLE ROW LEVEL SECURITY;
ALTER TABLE product_charges   ENABLE ROW LEVEL SECURITY;
ALTER TABLE order_item_charges ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS p_charge_types_tenant       ON charge_types;
DROP POLICY IF EXISTS p_charges_tenant            ON charges;
DROP POLICY IF EXISTS p_product_charges_tenant    ON product_charges;
DROP POLICY IF EXISTS p_order_item_charges_tenant ON order_item_charges;

CREATE POLICY p_charge_types_tenant       ON charge_types       USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_charges_tenant            ON charges            USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_product_charges_tenant    ON product_charges    USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_order_item_charges_tenant ON order_item_charges USING (tenant_id::text = current_setting('app.tenant_id', true));

-- Sequence no longer means anything — bill-level charges now sum independently, no stacking order.
ALTER TABLE order_charges DROP COLUMN IF EXISTS sequence;

-- Drop the old single-tax-per-product FK before dropping its target table.
ALTER TABLE products DROP COLUMN IF EXISTS tax_id;

DROP TABLE IF EXISTS tax_charges;
DROP TABLE IF EXISTS taxes;
