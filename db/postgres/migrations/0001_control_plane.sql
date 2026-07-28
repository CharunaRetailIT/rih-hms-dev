-- =============================================================================
-- Migration 0001: Control plane schema
-- Target DB: hms_control
-- Run with:  psql -d hms_control -f db/postgres/migrations/0001_control_plane.sql
-- =============================================================================

CREATE SCHEMA IF NOT EXISTS control;
SET search_path TO control, public;

-- -----------------------------------------------------------------------------
-- tenants — every customer that has signed up for the SaaS
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS control.tenants (
    id                uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    slug              varchar(60)   NOT NULL UNIQUE,
    display_name      varchar(200)  NOT NULL,
    database_name     varchar(100)  NOT NULL,
    database_host     varchar(255)  NOT NULL,
    status            int           NOT NULL DEFAULT 0,  -- 0=Pending..7=Deleted (see Domain/Tenant.cs)
    plan              varchar(40)   NOT NULL DEFAULT 'starter',
    trial_ends_at     timestamptz   NULL,
    owner_email       varchar(255)  NULL,
    country_code      char(2)       NOT NULL DEFAULT 'LK',
    default_currency  char(3)       NOT NULL DEFAULT 'LKR',
    time_zone         varchar(64)   NOT NULL DEFAULT 'Asia/Colombo',
    created_at        timestamptz   NOT NULL DEFAULT now(),
    updated_at        timestamptz   NOT NULL DEFAULT now(),
    is_deleted        boolean       NOT NULL DEFAULT false
);

CREATE INDEX IF NOT EXISTS ix_tenants_status ON control.tenants(status);
CREATE INDEX IF NOT EXISTS ix_tenants_plan   ON control.tenants(plan);

COMMENT ON TABLE  control.tenants IS 'One row per customer of the SaaS platform. Each tenant has its own hms_tenant_<slug> Postgres database.';
COMMENT ON COLUMN control.tenants.status IS '0=Pending, 1=Provisioning, 2=Trialing, 3=Active, 4=PastDue, 5=Suspended, 6=Cancelled, 7=Deleted';

-- -----------------------------------------------------------------------------
-- subscriptions — Stripe / PayHere subscription state (Sprint 2)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS control.subscriptions (
    id                  uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id           uuid          NOT NULL REFERENCES control.tenants(id),
    provider            varchar(40)   NOT NULL,   -- 'stripe' | 'payhere' | 'manual'
    provider_customer_id varchar(255) NULL,
    provider_sub_id      varchar(255) NULL,
    plan                varchar(40)   NOT NULL,
    status              varchar(40)   NOT NULL,   -- 'trialing'|'active'|'past_due'|'cancelled'
    current_period_start timestamptz  NULL,
    current_period_end   timestamptz  NULL,
    cancelled_at         timestamptz  NULL,
    created_at          timestamptz   NOT NULL DEFAULT now(),
    updated_at          timestamptz   NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS ix_subscriptions_tenant ON control.subscriptions(tenant_id);
CREATE INDEX IF NOT EXISTS ix_subscriptions_status ON control.subscriptions(status);
