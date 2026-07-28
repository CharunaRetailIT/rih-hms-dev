-- =============================================================================
-- Migration 0028: Tables / floor + reservations (#68)
-- Per-outlet dining tables (grid model; visual X/Y floor plan is a follow-on)
-- and table reservations. A table's "occupied" state is derived at query time
-- from open/confirmed orders that reference it (orders.table_id) — no stored flag
-- to drift. Reservations are a lightweight booking list with a status lifecycle.
-- =============================================================================

CREATE TABLE IF NOT EXISTS restaurant_tables (
    id          uuid         PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id   uuid         NOT NULL,
    location_id uuid         NOT NULL,
    code        varchar(20)  NOT NULL,           -- T1, BAR2, G3…
    name        varchar(60)  NULL,               -- optional friendly label
    seats       int          NOT NULL DEFAULT 2,
    area        varchar(40)  NULL,               -- section/zone: Garden, AC, Bar…
    sort_order  int          NOT NULL DEFAULT 0,
    is_active   boolean      NOT NULL DEFAULT true,
    created_at  timestamptz  NOT NULL DEFAULT now(),
    updated_at  timestamptz  NOT NULL DEFAULT now(),
    created_by  uuid         NULL,
    updated_by  uuid         NULL,
    is_deleted  boolean      NOT NULL DEFAULT false,
    CONSTRAINT uq_table_code UNIQUE (tenant_id, location_id, code)
);
CREATE INDEX IF NOT EXISTS ix_restaurant_tables_tenant   ON restaurant_tables(tenant_id);
CREATE INDEX IF NOT EXISTS ix_restaurant_tables_location ON restaurant_tables(location_id);

CREATE TABLE IF NOT EXISTS reservations (
    id               uuid         PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id        uuid         NOT NULL,
    location_id      uuid         NOT NULL,
    table_id         uuid         NULL,
    customer_name    varchar(120) NOT NULL,
    phone            varchar(40)  NULL,
    party_size       int          NOT NULL DEFAULT 2,
    reserved_at      timestamptz  NOT NULL,
    duration_minutes int          NOT NULL DEFAULT 90,
    status           varchar(20)  NOT NULL DEFAULT 'booked',  -- booked|seated|cancelled|no_show|completed
    notes            varchar(300) NULL,
    created_at       timestamptz  NOT NULL DEFAULT now(),
    updated_at       timestamptz  NOT NULL DEFAULT now(),
    created_by       uuid         NULL,
    updated_by       uuid         NULL,
    is_deleted       boolean      NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_reservations_tenant   ON reservations(tenant_id);
CREATE INDEX IF NOT EXISTS ix_reservations_location ON reservations(location_id);
CREATE INDEX IF NOT EXISTS ix_reservations_when     ON reservations(reserved_at);

-- Link an order to a real table (occupancy is derived from open orders).
ALTER TABLE orders ADD COLUMN IF NOT EXISTS table_id uuid NULL;

-- RLS
ALTER TABLE restaurant_tables ENABLE ROW LEVEL SECURITY;
ALTER TABLE reservations      ENABLE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS p_restaurant_tables_tenant ON restaurant_tables;
DROP POLICY IF EXISTS p_reservations_tenant      ON reservations;
CREATE POLICY p_restaurant_tables_tenant ON restaurant_tables USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_reservations_tenant      ON reservations      USING (tenant_id::text = current_setting('app.tenant_id', true));
