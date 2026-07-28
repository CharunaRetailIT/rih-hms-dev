-- =============================================================================
-- Migration 0038: PickMe POS API — per-outlet integration (#9)
-- PickMe issues ONE X-API-KEY per outlet (doc v1.4.7), so the key lives on the
-- outlet↔aggregator map, encrypted at rest (AES-GCM, like every other secret).
-- last_polled_at is the joblist poll cursor for observability.
-- =============================================================================

ALTER TABLE location_aggregator_map ADD COLUMN IF NOT EXISTS api_key_enc    text;
ALTER TABLE location_aggregator_map ADD COLUMN IF NOT EXISTS last_polled_at timestamptz;
