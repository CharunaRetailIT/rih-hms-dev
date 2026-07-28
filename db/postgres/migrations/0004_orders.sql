-- =============================================================================
-- Migration 0004: Orders / POS core — orders, order_items, kitchen_tickets, payments
-- Target DB: any hms_tenant_<slug>
-- Run with:  psql -d hms_tenant_demo -f db/postgres/migrations/0004_orders.sql
--
-- This is the transactional heart of v1. Two business rules from
-- docs/v2-business-rules.md are implemented in OrderService (NOT triggers):
--   R1  Stock decrement on settlement
--   R2  KOT/BOT generation on confirm
-- =============================================================================

-- -----------------------------------------------------------------------------
-- orders — the bill / tab
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS orders (
    id                     uuid           PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id              uuid           NOT NULL,
    location_id            uuid           NOT NULL REFERENCES locations(id),
    order_number           varchar(20)    NOT NULL,
    order_type             varchar(20)    NOT NULL DEFAULT 'dine_in',  -- dine_in | takeaway | delivery
    order_source           varchar(20)    NOT NULL DEFAULT 'pos',      -- pos | ubereats | pickme
    external_order_id      varchar(100)   NULL,                        -- aggregator order ref (Sprint 4)
    table_label            varchar(40)    NULL,
    covers                 int            NULL,
    status                 varchar(20)    NOT NULL DEFAULT 'open',     -- open | confirmed | settled | void
    subtotal_amount        numeric(15, 4) NOT NULL DEFAULT 0,
    discount_amount        numeric(15, 4) NOT NULL DEFAULT 0,
    service_charge_amount  numeric(15, 4) NOT NULL DEFAULT 0,
    tax_amount             numeric(15, 4) NOT NULL DEFAULT 0,
    total_amount           numeric(15, 4) NOT NULL DEFAULT 0,
    customer_id            uuid           NULL,
    cashier_id             uuid           NULL,
    opened_at              timestamptz    NOT NULL DEFAULT now(),
    confirmed_at           timestamptz    NULL,
    settled_at             timestamptz    NULL,
    voided_at              timestamptz    NULL,
    void_reason            varchar(200)   NULL,
    created_at             timestamptz    NOT NULL DEFAULT now(),
    updated_at             timestamptz    NOT NULL DEFAULT now(),
    created_by             uuid           NULL,
    updated_by             uuid           NULL,
    is_deleted             boolean        NOT NULL DEFAULT false,
    CONSTRAINT uq_orders_tenant_number UNIQUE (tenant_id, order_number)
);
CREATE INDEX IF NOT EXISTS ix_orders_tenant_status ON orders(tenant_id, status);
CREATE INDEX IF NOT EXISTS ix_orders_location      ON orders(location_id);
CREATE INDEX IF NOT EXISTS ix_orders_open          ON orders(tenant_id, location_id) WHERE status = 'open';
CREATE INDEX IF NOT EXISTS ix_orders_external      ON orders(tenant_id, external_order_id) WHERE external_order_id IS NOT NULL;

COMMENT ON TABLE orders IS 'A bill/tab. order_source schema-ready for Uber Eats / PickMe (wired Sprint 4).';

-- -----------------------------------------------------------------------------
-- order_items — line items (snapshots product name/price at time of order)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS order_items (
    id              uuid           PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id       uuid           NOT NULL,
    order_id        uuid           NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    product_id      uuid           NOT NULL,
    sku             varchar(40)    NOT NULL,       -- snapshot
    product_name    varchar(200)   NOT NULL,       -- snapshot
    quantity        numeric(15, 4) NOT NULL DEFAULT 1,
    unit_price      numeric(15, 4) NOT NULL,        -- snapshot
    line_subtotal   numeric(15, 4) NOT NULL,        -- quantity * unit_price
    tax_rate        numeric(8, 4)  NOT NULL DEFAULT 0,
    tax_amount      numeric(15, 4) NOT NULL DEFAULT 0,
    line_total      numeric(15, 4) NOT NULL,
    station         varchar(20)    NOT NULL DEFAULT 'kitchen',  -- kitchen | bar
    notes           varchar(500)   NULL,                        -- "extra spicy", "no onion"
    kot_status      varchar(20)    NOT NULL DEFAULT 'pending',  -- pending | sent | preparing | ready
    is_stocked      boolean        NOT NULL DEFAULT true,
    created_at      timestamptz    NOT NULL DEFAULT now(),
    updated_at      timestamptz    NOT NULL DEFAULT now(),
    created_by      uuid           NULL,
    updated_by      uuid           NULL,
    is_deleted      boolean        NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_order_items_order ON order_items(order_id);

-- -----------------------------------------------------------------------------
-- kitchen_tickets — generated KOT/BOT slips (R2). One per station per confirm.
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS kitchen_tickets (
    id              uuid           PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id       uuid           NOT NULL,
    location_id     uuid           NOT NULL,
    order_id        uuid           NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    ticket_number   varchar(20)    NOT NULL,
    station         varchar(20)    NOT NULL,        -- kitchen | bar | pizza | coffee
    order_label     varchar(60)    NOT NULL,        -- "Table 5" / "Uber Eats" / "Takeaway"
    order_source    varchar(20)    NOT NULL DEFAULT 'pos',
    status          varchar(20)    NOT NULL DEFAULT 'new',  -- new | preparing | ready | served
    items_json      text           NOT NULL,        -- denormalized snapshot of the items on this ticket
    created_at      timestamptz    NOT NULL DEFAULT now(),
    updated_at      timestamptz    NOT NULL DEFAULT now(),
    created_by      uuid           NULL,
    updated_by      uuid           NULL,
    ready_at        timestamptz    NULL,
    served_at       timestamptz    NULL,
    is_deleted      boolean        NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_kitchen_tickets_active ON kitchen_tickets(tenant_id, location_id, status)
    WHERE status IN ('new', 'preparing', 'ready');

-- -----------------------------------------------------------------------------
-- payments — tenders against an order (multi-tender supported)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS payments (
    id              uuid           PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id       uuid           NOT NULL,
    order_id        uuid           NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    pay_type        varchar(30)    NOT NULL,        -- cash | card | ubereats_prepaid | pickme_prepaid
    amount          numeric(15, 4) NOT NULL,
    reference       varchar(100)   NULL,
    created_at      timestamptz    NOT NULL DEFAULT now(),
    updated_at      timestamptz    NOT NULL DEFAULT now(),
    created_by      uuid           NULL,
    updated_by      uuid           NULL,
    is_deleted      boolean        NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_payments_order ON payments(order_id);

-- -----------------------------------------------------------------------------
-- order_number_sequence — per-tenant daily counter for human-readable numbers
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS order_counters (
    tenant_id    uuid         NOT NULL,
    counter_date date         NOT NULL,
    last_value   int          NOT NULL DEFAULT 0,
    PRIMARY KEY (tenant_id, counter_date)
);

-- =============================================================================
-- Row-Level Security
-- =============================================================================
ALTER TABLE orders          ENABLE ROW LEVEL SECURITY;
ALTER TABLE order_items     ENABLE ROW LEVEL SECURITY;
ALTER TABLE kitchen_tickets ENABLE ROW LEVEL SECURITY;
ALTER TABLE payments        ENABLE ROW LEVEL SECURITY;
ALTER TABLE order_counters  ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS p_orders_tenant          ON orders;
DROP POLICY IF EXISTS p_order_items_tenant      ON order_items;
DROP POLICY IF EXISTS p_kitchen_tickets_tenant  ON kitchen_tickets;
DROP POLICY IF EXISTS p_payments_tenant         ON payments;
DROP POLICY IF EXISTS p_order_counters_tenant   ON order_counters;

CREATE POLICY p_orders_tenant         ON orders
    USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_order_items_tenant    ON order_items
    USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_kitchen_tickets_tenant ON kitchen_tickets
    USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_payments_tenant       ON payments
    USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_order_counters_tenant ON order_counters
    USING (tenant_id::text = current_setting('app.tenant_id', true));
