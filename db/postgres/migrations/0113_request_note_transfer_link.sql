-- =============================================================================
-- Migration 0113: request_notes.transfer_id — lets a "transfer"-mode request
-- note link back to the TOG that fulfilled it, mirroring purchase_order_id for
-- "po"-mode notes. Re-adds what 0100 introduced and 0101 reverted; this time
-- the frontend bundling flow (Transfer create page's Request Note picker,
-- "Create Transfer from selected" on the request-notes list) is built to match.
-- =============================================================================

ALTER TABLE request_notes ADD COLUMN IF NOT EXISTS transfer_id uuid NULL;
