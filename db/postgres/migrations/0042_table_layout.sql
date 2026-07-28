-- =============================================================================
-- Migration 0042: Visual floor plan (#68 follow-on)
-- x/y position (in px) for each table so the Floor screen can render a draggable
-- visual layout. 0/0 = unplaced (the UI auto-grids those until arranged).
-- =============================================================================

ALTER TABLE restaurant_tables ADD COLUMN IF NOT EXISTS pos_x int NOT NULL DEFAULT 0;
ALTER TABLE restaurant_tables ADD COLUMN IF NOT EXISTS pos_y int NOT NULL DEFAULT 0;
