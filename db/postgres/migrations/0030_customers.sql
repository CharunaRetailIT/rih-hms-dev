-- =============================================================================
-- Migration 0030: CRM / customers (#70)
-- Customer master + categories, a per-customer (or per-category) default discount,
-- and credit customers (AR): a credit limit + a running outstanding balance that
-- credit settlements increase and AR receipts (customer_payments) reduce.
-- orders.customer_id already exists (0004); this adds the master it points at.
-- =============================================================================

CREATE TABLE IF NOT EXISTS customer_categories (
    id               uuid         PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id        uuid         NOT NULL,
    code             varchar(40)  NOT NULL,
    name             varchar(120) NOT NULL,
    discount_percent numeric(8,4) NOT NULL DEFAULT 0,   -- group discount (e.g. Staff 10%)
    is_active        boolean      NOT NULL DEFAULT true,
    notes            varchar(300) NULL,
    created_at       timestamptz  NOT NULL DEFAULT now(),
    updated_at       timestamptz  NOT NULL DEFAULT now(),
    created_by       uuid         NULL,
    updated_by       uuid         NULL,
    is_deleted       boolean      NOT NULL DEFAULT false,
    CONSTRAINT uq_customer_category_code UNIQUE (tenant_id, code)
);
CREATE INDEX IF NOT EXISTS ix_customer_categories_tenant ON customer_categories(tenant_id);

CREATE TABLE IF NOT EXISTS customers (
    id                 uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id          uuid          NOT NULL,
    code               varchar(40)   NOT NULL,
    name               varchar(160)  NOT NULL,
    category_id        uuid          NULL,
    phone              varchar(40)   NULL,
    email              varchar(160)  NULL,
    address            varchar(300)  NULL,
    tax_no             varchar(40)   NULL,                 -- NIC / VAT / BR for the invoice
    discount_percent   numeric(8,4)  NULL,                 -- per-customer override; else category
    is_credit_customer boolean       NOT NULL DEFAULT false,
    credit_limit       numeric(15,4) NOT NULL DEFAULT 0,
    current_balance    numeric(15,4) NOT NULL DEFAULT 0,   -- outstanding AR
    is_active          boolean       NOT NULL DEFAULT true,
    notes              varchar(300)  NULL,
    created_at         timestamptz   NOT NULL DEFAULT now(),
    updated_at         timestamptz   NOT NULL DEFAULT now(),
    created_by         uuid          NULL,
    updated_by         uuid          NULL,
    is_deleted         boolean       NOT NULL DEFAULT false,
    CONSTRAINT uq_customer_code UNIQUE (tenant_id, code)
);
CREATE INDEX IF NOT EXISTS ix_customers_tenant ON customers(tenant_id);
CREATE INDEX IF NOT EXISTS ix_customers_name   ON customers(tenant_id, name);

-- AR receipts: money a credit customer pays against their outstanding balance.
CREATE TABLE IF NOT EXISTS customer_payments (
    id          uuid          PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id   uuid          NOT NULL,
    customer_id uuid          NOT NULL,
    amount      numeric(15,4) NOT NULL,
    pay_type    varchar(20)   NOT NULL DEFAULT 'cash',
    reference   varchar(120)  NULL,
    received_at timestamptz   NOT NULL DEFAULT now(),
    notes       varchar(300)  NULL,
    created_at  timestamptz   NOT NULL DEFAULT now(),
    updated_at  timestamptz   NOT NULL DEFAULT now(),
    created_by  uuid          NULL,
    updated_by  uuid          NULL,
    is_deleted  boolean       NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_customer_payments_customer ON customer_payments(customer_id);

-- RLS
ALTER TABLE customer_categories ENABLE ROW LEVEL SECURITY;
ALTER TABLE customers           ENABLE ROW LEVEL SECURITY;
ALTER TABLE customer_payments   ENABLE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS p_customer_categories_tenant ON customer_categories;
DROP POLICY IF EXISTS p_customers_tenant           ON customers;
DROP POLICY IF EXISTS p_customer_payments_tenant   ON customer_payments;
CREATE POLICY p_customer_categories_tenant ON customer_categories USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_customers_tenant           ON customers           USING (tenant_id::text = current_setting('app.tenant_id', true));
CREATE POLICY p_customer_payments_tenant   ON customer_payments   USING (tenant_id::text = current_setting('app.tenant_id', true));
