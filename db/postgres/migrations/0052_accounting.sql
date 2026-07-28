-- =============================================================================
-- Migration 0052: GL / accounting (#73) — chart of accounts, double-entry
--                 journals, expenses, supplier (AP) payments.
-- Target DB: any hms_tenant_<slug>
--
-- Double-entry general ledger. Sales (settled orders) and purchases (GRNs) are
-- posted into balanced journal entries by the AccountingService; expenses and
-- supplier payments each create their own journal too. A trial balance + AP
-- aging report and a CSV export close the loop for the Accountant role.
--
-- RLS: every table ENABLEs + FORCEs row-level security with the standard tenant
-- policy. FORCE is required now (0051 only force-covered tables that existed when
-- it ran) so the owning app role is subject to the policy; the app sets
-- app.tenant_id per connection (TenantGucConnectionInterceptor).
-- =============================================================================

-- -----------------------------------------------------------------------------
-- gl_accounts — chart of accounts
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS gl_accounts (
    id            uuid         PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id     uuid         NOT NULL,
    code          varchar(20)  NOT NULL,
    name          varchar(160) NOT NULL,
    account_type  varchar(20)  NOT NULL,        -- asset | liability | equity | income | expense
    is_system     boolean      NOT NULL DEFAULT false,  -- seeded default account (used by the posting engine)
    is_active     boolean      NOT NULL DEFAULT true,
    created_at    timestamptz  NOT NULL DEFAULT now(),
    updated_at    timestamptz  NOT NULL DEFAULT now(),
    created_by    uuid         NULL,
    updated_by    uuid         NULL,
    is_deleted    boolean      NOT NULL DEFAULT false,
    CONSTRAINT uq_gl_accounts_tenant_code UNIQUE (tenant_id, code)
);

-- -----------------------------------------------------------------------------
-- gl_journal_entries — a balanced double-entry document (header)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS gl_journal_entries (
    id           uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id    uuid          NOT NULL,
    entry_no     varchar(30)   NOT NULL,
    entry_date   date          NOT NULL,
    memo         varchar(300)  NULL,
    source       varchar(20)   NOT NULL DEFAULT 'manual',  -- sales | purchase | expense | payment | manual
    source_ref   varchar(60)   NULL,                       -- order no / GRN no / etc (idempotency key per source)
    location_id  uuid          NULL,
    status       varchar(20)   NOT NULL DEFAULT 'posted',  -- draft | posted | void
    posted_at    timestamptz   NULL,
    created_at   timestamptz   NOT NULL DEFAULT now(),
    updated_at   timestamptz   NOT NULL DEFAULT now(),
    created_by   uuid          NULL,
    updated_by   uuid          NULL,
    is_deleted   boolean       NOT NULL DEFAULT false,
    CONSTRAINT uq_gl_entries_tenant_no UNIQUE (tenant_id, entry_no)
);
CREATE INDEX IF NOT EXISTS ix_gl_entries_date   ON gl_journal_entries(tenant_id, entry_date);
-- one auto-generated entry per source document (idempotent re-posting)
CREATE UNIQUE INDEX IF NOT EXISTS uq_gl_entries_source ON gl_journal_entries(tenant_id, source, source_ref)
    WHERE source_ref IS NOT NULL AND source <> 'manual';

-- -----------------------------------------------------------------------------
-- gl_journal_lines — debit/credit lines (sum(debit) = sum(credit) per entry)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS gl_journal_lines (
    id           uuid           PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id    uuid           NOT NULL,
    entry_id     uuid           NOT NULL REFERENCES gl_journal_entries(id) ON DELETE CASCADE,
    account_id   uuid           NOT NULL,
    account_code varchar(20)    NOT NULL,        -- snapshot
    account_name varchar(160)   NOT NULL,        -- snapshot
    debit        numeric(15, 4) NOT NULL DEFAULT 0,
    credit       numeric(15, 4) NOT NULL DEFAULT 0,
    line_memo    varchar(300)   NULL,
    sort         int            NOT NULL DEFAULT 0,
    created_at   timestamptz    NOT NULL DEFAULT now(),
    is_deleted   boolean        NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_gl_lines_entry   ON gl_journal_lines(entry_id);
CREATE INDEX IF NOT EXISTS ix_gl_lines_account ON gl_journal_lines(tenant_id, account_id);

-- -----------------------------------------------------------------------------
-- gl_expenses — operating / petty-cash expense capture (posts a journal)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS gl_expenses (
    id                 uuid           PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id          uuid           NOT NULL,
    expense_no         varchar(30)    NOT NULL,
    expense_date       date           NOT NULL,
    account_id         uuid           NOT NULL,        -- the expense account debited
    amount             numeric(15, 4) NOT NULL,
    payee              varchar(160)   NULL,
    payment_account_id uuid           NOT NULL,        -- cash/bank credited
    payment_method     varchar(30)    NULL,            -- cash | card | bank
    memo               varchar(300)   NULL,
    location_id        uuid           NULL,
    journal_entry_id   uuid           NULL,
    created_at         timestamptz    NOT NULL DEFAULT now(),
    updated_at         timestamptz    NOT NULL DEFAULT now(),
    created_by         uuid           NULL,
    updated_by         uuid           NULL,
    is_deleted         boolean        NOT NULL DEFAULT false,
    CONSTRAINT uq_gl_expenses_tenant_no UNIQUE (tenant_id, expense_no)
);
CREATE INDEX IF NOT EXISTS ix_gl_expenses_date ON gl_expenses(tenant_id, expense_date);

-- -----------------------------------------------------------------------------
-- ap_payments — payment to a supplier against AP (posts a journal)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS ap_payments (
    id                 uuid           PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id          uuid           NOT NULL,
    payment_no         varchar(30)    NOT NULL,
    supplier_id        uuid           NOT NULL,
    payment_date       date           NOT NULL,
    amount             numeric(15, 4) NOT NULL,
    payment_account_id uuid           NOT NULL,        -- cash/bank credited
    reference          varchar(100)   NULL,
    memo               varchar(300)   NULL,
    journal_entry_id   uuid           NULL,
    created_at         timestamptz    NOT NULL DEFAULT now(),
    updated_at         timestamptz    NOT NULL DEFAULT now(),
    created_by         uuid           NULL,
    updated_by         uuid           NULL,
    is_deleted         boolean        NOT NULL DEFAULT false,
    CONSTRAINT uq_ap_payments_tenant_no UNIQUE (tenant_id, payment_no)
);
CREATE INDEX IF NOT EXISTS ix_ap_payments_supplier ON ap_payments(tenant_id, supplier_id);

-- =============================================================================
-- Row-Level Security — ENABLE + FORCE + tenant policy (see migration 0051)
-- =============================================================================
DO $$
DECLARE t text;
BEGIN
    FOREACH t IN ARRAY ARRAY['gl_accounts','gl_journal_entries','gl_journal_lines','gl_expenses','ap_payments']
    LOOP
        EXECUTE format('ALTER TABLE public.%I ENABLE ROW LEVEL SECURITY', t);
        EXECUTE format('ALTER TABLE public.%I FORCE ROW LEVEL SECURITY', t);
        EXECUTE format('DROP POLICY IF EXISTS p_%s_tenant ON public.%I', t, t);
        EXECUTE format(
            'CREATE POLICY p_%s_tenant ON public.%I USING (tenant_id::text = current_setting(''app.tenant_id'', true))',
            t, t);
    END LOOP;
END $$;
