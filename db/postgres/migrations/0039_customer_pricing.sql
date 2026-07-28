-- =============================================================================
-- Migration 0039: Per-product customer pricing (#70 follow-on)
-- Contract prices: a specific customer (or a whole category) pays a fixed price
-- for a product, overriding price-levels and the base price at the till.
-- Resolution precedence: variant > customer price > category price > price-level > base.
-- =============================================================================

CREATE TABLE IF NOT EXISTS customer_product_prices (
    id          uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id   uuid          NOT NULL,
    customer_id uuid          NULL,        -- exactly one of customer_id / category_id is set
    category_id uuid          NULL,
    product_id  uuid          NOT NULL,
    price       numeric(18,4) NOT NULL,
    created_at  timestamptz   NOT NULL DEFAULT now(),
    updated_at  timestamptz   NOT NULL DEFAULT now(),
    created_by  uuid          NULL,
    updated_by  uuid          NULL,
    is_deleted  boolean       NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_cpp_tenant   ON customer_product_prices(tenant_id);
CREATE INDEX IF NOT EXISTS ix_cpp_customer ON customer_product_prices(customer_id);
CREATE INDEX IF NOT EXISTS ix_cpp_category ON customer_product_prices(category_id);
-- One live price per (customer, product) and per (category, product).
CREATE UNIQUE INDEX IF NOT EXISTS uq_cpp_customer ON customer_product_prices(tenant_id, customer_id, product_id) WHERE customer_id IS NOT NULL AND is_deleted = false;
CREATE UNIQUE INDEX IF NOT EXISTS uq_cpp_category ON customer_product_prices(tenant_id, category_id, product_id) WHERE category_id IS NOT NULL AND is_deleted = false;

ALTER TABLE customer_product_prices ENABLE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS p_cpp_tenant ON customer_product_prices;
CREATE POLICY p_cpp_tenant ON customer_product_prices USING (tenant_id::text = current_setting('app.tenant_id', true));
