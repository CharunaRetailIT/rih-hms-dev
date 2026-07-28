-- =============================================================================
-- Migration 0032: Activity / audit log (#77)
-- Append-only record of sensitive actions — who did what, when — for cash control
-- and dispute resolution: settlements, voids, discounts, shift open/close,
-- permission changes, AR receipts, stock counts. Written best-effort (a logging
-- failure must never break the underlying action).
-- =============================================================================

CREATE TABLE IF NOT EXISTS activity_log (
    id          uuid         PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id   uuid         NOT NULL,
    actor_id    uuid         NULL,
    actor_name  varchar(160) NULL,
    actor_role  varchar(40)  NULL,
    action      varchar(60)  NOT NULL,     -- order.settle, order.void, order.discount, shift.open, ...
    entity_type varchar(40)  NULL,
    entity_id   uuid         NULL,
    summary     varchar(400) NULL,
    meta        text         NULL,
    created_at  timestamptz  NOT NULL DEFAULT now(),
    updated_at  timestamptz  NOT NULL DEFAULT now(),
    created_by  uuid         NULL,
    updated_by  uuid         NULL,
    is_deleted  boolean      NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_activity_log_tenant_at ON activity_log(tenant_id, created_at DESC);
CREATE INDEX IF NOT EXISTS ix_activity_log_action    ON activity_log(tenant_id, action);

ALTER TABLE activity_log ENABLE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS p_activity_log_tenant ON activity_log;
CREATE POLICY p_activity_log_tenant ON activity_log USING (tenant_id::text = current_setting('app.tenant_id', true));
