-- =============================================================================
-- Migration 0102: Transfer of Goods approval workflow — draft -> submit ->
-- pending/dispatched -> approve (dispatches)/reject, mirroring the
-- stock_adjustments (0095) / wastage_notes (0097) maker-checker gate. Approving
-- a pending transfer is what actually decrements the sender's stock (the old
-- unconditional "dispatch" action); rejecting it never touches stock.
-- =============================================================================

ALTER TABLE stock_transfers ADD COLUMN IF NOT EXISTS submitted_at  timestamptz NULL;
ALTER TABLE stock_transfers ADD COLUMN IF NOT EXISTS approved_by   uuid NULL;
ALTER TABLE stock_transfers ADD COLUMN IF NOT EXISTS approved_at   timestamptz NULL;
ALTER TABLE stock_transfers ADD COLUMN IF NOT EXISTS rejected_by   uuid NULL;
ALTER TABLE stock_transfers ADD COLUMN IF NOT EXISTS rejected_at   timestamptz NULL;
ALTER TABLE stock_transfers ADD COLUMN IF NOT EXISTS reject_reason varchar(200) NULL;

ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS require_transfer_approval   boolean       NOT NULL DEFAULT false;
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS transfer_approval_threshold numeric(15,4) NOT NULL DEFAULT 0;
