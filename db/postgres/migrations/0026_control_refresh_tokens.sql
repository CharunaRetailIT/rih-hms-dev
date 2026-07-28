-- =============================================================================
-- Migration 0026: Refresh tokens (control plane)
-- Target DB: hms_control   (NOT a tenant DB)
-- Run with:  psql -d hms_control -f db/postgres/migrations/0026_control_refresh_tokens.sql
--
-- Long-lived, rotating refresh tokens so a short-lived access JWT (8h) can be
-- renewed without re-doing the magic-link. Only the SHA-256 hash is stored.
-- =============================================================================

CREATE TABLE IF NOT EXISTS control.refresh_tokens (
    id          uuid         PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id   uuid         NOT NULL,
    user_id     uuid         NOT NULL,
    token_hash  varchar(64)  NOT NULL,                 -- sha256 hex of the raw token
    expires_at  timestamptz  NOT NULL,
    revoked_at  timestamptz  NULL,
    created_at  timestamptz  NOT NULL DEFAULT now(),
    updated_at  timestamptz  NOT NULL DEFAULT now(),
    is_deleted  boolean      NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_refresh_tokens_hash ON control.refresh_tokens(token_hash);
CREATE INDEX IF NOT EXISTS ix_refresh_tokens_user ON control.refresh_tokens(user_id);
