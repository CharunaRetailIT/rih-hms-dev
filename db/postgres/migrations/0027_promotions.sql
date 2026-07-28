-- =============================================================================
-- Migration 0027: Promotions engine (#65)
-- Auto-applied, schedulable promotions. v1 ships two legacy promo types:
--   product_discount — % or amount off a product (optionally min-qty, time-windowed → happy hour)
--   bill_value       — spend ≥ threshold → % or amount off the bill
-- (buy_x_get_y, bundle, bank_bin are follow-on types under the same tables.)
-- Scheduling: date range + day-of-week mask + daily time window + order-type scope.
-- The resolved discount is snapshotted onto the order (promotion_discount_amount)
-- and the applied promotions recorded in order_promotions for reporting.
-- =============================================================================

CREATE TABLE IF NOT EXISTS promotions (
    id              uuid         PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id       uuid         NOT NULL,
    code            varchar(30)  NOT NULL,
    name            varchar(120) NOT NULL,
    promo_type      varchar(20)  NOT NULL,          -- product_discount | bill_value | buy_x_get_y | bundle
    is_active       boolean      NOT NULL DEFAULT true,
    auto_apply      boolean      NOT NULL DEFAULT true,
    priority        int          NOT NULL DEFAULT 0, -- lower = evaluated first
    starts_on       date         NULL,
    ends_on         date         NULL,
    days_mask       int          NOT NULL DEFAULT 127, -- bit0=Mon … bit6=Sun; 127 = every day
    start_time      time         NULL,               -- daily window (happy hour); null = all day
    end_time        time         NULL,
    applies_to_order_type varchar(20) NULL,          -- dine_in | takeaway | delivery; null = any
    display_message varchar(160) NULL,
    created_at      timestamptz  NOT NULL DEFAULT now(),
    updated_at      timestamptz  NOT NULL DEFAULT now(),
    created_by      uuid         NULL,
    updated_by      uuid         NULL,
    is_deleted      boolean      NOT NULL DEFAULT false,
    CONSTRAINT uq_promotion_code UNIQUE (tenant_id, code)
);
CREATE INDEX IF NOT EXISTS ix_promotions_tenant ON promotions(tenant_id);

-- Type-specific rules. Columns are interpreted per the parent promo_type.
CREATE TABLE IF NOT EXISTS promotion_lines (
    id                 uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id          uuid          NOT NULL,
    promotion_id       uuid          NOT NULL REFERENCES promotions(id) ON DELETE CASCADE,
    product_id         uuid          NULL,           -- product_discount / buy_x_get_y (the "get") / bundle
    min_qty            numeric(15,4) NOT NULL DEFAULT 0,  -- product_discount: qty threshold; buy_x: buy qty
    bill_from          numeric(18,4) NOT NULL DEFAULT 0,  -- bill_value: lower bound
    bill_to            numeric(18,4) NULL,                -- bill_value: upper bound (null = ∞)
    get_product_id     uuid          NULL,           -- buy_x_get_y: the rewarded product
    get_qty            numeric(15,4) NOT NULL DEFAULT 0,
    discount_percent   numeric(8,4)  NOT NULL DEFAULT 0,
    discount_amount    numeric(18,4) NOT NULL DEFAULT 0,
    bundle_price       numeric(18,4) NULL,           -- bundle: fixed group price
    created_at         timestamptz   NOT NULL DEFAULT now(),
    updated_at         timestamptz   NOT NULL DEFAULT now(),
    created_by         uuid          NULL,
    updated_by         uuid          NULL,
    is_deleted         boolean       NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_promotion_lines_promo ON promotion_lines(promotion_id);
CREATE INDEX IF NOT EXISTS ix_promotion_lines_tenant ON promotion_lines(tenant_id);

-- What actually fired on an order (for the bill + promo-usage reporting).
CREATE TABLE IF NOT EXISTS order_promotions (
    id              uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id       uuid          NOT NULL,
    order_id        uuid          NOT NULL,
    promotion_id    uuid          NOT NULL,
    code            varchar(30)   NOT NULL,
    name            varchar(120)  NOT NULL,
    discount_amount numeric(18,4) NOT NULL DEFAULT 0,
    created_at      timestamptz   NOT NULL DEFAULT now(),
    updated_at      timestamptz   NOT NULL DEFAULT now(),
    created_by      uuid          NULL,
    updated_by      uuid          NULL,
    is_deleted      boolean       NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_order_promotions_order ON order_promotions(order_id);
CREATE INDEX IF NOT EXISTS ix_order_promotions_tenant ON order_promotions(tenant_id);

-- Snapshot of the auto-applied promotion discount on the bill.
ALTER TABLE orders ADD COLUMN IF NOT EXISTS promotion_discount_amount numeric(18,4) NOT NULL DEFAULT 0;

-- RLS
ALTER TABLE promotions       ENABLE ROW LEVEL SECURITY;
ALTER TABLE promotion_lines  ENABLE ROW LEVEL SECURITY;
ALTER TABLE order_promotions ENABLE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS p_promotions_tenant ON promotions;
DROP POLICY IF EXISTS p_promotion_lines_tenant ON promotion_lines;
DROP POLICY IF EXISTS p_order_promotions_tenant ON order_promotions;
CREATE POLICY p_promotions_tenant       ON promotions       USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_promotion_lines_tenant  ON promotion_lines  USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_order_promotions_tenant ON order_promotions USING (tenant_id::text = current_setting('app.tenant_id', true));
