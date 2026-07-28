-- =============================================================================
-- Migration 0068 (CONTROL db / hms_main): RIT's own subscription-billing taxes (#110/#111).
-- Target DB: hms_main (filename contains "control" → control-plane classifier).
--
-- This is the tax RIT charges on its SaaS invoices — NOT the tenant's sales tax.
-- RIT admin configures one or many. Scope decides who pays, by the tenant's country
-- vs RIT's home country (Billing:HomeCountry, default "LK"):
--   domestic → tenant in home country (e.g. SL VAT 18%)
--   export   → tenant abroad (export of services; usually nil for SL)
--   all      → always
-- Seeded with SL VAT 18% domestic-only, so a UAE/foreign signup is zero-rated.
-- =============================================================================
SET search_path TO control, public;

CREATE TABLE IF NOT EXISTS control.billing_taxes (
    id           uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    code         varchar(40)   NOT NULL UNIQUE,
    name         varchar(120)  NOT NULL,
    rate_percent numeric(6,3)  NOT NULL DEFAULT 0,
    scope        varchar(20)   NOT NULL DEFAULT 'domestic',   -- domestic | export | all
    sort_order   int           NOT NULL DEFAULT 0,
    is_active    boolean       NOT NULL DEFAULT true,
    created_at   timestamptz   NOT NULL DEFAULT now(),
    updated_at   timestamptz   NOT NULL DEFAULT now(),
    is_deleted   boolean       NOT NULL DEFAULT false
);

INSERT INTO control.billing_taxes (code, name, rate_percent, scope, sort_order) VALUES
  ('vat', 'VAT', 18.000, 'domestic', 1)
ON CONFLICT (code) DO NOTHING;

-- New control-schema table is postgres-owned; hand it to the app role (owner of control.tenants).
DO $$
DECLARE approle text;
BEGIN
    SELECT tableowner INTO approle FROM pg_tables WHERE schemaname='control' AND tablename='tenants';
    IF approle IS NOT NULL AND approle <> 'postgres' THEN
        EXECUTE format('ALTER TABLE control.billing_taxes OWNER TO %I', approle);
    END IF;
END $$;
