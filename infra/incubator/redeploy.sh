#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# Redeploy HMS code to the incubator: build locally → rsync → restart services.
# Run from anywhere in the repo:  ./infra/incubator/redeploy.sh
# For CONFIG/secret changes, edit /opt/hms/hms.env on the box instead (then
# `sudo systemctl restart hms-api`). First-time provisioning: see docs/deploy.md.
# Requires (local): dotnet 8 SDK, pnpm, rsync, ssh.
# ─────────────────────────────────────────────────────────────────────────────
set -euo pipefail
export CI=${CI:-true}   # non-interactive pnpm (avoids the no-TTY modules-purge prompt)
KEY="${HMS_SSH_KEY:-$HOME/.ssh/retailit_incubator}"
HOST="${HMS_SSH_HOST:-azureuser@20.212.21.252}"
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
RSH="ssh -i $KEY -o BatchMode=yes -o ServerAliveInterval=15 -o ServerAliveCountMax=4"

# Gentle, resilient rsync: cap bandwidth so a transfer can't saturate the shared
# box's I/O/network (which once thrashed it into an OOM-style outage), resume
# partial files, and retry transient SSH drops instead of aborting the deploy.
RSYNC_OPTS=(-az --partial --timeout=120 --bwlimit=12000 -e "$RSH")
rs() { local i; for i in 1 2 3; do rsync "${RSYNC_OPTS[@]}" "$@" && return 0; echo "  ⟳ rsync retry $i after a drop…"; sleep 5; done; echo "  ✗ rsync failed after 3 attempts"; return 1; }

echo "▸ publish API (self-contained linux-x64)"
rm -rf /tmp/hms-api-linux
dotnet publish "$ROOT/apps/api/Hms.Api.csproj" -c Release -r linux-x64 --self-contained true \
  -o /tmp/hms-api-linux /p:UseAppHost=true >/dev/null

echo "▸ build web (Next.js standalone)"
( cd "$ROOT/apps/web" && mkdir -p public && pnpm install --frozen-lockfile >/dev/null && pnpm build >/dev/null )

echo "▸ rsync artifacts → /opt/hms (bandwidth-capped, resumable, retried)"
rs --delete /tmp/hms-api-linux/        "$HOST:/opt/hms/api/"
rs --delete "$ROOT/db/postgres/"        "$HOST:/opt/hms/db/postgres/"
rs          "$ROOT/apps/web/.next/standalone/" "$HOST:/opt/hms/web/"
rs          "$ROOT/apps/web/.next/static/"     "$HOST:/opt/hms/web/.next/static/"
rs          "$ROOT/apps/web/public/"           "$HOST:/opt/hms/web/public/"

# Apply any pending DB migrations BEFORE restarting onto the new binary, so the
# schema is ready when the new API starts. Ledger-tracked (schema_migrations per
# DB) and baseline-on-first-run: the first time this runs it records the current
# files as already-applied (the DB is assumed current) and applies nothing; after
# that only NEW files run — control migrations (0001/0026) on the control DB, the
# rest on each tenant DB. Idempotent + safe to re-run.
echo "▸ apply pending DB migrations (ledger-tracked)"
$RSH "$HOST" 'bash -s' <<'MIGRATE'
set -euo pipefail
MIG=/opt/hms/db/postgres/migrations
CTRL=hms_main   # control plane DB on the incubator (holds control.tenants)
psql_d() { sudo -u postgres psql -d "$1" -v ON_ERROR_STOP=1 -qtA "${@:2}"; }
apply_db() {                       # $1 db, $2 kind(control|tenant)
  local db="$1" kind="$2" existed
  existed=$(sudo -u postgres psql -d "$db" -tAc "SELECT to_regclass('public.schema_migrations') IS NOT NULL")
  sudo -u postgres psql -d "$db" -v ON_ERROR_STOP=1 -qc \
    "CREATE TABLE IF NOT EXISTS schema_migrations (filename text PRIMARY KEY, applied_at timestamptz NOT NULL DEFAULT now())"
  if [ "$existed" != "t" ]; then   # first run → baseline (assume DB already current)
    for f in "$MIG"/0*.sql; do sudo -u postgres psql -d "$db" -v ON_ERROR_STOP=1 -qc \
      "INSERT INTO schema_migrations(filename) VALUES ('$(basename "$f")') ON CONFLICT DO NOTHING"; done
    echo "  [$db] baselined"; return 0
  fi
  local applied=0
  for f in "$MIG"/0*.sql; do
    local b fkind done; b=$(basename "$f")
    # Control-plane migrations are identified by "control" in the filename (matches
    # ProvisioningService's convention) — runs on hms_main, never on tenant DBs.
    case "$b" in *control*) fkind=control;; *) fkind=tenant;; esac
    [ "$fkind" = "$kind" ] || continue
    done=$(sudo -u postgres psql -d "$db" -tAc "SELECT 1 FROM schema_migrations WHERE filename='$b'")
    [ "$done" = "1" ] && continue
    echo "  [$db] applying $b"
    sudo -u postgres psql -d "$db" -v ON_ERROR_STOP=1 -qf "$f"
    sudo -u postgres psql -d "$db" -v ON_ERROR_STOP=1 -qc "INSERT INTO schema_migrations(filename) VALUES ('$b')"
    applied=1
  done
  # Migrations run as the postgres superuser, so any tables/sequences they CREATE
  # are owned by postgres. The app connects as a least-privilege role that must
  # OWN its tables to run DDL and own new sequences. RLS is enforced for that role
  # regardless of ownership: the tables FORCE row-level security (migration 0051)
  # and the app sets app.tenant_id on every connection, so a query that forgot to
  # scope itself returns ZERO rows rather than leaking across tenants. Hand any
  # public objects still owned by postgres to the app role (the dominant
  # non-postgres owner in this DB). Idempotent; no-op when nothing is postgres-owned.
  [ "$applied" = "1" ] || return 0    # nothing new applied → skip (return 0, not the test's exit code)
  local approle
  approle=$(sudo -u postgres psql -d "$db" -tAc "SELECT tableowner FROM pg_tables WHERE schemaname='public' AND tableowner<>'postgres' GROUP BY tableowner ORDER BY count(*) DESC LIMIT 1")
  [ -n "$approle" ] || return 0
  sudo -u postgres psql -d "$db" -v ON_ERROR_STOP=1 -tAc \
    "SELECT 'ALTER TABLE public.\"'||tablename||'\" OWNER TO \"$approle\";' FROM pg_tables WHERE schemaname='public' AND tableowner='postgres'
     UNION ALL SELECT 'ALTER SEQUENCE public.\"'||sequencename||'\" OWNER TO \"$approle\";' FROM pg_sequences WHERE schemaname='public' AND sequenceowner='postgres'" \
    | sudo -u postgres psql -d "$db" -v ON_ERROR_STOP=1 -q && echo "  [$db] normalized new-table ownership → $approle"
}
apply_db "$CTRL" control
for t in $(sudo -u postgres psql -d "$CTRL" -tAc "SELECT database_name FROM control.tenants WHERE database_name IS NOT NULL"); do
  apply_db "$t" tenant
done
echo "  migrations up to date"
MIGRATE

echo "▸ restart services (sequential, health-gated — never spin up both runtimes at once)"
$RSH "$HOST" 'bash -s' <<'RESTART'
set -uo pipefail
restart_wait() {  # $1 service, $2 health-url
  sudo systemctl enable "$1" >/dev/null 2>&1 || true   # survive reboots — idempotent; a missing enable
                                                       # is what turned a VM restart into a prolonged outage
  sudo systemctl restart "$1"
  local code=000
  for _ in $(seq 1 20); do
    code=$(curl -sS -o /dev/null -w "%{http_code}" --max-time 5 "$2" 2>/dev/null || echo 000)
    [ "$code" = 200 ] && { echo "  $1: healthy ($code)"; return 0; }
    sleep 2
  done
  echo "  $1: NOT healthy after ~40s (last $code)"; systemctl is-active "$1" || true; return 1
}
restart_wait hms-api http://127.0.0.1:8002/health/ready || exit 1
restart_wait hms-web http://127.0.0.1:8003/            || exit 1
RESTART

echo "✅ redeploy complete — https://hms.retailit.lk"
