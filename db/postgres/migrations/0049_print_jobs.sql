-- =============================================================================
-- Migration 0049: Print-job queue (#16 — server side)
-- A durable queue a venue Print Agent polls to print KOT chits + bills to thermal
-- hardware. The agent (a thin local process) is a separate deliverable; this is
-- the contract it drives: enqueue → poll queued → ack printed/failed.
-- =============================================================================

CREATE TABLE IF NOT EXISTS print_jobs (
    id           uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id    uuid          NOT NULL,
    location_id  uuid          NOT NULL,
    kind         varchar(20)   NOT NULL,            -- kot | bill
    order_id     uuid          NULL,
    printer_name varchar(80)   NULL,                -- target printer (station / cashier)
    payload      text          NOT NULL,            -- receipt/ticket HTML (or ESC/POS)
    status       varchar(20)   NOT NULL DEFAULT 'queued',  -- queued | printed | failed
    attempts     int           NOT NULL DEFAULT 0,
    error        varchar(300)  NULL,
    created_at   timestamptz   NOT NULL DEFAULT now(),
    updated_at   timestamptz   NOT NULL DEFAULT now(),
    created_by   uuid          NULL,
    updated_by   uuid          NULL,
    printed_at   timestamptz   NULL,
    is_deleted   boolean       NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_print_jobs_poll ON print_jobs(tenant_id, location_id, status, created_at);

ALTER TABLE print_jobs ENABLE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS p_print_jobs_tenant ON print_jobs;
CREATE POLICY p_print_jobs_tenant ON print_jobs USING (tenant_id::text = current_setting('app.tenant_id', true));
