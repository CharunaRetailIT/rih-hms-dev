-- =============================================================================
-- Migration 0010: Inventory movements — transfers, returns, wastage, adjustments
-- Supply chain step 6. The Central Kitchen → outlet distribution + loss/correction
-- flows. All reuse the stock-movement + weighted-average-cost engine from step 5.
-- Target DB: any hms_tenant_<slug>
-- =============================================================================

-- Inter-location transfers (CK → outlet, outlet → CK return). Lifecycle:
--   draft → dispatched (sender stock −) → received (receiver stock +) | cancelled
CREATE TABLE IF NOT EXISTS stock_transfers (
    id                uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id         uuid          NOT NULL,
    transfer_number   varchar(20)   NOT NULL,
    from_location_id  uuid          NOT NULL REFERENCES locations(id),
    to_location_id    uuid          NOT NULL REFERENCES locations(id),
    status            varchar(20)   NOT NULL DEFAULT 'draft',  -- draft|dispatched|received|cancelled
    is_return         boolean       NOT NULL DEFAULT false,
    notes             varchar(500)  NULL,
    total_cost        numeric(15,4) NOT NULL DEFAULT 0,
    dispatched_at     timestamptz   NULL,
    received_at       timestamptz   NULL,
    created_at        timestamptz   NOT NULL DEFAULT now(),
    updated_at        timestamptz   NOT NULL DEFAULT now(),
    created_by        uuid          NULL,
    updated_by        uuid          NULL,
    is_deleted        boolean       NOT NULL DEFAULT false,
    CONSTRAINT uq_transfer_tenant_number UNIQUE (tenant_id, transfer_number),
    CONSTRAINT ck_transfer_distinct_locations CHECK (from_location_id <> to_location_id)
);
CREATE INDEX IF NOT EXISTS ix_transfers_tenant_status ON stock_transfers(tenant_id, status);

CREATE TABLE IF NOT EXISTS stock_transfer_lines (
    id            uuid           PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id     uuid           NOT NULL,
    transfer_id   uuid           NOT NULL REFERENCES stock_transfers(id) ON DELETE CASCADE,
    product_id    uuid           NOT NULL,
    sku           varchar(40)    NOT NULL,
    product_name  varchar(200)   NOT NULL,
    quantity      numeric(15,4)  NOT NULL,
    unit_cost     numeric(15,4)  NOT NULL DEFAULT 0,  -- snapshot of sender avg at dispatch
    line_total    numeric(15,4)  NOT NULL DEFAULT 0,
    created_at    timestamptz    NOT NULL DEFAULT now(),
    updated_at    timestamptz    NOT NULL DEFAULT now(),
    created_by    uuid           NULL,
    updated_by    uuid           NULL,
    is_deleted    boolean        NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_transfer_lines_transfer ON stock_transfer_lines(transfer_id);

-- Wastage (reason-coded loss). Posted immediately: stock −.
CREATE TABLE IF NOT EXISTS wastage_notes (
    id              uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id       uuid          NOT NULL,
    location_id     uuid          NOT NULL REFERENCES locations(id),
    wastage_number  varchar(20)   NOT NULL,
    reason          varchar(30)   NOT NULL DEFAULT 'spoilage',  -- spoilage|breakage|expiry|theft|other
    status          varchar(20)   NOT NULL DEFAULT 'posted',
    notes           varchar(500)  NULL,
    total_cost      numeric(15,4) NOT NULL DEFAULT 0,
    posted_at       timestamptz   NULL,
    created_at      timestamptz   NOT NULL DEFAULT now(),
    updated_at      timestamptz   NOT NULL DEFAULT now(),
    created_by      uuid          NULL,
    updated_by      uuid          NULL,
    is_deleted      boolean       NOT NULL DEFAULT false,
    CONSTRAINT uq_wastage_tenant_number UNIQUE (tenant_id, wastage_number)
);
CREATE INDEX IF NOT EXISTS ix_wastage_tenant ON wastage_notes(tenant_id);

CREATE TABLE IF NOT EXISTS wastage_lines (
    id            uuid           PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id     uuid           NOT NULL,
    wastage_id    uuid           NOT NULL REFERENCES wastage_notes(id) ON DELETE CASCADE,
    product_id    uuid           NOT NULL,
    sku           varchar(40)    NOT NULL,
    product_name  varchar(200)   NOT NULL,
    quantity      numeric(15,4)  NOT NULL,
    unit_cost     numeric(15,4)  NOT NULL DEFAULT 0,
    line_total    numeric(15,4)  NOT NULL DEFAULT 0,
    created_at    timestamptz    NOT NULL DEFAULT now(),
    updated_at    timestamptz    NOT NULL DEFAULT now(),
    created_by    uuid           NULL,
    updated_by    uuid           NULL,
    is_deleted    boolean        NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_wastage_lines_note ON wastage_lines(wastage_id);

-- Stock adjustments (signed correction, reason-coded). Posted immediately.
CREATE TABLE IF NOT EXISTS stock_adjustments (
    id                uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id         uuid          NOT NULL,
    location_id       uuid          NOT NULL REFERENCES locations(id),
    adjustment_number varchar(20)   NOT NULL,
    reason            varchar(30)   NOT NULL DEFAULT 'count',  -- count|correction|other
    status            varchar(20)   NOT NULL DEFAULT 'posted',
    notes             varchar(500)  NULL,
    posted_at         timestamptz   NULL,
    created_at        timestamptz   NOT NULL DEFAULT now(),
    updated_at        timestamptz   NOT NULL DEFAULT now(),
    created_by        uuid          NULL,
    updated_by        uuid          NULL,
    is_deleted        boolean       NOT NULL DEFAULT false,
    CONSTRAINT uq_adjustment_tenant_number UNIQUE (tenant_id, adjustment_number)
);
CREATE INDEX IF NOT EXISTS ix_adjustment_tenant ON stock_adjustments(tenant_id);

CREATE TABLE IF NOT EXISTS stock_adjustment_lines (
    id              uuid           PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id       uuid           NOT NULL,
    adjustment_id   uuid           NOT NULL REFERENCES stock_adjustments(id) ON DELETE CASCADE,
    product_id      uuid           NOT NULL,
    sku             varchar(40)    NOT NULL,
    product_name    varchar(200)   NOT NULL,
    quantity_delta  numeric(15,4)  NOT NULL,   -- signed: + adds, - removes
    unit_cost       numeric(15,4)  NOT NULL DEFAULT 0,
    created_at      timestamptz    NOT NULL DEFAULT now(),
    updated_at      timestamptz    NOT NULL DEFAULT now(),
    created_by      uuid           NULL,
    updated_by      uuid           NULL,
    is_deleted      boolean        NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_adjustment_lines_adj ON stock_adjustment_lines(adjustment_id);

-- RLS
DO $$
DECLARE t text;
BEGIN
  FOREACH t IN ARRAY ARRAY['stock_transfers','stock_transfer_lines','wastage_notes','wastage_lines','stock_adjustments','stock_adjustment_lines']
  LOOP
    EXECUTE format('ALTER TABLE %1$s ENABLE ROW LEVEL SECURITY', t);
    EXECUTE format('DROP POLICY IF EXISTS p_%1$s_tenant ON %1$s', t);
    EXECUTE format('CREATE POLICY p_%1$s_tenant ON %1$s USING (tenant_id::text = current_setting(''app.tenant_id'', true))', t);
  END LOOP;
END $$;
