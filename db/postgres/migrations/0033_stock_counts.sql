-- =============================================================================
-- Migration 0033: Physical stock count (#77)
-- A count sheet snapshots the system on-hand per stocked product at a location,
-- the team enters the counted quantity, and posting writes the variance back to
-- product_stock (counted becomes the new on-hand) and stamps last_counted_at.
-- The count + its lines are the permanent record of the variance.
-- =============================================================================

CREATE TABLE IF NOT EXISTS stock_counts (
    id          uuid         PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id   uuid         NOT NULL,
    location_id uuid         NOT NULL,
    count_number varchar(30) NULL,
    status      varchar(20)  NOT NULL DEFAULT 'draft',   -- draft | posted | void
    notes       varchar(300) NULL,
    posted_at   timestamptz  NULL,
    created_at  timestamptz  NOT NULL DEFAULT now(),
    updated_at  timestamptz  NOT NULL DEFAULT now(),
    created_by  uuid         NULL,
    updated_by  uuid         NULL,
    is_deleted  boolean      NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_stock_counts_tenant ON stock_counts(tenant_id);
CREATE INDEX IF NOT EXISTS ix_stock_counts_location ON stock_counts(location_id);

CREATE TABLE IF NOT EXISTS stock_count_lines (
    id             uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id      uuid          NOT NULL,
    stock_count_id uuid          NOT NULL,
    product_id     uuid          NOT NULL,
    system_qty     numeric(15,4) NOT NULL DEFAULT 0,   -- on-hand snapshot at count creation
    counted_qty    numeric(15,4) NOT NULL DEFAULT 0,
    variance       numeric(15,4) NOT NULL DEFAULT 0,   -- counted − system, fixed at post
    created_at     timestamptz   NOT NULL DEFAULT now(),
    updated_at     timestamptz   NOT NULL DEFAULT now(),
    created_by     uuid          NULL,
    updated_by     uuid          NULL,
    is_deleted     boolean       NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_stock_count_lines_count ON stock_count_lines(stock_count_id);

ALTER TABLE stock_counts      ENABLE ROW LEVEL SECURITY;
ALTER TABLE stock_count_lines ENABLE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS p_stock_counts_tenant      ON stock_counts;
DROP POLICY IF EXISTS p_stock_count_lines_tenant ON stock_count_lines;
CREATE POLICY p_stock_counts_tenant      ON stock_counts      USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_stock_count_lines_tenant ON stock_count_lines USING (tenant_id::text = current_setting('app.tenant_id', true));
