-- =============================================================================
-- Migration 0110: FCM device tokens (#floor-push, Phase 4) — the Flutter handheld
-- app's mobile sibling of push_subscriptions (web/VAPID). Captured on waiter
-- sign-in; drives floor-scoped push for new guest QR orders reaching the app even
-- when it's backgrounded or fully closed.
-- =============================================================================

CREATE TABLE IF NOT EXISTS device_tokens (
    id           uuid         PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id    uuid         NOT NULL,
    user_id      uuid         NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token        varchar(300) NOT NULL,
    platform     varchar(10)  NOT NULL DEFAULT 'android',   -- android | ios
    last_used_at timestamptz  NULL,
    created_at   timestamptz  NOT NULL DEFAULT now(),
    updated_at   timestamptz  NOT NULL DEFAULT now(),
    created_by   uuid         NULL,
    updated_by   uuid         NULL,
    is_deleted   boolean      NOT NULL DEFAULT false,
    CONSTRAINT uq_device_tokens_token UNIQUE (tenant_id, token)
);
CREATE INDEX IF NOT EXISTS ix_device_tokens_tenant ON device_tokens(tenant_id);
CREATE INDEX IF NOT EXISTS ix_device_tokens_user   ON device_tokens(user_id);

ALTER TABLE device_tokens ENABLE ROW LEVEL SECURITY;
ALTER TABLE device_tokens FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS p_device_tokens_tenant ON device_tokens;
CREATE POLICY p_device_tokens_tenant ON device_tokens USING (tenant_id::text = current_setting('app.tenant_id', true));
