-- =============================================================================
-- Migration 0059: simplify the PO workflow to a single status field instead of
-- status + approval_status. New status set: draft|pending|approved|rejected|
-- partially_received|received. Send/Cancel are retired (replaced by Submit and
-- a soft-delete Remove, respectively).
--
-- approval_status, sent_at, cancelled_at, cancel_reason are left in place but
-- unmapped by the app from here on — no data loss, nothing to roll back if this
-- turns out wrong.
-- =============================================================================

ALTER TABLE purchase_orders ADD COLUMN IF NOT EXISTS rejected_by  uuid        NULL;
ALTER TABLE purchase_orders ADD COLUMN IF NOT EXISTS rejected_at  timestamptz NULL;
ALTER TABLE purchase_orders ADD COLUMN IF NOT EXISTS submitted_at timestamptz NULL;

-- Collapse existing (status, approval_status) pairs into the new unified status.
-- 'sent' already meant "cleared approval, dispatched" — the new model has no
-- separate dispatch step, so it folds straight into 'approved'.
-- 'cancelled' rows are left untouched: legacy/inert, no longer a reachable state.
UPDATE purchase_orders SET status = 'pending'
  WHERE status = 'draft' AND approval_status = 'pending';

UPDATE purchase_orders SET status = 'approved', approved_at = COALESCE(approved_at, updated_at)
  WHERE status = 'draft' AND approval_status = 'approved';

UPDATE purchase_orders SET status = 'rejected', rejected_at = COALESCE(rejected_at, updated_at)
  WHERE status = 'draft' AND approval_status = 'rejected';

UPDATE purchase_orders SET status = 'approved', approved_at = COALESCE(approved_at, sent_at, updated_at)
  WHERE status = 'sent';
