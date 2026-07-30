#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# One-shot control-plane bootstrap. Creates the control database and applies every
# migration that targets the control schema (0001 schema+tenants+subscriptions,
# 0026 refresh_tokens, 0064-0079 billing catalog/taxes/platform settings — see
# `grep -l 'control\.' db/postgres/migrations/*.sql` for the authoritative list).
# Idempotent: safe to re-run on every `docker compose up`. Per-tenant DBs are NOT
# created here — the API provisions those on signup (POST /api/v1/tenants).
#
# Env (set by docker-compose): PGHOST PGPORT PGUSER PGPASSWORD CONTROL_DB MIG_DIR
# ─────────────────────────────────────────────────────────────────────────────
set -euo pipefail

CONTROL_DB="${CONTROL_DB:-hms_control}"
MIG_DIR="${MIG_DIR:-/db/postgres/migrations}"
PGPORT="${PGPORT:-5432}"
export PGPASSWORD

echo "[bootstrap] waiting for postgres ${PGHOST}:${PGPORT} ..."
for _ in $(seq 1 60); do
  if pg_isready -h "$PGHOST" -p "$PGPORT" -U "$PGUSER" >/dev/null 2>&1; then break; fi
  sleep 1
done
pg_isready -h "$PGHOST" -p "$PGPORT" -U "$PGUSER" >/dev/null 2>&1 \
  || { echo "[bootstrap] postgres never became ready"; exit 1; }

# 1) Control database.
if ! psql -d postgres -tAc "SELECT 1 FROM pg_database WHERE datname='${CONTROL_DB}'" | grep -q 1; then
  echo "[bootstrap] creating database ${CONTROL_DB}"
  psql -d postgres -v ON_ERROR_STOP=1 -c "CREATE DATABASE \"${CONTROL_DB}\""
else
  echo "[bootstrap] database ${CONTROL_DB} already exists"
fi

# 2) Control-plane migrations. Guard on a sentinel relation so re-runs are no-ops
#    even if a migration ever contained a non-idempotent statement.
apply() {  # $1 = file glob, $2 = sentinel regclass (schema.table)
  local present
  present="$(psql -d "${CONTROL_DB}" -tAc "SELECT to_regclass('$2')")"
  if [ -n "$present" ]; then
    echo "[bootstrap] $2 present — skipping ${1##*/}"
    return 0
  fi
  for f in $1; do
    [ -e "$f" ] || { echo "[bootstrap] WARN: no file matched ${1}"; continue; }
    echo "[bootstrap] applying $(basename "$f") -> ${CONTROL_DB}"
    psql -d "${CONTROL_DB}" -v ON_ERROR_STOP=1 -q -f "$f"
  done
}

# Column-add / seed-data-only migrations with no new sentinel table to key off —
# every statement in these files is itself idempotent (ADD COLUMN IF NOT EXISTS,
# ON CONFLICT DO NOTHING, WHERE NOT EXISTS, or an UPDATE that just re-sets the
# same values), so it's safe (and cheap) to just re-run them on every `up`.
apply_always() {  # $1 = file glob
  for f in $1; do
    [ -e "$f" ] || { echo "[bootstrap] WARN: no file matched ${1}"; continue; }
    echo "[bootstrap] applying $(basename "$f") -> ${CONTROL_DB}"
    psql -d "${CONTROL_DB}" -v ON_ERROR_STOP=1 -q -f "$f"
  done
}

apply "${MIG_DIR}/0001_"*.sql "control.tenants"
apply "${MIG_DIR}/0026_"*.sql "control.refresh_tokens"
# Billing catalog (#109) — plans/addons/platform settings the signup "choose plan"
# step and in-app billing screens read via GET /api/v1/billing/catalog.
apply "${MIG_DIR}/0064_"*.sql "control.plans"
apply_always "${MIG_DIR}/0066_"*.sql   # plan feature bullets (ALTER + conditional UPDATE)
apply_always "${MIG_DIR}/0067_"*.sql   # saved payment method columns (ALTER only)
apply "${MIG_DIR}/0068_"*.sql "control.billing_taxes"
apply "${MIG_DIR}/0069_"*.sql "control.platform_settings"
apply_always "${MIG_DIR}/0077_"*.sql   # plan feature bullets v2 (UPDATE only)
apply_always "${MIG_DIR}/0078_"*.sql   # plan max_locations (ALTER + UPDATE)
apply_always "${MIG_DIR}/0079_"*.sql   # e-receipt addons (ALTER + seed INSERT)

echo "[bootstrap] ✅ control plane ready on '${CONTROL_DB}'."
echo "[bootstrap]    Tenant databases are created on demand by the API at signup."
