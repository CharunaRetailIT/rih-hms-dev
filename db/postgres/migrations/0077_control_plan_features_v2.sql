-- #6 follow-up: rewrite the plan feature bullets in plain English so they match the
-- actual Lite/Pro/Enterprise gating (Accounting, Production, Loyalty+Promotions,
-- Catering are Pro+). Drives both the signup plan cards and the in-app billing
-- section (both read control.plans.features). Target: hms_control.
SET search_path TO control, public;

UPDATE control.plans SET features = ARRAY[
  'POS & billing',
  'Kitchen display (KOT)',
  'Menu & inventory',
  'Tables & floor plan',
  'Customers & sales reports',
  '1 outlet (add more any time)'
] WHERE code = 'lite';

UPDATE control.plans SET features = ARRAY[
  'Everything in Lite',
  'Accounting & financial reports',
  'Production & recipes (BOM)',
  'Loyalty & promotions',
  'Catering & events',
  'Multiple outlets'
] WHERE code = 'pro';

UPDATE control.plans SET features = ARRAY[
  'Everything in Pro',
  'Central multi-outlet hub',
  'API & custom integrations',
  'Priority support'
] WHERE code = 'enterprise';
