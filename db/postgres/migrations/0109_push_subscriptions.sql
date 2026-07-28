-- =============================================================================
-- Migration 0109: Web Push subscriptions (VAPID) — Phase 3 of #floor-push. A staff
-- member's browser subscription, captured once they opt in from the Floor screen.
-- Drives floor-scoped push for new guest QR orders, even with the tab/app closed.
-- =============================================================================

CREATE TABLE IF NOT EXISTS push_subscriptions (
    id           uuid         PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id    uuid         NOT NULL,
    user_id      uuid         NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    endpoint     varchar(500) NOT NULL,
    p256dh       varchar(300) NOT NULL,
    auth         varchar(300) NOT NULL,
    last_used_at timestamptz  NULL,
    created_at   timestamptz  NOT NULL DEFAULT now(),
    updated_at   timestamptz  NOT NULL DEFAULT now(),
    created_by   uuid         NULL,
    updated_by   uuid         NULL,
    is_deleted   boolean      NOT NULL DEFAULT false,
    CONSTRAINT uq_push_subscriptions_endpoint UNIQUE (tenant_id, endpoint)
);
CREATE INDEX IF NOT EXISTS ix_push_subscriptions_tenant ON push_subscriptions(tenant_id);
CREATE INDEX IF NOT EXISTS ix_push_subscriptions_user   ON push_subscriptions(user_id);

ALTER TABLE push_subscriptions ENABLE ROW LEVEL SECURITY;
ALTER TABLE push_subscriptions FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS p_push_subscriptions_tenant ON push_subscriptions;
CREATE POLICY p_push_subscriptions_tenant ON push_subscriptions USING (tenant_id::text = current_setting('app.tenant_id', true));
