-- Auto Product-Code generation (#2): admin-configurable prefix for auto-generated product
-- codes (e.g. ITM-0001). The running number is derived from existing codes (max suffix + 1),
-- so deletes never cause a collision. Manual entry stays available.
ALTER TABLE org_settings ADD COLUMN IF NOT EXISTS product_code_prefix text NOT NULL DEFAULT 'ITM';
