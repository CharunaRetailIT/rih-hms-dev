#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# End-to-end smoke test against the live incubator deployment:
#   sign up a throwaway tenant → read its magic-link token from the API log
#   (over SSH; Production suppresses it from the response) → log in → product →
#   open shift → POS order → settle.
# Run from anywhere:  ./infra/incubator/smoke-test.sh
# Requires (local): curl, python3, ssh.
# ─────────────────────────────────────────────────────────────────────────────
set -uo pipefail
KEY="${HMS_SSH_KEY:-$HOME/.ssh/retailit_incubator}"
HOST="${HMS_SSH_HOST:-azureuser@20.212.21.252}"
BASE="${BASE_URL:-https://hms.retailit.lk}"
SLUG="${SLUG:-smoke$(date +%H%M%S)}"
EMAIL="${OWNER_EMAIL:-owner@${SLUG}.test}"
ACCESS=""
py(){ python3 -c "import sys,json; d=json.load(sys.stdin); print($1)"; }
req(){ local m=$1 p=$2 d=${3:-}; local a=(-sS -X "$m" "$BASE$p" -H "Content-Type: application/json"); [ -n "$ACCESS" ] && a+=(-H "Authorization: Bearer $ACCESS"); [ -n "$d" ] && a+=(-d "$d"); local o c b; o=$(curl "${a[@]}" -w $'\n%{http_code}'); c=${o##*$'\n'}; b=${o%$'\n'*}; if [ "${c:0:1}" != 2 ]; then echo "HTTP $c on $m $p :: $b" >&2; return 1; fi; printf '%s' "$b"; }
step(){ printf '\n▸ %s\n' "$*"; }

step "sign up tenant '$SLUG'"; req POST /api/v1/tenants "{\"slug\":\"$SLUG\",\"displayName\":\"Smoke Co\",\"ownerEmail\":\"$EMAIL\"}" | py "'provisioned='+str(d.get('provisioned'))" || exit 1
step "magic-link + token from server log"
req POST /api/v1/auth/magic-link "{\"tenantSlug\":\"$SLUG\",\"email\":\"$EMAIL\"}" >/dev/null || exit 1
sleep 2
TOKEN=$(ssh -i "$KEY" -o BatchMode=yes "$HOST" "grep 'MAGIC LINK for $EMAIL' /var/log/hms/api.log | tail -1" | sed -E 's/.*token=([a-f0-9]+).*/\1/')
[ -n "$TOKEN" ] || { echo "no token in log"; exit 1; }; echo "  token=${TOKEN:0:10}…"
step "exchange"; EXCH=$(req POST /api/v1/auth/exchange "{\"token\":\"$TOKEN\"}") || exit 1
ACCESS=$(echo "$EXCH" | py "d['accessToken']")
step "locations + uom"; LOC=$(req GET /api/v1/locations | py "d[0]['id']") || exit 1; UOM=$(req GET /api/v1/units-of-measure | py "next(u['id'] for u in d if u['code']=='EA')") || exit 1
step "product"; PID=$(req POST /api/v1/products "{\"sku\":\"SMOKE-1\",\"name\":\"Smoke Burger\",\"unitOfMeasureId\":\"$UOM\",\"basePrice\":1000,\"taxClass\":\"standard\"}" | py "d['id']") || exit 1
step "open shift"; req POST /api/v1/shifts/open "{\"locationId\":\"$LOC\",\"openingFloat\":1000}" >/dev/null || exit 1
step "order: create + item + confirm"; OID=$(req POST /api/v1/orders "{\"locationId\":\"$LOC\",\"orderType\":\"takeaway\",\"orderSource\":\"pos\"}" | py "d['id']") || exit 1
req POST "/api/v1/orders/$OID/items" "{\"productId\":\"$PID\",\"quantity\":2}" >/dev/null || exit 1
req POST "/api/v1/orders/$OID/confirm" >/dev/null || exit 1
TOTAL=$(req GET "/api/v1/orders/$OID" | py "d['totalAmount']") || exit 1
step "settle"; SET=$(req POST "/api/v1/orders/$OID/settle" "{\"payments\":[{\"payType\":\"cash\",\"amount\":$TOTAL}]}") || exit 1
ST=$(echo "$SET" | py "d['status']")
[ "$ST" = settled ] && printf '\n✅ SMOKE TEST PASSED — %s (order total %s, invoice %s)\n' "$BASE" "$TOTAL" "$(echo "$SET" | py "d.get('invoiceNumber')")" || { echo "status=$ST (expected settled)"; exit 1; }
