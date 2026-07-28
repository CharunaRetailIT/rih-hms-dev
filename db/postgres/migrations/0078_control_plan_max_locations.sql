-- #6 tier ceilings: a hard cap on total outlets per plan (0 = unlimited). Enforced via
-- the projected org_settings.location_limit. Lite caps at 2 outlets, Pro at 3, Enterprise
-- unlimited. (User cap already = included_users: Lite 5, Pro 20, Enterprise 500.) Target: hms_control.
SET search_path TO control, public;

ALTER TABLE control.plans ADD COLUMN IF NOT EXISTS max_locations int NOT NULL DEFAULT 0;

UPDATE control.plans SET max_locations = 2 WHERE code = 'lite';
UPDATE control.plans SET max_locations = 3 WHERE code = 'pro';
UPDATE control.plans SET max_locations = 0 WHERE code = 'enterprise';   -- 0 = unlimited
