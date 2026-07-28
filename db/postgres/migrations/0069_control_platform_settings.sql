-- =============================================================================
-- Migration 0069 (CONTROL db / hms_main): RIT-wide platform settings (key/value).
-- Lets RIT admin flip platform behaviour at runtime (no redeploy). First setting:
-- require_card_at_signup (default 'true' = card-required trial; RIT can A/B to 'false').
-- =============================================================================
SET search_path TO control, public;

CREATE TABLE IF NOT EXISTS control.platform_settings (
    key        text         PRIMARY KEY,
    value      text         NOT NULL,
    updated_at timestamptz  NOT NULL DEFAULT now()
);

INSERT INTO control.platform_settings (key, value) VALUES ('require_card_at_signup', 'true')
ON CONFLICT (key) DO NOTHING;

DO $$
DECLARE approle text;
BEGIN
    SELECT tableowner INTO approle FROM pg_tables WHERE schemaname='control' AND tablename='tenants';
    IF approle IS NOT NULL AND approle <> 'postgres' THEN
        EXECUTE format('ALTER TABLE control.platform_settings OWNER TO %I', approle);
    END IF;
END $$;
