-- =============================================================================
-- Migration 0013: Production — recipes (BOM) + production orders
-- Supply chain step 7. Central kitchen makes finished goods from raw ingredients:
-- a production order consumes raw stock at avg cost and yields finished stock
-- with rolled-up cost. Packing is production with a bulk→packed recipe.
-- =============================================================================

ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS production_prefix varchar(10) NOT NULL DEFAULT 'PRD';

-- A recipe / bill-of-materials for ONE finished product.
CREATE TABLE IF NOT EXISTS recipes (
    id              uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id       uuid          NOT NULL,
    product_id      uuid          NOT NULL,                 -- the finished product
    yield_quantity  numeric(15,4) NOT NULL DEFAULT 1,       -- output units per batch of the listed ingredients
    notes           varchar(500)  NULL,
    is_active       boolean       NOT NULL DEFAULT true,
    created_at      timestamptz   NOT NULL DEFAULT now(),
    updated_at      timestamptz   NOT NULL DEFAULT now(),
    created_by      uuid          NULL,
    updated_by      uuid          NULL,
    is_deleted      boolean       NOT NULL DEFAULT false,
    CONSTRAINT uq_recipe_tenant_product UNIQUE (tenant_id, product_id)
);

CREATE TABLE IF NOT EXISTS recipe_lines (
    id                    uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id             uuid          NOT NULL,
    recipe_id             uuid          NOT NULL REFERENCES recipes(id) ON DELETE CASCADE,
    ingredient_product_id uuid          NOT NULL,
    sku                   varchar(40)   NOT NULL,
    product_name          varchar(200)  NOT NULL,
    quantity              numeric(15,4) NOT NULL,           -- per batch (per yield_quantity output)
    created_at            timestamptz   NOT NULL DEFAULT now(),
    updated_at            timestamptz   NOT NULL DEFAULT now(),
    created_by            uuid          NULL,
    updated_by            uuid          NULL,
    is_deleted            boolean       NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_recipe_lines_recipe ON recipe_lines(recipe_id);

CREATE TABLE IF NOT EXISTS production_orders (
    id                uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id         uuid          NOT NULL,
    location_id       uuid          NOT NULL REFERENCES locations(id),
    production_number varchar(20)   NOT NULL,
    product_id        uuid          NOT NULL,               -- finished product produced
    product_name      varchar(200)  NOT NULL,
    quantity          numeric(15,4) NOT NULL,               -- units produced
    status            varchar(20)   NOT NULL DEFAULT 'completed',
    total_input_cost  numeric(15,4) NOT NULL DEFAULT 0,     -- sum of consumed ingredient cost
    unit_cost         numeric(15,4) NOT NULL DEFAULT 0,     -- total_input_cost / quantity
    notes             varchar(500)  NULL,
    completed_at      timestamptz   NULL,
    created_at        timestamptz   NOT NULL DEFAULT now(),
    updated_at        timestamptz   NOT NULL DEFAULT now(),
    created_by        uuid          NULL,
    updated_by        uuid          NULL,
    is_deleted        boolean       NOT NULL DEFAULT false,
    CONSTRAINT uq_production_tenant_number UNIQUE (tenant_id, production_number)
);
CREATE INDEX IF NOT EXISTS ix_production_tenant ON production_orders(tenant_id);

CREATE TABLE IF NOT EXISTS production_consumptions (
    id                    uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id             uuid          NOT NULL,
    production_order_id   uuid          NOT NULL REFERENCES production_orders(id) ON DELETE CASCADE,
    ingredient_product_id uuid          NOT NULL,
    sku                   varchar(40)   NOT NULL,
    product_name          varchar(200)  NOT NULL,
    quantity_consumed     numeric(15,4) NOT NULL,
    unit_cost             numeric(15,4) NOT NULL,
    line_total            numeric(15,4) NOT NULL,
    created_at            timestamptz   NOT NULL DEFAULT now(),
    updated_at            timestamptz   NOT NULL DEFAULT now(),
    created_by            uuid          NULL,
    updated_by            uuid          NULL,
    is_deleted            boolean       NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_production_consumptions_po ON production_consumptions(production_order_id);

-- RLS
DO $$
DECLARE t text;
BEGIN
  FOREACH t IN ARRAY ARRAY['recipes','recipe_lines','production_orders','production_consumptions']
  LOOP
    EXECUTE format('ALTER TABLE %1$s ENABLE ROW LEVEL SECURITY', t);
    EXECUTE format('DROP POLICY IF EXISTS p_%1$s_tenant ON %1$s', t);
    EXECUTE format('CREATE POLICY p_%1$s_tenant ON %1$s USING (tenant_id::text = current_setting(''app.tenant_id'', true))', t);
  END LOOP;
END $$;
