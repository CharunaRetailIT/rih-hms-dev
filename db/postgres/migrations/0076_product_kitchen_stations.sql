-- #4b Multiple kitchens per item: a product can print its KOT at several stations.
-- text[] of kitchen-station codes; empty falls back to the single kitchen_station_code.
-- Backfill the existing single station into the array so current routing is preserved.
ALTER TABLE products ADD COLUMN IF NOT EXISTS kitchen_station_codes text[] NOT NULL DEFAULT '{}';
UPDATE products
   SET kitchen_station_codes = ARRAY[kitchen_station_code]
 WHERE kitchen_station_code IS NOT NULL
   AND kitchen_station_code <> ''
   AND (kitchen_station_codes IS NULL OR cardinality(kitchen_station_codes) = 0);
