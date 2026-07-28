-- =============================================================================
-- Migration 0106: Loyalty Card Schemes — matches the old system's "Card Type"
-- master (Type: Discount/Points/Promotion, Code, Name, Description, and a
-- tiered "Point Schema" of Bill Value From/To, Increment, Points for the
-- Points type). A customer enrolls into one scheme via customers.loyalty_card_scheme_id.
-- Kept separate from the existing loyalty_tiers table (unrelated concept).
-- =============================================================================

CREATE TABLE IF NOT EXISTS loyalty_card_schemes (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NOT NULL,
    code varchar(40) NOT NULL,
    name varchar(160) NOT NULL,
    description varchar(300),
    type varchar(20) NOT NULL DEFAULT 'points',
    discount_percent numeric(8,4) NOT NULL DEFAULT 0,
    promotion_id uuid,
    is_active boolean NOT NULL DEFAULT true,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    created_by uuid,
    updated_by uuid,
    is_deleted boolean NOT NULL DEFAULT false
);

CREATE UNIQUE INDEX IF NOT EXISTS uq_loyalty_card_schemes_tenant_code ON loyalty_card_schemes (tenant_id, code);

ALTER TABLE loyalty_card_schemes ENABLE ROW LEVEL SECURITY;
ALTER TABLE loyalty_card_schemes FORCE ROW LEVEL SECURITY;
CREATE POLICY p_loyalty_card_schemes_tenant ON loyalty_card_schemes
    USING (tenant_id::text = current_setting('app.tenant_id', true));

CREATE TABLE IF NOT EXISTS loyalty_card_scheme_tiers (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NOT NULL,
    scheme_id uuid NOT NULL REFERENCES loyalty_card_schemes(id) ON DELETE CASCADE,
    bill_from_value numeric(15,4) NOT NULL DEFAULT 0,
    bill_to_value numeric(15,4) NOT NULL DEFAULT 0,
    increment numeric(15,4) NOT NULL DEFAULT 0,
    points numeric(15,4) NOT NULL DEFAULT 0,
    sort_order int NOT NULL DEFAULT 0,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    created_by uuid,
    updated_by uuid,
    is_deleted boolean NOT NULL DEFAULT false
);

CREATE INDEX IF NOT EXISTS ix_loyalty_card_scheme_tiers_scheme ON loyalty_card_scheme_tiers (scheme_id);

ALTER TABLE loyalty_card_scheme_tiers ENABLE ROW LEVEL SECURITY;
ALTER TABLE loyalty_card_scheme_tiers FORCE ROW LEVEL SECURITY;
CREATE POLICY p_loyalty_card_scheme_tiers_tenant ON loyalty_card_scheme_tiers
    USING (tenant_id::text = current_setting('app.tenant_id', true));

ALTER TABLE customers ADD COLUMN IF NOT EXISTS loyalty_card_scheme_id uuid REFERENCES loyalty_card_schemes(id);
CREATE INDEX IF NOT EXISTS ix_customers_loyalty_card_scheme ON customers (loyalty_card_scheme_id);
