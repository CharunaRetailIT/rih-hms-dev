-- =============================================================================
-- Migration 0082: configurable approval workflows (#approvals).
-- Target DB: any hms_tenant_<slug> (filename has no "control" → tenant classifier).
--
-- rules → ordered steps (levels); a submitted document creates a request whose
-- actions carry single-use, time-limited tokens for email-link approval (no login).
-- Approve / Reject / Hold; remark mandatory for Reject & Hold (enforced in the API).
-- RLS: ENABLE + FORCE + tenant policy on every table (0051/0070 pattern).
-- =============================================================================

CREATE TABLE IF NOT EXISTS approval_rules (
    id          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id   uuid NOT NULL,
    doc_type    text NOT NULL,                 -- purchase_order | grn | stock_transfer
    name        text NOT NULL,
    min_amount  numeric(18,4) NOT NULL DEFAULT 0,
    location_id uuid NULL,
    sort_order  int  NOT NULL DEFAULT 0,
    is_active   boolean NOT NULL DEFAULT true,
    created_at  timestamptz NOT NULL DEFAULT now(),
    updated_at  timestamptz NOT NULL DEFAULT now(),
    created_by  uuid NULL, updated_by uuid NULL,
    is_deleted  boolean NOT NULL DEFAULT false
);

CREATE TABLE IF NOT EXISTS approval_rule_steps (
    id               uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id        uuid NOT NULL,
    rule_id          uuid NOT NULL,
    level            int  NOT NULL,
    approver_type    text NOT NULL DEFAULT 'role',   -- user | role | email
    approver_user_id uuid NULL,
    approver_role    int  NULL,
    approver_email   text NULL,
    approver_label   text NOT NULL,
    created_at  timestamptz NOT NULL DEFAULT now(),
    updated_at  timestamptz NOT NULL DEFAULT now(),
    created_by  uuid NULL, updated_by uuid NULL,
    is_deleted  boolean NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_appr_steps_rule ON approval_rule_steps (tenant_id, rule_id, level);

CREATE TABLE IF NOT EXISTS approval_requests (
    id                uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id         uuid NOT NULL,
    doc_type          text NOT NULL,
    doc_id            uuid NOT NULL,
    doc_number        text NOT NULL,
    doc_summary       text NOT NULL DEFAULT '{}',
    amount            numeric(18,4) NOT NULL DEFAULT 0,
    rule_id           uuid NOT NULL,
    status            text NOT NULL DEFAULT 'pending', -- pending | approved | rejected | on_hold
    current_level     int  NOT NULL DEFAULT 1,
    requested_by      uuid NULL,
    requested_by_name text NULL,
    decided_at        timestamptz NULL,
    created_at  timestamptz NOT NULL DEFAULT now(),
    updated_at  timestamptz NOT NULL DEFAULT now(),
    created_by  uuid NULL, updated_by uuid NULL,
    is_deleted  boolean NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_appr_req_doc ON approval_requests (tenant_id, doc_type, doc_id);

CREATE TABLE IF NOT EXISTS approval_actions (
    id               uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id        uuid NOT NULL,
    request_id       uuid NOT NULL,
    level            int  NOT NULL,
    approver_type    text NOT NULL DEFAULT 'role',
    approver_label   text NOT NULL,
    approver_email   text NULL,
    token            text NOT NULL,
    token_expires_at timestamptz NOT NULL,
    status           text NOT NULL DEFAULT 'pending',
    remark           text NULL,
    acted_by         text NULL,
    acted_at         timestamptz NULL,
    created_at  timestamptz NOT NULL DEFAULT now(),
    updated_at  timestamptz NOT NULL DEFAULT now(),
    created_by  uuid NULL, updated_by uuid NULL,
    is_deleted  boolean NOT NULL DEFAULT false
);
CREATE UNIQUE INDEX IF NOT EXISTS ux_appr_action_token ON approval_actions (token);
CREATE INDEX IF NOT EXISTS ix_appr_action_request ON approval_actions (tenant_id, request_id);

DO $$
DECLARE t text;
DECLARE app_role text := 'hms_app';   -- the runtime DB role; tables MUST be owned by it (RLS FORCE + grants)
BEGIN
    FOREACH t IN ARRAY ARRAY['approval_rules','approval_rule_steps','approval_requests','approval_actions']
    LOOP
        EXECUTE format('ALTER TABLE public.%I ENABLE ROW LEVEL SECURITY', t);
        EXECUTE format('ALTER TABLE public.%I FORCE ROW LEVEL SECURITY', t);
        EXECUTE format('DROP POLICY IF EXISTS p_%s_tenant ON public.%I', t, t);
        EXECUTE format('CREATE POLICY p_%s_tenant ON public.%I USING (tenant_id::text = current_setting(''app.tenant_id'', true))', t, t);
        -- If applied as a superuser (out-of-band), the tables would be postgres-owned → the app role
        -- gets "permission denied". Hand ownership to the runtime role to match every other table.
        IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = app_role) THEN
            EXECUTE format('ALTER TABLE public.%I OWNER TO %I', t, app_role);
        END IF;
    END LOOP;
END $$;
