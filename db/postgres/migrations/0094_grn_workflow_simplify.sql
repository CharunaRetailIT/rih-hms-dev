-- =============================================================================
-- Migration 0060: simplify the GRN workflow to a single status field instead of
-- status + approval_status — mirrors migration 0059's purchase_orders change.
-- New status set: draft|pending|approved|rejected|void. "posted" is retired —
-- it folds into "approved" (that's already the moment stock gets posted).
--
-- approval_status is left in place but unmapped by the app from here on — no
-- data loss, nothing to roll back if this turns out wrong.
-- =============================================================================

ALTER TABLE goods_received_notes ADD COLUMN IF NOT EXISTS rejected_by  uuid        NULL;
ALTER TABLE goods_received_notes ADD COLUMN IF NOT EXISTS rejected_at  timestamptz NULL;
ALTER TABLE goods_received_notes ADD COLUMN IF NOT EXISTS submitted_at timestamptz NULL;

-- Collapse existing (status, approval_status) pairs into the new unified status.
UPDATE goods_received_notes SET status = 'pending'
  WHERE status = 'draft' AND approval_status = 'pending';

UPDATE goods_received_notes SET status = 'approved', approved_at = COALESCE(approved_at, updated_at)
  WHERE status = 'draft' AND approval_status = 'approved';

UPDATE goods_received_notes SET status = 'rejected', rejected_at = COALESCE(rejected_at, updated_at)
  WHERE status = 'draft' AND approval_status = 'rejected';

UPDATE goods_received_notes SET status = 'approved', approved_at = COALESCE(approved_at, posted_at, updated_at)
  WHERE status = 'posted';

-- 'void' rows are left untouched — already terminal, unaffected by the merge.
