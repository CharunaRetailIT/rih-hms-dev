-- =============================================================================
-- Migration 0066: Plan feature bullets — admin-configurable (#109). Target: hms_control.
-- The signup plan cards render these straight from the catalog (nothing hardcoded in the UI).
-- =============================================================================
SET search_path TO control, public;

ALTER TABLE control.plans ADD COLUMN IF NOT EXISTS features text[] NOT NULL DEFAULT '{}';

UPDATE control.plans SET features = ARRAY['Basic POS Interface','Real-time Inventory','Single Outlet Support']
  WHERE code = 'lite' AND cardinality(features) = 0;
UPDATE control.plans SET features = ARRAY['Advanced HMS Features','KDS Integration','Loyalty & Gift Cards','Detailed Operational Analytics']
  WHERE code = 'pro' AND cardinality(features) = 0;
UPDATE control.plans SET features = ARRAY['Multi-outlet Central Hub','Full Workflow Automation','API Access & Custom Integrations','Dedicated Account Manager','Centralized Inventory & ERP']
  WHERE code = 'enterprise' AND cardinality(features) = 0;

-- Hand ownership to the control app role (postgres-created tables; deploy normalizer is public-only).
DO $$
DECLARE approle text;
BEGIN
    SELECT tableowner INTO approle FROM pg_tables WHERE schemaname='control' AND tablename='tenants';
    IF approle IS NOT NULL AND approle <> 'postgres' THEN
        -- plans table already owned by approle from 0064; column add inherits. No-op safety:
        EXECUTE format('ALTER TABLE control.plans OWNER TO %I', approle);
    END IF;
END $$;
