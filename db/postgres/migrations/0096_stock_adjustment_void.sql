-- =============================================================================
-- Migration 0096: (superseded) stock adjustments don't get a void action —
-- unlike GRN, a PO-style doc never touches stock directly, and this feature's
-- approve step mirrors PO's, not GRN's. Columns added then immediately
-- retired; this migration is now a no-op kept for numbering continuity.
-- =============================================================================

ALTER TABLE stock_adjustments DROP COLUMN IF EXISTS voided_at;
ALTER TABLE stock_adjustments DROP COLUMN IF EXISTS void_reason;
