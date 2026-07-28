-- =============================================================================
-- Migration 0099: link a "po"-mode request note back to the Purchase Order it
-- was bundled into. Status gains a terminal "fulfilled" value set at that point
-- (0098's draft|pending|approved|rejected set is otherwise unchanged).
-- =============================================================================

ALTER TABLE request_notes ADD COLUMN IF NOT EXISTS purchase_order_id uuid NULL;
