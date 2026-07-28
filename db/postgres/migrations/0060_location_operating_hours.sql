-- Per-location operating hours. JSON keyed by weekday (mon..sun), each {open,close}
-- in "HH:mm"; a missing/null day means closed. Stored as text — the POS reads the
-- whole location and computes "outside operating hours" client-side (soft warning only),
-- so we never query inside this column. NULL = not configured (no warning shown).
ALTER TABLE locations ADD COLUMN IF NOT EXISTS operating_hours text;
