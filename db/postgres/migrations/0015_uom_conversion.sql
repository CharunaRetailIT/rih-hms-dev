-- =============================================================================
-- Migration 0015: Units of measure — dimension + conversion factor, and a unit
-- on recipe lines. Lets a recipe say "500 g rice" while rice is stocked in kg:
-- production converts recipe-unit → stock-unit by the factor ratio (same
-- dimension only). Mirrors the legacy UnitConversion (1 kg = 1000 g).
-- =============================================================================

-- dimension groups convertible units; factor_to_base = how many BASE units one
-- of this unit equals (mass base = g, volume base = ml, count base = each).
ALTER TABLE units_of_measure ADD COLUMN IF NOT EXISTS dimension      varchar(20)   NOT NULL DEFAULT 'count';
ALTER TABLE units_of_measure ADD COLUMN IF NOT EXISTS factor_to_base numeric(18,6) NOT NULL DEFAULT 1;

-- the unit a recipe line's quantity is expressed in (NULL = the ingredient's
-- own stock unit, for backward compatibility).
ALTER TABLE recipe_lines ADD COLUMN IF NOT EXISTS unit_id     uuid        NULL;
ALTER TABLE recipe_lines ADD COLUMN IF NOT EXISTS unit_symbol varchar(10) NULL;

-- Backfill the conventional codes so existing tenants get sane conversions.
UPDATE units_of_measure SET dimension = 'mass',   factor_to_base = 1     WHERE upper(code) = 'G';
UPDATE units_of_measure SET dimension = 'mass',   factor_to_base = 1000  WHERE upper(code) = 'KG';
UPDATE units_of_measure SET dimension = 'volume', factor_to_base = 1     WHERE upper(code) = 'ML';
UPDATE units_of_measure SET dimension = 'volume', factor_to_base = 1000  WHERE upper(code) = 'L';
UPDATE units_of_measure SET dimension = 'count',  factor_to_base = 1     WHERE upper(code) IN ('EA', 'BOT', 'PLT', 'PCS', 'PORTION');
