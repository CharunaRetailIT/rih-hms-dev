-- =============================================================================
-- Migration 0031: Granular role permissions (#71)
-- Per-role limits enforced at the POS: a max discount % a role may apply, and
-- whether it may void a bill or comp / open the drawer (no-sale). Owner (role 0)
-- always bypasses. A role with no row gets permissive defaults (max 100%, all on)
-- so existing behaviour is unchanged until an owner tightens it.
-- =============================================================================

CREATE TABLE IF NOT EXISTS role_permissions (
    id                   uuid         PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id            uuid         NOT NULL,
    role                 int          NOT NULL,            -- 1 Manager · 2 Cashier · 3 Kitchen · 4 Accountant
    max_discount_percent numeric(8,4) NOT NULL DEFAULT 100,
    can_apply_discount   boolean      NOT NULL DEFAULT true,
    can_void             boolean      NOT NULL DEFAULT true,
    can_comp             boolean      NOT NULL DEFAULT true,   -- comp / no-sale drawer open
    created_at           timestamptz  NOT NULL DEFAULT now(),
    updated_at           timestamptz  NOT NULL DEFAULT now(),
    created_by           uuid         NULL,
    updated_by           uuid         NULL,
    is_deleted           boolean      NOT NULL DEFAULT false,
    CONSTRAINT uq_role_permission_role UNIQUE (tenant_id, role)
);
CREATE INDEX IF NOT EXISTS ix_role_permissions_tenant ON role_permissions(tenant_id);

ALTER TABLE role_permissions ENABLE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS p_role_permissions_tenant ON role_permissions;
CREATE POLICY p_role_permissions_tenant ON role_permissions USING (tenant_id::text = current_setting('app.tenant_id', true));
