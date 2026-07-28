-- =============================================================================
-- Migration 0014: Shifts — cashier sessions + cash-up (Z-report)
-- A shift is a cashier's session at an outlet from open to close. Closing
-- declares the counted cash; the system computes expected cash (opening float +
-- cash that should be in the drawer) and the variance, plus a by-tender summary.
-- One OPEN shift per location at a time (partial unique index).
-- =============================================================================

ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS shift_prefix varchar(10) NOT NULL DEFAULT 'SH';

CREATE TABLE IF NOT EXISTS shifts (
    id              uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id       uuid          NOT NULL,
    location_id     uuid          NOT NULL REFERENCES locations(id),
    user_id         uuid          NULL,                       -- the cashier (from JWT sub)
    opened_by_name  varchar(200)  NULL,
    shift_number    varchar(20)   NOT NULL,
    opened_at       timestamptz   NOT NULL DEFAULT now(),
    opening_float   numeric(15,2) NOT NULL DEFAULT 0,         -- cash put in the drawer at open
    closed_at       timestamptz   NULL,
    status          varchar(20)   NOT NULL DEFAULT 'open',    -- open | closed
    declared_cash   numeric(15,2) NULL,                       -- counted at close
    expected_cash   numeric(15,2) NULL,                       -- float + cash drawer takings
    cash_variance   numeric(15,2) NULL,                       -- declared - expected
    total_sales     numeric(15,2) NOT NULL DEFAULT 0,         -- sum of settled order totals
    cash_sales      numeric(15,2) NOT NULL DEFAULT 0,         -- cash that should be in drawer
    card_sales      numeric(15,2) NOT NULL DEFAULT 0,
    other_sales     numeric(15,2) NOT NULL DEFAULT 0,         -- aggregator prepaid etc.
    order_count     int           NOT NULL DEFAULT 0,
    notes           varchar(500)  NULL,
    created_at      timestamptz   NOT NULL DEFAULT now(),
    updated_at      timestamptz   NOT NULL DEFAULT now(),
    created_by      uuid          NULL,
    updated_by      uuid          NULL,
    is_deleted      boolean       NOT NULL DEFAULT false,
    CONSTRAINT uq_shift_tenant_number UNIQUE (tenant_id, shift_number)
);
CREATE INDEX IF NOT EXISTS ix_shifts_tenant ON shifts(tenant_id);
CREATE INDEX IF NOT EXISTS ix_shifts_location_opened ON shifts(location_id, opened_at);

-- At most one OPEN shift per location.
CREATE UNIQUE INDEX IF NOT EXISTS uq_one_open_shift_per_location
    ON shifts(tenant_id, location_id) WHERE status = 'open' AND is_deleted = false;

-- RLS
DO $$
DECLARE t text;
BEGIN
  FOREACH t IN ARRAY ARRAY['shifts']
  LOOP
    EXECUTE format('ALTER TABLE %1$s ENABLE ROW LEVEL SECURITY', t);
    EXECUTE format('DROP POLICY IF EXISTS p_%1$s_tenant ON %1$s', t);
    EXECUTE format('CREATE POLICY p_%1$s_tenant ON %1$s USING (tenant_id::text = current_setting(''app.tenant_id'', true))', t);
  END LOOP;
END $$;
