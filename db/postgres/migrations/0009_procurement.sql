-- =============================================================================
-- Migration 0009: Procurement — suppliers, purchase orders, goods-received notes
-- Supply chain step 5. PO → GRN receives stock and updates weighted-average cost.
-- Target DB: any hms_tenant_<slug>
-- =============================================================================

-- Generic per-type document counter (PO-0001, GRN-0001, …). Not date-scoped.
CREATE TABLE IF NOT EXISTS document_counters (
    tenant_id  uuid        NOT NULL,
    doc_type   varchar(20) NOT NULL,
    last_value int         NOT NULL DEFAULT 0,
    PRIMARY KEY (tenant_id, doc_type)
);

CREATE TABLE IF NOT EXISTS suppliers (
    id                uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id         uuid          NOT NULL,
    code              varchar(30)   NOT NULL,
    name              varchar(200)  NOT NULL,
    contact_name      varchar(120)  NULL,
    phone             varchar(40)   NULL,
    email             varchar(255)  NULL,
    address           varchar(500)  NULL,
    payment_terms_days int          NOT NULL DEFAULT 0,
    is_active         boolean       NOT NULL DEFAULT true,
    created_at        timestamptz   NOT NULL DEFAULT now(),
    updated_at        timestamptz   NOT NULL DEFAULT now(),
    created_by        uuid          NULL,
    updated_by        uuid          NULL,
    is_deleted        boolean       NOT NULL DEFAULT false,
    CONSTRAINT uq_suppliers_tenant_code UNIQUE (tenant_id, code)
);
CREATE INDEX IF NOT EXISTS ix_suppliers_tenant ON suppliers(tenant_id) WHERE NOT is_deleted;

CREATE TABLE IF NOT EXISTS purchase_orders (
    id              uuid           PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id       uuid           NOT NULL,
    location_id     uuid           NOT NULL REFERENCES locations(id),
    supplier_id     uuid           NOT NULL REFERENCES suppliers(id),
    po_number       varchar(20)    NOT NULL,
    status          varchar(20)    NOT NULL DEFAULT 'draft',  -- draft|sent|partially_received|received|cancelled
    order_date      date           NOT NULL DEFAULT current_date,
    expected_date   date           NULL,
    subtotal_amount numeric(15,4)  NOT NULL DEFAULT 0,
    total_amount    numeric(15,4)  NOT NULL DEFAULT 0,
    notes           varchar(500)   NULL,
    created_at      timestamptz    NOT NULL DEFAULT now(),
    updated_at      timestamptz    NOT NULL DEFAULT now(),
    created_by      uuid           NULL,
    updated_by      uuid           NULL,
    is_deleted      boolean        NOT NULL DEFAULT false,
    CONSTRAINT uq_po_tenant_number UNIQUE (tenant_id, po_number)
);
CREATE INDEX IF NOT EXISTS ix_po_tenant_status ON purchase_orders(tenant_id, status);

CREATE TABLE IF NOT EXISTS purchase_order_lines (
    id                uuid           PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id         uuid           NOT NULL,
    purchase_order_id uuid           NOT NULL REFERENCES purchase_orders(id) ON DELETE CASCADE,
    product_id        uuid           NOT NULL,
    sku               varchar(40)    NOT NULL,
    product_name      varchar(200)   NOT NULL,
    quantity_ordered  numeric(15,4)  NOT NULL,
    quantity_received numeric(15,4)  NOT NULL DEFAULT 0,
    unit_cost         numeric(15,4)  NOT NULL,
    line_total        numeric(15,4)  NOT NULL,
    created_at        timestamptz    NOT NULL DEFAULT now(),
    updated_at        timestamptz    NOT NULL DEFAULT now(),
    created_by        uuid           NULL,
    updated_by        uuid           NULL,
    is_deleted        boolean        NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_po_lines_po ON purchase_order_lines(purchase_order_id);

CREATE TABLE IF NOT EXISTS goods_received_notes (
    id              uuid           PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id       uuid           NOT NULL,
    location_id     uuid           NOT NULL REFERENCES locations(id),
    supplier_id     uuid           NOT NULL REFERENCES suppliers(id),
    purchase_order_id uuid         NULL REFERENCES purchase_orders(id),
    grn_number      varchar(20)    NOT NULL,
    status          varchar(20)    NOT NULL DEFAULT 'draft',  -- draft|posted
    received_date   date           NOT NULL DEFAULT current_date,
    total_cost      numeric(15,4)  NOT NULL DEFAULT 0,
    notes           varchar(500)   NULL,
    posted_at       timestamptz    NULL,
    created_at      timestamptz    NOT NULL DEFAULT now(),
    updated_at      timestamptz    NOT NULL DEFAULT now(),
    created_by      uuid           NULL,
    updated_by      uuid           NULL,
    is_deleted      boolean        NOT NULL DEFAULT false,
    CONSTRAINT uq_grn_tenant_number UNIQUE (tenant_id, grn_number)
);
CREATE INDEX IF NOT EXISTS ix_grn_tenant_status ON goods_received_notes(tenant_id, status);

CREATE TABLE IF NOT EXISTS grn_lines (
    id                uuid           PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id         uuid           NOT NULL,
    grn_id            uuid           NOT NULL REFERENCES goods_received_notes(id) ON DELETE CASCADE,
    product_id        uuid           NOT NULL,
    sku               varchar(40)    NOT NULL,
    product_name      varchar(200)   NOT NULL,
    quantity_received numeric(15,4)  NOT NULL,
    unit_cost         numeric(15,4)  NOT NULL,
    line_total        numeric(15,4)  NOT NULL,
    batch_no          varchar(50)    NULL,
    expiry_date       date           NULL,
    created_at        timestamptz    NOT NULL DEFAULT now(),
    updated_at        timestamptz    NOT NULL DEFAULT now(),
    created_by        uuid           NULL,
    updated_by        uuid           NULL,
    is_deleted        boolean        NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_grn_lines_grn ON grn_lines(grn_id);

-- RLS
ALTER TABLE document_counters    ENABLE ROW LEVEL SECURITY;
ALTER TABLE suppliers            ENABLE ROW LEVEL SECURITY;
ALTER TABLE purchase_orders      ENABLE ROW LEVEL SECURITY;
ALTER TABLE purchase_order_lines ENABLE ROW LEVEL SECURITY;
ALTER TABLE goods_received_notes ENABLE ROW LEVEL SECURITY;
ALTER TABLE grn_lines            ENABLE ROW LEVEL SECURITY;

DO $$
DECLARE t text;
BEGIN
  FOREACH t IN ARRAY ARRAY['document_counters','suppliers','purchase_orders','purchase_order_lines','goods_received_notes','grn_lines']
  LOOP
    EXECUTE format('DROP POLICY IF EXISTS p_%1$s_tenant ON %1$s', t);
    EXECUTE format('CREATE POLICY p_%1$s_tenant ON %1$s USING (tenant_id::text = current_setting(''app.tenant_id'', true))', t);
  END LOOP;
END $$;
