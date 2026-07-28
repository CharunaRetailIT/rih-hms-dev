-- =============================================================================
-- Migration 0041: Month-end period lock (#77 follow-on)
-- Close the books through a date: postings (and reversals) dated on/before it are
-- rejected, so a closed month's sales / receipts / production can't be changed.
-- =============================================================================

ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS books_locked_through date NULL;  -- everything on/before this date is locked
