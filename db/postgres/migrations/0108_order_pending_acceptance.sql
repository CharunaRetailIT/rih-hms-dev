-- =============================================================================
-- Migration 0108: Order acceptance gate for guest QR orders (#108 follow-on).
-- A guest QR order now lands with pending_acceptance=true instead of being
-- auto-confirmed to the kitchen — a steward must accept it first (POST
-- /api/v1/orders/{id}/accept). POS/aggregator orders never set this flag and
-- their existing confirm-is-immediate behavior is unchanged.
-- =============================================================================

ALTER TABLE orders ADD COLUMN IF NOT EXISTS pending_acceptance boolean NOT NULL DEFAULT false;
CREATE INDEX IF NOT EXISTS ix_orders_pending_acceptance ON orders(tenant_id, pending_acceptance) WHERE pending_acceptance;
