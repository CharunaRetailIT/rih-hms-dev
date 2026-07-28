-- =============================================================================
-- Migration 0035: Loyalty program (#66) — built on the CRM customer (#70)
-- Customers earn points on settled bills (earn_rate points per LKR) and redeem
-- them at the till as a "loyalty" tender (redeem_value LKR per point). Every
-- movement is recorded in loyalty_transactions. Tiers + scheduled expiry are
-- follow-on; this captures earn / balance / redeem / ledger.
-- =============================================================================

-- Org-level loyalty config (off by default).
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS loyalty_enabled      boolean      NOT NULL DEFAULT false;
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS loyalty_earn_rate    numeric(10,4) NOT NULL DEFAULT 0;  -- points earned per 1 LKR spent
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS loyalty_redeem_value numeric(10,4) NOT NULL DEFAULT 1;  -- LKR a point is worth on redemption

-- Per-customer balances.
ALTER TABLE customers ADD COLUMN IF NOT EXISTS loyalty_points          numeric(15,4) NOT NULL DEFAULT 0;
ALTER TABLE customers ADD COLUMN IF NOT EXISTS loyalty_lifetime_points numeric(15,4) NOT NULL DEFAULT 0;

CREATE TABLE IF NOT EXISTS loyalty_transactions (
    id            uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id     uuid          NOT NULL,
    customer_id   uuid          NOT NULL,
    order_id      uuid          NULL,
    txn_type      varchar(20)   NOT NULL,                 -- earn | redeem | adjust
    points        numeric(15,4) NOT NULL,                 -- + earned, − redeemed
    balance_after numeric(15,4) NOT NULL,
    note          varchar(200)  NULL,
    created_at    timestamptz   NOT NULL DEFAULT now(),
    updated_at    timestamptz   NOT NULL DEFAULT now(),
    created_by    uuid          NULL,
    updated_by    uuid          NULL,
    is_deleted    boolean       NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_loyalty_txn_customer ON loyalty_transactions(customer_id);

ALTER TABLE loyalty_transactions ENABLE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS p_loyalty_txn_tenant ON loyalty_transactions;
CREATE POLICY p_loyalty_txn_tenant ON loyalty_transactions USING (tenant_id::text = current_setting('app.tenant_id', true));
