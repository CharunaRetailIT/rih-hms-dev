-- =============================================================================
-- Migration 0048: Sales budgets (#72 follow-on) — budget vs actual per outlet/month.
-- =============================================================================

CREATE TABLE IF NOT EXISTS sales_budgets (
    id           uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id    uuid          NOT NULL,
    location_id  uuid          NOT NULL,
    period_month date          NOT NULL,            -- 1st of the budgeted month
    amount       numeric(18,4) NOT NULL DEFAULT 0,
    created_at   timestamptz   NOT NULL DEFAULT now(),
    updated_at   timestamptz   NOT NULL DEFAULT now(),
    created_by   uuid          NULL,
    updated_by   uuid          NULL,
    is_deleted   boolean       NOT NULL DEFAULT false
);
CREATE UNIQUE INDEX IF NOT EXISTS uq_sales_budget ON sales_budgets(tenant_id, location_id, period_month) WHERE is_deleted = false;

ALTER TABLE sales_budgets ENABLE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS p_sales_budgets_tenant ON sales_budgets;
CREATE POLICY p_sales_budgets_tenant ON sales_budgets USING (tenant_id::text = current_setting('app.tenant_id', true));
