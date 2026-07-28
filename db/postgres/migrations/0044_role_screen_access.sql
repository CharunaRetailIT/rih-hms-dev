-- =============================================================================
-- Migration 0044: Function-level (per-screen) permissions (#71 follow-on)
-- Owners can hide whole screens/functions from a role, on top of the built-in
-- RBAC policies that already gate the API server-side. A role with no row for a
-- screen keeps the default (visible) behaviour.
-- =============================================================================

CREATE TABLE IF NOT EXISTS role_screen_access (
    id         uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id  uuid        NOT NULL,
    role       int         NOT NULL,            -- 1 Manager · 2 Cashier · 3 Kitchen · 4 Accountant
    screen     varchar(40) NOT NULL,            -- nav key (e.g. /reports, /inventory)
    allowed    boolean     NOT NULL DEFAULT true,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    created_by uuid        NULL,
    updated_by uuid        NULL,
    is_deleted boolean     NOT NULL DEFAULT false
);
CREATE UNIQUE INDEX IF NOT EXISTS uq_role_screen ON role_screen_access(tenant_id, role, screen) WHERE is_deleted = false;

ALTER TABLE role_screen_access ENABLE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS p_role_screen_tenant ON role_screen_access;
CREATE POLICY p_role_screen_tenant ON role_screen_access USING (tenant_id::text = current_setting('app.tenant_id', true));
