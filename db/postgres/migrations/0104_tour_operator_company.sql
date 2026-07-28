-- =============================================================================
-- Migration 0104: Tour Agent Company + missing Tour Operator detail fields —
-- matches the old system's separate "Tour Agent Company" (Code, Name, Address
-- 1/2, Mobile, Telephone, Fax No, E Mail, Web Address, Contact Person,
-- Commission Amount) and "Tour Agent" (Title, NIC, Address 1/2/3, Percentage %,
-- Amount, Remarks, linked to a company) screens.
-- =============================================================================

CREATE TABLE IF NOT EXISTS tour_operator_companies (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NOT NULL,
    code varchar(40) NOT NULL,
    name varchar(160) NOT NULL,
    address1 varchar(200),
    address2 varchar(200),
    mobile varchar(40),
    telephone varchar(40),
    fax_no varchar(40),
    email varchar(160),
    web_address varchar(200),
    contact_person varchar(160),
    commission_amount numeric(18,2) NOT NULL DEFAULT 0,
    is_active boolean NOT NULL DEFAULT true,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    created_by uuid,
    updated_by uuid,
    is_deleted boolean NOT NULL DEFAULT false
);

CREATE UNIQUE INDEX IF NOT EXISTS uq_tour_operator_companies_tenant_code ON tour_operator_companies (tenant_id, code);

ALTER TABLE tour_operator_companies ENABLE ROW LEVEL SECURITY;
ALTER TABLE tour_operator_companies FORCE ROW LEVEL SECURITY;
CREATE POLICY p_tour_operator_companies_tenant ON tour_operator_companies
    USING (tenant_id::text = current_setting('app.tenant_id', true));

ALTER TABLE tour_operators ADD COLUMN IF NOT EXISTS company_id uuid REFERENCES tour_operator_companies(id);
ALTER TABLE tour_operators ADD COLUMN IF NOT EXISTS title varchar(20);
ALTER TABLE tour_operators ADD COLUMN IF NOT EXISTS nic varchar(30);
ALTER TABLE tour_operators ADD COLUMN IF NOT EXISTS address1 varchar(200);
ALTER TABLE tour_operators ADD COLUMN IF NOT EXISTS address2 varchar(200);
ALTER TABLE tour_operators ADD COLUMN IF NOT EXISTS address3 varchar(200);
ALTER TABLE tour_operators ADD COLUMN IF NOT EXISTS mobile varchar(40);
ALTER TABLE tour_operators ADD COLUMN IF NOT EXISTS email varchar(160);
ALTER TABLE tour_operators ADD COLUMN IF NOT EXISTS amount numeric(18,2) NOT NULL DEFAULT 0;
ALTER TABLE tour_operators ADD COLUMN IF NOT EXISTS remarks varchar(300);
