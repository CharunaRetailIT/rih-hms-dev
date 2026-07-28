-- =============================================================================
-- Migration 0111: Country for tour agents and tour agent companies — the web
-- form's Country field (a dropdown, like Locations/Suppliers) needs somewhere
-- to persist to.
-- =============================================================================

ALTER TABLE tour_operators           ADD COLUMN IF NOT EXISTS country_code char(2);
ALTER TABLE tour_operator_companies  ADD COLUMN IF NOT EXISTS country_code char(2);
