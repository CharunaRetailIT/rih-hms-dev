-- 0067 (CONTROL db / hms_main): saved payment method on a subscription (#110 PayHere).
-- The PayHere preapproval flow tokenizes the customer's card; we store the resulting
-- customer_token (+ masked card display) so the Charging API can bill renewals /
-- mid-cycle upgrades server-to-server. control.subscriptions is already owned by
-- hms_app, so ALTER needs no ownership handoff.
ALTER TABLE control.subscriptions
  ADD COLUMN IF NOT EXISTS customer_token text,
  ADD COLUMN IF NOT EXISTS card_brand text,
  ADD COLUMN IF NOT EXISTS card_last4 text,
  ADD COLUMN IF NOT EXISTS payment_method_updated_at timestamptz;
