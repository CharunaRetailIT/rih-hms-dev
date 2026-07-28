-- =============================================================================
-- Migration 0006: Delivery aggregators (Uber Eats / PickMe) — mock-ready
-- Target DB: any hms_tenant_<slug>
-- Implements the contract in docs/integration/uber-eats.md so we can test the
-- full delivery flow with a simulator now, and swap in the real APIs later.
-- =============================================================================

-- orders: delivery + aggregator fields (order_source + external_order_id already exist)
ALTER TABLE orders ADD COLUMN IF NOT EXISTS delivery_address   varchar(500) NULL;
ALTER TABLE orders ADD COLUMN IF NOT EXISTS delivery_phone     varchar(50)  NULL;
ALTER TABLE orders ADD COLUMN IF NOT EXISTS delivery_notes     varchar(500) NULL;
ALTER TABLE orders ADD COLUMN IF NOT EXISTS promised_time      timestamptz  NULL;
ALTER TABLE orders ADD COLUMN IF NOT EXISTS aggregator_payload text         NULL;

-- Idempotency: a given (source, external id) maps to exactly one order.
CREATE UNIQUE INDEX IF NOT EXISTS uq_orders_external_source
    ON orders(tenant_id, order_source, external_order_id)
    WHERE external_order_id IS NOT NULL;

-- -----------------------------------------------------------------------------
-- aggregator_outbox — outbound status callbacks to the aggregator (accept,
-- preparing, ready, picked_up, cancel). In dev these are "sent" by a mock
-- processor; in prod a Hangfire worker POSTs them with retry.
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS aggregator_outbox (
    id                uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id         uuid          NOT NULL,
    aggregator        varchar(20)   NOT NULL,        -- ubereats | pickme
    external_order_id varchar(100)  NOT NULL,
    operation         varchar(40)   NOT NULL,        -- accept | status | cancel
    payload_json      text          NOT NULL,
    status            varchar(20)   NOT NULL DEFAULT 'pending', -- pending | sent | failed
    attempts          int           NOT NULL DEFAULT 0,
    last_error        varchar(2000) NULL,
    created_at        timestamptz   NOT NULL DEFAULT now(),
    updated_at        timestamptz   NOT NULL DEFAULT now(),
    sent_at           timestamptz   NULL
);
CREATE INDEX IF NOT EXISTS ix_aggregator_outbox_pending
    ON aggregator_outbox(tenant_id, status) WHERE status = 'pending';

-- RLS
ALTER TABLE aggregator_outbox ENABLE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS p_aggregator_outbox_tenant ON aggregator_outbox;
CREATE POLICY p_aggregator_outbox_tenant ON aggregator_outbox
    USING (tenant_id::text = current_setting('app.tenant_id', true));
