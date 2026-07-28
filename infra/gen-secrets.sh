#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# Generate ./.env (repo root, gitignored) from infra/.env.example with FRESH
# secrets. Refuses to clobber an existing .env. Run once per box, then edit the
# non-secret values (PUBLIC_BASE_URL / SITE_ADDRESS / PLATFORM_ADMIN_EMAIL).
# ─────────────────────────────────────────────────────────────────────────────
set -euo pipefail

HERE="$(cd "$(dirname "$0")" && pwd)"
EXAMPLE="${HERE}/.env.example"
ENV_FILE="${HERE}/../.env"

command -v openssl >/dev/null || { echo "openssl is required"; exit 1; }
[ -f "$EXAMPLE" ] || { echo "missing $EXAMPLE"; exit 1; }
if [ -f "$ENV_FILE" ]; then
  echo "Refusing to overwrite existing ${ENV_FILE} (it may hold live secrets)."
  echo "Delete it first if you really want to regenerate."
  exit 1
fi

# >= 32 chars (HMAC signing key); 32-byte AES-256-GCM key (base64); alnum DB password.
JWT_SIGNING_KEY="$(openssl rand -base64 48 | tr -d '\n')"
SECRETS_MASTER_KEY="$(openssl rand -base64 32 | tr -d '\n')"
POSTGRES_PASSWORD="$(LC_ALL=C tr -dc 'A-Za-z0-9' </dev/urandom | head -c 28)"

cp "$EXAMPLE" "$ENV_FILE"
# '|' delimiter — the base64 alphabet (A-Za-z0-9+/=) contains no '|'.
sed -i.bak \
  -e "s|^JWT_SIGNING_KEY=.*|JWT_SIGNING_KEY=${JWT_SIGNING_KEY}|" \
  -e "s|^SECRETS_MASTER_KEY=.*|SECRETS_MASTER_KEY=${SECRETS_MASTER_KEY}|" \
  -e "s|^POSTGRES_PASSWORD=.*|POSTGRES_PASSWORD=${POSTGRES_PASSWORD}|" \
  "$ENV_FILE"
rm -f "${ENV_FILE}.bak"
chmod 600 "$ENV_FILE"

echo "✅ Wrote ${ENV_FILE} with freshly generated secrets (mode 600)."
echo
echo "Now edit ${ENV_FILE} and set, before 'docker compose up':"
echo "  PUBLIC_BASE_URL       e.g. http://<vps-ip>   or   https://hms.example.com"
echo "  SITE_ADDRESS          http://  (IP/HTTP)      or   hms.example.com (auto-HTTPS)"
echo "  PLATFORM_ADMIN_EMAIL  the platform admin (= first test tenant's owner email)"
