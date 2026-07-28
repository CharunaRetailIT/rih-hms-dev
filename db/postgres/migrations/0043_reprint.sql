-- =============================================================================
-- Migration 0043: Recall settled bill + reprint (#78 follow-on)
-- Track how many duplicate copies of a bill have been printed (legacy reprint).
-- =============================================================================

ALTER TABLE orders ADD COLUMN IF NOT EXISTS reprint_count int NOT NULL DEFAULT 0;
