-- =============================================================================
-- Migration 0022: Kitchen / printer stations + per-product routing
-- A product routes to a station (Hot Kitchen / Bar / Dessert / Grill …); on
-- confirm, KOTs are split per station so each prints at the right place.
-- (KOT grouping already keys off order_items.station — this makes it configurable
--  and per-product rather than a category heuristic.)
-- =============================================================================

CREATE TABLE IF NOT EXISTS kitchen_stations (
    id          uuid         PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id   uuid         NOT NULL,
    code        varchar(20)  NOT NULL,
    name        varchar(80)  NOT NULL,
    printer_name varchar(80) NULL,           -- physical/logical printer (for the print agent)
    sort_order  int          NOT NULL DEFAULT 0,
    is_active   boolean      NOT NULL DEFAULT true,
    created_at  timestamptz  NOT NULL DEFAULT now(),
    updated_at  timestamptz  NOT NULL DEFAULT now(),
    created_by  uuid         NULL,
    updated_by  uuid         NULL,
    is_deleted  boolean      NOT NULL DEFAULT false,
    CONSTRAINT uq_kitchen_station_code UNIQUE (tenant_id, code)
);
CREATE INDEX IF NOT EXISTS ix_kitchen_stations_tenant ON kitchen_stations(tenant_id);

-- Which station a product's items route to (code, denormalised onto order lines).
ALTER TABLE products ADD COLUMN IF NOT EXISTS kitchen_station_code varchar(20) NULL;

-- RLS
ALTER TABLE kitchen_stations ENABLE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS p_kitchen_stations_tenant ON kitchen_stations;
CREATE POLICY p_kitchen_stations_tenant ON kitchen_stations
    USING (tenant_id::text = current_setting('app.tenant_id', true));
