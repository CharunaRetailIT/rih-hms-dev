-- =============================================================================
-- Migration 0105: Tour Agent Company gains CommissionPercent, alongside the
-- existing CommissionAmount — mirrors Tour Operator (which already has both
-- CommissionPercent and Amount). The app now enforces that only one of the
-- two is set per row (percentage-based OR flat-amount commission, not both).
-- =============================================================================

ALTER TABLE tour_operator_companies ADD COLUMN IF NOT EXISTS commission_percent numeric(8,4) NOT NULL DEFAULT 0;
