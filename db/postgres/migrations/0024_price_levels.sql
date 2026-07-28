-- =============================================================================
-- Migration 0024: Price levels — per-channel / per-location pricing (#55c)
-- A price level is a named price list (Dine-in, Takeaway, Delivery, Aggregator,
-- a specific branch …). A product may override its base price per level; an
-- order resolves a level (explicit > level matching the order type > default)
-- and lines are priced at that level. The killer case: delivery / aggregator
-- orders priced higher than dine-in, automatically.
-- =============================================================================

CREATE TABLE IF NOT EXISTS price_levels (
    id          uuid         PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id   uuid         NOT NULL,
    code        varchar(20)  NOT NULL,
    name        varchar(60)  NOT NULL,
    is_default  boolean      NOT NULL DEFAULT false,   -- the fallback list (base price)
    applies_to_order_type varchar(20) NULL,            -- dine_in | takeaway | delivery (auto-select)
    sort_order  int          NOT NULL DEFAULT 0,
    is_active   boolean      NOT NULL DEFAULT true,
    created_at  timestamptz  NOT NULL DEFAULT now(),
    updated_at  timestamptz  NOT NULL DEFAULT now(),
    created_by  uuid         NULL,
    updated_by  uuid         NULL,
    is_deleted  boolean      NOT NULL DEFAULT false,
    CONSTRAINT uq_price_level_code UNIQUE (tenant_id, code)
);
CREATE INDEX IF NOT EXISTS ix_price_levels_tenant ON price_levels(tenant_id);

-- Per-product price override for a level. No row → the level uses the base price.
CREATE TABLE IF NOT EXISTS product_prices (
    id             uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id      uuid          NOT NULL,
    product_id     uuid          NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    price_level_id uuid          NOT NULL REFERENCES price_levels(id) ON DELETE CASCADE,
    price          numeric(18,4) NOT NULL DEFAULT 0,
    created_at     timestamptz   NOT NULL DEFAULT now(),
    updated_at     timestamptz   NOT NULL DEFAULT now(),
    created_by     uuid          NULL,
    updated_by     uuid          NULL,
    is_deleted     boolean       NOT NULL DEFAULT false,
    CONSTRAINT uq_product_price UNIQUE (tenant_id, product_id, price_level_id)
);
CREATE INDEX IF NOT EXISTS ix_product_prices_tenant  ON product_prices(tenant_id);
CREATE INDEX IF NOT EXISTS ix_product_prices_product ON product_prices(product_id);

-- The level an order was priced at (snapshotted).
ALTER TABLE orders ADD COLUMN IF NOT EXISTS price_level_id uuid NULL;

-- RLS
ALTER TABLE price_levels   ENABLE ROW LEVEL SECURITY;
ALTER TABLE product_prices ENABLE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS p_price_levels_tenant   ON price_levels;
DROP POLICY IF EXISTS p_product_prices_tenant ON product_prices;
CREATE POLICY p_price_levels_tenant   ON price_levels
    USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_product_prices_tenant ON product_prices
    USING (tenant_id::text = current_setting('app.tenant_id', true));
