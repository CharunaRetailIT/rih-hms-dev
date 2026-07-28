-- Tour operators can be a company (travel agency) or an individual (independent guide/agent) (#76).
ALTER TABLE tour_operators ADD COLUMN IF NOT EXISTS kind text NOT NULL DEFAULT 'company';
