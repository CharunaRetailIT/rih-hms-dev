-- =============================================================================
-- Migration 0101: revert 0100's request_notes.transfer_id — a Transfer of Goods
-- never consumes a request note directly. Its "GRN based" mode instead traces
-- back through GoodsReceivedNote.purchase_order_id -> request_notes.purchase_order_id
-- (already in place since 0099), purely for display.
-- =============================================================================

ALTER TABLE request_notes DROP COLUMN IF EXISTS transfer_id;
