#!/usr/bin/env bash
# Seed demo TRANSACTIONAL data via the API (so real business logic runs):
# customer categories + customers (incl. a credit account), promotions, and a
# supplier → purchase order → GRN (which moves stock + recomputes average cost).
# Requires the API running on :5000 and the demo tenant seeded (scripts/seed.sh).
# Idempotent-ish: customers/promotions/suppliers upsert by code; re-running adds
# another PO/GRN.
set -euo pipefail

API=${API:-http://localhost:5000}
DB_CONTROL=${DB_CONTROL:-hms_control}
DB_TENANT=${DB_TENANT:-hms_tenant_demo}
# Dev JWT signing key (matches appsettings.Development / the test harness).
KEY="dev-only-signing-key-replace-in-production-with-vault-secret-min-32-chars"

TENANT=$(psql -d "$DB_CONTROL" -tAc "SELECT id FROM control.tenants WHERE slug='demo'")
LOC=$(psql -d "$DB_TENANT" -tAc "SELECT id FROM locations WHERE code='MAIN'")
pid() { psql -d "$DB_TENANT" -tAc "SELECT id FROM products WHERE sku='$1'"; }
COKE=$(pid BEV-COKE); LION=$(pid BEV-LION); PEPSI=$(pid BEV-PEPSI)
[ -n "$TENANT" ] && [ -n "$LOC" ] || { echo "demo tenant/location not found — run scripts/seed.sh first"; exit 1; }

# Mint a short-lived Owner JWT for the demo tenant.
b64() { openssl base64 -e -A | tr '+/' '-_' | tr -d '='; }
H=$(printf '{"alg":"HS256","typ":"JWT"}' | b64)
P=$(printf '{"iss":"https://localhost:5001","aud":"rit-hms-api","tenant_id":"%s","role":"Owner","exp":%s}' "$TENANT" "$(( $(date +%s) + 3600 ))" | b64)
S=$(printf '%s.%s' "$H" "$P" | openssl dgst -sha256 -hmac "$KEY" -binary | b64)
JWT="$H.$P.$S"

api()  { curl -s -X "$1" "$API$2" -H "Authorization: Bearer $JWT" -H 'Content-Type: application/json' ${3:+-d "$3"}; }
idof() { grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4; }

echo "==> Customer categories"
CORP=$(api PUT /api/v1/customer-categories '{"name":"Corporate","discountPercent":5,"isActive":true}' | idof)
STAFF=$(api PUT /api/v1/customer-categories '{"name":"Staff","discountPercent":10,"isActive":true}' | idof)

echo "==> Customers"
api PUT /api/v1/customers "{\"code\":\"CG\",\"name\":\"Cinnamon Grand (Corporate)\",\"categoryId\":\"$CORP\",\"phone\":\"0112345678\",\"taxNo\":\"134000000-7000\",\"isCreditCustomer\":true,\"creditLimit\":100000,\"isActive\":true}" >/dev/null
api PUT /api/v1/customers "{\"code\":\"NIMAL\",\"name\":\"Nimal Perera\",\"phone\":\"0771234567\",\"discountPercent\":5,\"isCreditCustomer\":false,\"creditLimit\":0,\"isActive\":true}" >/dev/null
api PUT /api/v1/customers "{\"code\":\"ASHA\",\"name\":\"Asha Fernando\",\"categoryId\":\"$STAFF\",\"phone\":\"0719998887\",\"isCreditCustomer\":false,\"creditLimit\":0,\"isActive\":true}" >/dev/null
api PUT /api/v1/customers "{\"code\":\"KANDY\",\"name\":\"Kandy Catering (Credit)\",\"categoryId\":\"$CORP\",\"phone\":\"0812233445\",\"isCreditCustomer\":true,\"creditLimit\":50000,\"isActive\":true}" >/dev/null

echo "==> Promotions"
api PUT /api/v1/promotions "{\"code\":\"HAPPYHR\",\"name\":\"Happy Hour — 20% off beer\",\"promoType\":\"product_discount\",\"isActive\":true,\"autoApply\":true,\"priority\":1,\"daysMask\":127,\"startTime\":\"17:00:00\",\"endTime\":\"19:00:00\",\"appliesToOrderType\":\"dine_in\",\"displayMessage\":\"Happy Hour 5–7pm: 20% off Lion Lager\",\"lines\":[{\"productId\":\"$LION\",\"discountPercent\":20}]}" >/dev/null
api PUT /api/v1/promotions "{\"code\":\"SPENDSAVE\",\"name\":\"Spend 5,000 save 300\",\"promoType\":\"bill_value\",\"isActive\":true,\"autoApply\":true,\"priority\":2,\"daysMask\":127,\"displayMessage\":\"Spend LKR 5,000+ and save 300\",\"lines\":[{\"billFrom\":5000,\"discountAmount\":300}]}" >/dev/null

echo "==> Supplier + Purchase Order + GRN"
SUP=$(api POST /api/v1/suppliers '{"code":"CFF","name":"Colombo Fresh Foods","phone":"0114567890","email":"orders@cff.lk"}' | idof)
PO=$(api POST /api/v1/purchase-orders "{\"locationId\":\"$LOC\",\"supplierId\":\"$SUP\",\"lines\":[{\"productId\":\"$COKE\",\"quantity\":100,\"unitCost\":120},{\"productId\":\"$LION\",\"quantity\":48,\"unitCost\":380},{\"productId\":\"$PEPSI\",\"quantity\":100,\"unitCost\":115}]}" | idof)
api POST /api/v1/grn "{\"locationId\":\"$LOC\",\"supplierId\":\"$SUP\",\"purchaseOrderId\":\"$PO\",\"supplierInvoiceNo\":\"CFF-2026-0042\",\"lines\":[{\"productId\":\"$COKE\",\"quantity\":100,\"unitCost\":120},{\"productId\":\"$LION\",\"quantity\":48,\"unitCost\":380},{\"productId\":\"$PEPSI\",\"quantity\":100,\"unitCost\":115}]}" >/dev/null

# A second PO left open (sent, not yet received) so the Purchasing screen shows a "Receive" action.
api POST /api/v1/purchase-orders "{\"locationId\":\"$LOC\",\"supplierId\":\"$SUP\",\"lines\":[{\"productId\":\"$COKE\",\"quantity\":60,\"unitCost\":122}]}" >/dev/null

# ── End-to-end sales: shift → orders → KOT → settlements (cash/card/credit) ──
echo "==> Shift + orders (KOT + settlements)"
CHK=$(pid KOTTU-CHK); RICE=$(pid RICE-CHK); TEA=$(pid BEV-TEA); WAT=$(pid DES-WAT)
cust() { psql -d "$DB_TENANT" -tAc "SELECT id FROM customers WHERE code='$1'"; }
NIMAL_ID=$(cust NIMAL); CG_ID=$(cust CG)

# Open a shift (settling requires one). Ignore if one is already open.
api POST /api/v1/shifts/open "{\"locationId\":\"$LOC\",\"openingFloat\":5000}" >/dev/null 2>&1 || true

neworder() { api POST /api/v1/orders "$1" | idof; }
additem()  { api POST "/api/v1/orders/$1/items" "{\"productId\":\"$2\",\"quantity\":$3,\"station\":\"$4\"}" >/dev/null; }
total()    { api GET "/api/v1/orders/$1" | grep -o '"totalAmount":[0-9.]*' | head -1 | cut -d: -f2; }
settle()   { api POST "/api/v1/orders/$1/settle" "{\"payments\":[{\"payType\":\"$2\",\"amount\":$(total "$1")}]}" >/dev/null; }

# 1) Dine-in, sent to KOT, settled cash
O=$(neworder "{\"locationId\":\"$LOC\",\"orderType\":\"dine_in\",\"tableLabel\":\"5\",\"covers\":2}")
additem "$O" "$CHK" 2 kitchen; additem "$O" "$TEA" 2 bar
api POST "/api/v1/orders/$O/confirm" '' >/dev/null; settle "$O" cash

# 2) Takeaway, settled card
O=$(neworder "{\"locationId\":\"$LOC\",\"orderType\":\"takeaway\",\"covers\":1}")
additem "$O" "$RICE" 1 kitchen; settle "$O" card

# 3) Dine-in with a customer (5% discount), settled cash
O=$(neworder "{\"locationId\":\"$LOC\",\"orderType\":\"dine_in\",\"tableLabel\":\"8\",\"covers\":3}")
additem "$O" "$CHK" 1 kitchen; additem "$O" "$WAT" 2 kitchen
api POST "/api/v1/orders/$O/customer" "{\"customerId\":\"$NIMAL_ID\"}" >/dev/null; settle "$O" cash

# 4) Charge to a credit account (raises their AR balance)
O=$(neworder "{\"locationId\":\"$LOC\",\"orderType\":\"dine_in\",\"tableLabel\":\"12\",\"covers\":4}")
additem "$O" "$CHK" 3 kitchen; additem "$O" "$TEA" 4 bar
api POST "/api/v1/orders/$O/customer" "{\"customerId\":\"$CG_ID\"}" >/dev/null; settle "$O" credit

# 5) One OPEN bill left live on the floor (sent to KOT, not settled)
O=$(neworder "{\"locationId\":\"$LOC\",\"orderType\":\"dine_in\",\"tableLabel\":\"3\",\"covers\":2}")
additem "$O" "$RICE" 2 kitchen; additem "$O" "$TEA" 2 bar
api POST "/api/v1/orders/$O/confirm" '' >/dev/null

# ── Staff PINs so POS staff can sign in without email (4–8 digits, hashed) ──
echo "==> Staff PINs"
setpin() { local uid; uid=$(psql -d "$DB_TENANT" -tAc "SELECT id FROM users WHERE email='$1'"); [ -n "$uid" ] && api PUT "/api/v1/users/$uid/pin" "{\"pin\":\"$2\"}" >/dev/null; }
setpin cashier@demo.local 2468
setpin kitchen@demo.local 1357
setpin manager@demo.local 1234
# Usernames for PIN sign-in (workspace + username + PIN; no public roster).
psql -d "$DB_TENANT" -v ON_ERROR_STOP=1 -c \
  "UPDATE users SET username='asela' WHERE email='cashier@demo.local'; \
   UPDATE users SET username='kanan' WHERE email='kitchen@demo.local'; \
   UPDATE users SET username='maya'  WHERE email='manager@demo.local';" >/dev/null

# ── Loyalty: enable + give two customers a starting balance to demo redemption ──
echo "==> Loyalty"
api PUT /api/v1/settings '{"loyaltyEnabled":true,"loyaltyEarnRate":0.01,"loyaltyRedeemValue":1}' >/dev/null
psql -d "$DB_TENANT" -v ON_ERROR_STOP=1 -c "UPDATE customers SET loyalty_points=500, loyalty_lifetime_points=500 WHERE code IN ('NIMAL','CG');" >/dev/null

# ── POS depth (#76): servers, a tour operator, and foreign tender currencies ──
# A steward IS a user flagged is_server (managed under Team) — not a separate
# master. These three are login-less server records (name only); the demo cashier
# is also flagged so they appear in the POS "served by" dropdown.
echo "==> POS depth (servers / tour operator / currencies)"
api POST /api/v1/users '{"displayName":"Nuwan Silva","role":2,"isServer":true}' >/dev/null
api POST /api/v1/users '{"displayName":"Dilani Jay","role":2,"isServer":true}' >/dev/null
api POST /api/v1/users '{"displayName":"Roshan K","role":2,"isServer":true}' >/dev/null
psql -d "$DB_TENANT" -v ON_ERROR_STOP=1 -c "UPDATE users SET is_server=true WHERE email='cashier@demo.local';" >/dev/null
api POST /api/v1/tour-operators '{"code":"JETWING","name":"Jetwing Travels","commissionPercent":10,"isActive":true}' >/dev/null
api POST /api/v1/tour-operators '{"code":"AITKEN","name":"Aitken Spence Travels","commissionPercent":12.5,"isActive":true}' >/dev/null
api POST /api/v1/currencies '{"code":"LKR","name":"Sri Lankan Rupee","symbol":"Rs","isBase":true,"isActive":true}' >/dev/null
api POST /api/v1/currencies '{"code":"USD","name":"US Dollar","symbol":"$","rateToBase":300,"isActive":true}' >/dev/null
api POST /api/v1/currencies '{"code":"EUR","name":"Euro","symbol":"€","rateToBase":325,"isActive":true}' >/dev/null

# ── Catering / banquet (#75): a hall, a per-head package, a booking + deposit ──
echo "==> Catering (hall / package / booking)"
HALL=$(api POST /api/v1/catering/halls '{"code":"BALL","name":"Grand Ballroom","capacity":300,"isActive":true}' | idof)
api POST /api/v1/catering/halls '{"code":"LAWN","name":"Garden Lawn","capacity":150,"isActive":true}' >/dev/null
PKG=$(api POST /api/v1/catering/packages '{"code":"BUF-A","name":"Buffet A","pricePerHead":3500,"isActive":true}' | idof)
api POST /api/v1/catering/packages '{"code":"BUF-B","name":"Buffet B (premium)","pricePerHead":5500,"isActive":true}' >/dev/null
EV=$(api POST /api/v1/catering/events "{\"title\":\"Perera Wedding\",\"customerName\":\"Mr Perera\",\"customerPhone\":\"0771234567\",\"hallId\":\"$HALL\",\"packageId\":\"$PKG\",\"pax\":120,\"startsAt\":\"2026-12-20T18:00:00Z\",\"endsAt\":\"2026-12-20T23:00:00Z\",\"discountAmount\":20000}" | idof)
[ -n "$EV" ] && api POST "/api/v1/catering/events/$EV/payments" '{"amount":100000,"payType":"bank","kind":"deposit","reference":"DEP-001"}' >/dev/null
[ -n "$EV" ] && api POST "/api/v1/catering/events/$EV/status" '{"status":"confirmed"}' >/dev/null

echo "==> Done — customers, promotions, supplier, GRN + open PO, an open shift,"
echo "    4 settled bills (cash/card/discount/credit) + 1 open bill with live KOT,"
echo "    servers (is_server users) + tour operators + USD/EUR currencies."
