-- =============================================================================
-- Migration 0050: POS depth (#76) — stewards/tips, multi-currency tender,
--                 tour-operator commission. (Covers already exist on orders.)
-- Target DB: any hms_tenant_<slug>
--
-- Legacy parity: SMART_HMS attributes a dine-in bill to a steward (waiter),
-- captures a discretionary tip for staff payout, can tender a bill in a foreign
-- currency at a configured rate, and pays a commission to the tour operator that
-- brought the guests. None of that changes what the guest pays except the tip
-- (added to the bill) — the commission is a payable booked against the operator.
-- =============================================================================

-- -----------------------------------------------------------------------------
-- Servers (#76): a steward IS a user. Rather than a parallel people-master we add
-- an attribution flag on users (orthogonal to their permission Role) — a Cashier
-- or Manager can also be a server. Waiters who never sign in are just user rows
-- with a display name + is_server and no email/PIN. The POS "served by" dropdown
-- lists users where is_server = true; orders.steward_id points at users(id).
-- -----------------------------------------------------------------------------
ALTER TABLE users ADD COLUMN IF NOT EXISTS is_server boolean NOT NULL DEFAULT false;

-- -----------------------------------------------------------------------------
-- tour_operators — travel agents that bring guests for a commission
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS tour_operators (
    id                 uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id          uuid          NOT NULL,
    code               varchar(40)   NOT NULL,
    name               varchar(160)  NOT NULL,
    commission_percent numeric(8, 4) NOT NULL DEFAULT 0,
    is_active          boolean       NOT NULL DEFAULT true,
    created_at         timestamptz   NOT NULL DEFAULT now(),
    updated_at         timestamptz   NOT NULL DEFAULT now(),
    created_by         uuid          NULL,
    updated_by         uuid          NULL,
    is_deleted         boolean       NOT NULL DEFAULT false,
    CONSTRAINT uq_tour_operators_tenant_code UNIQUE (tenant_id, code)
);

-- -----------------------------------------------------------------------------
-- currencies — accepted tender currencies and their rate to the base currency
-- (base = OrgSettings.base_currency, rate_to_base = 1). Foreign tenders convert
-- amount × rate_to_base into base for settlement + reporting.
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS currencies (
    id           uuid           PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id    uuid           NOT NULL,
    code         varchar(3)     NOT NULL,        -- ISO 4217 (LKR, USD, EUR, GBP …)
    name         varchar(60)    NOT NULL,
    symbol       varchar(8)     NULL,
    rate_to_base numeric(18, 8) NOT NULL DEFAULT 1,
    is_base      boolean        NOT NULL DEFAULT false,
    is_active    boolean        NOT NULL DEFAULT true,
    created_at   timestamptz    NOT NULL DEFAULT now(),
    updated_at   timestamptz    NOT NULL DEFAULT now(),
    created_by   uuid           NULL,
    updated_by   uuid           NULL,
    is_deleted   boolean        NOT NULL DEFAULT false,
    CONSTRAINT uq_currencies_tenant_code UNIQUE (tenant_id, code)
);

-- -----------------------------------------------------------------------------
-- orders — steward attribution, tip, tour operator + commission snapshot
-- -----------------------------------------------------------------------------
ALTER TABLE orders ADD COLUMN IF NOT EXISTS steward_id             uuid           NULL;  -- references users(id) — the server the bill is attributed to
ALTER TABLE orders ADD COLUMN IF NOT EXISTS tip_amount             numeric(15, 4) NOT NULL DEFAULT 0;
ALTER TABLE orders ADD COLUMN IF NOT EXISTS tour_operator_id       uuid           NULL;
ALTER TABLE orders ADD COLUMN IF NOT EXISTS tour_commission_amount numeric(15, 4) NOT NULL DEFAULT 0;

-- -----------------------------------------------------------------------------
-- payments — currency of the tender + rate + converted base amount
-- (base_amount is what counts toward settling the bill; legacy/base tenders use
-- rate 1 so base_amount = amount).
-- -----------------------------------------------------------------------------
ALTER TABLE payments ADD COLUMN IF NOT EXISTS currency_code varchar(3)     NULL;
ALTER TABLE payments ADD COLUMN IF NOT EXISTS fx_rate       numeric(18, 8) NOT NULL DEFAULT 1;
ALTER TABLE payments ADD COLUMN IF NOT EXISTS base_amount   numeric(15, 4) NOT NULL DEFAULT 0;

-- =============================================================================
-- Row-Level Security
-- =============================================================================
ALTER TABLE tour_operators ENABLE ROW LEVEL SECURITY;
ALTER TABLE currencies     ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS p_tour_operators_tenant ON tour_operators;
DROP POLICY IF EXISTS p_currencies_tenant     ON currencies;

CREATE POLICY p_tour_operators_tenant ON tour_operators
    USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_currencies_tenant     ON currencies
    USING (tenant_id::text = current_setting('app.tenant_id', true));
