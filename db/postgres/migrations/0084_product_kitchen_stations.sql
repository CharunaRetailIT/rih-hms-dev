-- =============================================================================
-- Migration 0050: Product ↔ kitchen station mapping
-- A product can route to more than one kitchen station (e.g. a combo that
-- prints to both Grill and Dessert), optionally scoped to a single location.
-- =============================================================================

CREATE TABLE IF NOT EXISTS product_kitchen_stations (
    id                 uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id          uuid        NOT NULL,
    product_id         uuid        NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    kitchen_station_id uuid        NOT NULL REFERENCES kitchen_stations(id) ON DELETE CASCADE,
    location_id        uuid        NULL REFERENCES locations(id) ON DELETE CASCADE,
    created_at         timestamptz NOT NULL DEFAULT now(),
    updated_at         timestamptz NOT NULL DEFAULT now(),
    created_by         uuid        NULL,
    updated_by         uuid        NULL,
    is_deleted         boolean     NOT NULL DEFAULT false,
    CONSTRAINT uq_product_kitchen_station UNIQUE (tenant_id, product_id, kitchen_station_id, location_id)
);
CREATE INDEX IF NOT EXISTS ix_product_kitchen_stations_tenant   ON product_kitchen_stations(tenant_id);
CREATE INDEX IF NOT EXISTS ix_product_kitchen_stations_product  ON product_kitchen_stations(product_id);
CREATE INDEX IF NOT EXISTS ix_product_kitchen_stations_station  ON product_kitchen_stations(kitchen_station_id);
CREATE INDEX IF NOT EXISTS ix_product_kitchen_stations_location ON product_kitchen_stations(location_id);

-- RLS
ALTER TABLE product_kitchen_stations ENABLE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS p_product_kitchen_stations_tenant ON product_kitchen_stations;
CREATE POLICY p_product_kitchen_stations_tenant ON product_kitchen_stations
    USING (tenant_id::text = current_setting('app.tenant_id', true));
