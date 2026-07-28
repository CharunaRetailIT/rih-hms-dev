-- =============================================================================
-- Migration 0058: GRN maker-checker approval (mirrors purchase_orders' approval,
-- added in 0009/0055). While approval_status = 'pending', the GRN sits in
-- status 'draft' and has NOT touched stock — approving is what actually posts
-- it (adds stock + recomputes weighted-average cost). Gated by org settings,
-- same pattern as require_po_approval / po_approval_threshold.
-- =============================================================================

ALTER TABLE goods_received_notes ADD COLUMN IF NOT EXISTS approval_status varchar(20)  NOT NULL DEFAULT 'not_required';
ALTER TABLE goods_received_notes ADD COLUMN IF NOT EXISTS approved_by     uuid         NULL;
ALTER TABLE goods_received_notes ADD COLUMN IF NOT EXISTS approved_at     timestamptz  NULL;
ALTER TABLE goods_received_notes ADD COLUMN IF NOT EXISTS reject_reason   varchar(200) NULL;

ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS require_grn_approval   boolean       NOT NULL DEFAULT false;
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS grn_approval_threshold numeric(15,4) NOT NULL DEFAULT 0;
