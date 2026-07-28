-- =============================================================================
-- Migration 0064: Control-plane billing catalog + subscription line-items (#109).
-- Target DB: hms_control (filename contains "control" → control-plane classifier).
--
-- Builds the commercial backbone on the subscriptions stub from 0001:
--   control.plans              — base plan catalog (RIT-priced; included locations/users)
--   control.addons             — add-on catalog (tab device seat, guest-QR, extra outlet…)
--   control.subscription_items — per-subscription line items (what quantities were bought)
-- Owners self-serve purchases against this catalog; RIT sets the prices + watches MRR.
-- =============================================================================
SET search_path TO control, public;

CREATE TABLE IF NOT EXISTS control.plans (
    id                 uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    code               varchar(40)   NOT NULL UNIQUE,
    name               varchar(120)  NOT NULL,
    monthly_price      numeric(15,2) NOT NULL DEFAULT 0,
    currency           char(3)       NOT NULL DEFAULT 'LKR',
    included_locations int           NOT NULL DEFAULT 1,
    included_users     int           NOT NULL DEFAULT 5,
    sort_order         int           NOT NULL DEFAULT 0,
    is_active          boolean       NOT NULL DEFAULT true,
    created_at         timestamptz   NOT NULL DEFAULT now(),
    updated_at         timestamptz   NOT NULL DEFAULT now(),
    is_deleted         boolean       NOT NULL DEFAULT false
);

CREATE TABLE IF NOT EXISTS control.addons (
    id          uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    code        varchar(40)   NOT NULL UNIQUE,
    name        varchar(120)  NOT NULL,
    unit        varchar(40)   NOT NULL,           -- per_device_month | flat_month | per_location_month
    unit_price  numeric(15,2) NOT NULL DEFAULT 0,
    currency    char(3)       NOT NULL DEFAULT 'LKR',
    is_active   boolean       NOT NULL DEFAULT true,
    created_at  timestamptz   NOT NULL DEFAULT now(),
    updated_at  timestamptz   NOT NULL DEFAULT now(),
    is_deleted  boolean       NOT NULL DEFAULT false
);

CREATE TABLE IF NOT EXISTS control.subscription_items (
    id              uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    subscription_id uuid          NOT NULL REFERENCES control.subscriptions(id),
    item_type       varchar(40)   NOT NULL,       -- plan | addon | location
    item_code       varchar(40)   NOT NULL,       -- plan/addon code (or 'location')
    quantity        int           NOT NULL DEFAULT 1,
    unit_price      numeric(15,2) NOT NULL DEFAULT 0,   -- price snapshot at purchase
    currency        char(3)       NOT NULL DEFAULT 'LKR',
    created_at      timestamptz   NOT NULL DEFAULT now(),
    updated_at      timestamptz   NOT NULL DEFAULT now(),
    is_deleted      boolean       NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_sub_items_sub ON control.subscription_items(subscription_id);

-- subscriptions (from 0001) predates the ControlEntity soft-delete column.
ALTER TABLE control.subscriptions ADD COLUMN IF NOT EXISTS is_deleted boolean NOT NULL DEFAULT false;

-- Seed the catalog from the RIT HMS onboarding design (Stitch Step 2). RIT edits prices in the admin console.
INSERT INTO control.plans (code, name, monthly_price, currency, included_locations, included_users, sort_order) VALUES
  ('lite',       'Lite',         4500, 'LKR',  1,   5, 1),   -- single POS, small restaurants/cafés (no KDS)
  ('pro',        'Pro',         12000, 'LKR',  1,  20, 2),   -- busy single-venue (most popular)
  ('enterprise', 'Enterprise',  28500, 'LKR',  1, 500, 3)    -- HOD multi-outlet; +extra_location per added outlet
ON CONFLICT (code) DO NOTHING;

INSERT INTO control.addons (code, name, unit, unit_price, currency) VALUES
  ('tab_device',     'Tab Ordering Module', 'per_device_month',   2500, 'LKR'),  -- per-device (owner-confirmed), design label said per-user
  ('guest_qr',       'Guest QR Ordering',   'flat_month',         1000, 'LKR'),  -- not in Stitch yet — confirm
  ('e_receipt',      'E-receipt Bundle',    'flat_month',         5000, 'LKR'),  -- 1,000 SMS/email
  ('rider_delivery', 'Rider Delivery',      'flat_month',         7500, 'LKR'),  -- own-fleet rider management
  ('extra_location', 'Additional Outlet',   'per_location_month', 5000, 'LKR')   -- HOD expansion
ON CONFLICT (code) DO NOTHING;

-- These tables are created by the postgres superuser; hand them to the app role that
-- owns the rest of the control schema (control.tenants) so the least-privilege app
-- connection can read/write them. The deploy's ownership-normalizer only covers the
-- public schema, so we do it here for the control schema. Idempotent.
DO $$
DECLARE approle text;
BEGIN
    SELECT tableowner INTO approle FROM pg_tables WHERE schemaname='control' AND tablename='tenants';
    IF approle IS NOT NULL AND approle <> 'postgres' THEN
        EXECUTE format('ALTER TABLE control.plans OWNER TO %I', approle);
        EXECUTE format('ALTER TABLE control.addons OWNER TO %I', approle);
        EXECUTE format('ALTER TABLE control.subscription_items OWNER TO %I', approle);
    END IF;
END $$;
