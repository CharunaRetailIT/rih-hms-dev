-- =============================================================================
-- Migration 0020: Purchase returns / debit notes (PRN) to suppliers
-- Return received goods back to a supplier: decrements stock at the location and
-- reverses the reclaimable input VAT (netted in the VAT return). Optionally
-- references the originating GRN.
-- =============================================================================

CREATE TABLE IF NOT EXISTS purchase_returns (
    id                 uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id          uuid          NOT NULL,
    location_id        uuid          NOT NULL REFERENCES locations(id),
    supplier_id        uuid          NOT NULL REFERENCES suppliers(id),
    grn_id             uuid          NULL REFERENCES goods_received_notes(id),
    prn_number         varchar(20)   NOT NULL,
    status             varchar(20)   NOT NULL DEFAULT 'posted',   -- posted|void
    return_date        date          NOT NULL DEFAULT current_date,
    reason             varchar(200)  NULL,
    supplier_credit_no varchar(50)   NULL,
    total_cost         numeric(15,4) NOT NULL DEFAULT 0,          -- goods value returned (ex-VAT)
    tax_amount         numeric(15,4) NOT NULL DEFAULT 0,          -- input VAT reversed
    notes              varchar(500)  NULL,
    posted_at          timestamptz   NULL,
    voided_at          timestamptz   NULL,
    void_reason        varchar(200)  NULL,
    created_at         timestamptz   NOT NULL DEFAULT now(),
    updated_at         timestamptz   NOT NULL DEFAULT now(),
    created_by         uuid          NULL,
    updated_by         uuid          NULL,
    is_deleted         boolean       NOT NULL DEFAULT false,
    CONSTRAINT uq_prn_tenant_number UNIQUE (tenant_id, prn_number)
);
CREATE INDEX IF NOT EXISTS ix_purchase_returns_tenant ON purchase_returns(tenant_id);

CREATE TABLE IF NOT EXISTS purchase_return_lines (
    id                uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id         uuid          NOT NULL,
    return_id         uuid          NOT NULL REFERENCES purchase_returns(id) ON DELETE CASCADE,
    product_id        uuid          NOT NULL,
    sku               varchar(40)   NOT NULL,
    product_name      varchar(200)  NOT NULL,
    quantity          numeric(15,4) NOT NULL,           -- in the return unit
    unit_id           uuid          NULL,
    unit_symbol       varchar(10)   NULL,
    unit_cost         numeric(15,4) NOT NULL,
    line_total        numeric(15,4) NOT NULL,           -- goods value ex-VAT
    tax_rate          numeric(8,4)  NOT NULL DEFAULT 0,
    tax_amount        numeric(15,4) NOT NULL DEFAULT 0,
    stock_quantity    numeric(15,4) NOT NULL DEFAULT 0, -- qty removed from stock (converted)
    batch_no          varchar(50)   NULL,
    created_at        timestamptz   NOT NULL DEFAULT now(),
    updated_at        timestamptz   NOT NULL DEFAULT now(),
    created_by        uuid          NULL,
    updated_by        uuid          NULL,
    is_deleted        boolean       NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_purchase_return_lines_return ON purchase_return_lines(return_id);

-- RLS
DO $$
DECLARE t text;
BEGIN
  FOREACH t IN ARRAY ARRAY['purchase_returns','purchase_return_lines']
  LOOP
    EXECUTE format('ALTER TABLE %1$s ENABLE ROW LEVEL SECURITY', t);
    EXECUTE format('DROP POLICY IF EXISTS p_%1$s_tenant ON %1$s', t);
    EXECUTE format('CREATE POLICY p_%1$s_tenant ON %1$s USING (tenant_id::text = current_setting(''app.tenant_id'', true))', t);
  END LOOP;
END $$;
