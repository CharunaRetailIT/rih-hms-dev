-- =============================================================================
-- Migration 0012: Configurable document prefixes + bill branding + tax certs
-- The restaurant admin edits these in Settings; they thread through every
-- document-number generator and the bill/invoice print.
-- =============================================================================

-- Document number prefixes (invoice_prefix already exists from 0005).
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS order_prefix      varchar(10) NOT NULL DEFAULT 'ORD';
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS po_prefix         varchar(10) NOT NULL DEFAULT 'PO';
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS grn_prefix        varchar(10) NOT NULL DEFAULT 'GRN';
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS supplier_prefix   varchar(10) NOT NULL DEFAULT 'SUP';
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS transfer_prefix   varchar(10) NOT NULL DEFAULT 'TRF';
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS wastage_prefix    varchar(10) NOT NULL DEFAULT 'WST';
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS adjustment_prefix varchar(10) NOT NULL DEFAULT 'ADJ';

-- Tax certificate numbers (vat_registration_number + business_registration_no exist).
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS svat_number varchar(40) NULL;

-- Bill / receipt branding.
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS bill_header       text    NULL;
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS bill_footer       text    NULL;
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS show_vat_on_bills boolean NOT NULL DEFAULT true;
