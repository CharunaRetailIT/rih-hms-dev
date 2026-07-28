-- =============================================================================
-- Migration 0053: company-wide (group) sales budgets.
-- location_id becomes nullable — NULL means a single all-outlets target for the
-- month; a non-NULL location is a per-outlet target (unchanged). Two partial
-- unique indexes keep one row per (tenant, outlet, month) and one group row per
-- (tenant, month). RLS already enabled+forced on this table (0048/0051).
-- =============================================================================

ALTER TABLE sales_budgets ALTER COLUMN location_id DROP NOT NULL;

DROP INDEX IF EXISTS uq_sales_budget;
CREATE UNIQUE INDEX IF NOT EXISTS uq_sales_budget_outlet
    ON sales_budgets(tenant_id, location_id, period_month)
    WHERE is_deleted = false AND location_id IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS uq_sales_budget_group
    ON sales_budgets(tenant_id, period_month)
    WHERE is_deleted = false AND location_id IS NULL;
