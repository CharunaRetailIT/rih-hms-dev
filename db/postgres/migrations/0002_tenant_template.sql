-- =============================================================================
-- Migration 0002: Tenant database schema (the "template" applied to every
-- new hms_tenant_<slug> DB at provisioning time)
-- Target DB: any hms_tenant_<slug>
-- Run with:  psql -d hms_tenant_demo -f db/postgres/migrations/0002_tenant_template.sql
--
-- v1 minimum: users, locations. Sprint 2 adds products, orders, KOT, etc.
-- =============================================================================

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- -----------------------------------------------------------------------------
-- users — tenant staff (cashiers, managers, kitchen, accountants, owner)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS users (
    id            uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id     uuid          NOT NULL,
    email         varchar(255)  NOT NULL,
    display_name  varchar(200)  NOT NULL,
    role          int           NOT NULL DEFAULT 2,  -- 0=Owner, 1=Manager, 2=Cashier, 3=Kitchen, 4=Accountant
    is_active     boolean       NOT NULL DEFAULT true,
    last_login_at timestamptz   NULL,
    phone_e164    varchar(20)   NULL,
    created_at    timestamptz   NOT NULL DEFAULT now(),
    updated_at    timestamptz   NOT NULL DEFAULT now(),
    created_by    uuid          NULL,
    updated_by    uuid          NULL,
    is_deleted    boolean       NOT NULL DEFAULT false,
    CONSTRAINT uq_users_tenant_email UNIQUE (tenant_id, email)
);

CREATE INDEX IF NOT EXISTS ix_users_tenant ON users(tenant_id);
CREATE INDEX IF NOT EXISTS ix_users_role   ON users(role);

COMMENT ON TABLE users IS 'Staff users within a tenant. v1 supports magic-link auth.';
COMMENT ON COLUMN users.role IS '0=Owner, 1=Manager, 2=Cashier, 3=Kitchen, 4=Accountant';

-- -----------------------------------------------------------------------------
-- locations — physical outlets (restaurants/cafés/hotel F&B venues)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS locations (
    id             uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id      uuid          NOT NULL,
    code           varchar(20)   NOT NULL,
    name           varchar(200)  NOT NULL,
    address_line1  varchar(255)  NOT NULL,
    address_line2  varchar(255)  NULL,
    city           varchar(100)  NOT NULL,
    country_code   char(2)       NOT NULL DEFAULT 'LK',
    time_zone      varchar(64)   NOT NULL DEFAULT 'Asia/Colombo',
    currency       char(3)       NOT NULL DEFAULT 'LKR',
    phone_e164     varchar(20)   NULL,
    is_active      boolean       NOT NULL DEFAULT true,
    created_at     timestamptz   NOT NULL DEFAULT now(),
    updated_at     timestamptz   NOT NULL DEFAULT now(),
    created_by     uuid          NULL,
    updated_by     uuid          NULL,
    is_deleted     boolean       NOT NULL DEFAULT false,
    CONSTRAINT uq_locations_tenant_code UNIQUE (tenant_id, code)
);

CREATE INDEX IF NOT EXISTS ix_locations_tenant ON locations(tenant_id);

COMMENT ON TABLE locations IS 'Physical outlets owned by the tenant. v1 ships with one location per tenant; v2 supports many.';

-- =============================================================================
-- Defence-in-depth: Row-Level Security
-- Even though each tenant has its own DB, we keep RLS on so a misconfigured
-- connection cannot cross-tenant-leak. The application sets
--   app.tenant_id = '<uuid>'
-- on every tenant connection as it opens (TenantGucConnectionInterceptor), and
-- migration 0051 FORCEs RLS so the owning app role cannot bypass these policies.
-- =============================================================================

ALTER TABLE users     ENABLE ROW LEVEL SECURITY;
ALTER TABLE locations ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS p_users_tenant     ON users;
DROP POLICY IF EXISTS p_locations_tenant ON locations;

CREATE POLICY p_users_tenant ON users
    USING (tenant_id::text = current_setting('app.tenant_id', true));

CREATE POLICY p_locations_tenant ON locations
    USING (tenant_id::text = current_setting('app.tenant_id', true));
