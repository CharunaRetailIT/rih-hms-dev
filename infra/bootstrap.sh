#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# One-shot control-plane bootstrap. Creates the control database and applies its
# migrations (0001 schema+tenants+subscriptions, 0026 refresh_tokens). Idempotent:
# safe to re-run on every `docker compose up`. Per-tenant DBs are NOT created here
# — the API provisions those on signup (POST /api/v1/tenants).
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

apply "${MIG_DIR}/0001_"*.sql "control.tenants"
apply "${MIG_DIR}/0026_"*.sql "control.refresh_tokens"

echo "[bootstrap] ✅ control plane ready on '${CONTROL_DB}'."
echo "[bootstrap]    Tenant databases are created on demand by the API at signup."
