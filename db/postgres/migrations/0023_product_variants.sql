-- =============================================================================
-- Migration 0023: Serving-size variants (#55b)
-- One product, several sellable sizes — Tea → Cup / Pot, Pizza → S / M / L —
-- each with its own sell price and printed name. A variant changes the line's
-- unit price and the name shown on the bill & KOT; stock still decrements by
-- quantity (sizes are price points, not separate stock items in v1).
-- =============================================================================

CREATE TABLE IF NOT EXISTS product_variants (
    id          uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id   uuid          NOT NULL,
    product_id  uuid          NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    code        varchar(20)   NOT NULL,            -- CUP / POT / S / M / L
    name        varchar(60)   NOT NULL,            -- "Cup", "Large"
    price       numeric(18,4) NOT NULL DEFAULT 0,  -- absolute sell price for this size
    sort_order  int           NOT NULL DEFAULT 0,
    is_active   boolean       NOT NULL DEFAULT true,
    created_at  timestamptz   NOT NULL DEFAULT now(),
    updated_at  timestamptz   NOT NULL DEFAULT now(),
    created_by  uuid          NULL,
    updated_by  uuid          NULL,
    is_deleted  boolean       NOT NULL DEFAULT false,
    CONSTRAINT uq_product_variant_code UNIQUE (tenant_id, product_id, code)
);
CREATE INDEX IF NOT EXISTS ix_product_variants_tenant  ON product_variants(tenant_id);
CREATE INDEX IF NOT EXISTS ix_product_variants_product ON product_variants(product_id);

-- The chosen size, snapshotted onto the order line (label printed on bill/KOT).
ALTER TABLE order_items ADD COLUMN IF NOT EXISTS variant_id   uuid        NULL;
ALTER TABLE order_items ADD COLUMN IF NOT EXISTS variant_name varchar(60) NULL;

-- RLS
ALTER TABLE product_variants ENABLE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS p_product_variants_tenant ON product_variants;
CREATE POLICY p_product_variants_tenant ON product_variants
    USING (tenant_id::text = current_setting('app.tenant_id', true));
