-- =============================================================================
-- Migration 0097: Wastage document workflow — draft -> submit -> pending/
-- approved -> approve/reject, mirroring stock_adjustments (0095) and
-- purchase_orders/GRN before it. The old Record Wastage modal (Inventory
-- page) is being removed in the same change; existing rows stay status=
-- 'posted' (legacy, untouched by this migration).
-- =============================================================================

ALTER TABLE wastage_notes ADD COLUMN IF NOT EXISTS submitted_at  timestamptz NULL;
ALTER TABLE wastage_notes ADD COLUMN IF NOT EXISTS approved_by   uuid NULL;
ALTER TABLE wastage_notes ADD COLUMN IF NOT EXISTS approved_at   timestamptz NULL;
ALTER TABLE wastage_notes ADD COLUMN IF NOT EXISTS rejected_by   uuid NULL;
ALTER TABLE wastage_notes ADD COLUMN IF NOT EXISTS rejected_at   timestamptz NULL;
ALTER TABLE wastage_notes ADD COLUMN IF NOT EXISTS reject_reason varchar(200) NULL;

ALTER TABLE wastage_lines ADD COLUMN IF NOT EXISTS current_stock numeric(15,4) NOT NULL DEFAULT 0;
ALTER TABLE wastage_lines ADD COLUMN IF NOT EXISTS new_stock     numeric(15,4) NOT NULL DEFAULT 0;

ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS require_wastage_approval   boolean       NOT NULL DEFAULT false;
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS wastage_approval_threshold numeric(15,4) NOT NULL DEFAULT 0;
