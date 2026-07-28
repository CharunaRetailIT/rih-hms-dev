-- =============================================================================
-- Migration 0019: PO approval policy (org setting for maker-checker)
--   require_po_approval     — always route POs through approval
--   po_approval_threshold   — if > 0, approval required when total >= threshold
-- =============================================================================
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS require_po_approval   boolean       NOT NULL DEFAULT false;
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS po_approval_threshold numeric(15,4) NOT NULL DEFAULT 0;
