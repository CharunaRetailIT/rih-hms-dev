-- Structured address (country + province/state/emirate + district/city + postal code)
-- for every entity whose form captures an address. Smart region pick-lists live in the
-- frontend (apps/web/lib/regions.ts); the DB just stores the chosen values as text.
-- All nullable / additive — existing free-text street lines (address / address_line1 /
-- delivery_address) are kept as the street line alongside these.

ALTER TABLE suppliers       ADD COLUMN IF NOT EXISTS country_code char(2);
ALTER TABLE suppliers       ADD COLUMN IF NOT EXISTS province     varchar(100);
ALTER TABLE suppliers       ADD COLUMN IF NOT EXISTS district     varchar(100);
ALTER TABLE suppliers       ADD COLUMN IF NOT EXISTS postal_code  varchar(20);

ALTER TABLE customers       ADD COLUMN IF NOT EXISTS country_code char(2);
ALTER TABLE customers       ADD COLUMN IF NOT EXISTS province     varchar(100);
ALTER TABLE customers       ADD COLUMN IF NOT EXISTS district     varchar(100);
ALTER TABLE customers       ADD COLUMN IF NOT EXISTS postal_code  varchar(20);

ALTER TABLE catering_events ADD COLUMN IF NOT EXISTS country_code char(2);
ALTER TABLE catering_events ADD COLUMN IF NOT EXISTS province     varchar(100);
ALTER TABLE catering_events ADD COLUMN IF NOT EXISTS district     varchar(100);
ALTER TABLE catering_events ADD COLUMN IF NOT EXISTS postal_code  varchar(20);

-- locations already has country_code + city; add the finer-grained region fields.
ALTER TABLE locations       ADD COLUMN IF NOT EXISTS province     varchar(100);
ALTER TABLE locations       ADD COLUMN IF NOT EXISTS district     varchar(100);
ALTER TABLE locations       ADD COLUMN IF NOT EXISTS postal_code  varchar(20);
