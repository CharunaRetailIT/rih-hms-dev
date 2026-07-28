-- =============================================================================
-- Migration 0059: Per-location VAT exemption
-- A same-country outlet may be tax-exempt (e.g. a Sri Lankan duty-free outlet):
-- its bills carry no VAT/tax charges, while service charges still apply. Books
-- and billing remain in the business base currency.
-- =============================================================================

ALTER TABLE locations ADD COLUMN IF NOT EXISTS vat_exempt boolean NOT NULL DEFAULT false;
