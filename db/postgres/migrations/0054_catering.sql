-- =============================================================================
-- Migration 0054: Catering / banquet (#75) — halls, per-head packages, event
--                 bookings + function billing, own-fleet off-site delivery.
-- Target DB: any hms_tenant_<slug>
--
-- An event books a hall for a date (double-booking blocked), priced as
-- pax × package price/head + ad-hoc extras − discount. Deposits/advances reduce
-- the balance. Off-site jobs carry delivery address + vehicle/driver + dispatch.
-- RLS: every table ENABLE + FORCE + tenant policy (0051 pattern — FORCE here
-- because 0051 only covered tables that existed when it ran).
-- =============================================================================

-- -----------------------------------------------------------------------------
-- event_halls — bookable venues/spaces
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS event_halls (
    id          uuid         PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id   uuid         NOT NULL,
    location_id uuid         NULL,           -- owning outlet (optional)
    code        varchar(40)  NOT NULL,
    name        varchar(160) NOT NULL,
    capacity    int          NOT NULL DEFAULT 0,
    notes       varchar(300) NULL,
    is_active   boolean      NOT NULL DEFAULT true,
    created_at  timestamptz  NOT NULL DEFAULT now(),
    updated_at  timestamptz  NOT NULL DEFAULT now(),
    created_by  uuid         NULL,
    updated_by  uuid         NULL,
    is_deleted  boolean      NOT NULL DEFAULT false,
    CONSTRAINT uq_event_halls_tenant_code UNIQUE (tenant_id, code)
);

-- -----------------------------------------------------------------------------
-- catering_packages — per-head menu packages
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS catering_packages (
    id             uuid           PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id      uuid           NOT NULL,
    code           varchar(40)    NOT NULL,
    name           varchar(160)   NOT NULL,
    price_per_head numeric(15, 4) NOT NULL DEFAULT 0,
    description    varchar(500)   NULL,
    is_active      boolean        NOT NULL DEFAULT true,
    created_at     timestamptz    NOT NULL DEFAULT now(),
    updated_at     timestamptz    NOT NULL DEFAULT now(),
    created_by     uuid           NULL,
    updated_by     uuid           NULL,
    is_deleted     boolean        NOT NULL DEFAULT false,
    CONSTRAINT uq_catering_packages_tenant_code UNIQUE (tenant_id, code)
);

-- -----------------------------------------------------------------------------
-- catering_events — a booking + function-bill header
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS catering_events (
    id                 uuid           PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id          uuid           NOT NULL,
    event_no           varchar(30)    NOT NULL,
    title              varchar(200)   NULL,            -- "Perera Wedding"
    location_id        uuid           NULL,
    hall_id            uuid           NULL,
    package_id         uuid           NULL,
    customer_id        uuid           NULL,
    customer_name      varchar(160)   NULL,
    customer_phone     varchar(40)    NULL,
    pax                int            NOT NULL DEFAULT 0,
    starts_at          timestamptz    NOT NULL,
    ends_at            timestamptz    NULL,
    status             varchar(20)    NOT NULL DEFAULT 'enquiry',  -- enquiry|confirmed|running|completed|cancelled
    -- billing (recomputed by the service)
    price_per_head     numeric(15, 4) NOT NULL DEFAULT 0,          -- snapshot of the package rate
    package_total      numeric(15, 4) NOT NULL DEFAULT 0,          -- pax × price_per_head
    extras_total       numeric(15, 4) NOT NULL DEFAULT 0,
    discount_amount    numeric(15, 4) NOT NULL DEFAULT 0,
    service_charge     numeric(15, 4) NOT NULL DEFAULT 0,
    tax_amount         numeric(15, 4) NOT NULL DEFAULT 0,
    total_amount       numeric(15, 4) NOT NULL DEFAULT 0,
    paid_amount        numeric(15, 4) NOT NULL DEFAULT 0,          -- Σ deposits/advances/settlements
    -- own-fleet / off-site
    is_offsite         boolean        NOT NULL DEFAULT false,
    delivery_address   varchar(300)   NULL,
    vehicle            varchar(80)    NULL,
    driver             varchar(120)   NULL,
    dispatch_status    varchar(20)    NULL,            -- pending|dispatched|delivered
    notes              varchar(500)   NULL,
    created_at         timestamptz    NOT NULL DEFAULT now(),
    updated_at         timestamptz    NOT NULL DEFAULT now(),
    created_by         uuid           NULL,
    updated_by         uuid           NULL,
    is_deleted         boolean        NOT NULL DEFAULT false,
    CONSTRAINT uq_catering_events_tenant_no UNIQUE (tenant_id, event_no)
);
CREATE INDEX IF NOT EXISTS ix_catering_events_when ON catering_events(tenant_id, starts_at);
CREATE INDEX IF NOT EXISTS ix_catering_events_hall ON catering_events(tenant_id, hall_id);

-- -----------------------------------------------------------------------------
-- catering_event_items — ad-hoc extras / line items on a booking
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS catering_event_items (
    id          uuid           PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id   uuid           NOT NULL,
    event_id    uuid           NOT NULL REFERENCES catering_events(id) ON DELETE CASCADE,
    description varchar(200)   NOT NULL,
    quantity    numeric(15, 4) NOT NULL DEFAULT 1,
    unit_price  numeric(15, 4) NOT NULL DEFAULT 0,
    line_total  numeric(15, 4) NOT NULL DEFAULT 0,
    created_at  timestamptz    NOT NULL DEFAULT now(),
    is_deleted  boolean        NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_catering_event_items_event ON catering_event_items(event_id);

-- -----------------------------------------------------------------------------
-- catering_event_payments — deposits / advances / balance settlements
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS catering_event_payments (
    id          uuid           PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id   uuid           NOT NULL,
    event_id    uuid           NOT NULL REFERENCES catering_events(id) ON DELETE CASCADE,
    amount      numeric(15, 4) NOT NULL,
    pay_type    varchar(30)    NOT NULL DEFAULT 'cash',   -- cash|card|bank|advance
    kind        varchar(20)    NOT NULL DEFAULT 'deposit', -- deposit|advance|balance
    reference   varchar(100)   NULL,
    paid_at     timestamptz    NOT NULL DEFAULT now(),
    created_at  timestamptz    NOT NULL DEFAULT now(),
    created_by  uuid           NULL,
    is_deleted  boolean        NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_catering_event_payments_event ON catering_event_payments(event_id);

-- =============================================================================
-- Row-Level Security — ENABLE + FORCE + tenant policy
-- =============================================================================
DO $$
DECLARE t text;
BEGIN
    FOREACH t IN ARRAY ARRAY['event_halls','catering_packages','catering_events','catering_event_items','catering_event_payments']
    LOOP
        EXECUTE format('ALTER TABLE public.%I ENABLE ROW LEVEL SECURITY', t);
        EXECUTE format('ALTER TABLE public.%I FORCE ROW LEVEL SECURITY', t);
        EXECUTE format('DROP POLICY IF EXISTS p_%s_tenant ON public.%I', t, t);
        EXECUTE format(
            'CREATE POLICY p_%s_tenant ON public.%I USING (tenant_id::text = current_setting(''app.tenant_id'', true))',
            t, t);
    END LOOP;
END $$;
