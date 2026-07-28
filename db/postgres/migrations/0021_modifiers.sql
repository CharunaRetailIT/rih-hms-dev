-- =============================================================================
-- Migration 0021: Modifiers / add-ons
--   modifier_groups        — e.g. "Add-ons", "Size", "Spice" (min/max/required)
--   modifier_items         — choices in a group, each with a price delta
--   product_modifier_groups— attach reusable groups to products (many-to-many)
--   order_item_modifiers   — the choices made on an order line (wired in next tranche)
-- =============================================================================

CREATE TABLE IF NOT EXISTS modifier_groups (
    id          uuid         PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id   uuid         NOT NULL,
    name        varchar(120) NOT NULL,
    min_select  int          NOT NULL DEFAULT 0,
    max_select  int          NOT NULL DEFAULT 0,   -- 0 = unlimited
    is_required boolean      NOT NULL DEFAULT false,
    sort_order  int          NOT NULL DEFAULT 0,
    is_active   boolean      NOT NULL DEFAULT true,
    created_at  timestamptz  NOT NULL DEFAULT now(),
    updated_at  timestamptz  NOT NULL DEFAULT now(),
    created_by  uuid         NULL,
    updated_by  uuid         NULL,
    is_deleted  boolean      NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_modifier_groups_tenant ON modifier_groups(tenant_id);

CREATE TABLE IF NOT EXISTS modifier_items (
    id          uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id   uuid          NOT NULL,
    group_id    uuid          NOT NULL REFERENCES modifier_groups(id) ON DELETE CASCADE,
    name        varchar(120)  NOT NULL,
    price_delta numeric(15,4) NOT NULL DEFAULT 0,
    is_default  boolean       NOT NULL DEFAULT false,
    sort_order  int           NOT NULL DEFAULT 0,
    is_active   boolean       NOT NULL DEFAULT true,
    created_at  timestamptz   NOT NULL DEFAULT now(),
    updated_at  timestamptz   NOT NULL DEFAULT now(),
    created_by  uuid          NULL,
    updated_by  uuid          NULL,
    is_deleted  boolean       NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_modifier_items_group ON modifier_items(group_id);

CREATE TABLE IF NOT EXISTS product_modifier_groups (
    id          uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id   uuid        NOT NULL,
    product_id  uuid        NOT NULL,
    group_id    uuid        NOT NULL REFERENCES modifier_groups(id) ON DELETE CASCADE,
    sort_order  int         NOT NULL DEFAULT 0,
    created_at  timestamptz NOT NULL DEFAULT now(),
    updated_at  timestamptz NOT NULL DEFAULT now(),
    created_by  uuid        NULL,
    updated_by  uuid        NULL,
    is_deleted  boolean     NOT NULL DEFAULT false,
    CONSTRAINT uq_product_modifier UNIQUE (tenant_id, product_id, group_id)
);
CREATE INDEX IF NOT EXISTS ix_product_modifier_groups_product ON product_modifier_groups(product_id);

CREATE TABLE IF NOT EXISTS order_item_modifiers (
    id               uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id        uuid          NOT NULL,
    order_item_id    uuid          NOT NULL REFERENCES order_items(id) ON DELETE CASCADE,
    modifier_item_id uuid          NULL,
    name             varchar(120)  NOT NULL,
    price_delta      numeric(15,4) NOT NULL DEFAULT 0,
    created_at       timestamptz   NOT NULL DEFAULT now(),
    updated_at       timestamptz   NOT NULL DEFAULT now(),
    created_by       uuid          NULL,
    updated_by       uuid          NULL,
    is_deleted       boolean       NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_order_item_modifiers_item ON order_item_modifiers(order_item_id);

-- RLS
DO $$
DECLARE t text;
BEGIN
  FOREACH t IN ARRAY ARRAY['modifier_groups','modifier_items','product_modifier_groups','order_item_modifiers']
  LOOP
    EXECUTE format('ALTER TABLE %1$s ENABLE ROW LEVEL SECURITY', t);
    EXECUTE format('DROP POLICY IF EXISTS p_%1$s_tenant ON %1$s', t);
    EXECUTE format('CREATE POLICY p_%1$s_tenant ON %1$s USING (tenant_id::text = current_setting(''app.tenant_id'', true))', t);
  END LOOP;
END $$;
