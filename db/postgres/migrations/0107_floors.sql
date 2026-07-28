-- =============================================================================
-- Migration 0107: Floors — a physical floor/section of an outlet, and steward-to-floor
-- assignment. Foundation for floor-scoped notification routing on guest QR orders
-- (a steward is only pushed orders for tables on their assigned floor).
--
-- restaurant_tables.area (free text) is left as-is for display/back-compat; floor_id is
-- an additive, optional FK. Existing distinct `area` values per (tenant, location) are
-- backfilled into real floors rows and tables are pointed at them below.
-- =============================================================================

CREATE TABLE IF NOT EXISTS floors (
    id          uuid         PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id   uuid         NOT NULL,
    location_id uuid         NOT NULL,
    name        varchar(60)  NOT NULL,
    sort_order  int          NOT NULL DEFAULT 0,
    is_active   boolean      NOT NULL DEFAULT true,
    created_at  timestamptz  NOT NULL DEFAULT now(),
    updated_at  timestamptz  NOT NULL DEFAULT now(),
    created_by  uuid         NULL,
    updated_by  uuid         NULL,
    is_deleted  boolean      NOT NULL DEFAULT false,
    CONSTRAINT uq_floor_name UNIQUE (tenant_id, location_id, name)
);
CREATE INDEX IF NOT EXISTS ix_floors_tenant   ON floors(tenant_id);
CREATE INDEX IF NOT EXISTS ix_floors_location ON floors(location_id);

ALTER TABLE restaurant_tables ADD COLUMN IF NOT EXISTS floor_id uuid NULL REFERENCES floors(id);
CREATE INDEX IF NOT EXISTS ix_restaurant_tables_floor ON restaurant_tables(floor_id);

CREATE TABLE IF NOT EXISTS user_floors (
    id          uuid         PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id   uuid         NOT NULL,
    user_id     uuid         NOT NULL REFERENCES users(id)  ON DELETE CASCADE,
    floor_id    uuid         NOT NULL REFERENCES floors(id) ON DELETE CASCADE,
    created_at  timestamptz  NOT NULL DEFAULT now(),
    updated_at  timestamptz  NOT NULL DEFAULT now(),
    created_by  uuid         NULL,
    updated_by  uuid         NULL,
    is_deleted  boolean      NOT NULL DEFAULT false,
    CONSTRAINT uq_user_floor UNIQUE (tenant_id, user_id, floor_id)
);
CREATE INDEX IF NOT EXISTS ix_user_floors_tenant ON user_floors(tenant_id);
CREATE INDEX IF NOT EXISTS ix_user_floors_user   ON user_floors(user_id);
CREATE INDEX IF NOT EXISTS ix_user_floors_floor  ON user_floors(floor_id);

-- Backfill: each tenant/location's distinct existing `area` strings become real floor
-- rows, and tables get pointed at them. Tables with a blank/NULL area stay floor_id NULL.
INSERT INTO floors (tenant_id, location_id, name)
SELECT DISTINCT tenant_id, location_id, trim(area)
FROM restaurant_tables
WHERE area IS NOT NULL AND trim(area) <> ''
ON CONFLICT (tenant_id, location_id, name) DO NOTHING;

UPDATE restaurant_tables t
SET floor_id = f.id
FROM floors f
WHERE t.area IS NOT NULL AND trim(t.area) <> ''
  AND f.tenant_id = t.tenant_id AND f.location_id = t.location_id AND f.name = trim(t.area);

-- RLS
ALTER TABLE floors ENABLE ROW LEVEL SECURITY;
ALTER TABLE floors FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS p_floors_tenant ON floors;
CREATE POLICY p_floors_tenant ON floors USING (tenant_id::text = current_setting('app.tenant_id', true));

ALTER TABLE user_floors ENABLE ROW LEVEL SECURITY;
ALTER TABLE user_floors FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS p_user_floors_tenant ON user_floors;
CREATE POLICY p_user_floors_tenant ON user_floors USING (tenant_id::text = current_setting('app.tenant_id', true));
