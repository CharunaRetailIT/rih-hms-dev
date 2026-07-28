-- =============================================================================
-- Migration 0046: Customer advance / deposit balance (#70 follow-on)
-- Prepaid money the customer can draw on at the till ("advance" tender). Kept
-- separate from AR (current_balance) — advances are a liability, not a receivable.
-- (Suspended bills are already covered by held/open orders + recall.)
-- =============================================================================

ALTER TABLE customers ADD COLUMN IF NOT EXISTS advance_balance numeric(18,4) NOT NULL DEFAULT 0;
