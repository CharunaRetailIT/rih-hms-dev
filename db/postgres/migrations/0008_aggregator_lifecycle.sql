-- =============================================================================
-- Migration 0008: Real aggregator order lifecycle + menu availability (86) sync
-- pending → accepted/preparing (with prep time) → ready (for pickup) → picked_up
-- + per-product online availability that syncs to Uber/PickMe.
-- =============================================================================

-- Delivery lifecycle, separate from the financial status (open/settled/void).
ALTER TABLE orders ADD COLUMN IF NOT EXISTS aggregator_status varchar(30) NULL;
    -- pending | preparing | ready | picked_up | rejected | cancelled
ALTER TABLE orders ADD COLUMN IF NOT EXISTS prep_minutes int NULL;
ALTER TABLE orders ADD COLUMN IF NOT EXISTS accepted_at  timestamptz NULL;
ALTER TABLE orders ADD COLUMN IF NOT EXISTS ready_at     timestamptz NULL;
ALTER TABLE orders ADD COLUMN IF NOT EXISTS picked_up_at timestamptz NULL;
CREATE INDEX IF NOT EXISTS ix_orders_incoming
    ON orders(tenant_id, aggregator_status) WHERE aggregator_status = 'pending';

-- Per-product online availability (86 an item on the aggregator menus).
ALTER TABLE products ADD COLUMN IF NOT EXISTS is_available_online boolean NOT NULL DEFAULT true;

-- Default prep time per outlet (merchant-configurable).
ALTER TABLE locations ADD COLUMN IF NOT EXISTS default_prep_minutes int NOT NULL DEFAULT 20;
