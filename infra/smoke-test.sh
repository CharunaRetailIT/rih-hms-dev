#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# End-to-end smoke test against a running stack: sign up a tenant → grab the
# magic-link token from the API logs (Production suppresses it from the response)
# → log in → create a product → open a shift → take a POS order → settle it.
#
# Run from the repo root on the box where the stack is up:
#   ./infra/smoke-test.sh
# Override:  BASE_URL=http://1.2.3.4  SLUG=demo  ./infra/smoke-test.sh
# Requires:  curl, jq, and the `docker compose` CLI (to read the API logs).
# ─────────────────────────────────────────────────────────────────────────────
set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost}"
SLUG="${SLUG:-smoke$(date +%H%M%S)}"
DISPLAY_NAME="${DISPLAY_NAME:-Smoke Test Co}"
OWNER_EMAIL="${OWNER_EMAIL:-owner@${SLUG}.test}"
COMPOSE="${COMPOSE:-docker compose}"

command -v jq   >/dev/null || { echo "jq is required (apt-get install -y jq)"; exit 1; }
command -v curl >/dev/null || { echo "curl is required"; exit 1; }

step() { printf '\n\033[1;36m▸ %s\033[0m\n' "$*"; }
ok()   { printf '  \033[32m✓ %s\033[0m\n' "$*"; }
fail() { printf '\n\033[31m✗ FAILED: %s\033[0m\n' "$*"; exit 1; }

JSON='-H Content-Type:application/json'

step "1. Sign up tenant '${SLUG}' (triggers DB provisioning + baseline seed)"
SIGNUP=$(curl -fsS $JSON -X POST "${BASE_URL}/api/v1/tenants" \
  -d "{\"slug\":\"${SLUG}\",\"displayName\":\"${DISPLAY_NAME}\",\"ownerEmail\":\"${OWNER_EMAIL}\"}") \
  || fail "signup request"
echo "$SIGNUP" | jq -e '.provisioned == true' >/dev/null || fail "tenant not provisioned: $SIGNUP"
ok "tenant provisioned ($(echo "$SIGNUP" | jq -r '.slug'))"

step "2. Request a magic link for ${OWNER_EMAIL}"
curl -fsS $JSON -X POST "${BASE_URL}/api/v1/auth/magic-link" \
  -d "{\"tenantSlug\":\"${SLUG}\",\"email\":\"${OWNER_EMAIL}\"}" >/dev/null || fail "magic-link request"
ok "requested (Production does not return the link — reading it from the API logs)"

step "3. Extract the token from the API logs"
sleep 2
TOKEN=$($COMPOSE logs --no-color api 2>/dev/null \
  | grep "MAGIC LINK for ${OWNER_EMAIL}" | tail -1 \
  | sed -E 's/.*token=([a-f0-9]+).*/\1/')
[ -n "${TOKEN:-}" ] || fail "could not find a magic-link token in 'docker compose logs api'"
ok "token ${TOKEN:0:8}…"

step "4. Exchange the token for an access JWT"
EXCH=$(curl -fsS $JSON -X POST "${BASE_URL}/api/v1/auth/exchange" -d "{\"token\":\"${TOKEN}\"}") \
  || fail "token exchange"
ACCESS=$(echo "$EXCH" | jq -r '.accessToken')
[ -n "$ACCESS" ] && [ "$ACCESS" != "null" ] || fail "no accessToken: $EXCH"
AUTH="Authorization: Bearer ${ACCESS}"
ok "logged in as $(echo "$EXCH" | jq -r '.user.email') (role $(echo "$EXCH" | jq -r '.user.role'))"

step "5. Look up the seeded MAIN outlet + an 'each' unit of measure"
LOCATION_ID=$(curl -fsS -H "$AUTH" "${BASE_URL}/api/v1/locations" | jq -r '.[0].id')
UOM_ID=$(curl -fsS -H "$AUTH" "${BASE_URL}/api/v1/units-of-measure" \
  | jq -r '[.[] | select(.code=="EA")][0].id // .[0].id')
[ -n "$LOCATION_ID" ] && [ "$LOCATION_ID" != "null" ] || fail "no location"
[ -n "$UOM_ID" ] && [ "$UOM_ID" != "null" ] || fail "no unit of measure"
ok "location ${LOCATION_ID:0:8}… · uom ${UOM_ID:0:8}…"

step "6. Create a product"
PROD=$(curl -fsS $JSON -H "$AUTH" -X POST "${BASE_URL}/api/v1/products" \
  -d "{\"sku\":\"SMOKE-1\",\"name\":\"Smoke Test Burger\",\"unitOfMeasureId\":\"${UOM_ID}\",\"basePrice\":1000,\"taxClass\":\"standard\"}") \
  || fail "create product"
PRODUCT_ID=$(echo "$PROD" | jq -r '.id')
[ -n "$PRODUCT_ID" ] && [ "$PRODUCT_ID" != "null" ] || fail "no product id: $PROD"
ok "product ${PRODUCT_ID:0:8}…"

step "7. Open a shift (float 1000)"
curl -fsS $JSON -H "$AUTH" -X POST "${BASE_URL}/api/v1/shifts/open" \
  -d "{\"locationId\":\"${LOCATION_ID}\",\"openingFloat\":1000}" >/dev/null || fail "open shift"
ok "shift open"

step "8. Take a POS order: create → add 2× product → confirm (KOT)"
ORDER=$(curl -fsS $JSON -H "$AUTH" -X POST "${BASE_URL}/api/v1/orders" \
  -d "{\"locationId\":\"${LOCATION_ID}\",\"orderType\":\"takeaway\",\"orderSource\":\"pos\"}") \
  || fail "create order"
ORDER_ID=$(echo "$ORDER" | jq -r '.id')
[ -n "$ORDER_ID" ] && [ "$ORDER_ID" != "null" ] || fail "no order id: $ORDER"
curl -fsS $JSON -H "$AUTH" -X POST "${BASE_URL}/api/v1/orders/${ORDER_ID}/items" \
  -d "{\"productId\":\"${PRODUCT_ID}\",\"quantity\":2}" >/dev/null || fail "add item"
curl -fsS $JSON -H "$AUTH" -X POST "${BASE_URL}/api/v1/orders/${ORDER_ID}/confirm" >/dev/null || fail "confirm order"
TOTAL=$(curl -fsS -H "$AUTH" "${BASE_URL}/api/v1/orders/${ORDER_ID}" | jq -r '.totalAmount')
ok "order ${ORDER_ID:0:8}… · total ${TOTAL}"

step "9. Settle the bill (cash)"
SETTLED=$(curl -fsS $JSON -H "$AUTH" -X POST "${BASE_URL}/api/v1/orders/${ORDER_ID}/settle" \
  -d "{\"payments\":[{\"payType\":\"cash\",\"amount\":${TOTAL}}]}") \
  || fail "settle order"
STATUS=$(echo "$SETTLED" | jq -r '.status')
SETTLED_AT=$(echo "$SETTLED" | jq -r '.settledAt')
[ "$STATUS" = "settled" ] || fail "order not settled (status=${STATUS}): $SETTLED"
ok "settled at ${SETTLED_AT} · invoice $(echo "$SETTLED" | jq -r '.invoiceNumber')"

printf '\n\033[1;32m✅ SMOKE TEST PASSED\033[0m  tenant=%s  order=%s  total=%s\n' "$SLUG" "$ORDER_ID" "$TOTAL"
