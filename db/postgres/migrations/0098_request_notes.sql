-- =============================================================================
-- Migration 0098: Request Notes — a requisition raised by one location against
-- another, upstream of actual fulfillment (fulfilled by a Purchase Order in
-- "po" mode, or a Transfer of Goods in "transfer" mode). Same draft -> submit
-- -> pending/approved -> approve/reject -> remove workflow as stock_adjustments
-- (0095) and wastage_notes (0097). Approving never touches stock — it only
-- authorises the request to be picked up downstream. RLS: ENABLE+FORCE+tenant
-- policy, ownership handed to the app role (0051/0082/0083 pattern).
-- =============================================================================

CREATE TABLE IF NOT EXISTS request_notes (
    id                      uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id               uuid NOT NULL,
    request_number          varchar(20) NOT NULL,
    mode                    varchar(10) NOT NULL DEFAULT 'po',
    from_location_id        uuid NOT NULL,
    to_location_id          uuid NOT NULL,
    expected_delivery_date  date NOT NULL,
    status                  varchar(20) NOT NULL DEFAULT 'draft',
    common_remark           varchar(500) NULL,
    submitted_at            timestamptz NULL,
    approved_by             uuid NULL,
    approved_at             timestamptz NULL,
    rejected_by             uuid NULL,
    rejected_at             timestamptz NULL,
    reject_reason           varchar(200) NULL,
    created_at              timestamptz NOT NULL DEFAULT now(),
    updated_at              timestamptz NOT NULL DEFAULT now(),
    created_by              uuid NULL,
    updated_by              uuid NULL,
    is_deleted              boolean NOT NULL DEFAULT false
);
CREATE UNIQUE INDEX IF NOT EXISTS ux_request_notes_tenant_number ON request_notes (tenant_id, request_number);

CREATE TABLE IF NOT EXISTS request_note_lines (
    id            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id     uuid NOT NULL,
    request_id    uuid NOT NULL,
    product_id    uuid NOT NULL,
    sku           varchar(40) NOT NULL,
    product_name  varchar(200) NOT NULL,
    sih           numeric(15,4) NOT NULL DEFAULT 0,
    quantity      numeric(15,4) NOT NULL DEFAULT 0,
    remark        varchar(200) NULL,
    created_at    timestamptz NOT NULL DEFAULT now(),
    updated_at    timestamptz NOT NULL DEFAULT now(),
    created_by    uuid NULL,
    updated_by    uuid NULL,
    is_deleted    boolean NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_request_note_lines_request ON request_note_lines (request_id);

ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS request_note_prefix           varchar(10) NOT NULL DEFAULT 'REQ';
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS require_request_note_approval boolean     NOT NULL DEFAULT false;

DO $$
DECLARE app_role text := 'hms_app';
BEGIN
    EXECUTE 'ALTER TABLE public.request_notes ENABLE ROW LEVEL SECURITY';
    EXECUTE 'ALTER TABLE public.request_notes FORCE ROW LEVEL SECURITY';
    EXECUTE 'DROP POLICY IF EXISTS p_request_notes_tenant ON public.request_notes';
    EXECUTE 'CREATE POLICY p_request_notes_tenant ON public.request_notes USING (tenant_id::text = current_setting(''app.tenant_id'', true))';

    EXECUTE 'ALTER TABLE public.request_note_lines ENABLE ROW LEVEL SECURITY';
    EXECUTE 'ALTER TABLE public.request_note_lines FORCE ROW LEVEL SECURITY';
    EXECUTE 'DROP POLICY IF EXISTS p_request_note_lines_tenant ON public.request_note_lines';
    EXECUTE 'CREATE POLICY p_request_note_lines_tenant ON public.request_note_lines USING (tenant_id::text = current_setting(''app.tenant_id'', true))';

    -- If applied as a superuser the tables are postgres-owned → the app role hits
    -- "permission denied". Hand ownership to the runtime role like every other table.
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = app_role) THEN
        EXECUTE 'ALTER TABLE public.request_notes OWNER TO hms_app';
        EXECUTE 'ALTER TABLE public.request_note_lines OWNER TO hms_app';
    END IF;
END $$;
