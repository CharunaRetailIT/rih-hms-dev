#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# Seed demo TRANSACTIONAL data on the incubator (the .NET API rejects forged dev
# JWTs, so this obtains a REAL Owner token via magic-link — the link is read from
# the API log, since email isn't wired). Mirrors scripts/seed-demo.sh: customers,
# promotions, supplier→PO→GRN, orders+KOT+settlements, staff PINs, loyalty.
# Prereq: scripts/seed.sh master data already applied to the tenant DB.
#
# Run ON the box, or from the repo:
#   PW=$(grep '^HMS_DB_PASSWORD=' ~/Downloads/rit-incubator-creds.txt | cut -d= -f2-)
#   ssh -i ~/.ssh/retailit_incubator azureuser@20.212.21.252 \
#     "DB_CONTROL=hms_main DB_TENANT=hms_tenant_demo PGHOST=127.0.0.1 PGUSER=hms_app PGPASSWORD=$PW bash -s" \
#     < infra/incubator/seed-demo.sh
# ─────────────────────────────────────────────────────────────────────────────
set -euo pipefail

API=${API:-http://127.0.0.1:8002}
SLUG=${SLUG:-demo}
OWNER=${OWNER:-owner@demo.local}
DB_CONTROL=${DB_CONTROL:-hms_main}
DB_TENANT=${DB_TENANT:-hms_tenant_demo}
LOG=${LOG:-/var/log/hms/api.log}

# ── Real Owner JWT via magic-link (link is logged, not emailed) ──
curl -s -X POST "$API/api/v1/auth/magic-link" -H 'Content-Type: application/json' \
  -d "{\"tenantSlug\":\"$SLUG\",\"email\":\"$OWNER\"}" >/dev/null
sleep 2
TOKEN=$(grep "MAGIC LINK for $OWNER" "$LOG" | tail -1 | sed -E 's/.*token=([a-f0-9]+).*/\1/')
JWT=$(curl -s -X POST "$API/api/v1/auth/exchange" -H 'Content-Type: application/json' \
  -d "{\"token\":\"$TOKEN\"}" | grep -o '"accessToken":"[^"]*"' | head -1 | cut -d'"' -f4)
[ -n "$JWT" ] || { echo "FATAL: could not obtain an Owner JWT (check $LOG)"; exit 1; }

TENANT=$(psql -d "$DB_CONTROL" -tAc "SELECT id FROM control.tenants WHERE slug='$SLUG'")
LOC=$(psql -d "$DB_TENANT" -tAc "SELECT id FROM locations WHERE code='MAIN'")
pid() { psql -d "$DB_TENANT" -tAc "SELECT id FROM products WHERE sku='$1'"; }
COKE=$(pid BEV-COKE); LION=$(pid BEV-LION); PEPSI=$(pid BEV-PEPSI)
[ -n "$TENANT" ] && [ -n "$LOC" ] || { echo "demo tenant/location not found — run scripts/seed.sh first"; exit 1; }

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

echo "==> Shift + orders (KOT + settlements)"
CHK=$(pid KOTTU-CHK); RICE=$(pid RICE-CHK); TEA=$(pid BEV-TEA); WAT=$(pid DES-WAT)
cust() { psql -d "$DB_TENANT" -tAc "SELECT id FROM customers WHERE code='$1'"; }
NIMAL_ID=$(cust NIMAL); CG_ID=$(cust CG)

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

echo "==> Staff PINs"
setpin() { local uid; uid=$(psql -d "$DB_TENANT" -tAc "SELECT id FROM users WHERE email='$1'"); [ -n "$uid" ] && api PUT "/api/v1/users/$uid/pin" "{\"pin\":\"$2\"}" >/dev/null; }
setpin cashier@demo.local 2468
setpin kitchen@demo.local 1357
setpin manager@demo.local 1234
psql -d "$DB_TENANT" -v ON_ERROR_STOP=1 -c \
  "UPDATE users SET username='asela' WHERE email='cashier@demo.local'; \
   UPDATE users SET username='kanan' WHERE email='kitchen@demo.local'; \
   UPDATE users SET username='maya'  WHERE email='manager@demo.local';" >/dev/null

echo "==> Loyalty"
api PUT /api/v1/settings '{"loyaltyEnabled":true,"loyaltyEarnRate":0.01,"loyaltyRedeemValue":1}' >/dev/null
psql -d "$DB_TENANT" -v ON_ERROR_STOP=1 -c "UPDATE customers SET loyalty_points=500, loyalty_lifetime_points=500 WHERE code IN ('NIMAL','CG');" >/dev/null

echo "==> Modifiers (groups + attach to a product)"
lid() { psql -d "$DB_TENANT" -tAc "SELECT id FROM locations WHERE code='$1'"; }
CK=$(lid CK); PAP=$(pid SIDE-PAPAD); LAMP=$(pid RICE-LAMP)
SPICE=$(api PUT /api/v1/modifier-groups '{"name":"Spice Level","minSelect":1,"maxSelect":1,"isRequired":true,"sortOrder":0,"items":[{"name":"Mild","priceDelta":0},{"name":"Medium","priceDelta":0},{"name":"Hot","priceDelta":0}]}' | idof)
ADDON=$(api PUT /api/v1/modifier-groups '{"name":"Add-ons","minSelect":0,"maxSelect":3,"isRequired":false,"sortOrder":1,"items":[{"name":"Extra Cheese","priceDelta":150},{"name":"Fried Egg","priceDelta":120},{"name":"Extra Gravy","priceDelta":80}]}' | idof)
api PUT "/api/v1/products/$CHK/modifiers" "{\"groupIds\":[\"$SPICE\",\"$ADDON\"]}" >/dev/null

echo "==> Stock transfer (Main -> Central Kitchen, dispatched + received)"
TR=$(api POST /api/v1/transfers "{\"fromLocationId\":\"$LOC\",\"toLocationId\":\"$CK\",\"isReturn\":false,\"notes\":\"Stock to central kitchen\",\"lines\":[{\"productId\":\"$RICE\",\"quantity\":20},{\"productId\":\"$PAP\",\"quantity\":20},{\"productId\":\"$CHK\",\"quantity\":10}]}" | idof)
api POST "/api/v1/transfers/$TR/dispatch" >/dev/null; api POST "/api/v1/transfers/$TR/receive" >/dev/null

echo "==> Production (recipe + a posted batch at the central kitchen)"
api PUT /api/v1/recipes "{\"productId\":\"$LAMP\",\"yieldQuantity\":1,\"notes\":\"Lamprais assembly\",\"lines\":[{\"ingredientProductId\":\"$RICE\",\"quantity\":1},{\"ingredientProductId\":\"$PAP\",\"quantity\":1}]}" >/dev/null
api POST /api/v1/production "{\"locationId\":\"$CK\",\"productId\":\"$LAMP\",\"quantity\":10,\"notes\":\"Lamprais batch\"}" >/dev/null

echo "==> Stock count (Main, with variance, posted)"
SC=$(api POST /api/v1/stock-counts "{\"locationId\":\"$LOC\",\"notes\":\"Beverage spot count\"}" | idof)
api PUT "/api/v1/stock-counts/$SC/lines" "{\"lines\":[{\"productId\":\"$COKE\",\"countedQty\":95},{\"productId\":\"$CHK\",\"countedQty\":48}]}" >/dev/null
api POST "/api/v1/stock-counts/$SC/post" >/dev/null

echo "==> Done — customers, promotions, supplier + GRN + open PO, an open shift,"
echo "    5 bills (4 settled cash/card/discount/credit + 1 open w/ live KOT),"
echo "    modifiers, a stock transfer, a production batch, and a posted stock count."
