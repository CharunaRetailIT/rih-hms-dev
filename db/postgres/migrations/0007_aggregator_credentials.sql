-- =============================================================================
-- Migration 0007: Per-merchant + per-location aggregator credentials
-- Target DB: any hms_tenant_<slug>
-- Merchant-level OAuth keys (encrypted at rest) + per-location store IDs.
-- Editable from the merchant dashboard — NOT from env. The only infra secret
-- is the master encryption key (Key Vault / env) used to encrypt these rows.
-- =============================================================================

-- Merchant-level credentials (one row per aggregator per tenant)
CREATE TABLE IF NOT EXISTS aggregator_credentials (
    id                  uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id           uuid          NOT NULL,
    aggregator          varchar(20)   NOT NULL,          -- ubereats | pickme
    client_id           varchar(255)  NULL,
    client_secret_enc   text          NULL,              -- AES-GCM encrypted
    webhook_secret_enc  text          NULL,              -- AES-GCM encrypted
    environment         varchar(20)   NOT NULL DEFAULT 'sandbox',  -- sandbox | live
    base_url            varchar(255)  NULL,
    is_enabled          boolean       NOT NULL DEFAULT false,
    created_at          timestamptz   NOT NULL DEFAULT now(),
    updated_at          timestamptz   NOT NULL DEFAULT now(),
    updated_by          uuid          NULL,
    CONSTRAINT uq_agg_cred_tenant_aggregator UNIQUE (tenant_id, aggregator)
);

-- Per-location store mapping (Uber store_id / PickMe outlet id)
CREATE TABLE IF NOT EXISTS location_aggregator_map (
    id                 uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id          uuid          NOT NULL,
    location_id        uuid          NOT NULL REFERENCES locations(id),
    aggregator         varchar(20)   NOT NULL,
    external_store_id  varchar(120)  NULL,               -- the aggregator's store/outlet id
    is_enabled         boolean       NOT NULL DEFAULT true,
    created_at         timestamptz   NOT NULL DEFAULT now(),
    updated_at         timestamptz   NOT NULL DEFAULT now(),
    CONSTRAINT uq_loc_agg_map UNIQUE (tenant_id, location_id, aggregator)
);
-- Reverse lookup: webhook carries external_store_id → resolve location (+ tenant).
CREATE INDEX IF NOT EXISTS ix_loc_agg_store ON location_aggregator_map(aggregator, external_store_id)
    WHERE external_store_id IS NOT NULL;

-- RLS
ALTER TABLE aggregator_credentials   ENABLE ROW LEVEL SECURITY;
ALTER TABLE location_aggregator_map  ENABLE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS p_agg_cred_tenant ON aggregator_credentials;
DROP POLICY IF EXISTS p_loc_agg_tenant  ON location_aggregator_map;
CREATE POLICY p_agg_cred_tenant ON aggregator_credentials
    USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_loc_agg_tenant ON location_aggregator_map
    USING (tenant_id::text = current_setting('app.tenant_id', true));
