# RIT HMS v2 — REST API Surface

All endpoints are under `/api/v1/`. All require a tenant-scoped JWT (Entra External ID issued, `tenant_id` claim). All responses are JSON.

Related: [v2 Entities](./v2-entities.md) · [v2 Business rules](./v2-business-rules.md) · [v2 Multi-tenancy](./v2-multi-tenancy.md) · [v2 v1 scope](./v2-v1-scope.md).

---

## 1. Tenancy & Auth (control plane)

These endpoints live on the **control-plane** API host (not the tenant API host).

| Method | Path | Purpose | v1/v2 | Auth |
|---|---|---|---|---|
| `POST` | `/api/v1/auth/signup` | Tenant signup. Body: `{email, company_name, plan_id, payment_token}`. Triggers auto-provisioning. | v1 | public |
| `POST` | `/api/v1/auth/login` | Email-based login. Returns magic-link or Entra redirect URL. | v1 | public |
| `POST` | `/api/v1/auth/verify-link` | Verify magic-link token. Returns JWT. | v1 | public |
| `POST` | `/api/v1/auth/refresh` | Refresh JWT. | v1 | Bearer |
| `POST` | `/api/v1/auth/logout` | Revoke session. | v1 | Bearer |
| `GET` | `/api/v1/subscriptions/me` | Get current tenant subscription. | v1 | Bearer |
| `POST` | `/api/v1/subscriptions/change-plan` | Upgrade/downgrade plan. | v2 | Bearer |
| `POST` | `/api/v1/subscriptions/cancel` | Cancel subscription (end of period). | v1 | Bearer |
| `POST` | `/api/v1/webhooks/stripe` | Stripe webhook. | v1 | HMAC |
| `POST` | `/api/v1/webhooks/payhere` | PayHere webhook. | v1 | HMAC |
| `GET` | `/api/v1/provisioning/{job_id}` | Check tenant provisioning job status. | v1 | Bearer |

---

## 2. Identity & Access (tenant API)

| Method | Path | Purpose | v1/v2 |
|---|---|---|---|
| `GET` | `/api/v1/users` | List users in tenant. | v1 |
| `GET` | `/api/v1/users/{id}` | Get user details. | v1 |
| `POST` | `/api/v1/users` | Invite new user (sends Entra invitation). | v1 |
| `PATCH` | `/api/v1/users/{id}` | Update role / outlet scope / activation. | v1 |
| `DELETE` | `/api/v1/users/{id}` | Deactivate user. | v1 |
| `GET` | `/api/v1/users/me` | Current user profile + claims. | v1 |
| `GET` | `/api/v1/roles` | List roles + permissions. | v1 |
| `PATCH` | `/api/v1/roles/{id}/permissions` | Edit permission grid (Owner only). | v2 |
| `GET` | `/api/v1/outlets` | List outlets. v1: 1 outlet. | v1 |
| `POST` | `/api/v1/outlets` | Create outlet. | v2 |

---

## 3. Master Data

### Customers
| Method | Path | Purpose | v1/v2 |
|---|---|---|---|
| `GET` | `/api/v1/customers` | List with filters (`category_id`, `is_active`, search). | v1 |
| `GET` | `/api/v1/customers/{id}` | Customer + addresses + identifiers. | v1 |
| `POST` | `/api/v1/customers` | Create customer. | v1 |
| `PATCH` | `/api/v1/customers/{id}` | Update core. | v1 |
| `POST` | `/api/v1/customers/{id}/addresses` | Add address. | v1 |
| `GET` | `/api/v1/customers/{id}/discounts` | List active per-product discounts. | v1 |
| `POST` | `/api/v1/customers/{id}/discounts` | Create discount (manager). | v1 |
| `GET` | `/api/v1/customer-categories` | List categories. | v1 |

### Employees
| Method | Path | Purpose | v1/v2 |
|---|---|---|---|
| `GET` | `/api/v1/employees` | List with `role_type`, `department_id`, `is_active` filters. | v1 |
| `GET` | `/api/v1/employees/{id}` | Employee detail. | v1 |
| `POST` | `/api/v1/employees` | Create employee. | v1 |
| `PATCH` | `/api/v1/employees/{id}` | Update. | v1 |

### Reference / Master
| Method | Path | Purpose | v1/v2 |
|---|---|---|---|
| `GET` | `/api/v1/departments` | List departments. | v1 |
| `GET` | `/api/v1/categories` | List categories (filter by `department_id`). | v1 |
| `GET` | `/api/v1/sub-categories` | List sub-categories (filter by `category_id`). | v1 |
| `GET` | `/api/v1/units-of-measure` | List UOMs (shared reference). | v1 |
| `GET` | `/api/v1/unit-conversions` | List conversions. | v1 |
| `GET` | `/api/v1/currencies` | List currencies + current rate. | v1 |
| `GET` | `/api/v1/currencies/{code}/history` | Rate history. | v2 |
| `GET` | `/api/v1/taxes` | List tax definitions. | v1 |
| `GET` | `/api/v1/taxes/composition` | Returns stacked tax composition for `(product_id, outlet_id, payment_method_id, catering_mode)`. | v2 |
| `GET` | `/api/v1/banks` | List banks. | v1 |
| `GET` | `/api/v1/payment-methods` | List payment methods. | v1 |
| `GET` | `/api/v1/payment-terms` | List credit terms. | v1 |

---

## 4. Inventory

### Products
| Method | Path | Purpose | v1/v2 |
|---|---|---|---|
| `GET` | `/api/v1/products` | List with `is_raw_material`, `is_active`, search. Cursor pagination. | v1 |
| `GET` | `/api/v1/products/{id}` | Product detail incl. serving units. | v1 |
| `POST` | `/api/v1/products` | Create. | v1 |
| `PATCH` | `/api/v1/products/{id}` | Update. | v1 |
| `POST` | `/api/v1/products/{id}/serving-units` | Add serving unit + price. | v1 |
| `GET` | `/api/v1/products/{id}/station-mappings` | List kitchen stations product routes to. | v1 |

### Stock
| Method | Path | Purpose | v1/v2 |
|---|---|---|---|
| `GET` | `/api/v1/stock-on-hand` | All products at current outlet. | v1 |
| `GET` | `/api/v1/stock-on-hand/{product_id}` | Stock for one product. | v1 |
| `POST` | `/api/v1/stock-on-hand/initialize` | Bulk opening balances (one-time per outlet). | v1 |
| `POST` | `/api/v1/stock-adjustments` | Create adjustment. | v2 |
| `PATCH` | `/api/v1/stock-adjustments/{id}/approve` | Approve. | v2 |
| `PATCH` | `/api/v1/stock-adjustments/{id}/post` | Post → updates stock. | v2 |

### Suppliers
| Method | Path | Purpose | v1/v2 |
|---|---|---|---|
| `GET` | `/api/v1/suppliers` | List. | v1 |
| `GET` | `/api/v1/suppliers/{id}` | Detail. | v1 |
| `POST` | `/api/v1/suppliers` | Create. | v1 |
| `PATCH` | `/api/v1/suppliers/{id}` | Update. | v1 |

### POs / GRNs / Transfers (all v2)
| Method | Path | Purpose | v1/v2 |
|---|---|---|---|
| `POST` | `/api/v1/purchase-orders` | Create PO. | v2 |
| `PATCH` | `/api/v1/purchase-orders/{id}/approve` | Approve. | v2 |
| `POST` | `/api/v1/grns` | Create GRN linked to PO. | v2 |
| `PATCH` | `/api/v1/grns/{id}/accept` | Accept → increments stock. | v2 |
| `POST` | `/api/v1/stock-transfers` | Create transfer. | v2 |
| `PATCH` | `/api/v1/stock-transfers/{id}/confirm-receipt` | Confirm at destination. | v2 |
| `POST` | `/api/v1/recipes` | Create recipe + ingredients. | v1 |
| `GET` | `/api/v1/recipes` | List recipes (filter by `product_id`). | v1 |

---

## 5. POS / Orders / Settlement

| Method | Path | Purpose | v1/v2 |
|---|---|---|---|
| `POST` | `/api/v1/orders` | Open new order (returns `order_id`). Supports `Idempotency-Key`. | v1 |
| `GET` | `/api/v1/orders/{id}` | Order header + lines + payments. | v1 |
| `GET` | `/api/v1/orders?status=&date_from=&date_to=` | List with filters. Cursor pagination. | v1 |
| `POST` | `/api/v1/orders/{id}/lines` | Add line. Triggers KOT-routing preview. | v1 |
| `PATCH` | `/api/v1/orders/{id}/lines/{line_id}` | Modify qty / void. | v1 |
| `DELETE` | `/api/v1/orders/{id}/lines/{line_id}` | Soft-delete line. | v1 |
| `POST` | `/api/v1/orders/{id}/suspend` | Suspend (hold). | v1 |
| `POST` | `/api/v1/orders/{id}/recall` | Recall suspended. | v1 |
| `POST` | `/api/v1/orders/{id}/payments` | Submit payment line (multi-tender). Emits KOT-print event. | v1 |
| `POST` | `/api/v1/orders/{id}/settle` | Finalize. Inserts settled state. Emits stock-decrement event. | v1 |
| `POST` | `/api/v1/orders/{id}/void` | Void settled order. Reverses stock + GL. Supervisor only. | v1 |
| `POST` | `/api/v1/orders/{id}/print-receipt` | Re-print receipt. | v1 |
| `POST` | `/api/v1/orders/{id}/print-kot` | Manual KOT reprint. | v1 |
| `POST` | `/api/v1/orders/from-aggregator` | Create order from Uber/PickMe webhook. Sets `order_source`. | v2 |
| `PATCH` | `/api/v1/orders/{id}/aggregator-status` | Update aggregator status. | v2 |
| `GET` | `/api/v1/suspend-orders` | List held orders at current outlet. | v1 |
| `GET` | `/api/v1/tables` | List tables + occupancy state. | v1 |
| `PATCH` | `/api/v1/tables/{id}/state` | Mark table open/occupied/reserved. | v1 |
| `GET` | `/api/v1/catering-modes` | List dine-in/takeaway/etc. | v1 |
| `GET` | `/api/v1/transaction-logs` | Audit log (filter by date/user/document). | v1 |

---

## 6. Kitchen / KOT-BOT

| Method | Path | Purpose | v1/v2 |
|---|---|---|---|
| `GET` | `/api/v1/kitchen-stations` | List active stations + printer config. | v1 |
| `POST` | `/api/v1/kitchen-stations` | Create station. | v1 |
| `PATCH` | `/api/v1/kitchen-stations/{id}` | Update (printer config etc.). | v1 |
| `GET` | `/api/v1/kitchen-tickets/pending?station_id=` | **Print Agent polls this**. Returns pending tickets. | v1 |
| `PATCH` | `/api/v1/kitchen-tickets/{id}/status` | Print Agent reports `printed`/`reprinted`/`voided`. | v1 |
| `GET` | `/api/v1/kitchen-tickets?order_id=` | List tickets for an order. | v1 |
| `POST` | `/api/v1/kitchen-tickets/{id}/reprint` | Trigger manual reprint. | v1 |

---

## 7. Finance / Reports

| Method | Path | Purpose | v1/v2 |
|---|---|---|---|
| `GET` | `/api/v1/reports/daily-sales` | Daily sales CSV. Filters: `date`, `outlet_id`. | v1 |
| `GET` | `/api/v1/reports/payment-method-summary` | Cash-up sheet for a date+outlet. | v1 |
| `GET` | `/api/v1/reports/kot-log` | KOT log by date/outlet/station. | v1 |
| `GET` | `/api/v1/reports/sales-by-category` | Sales aggregated by category. | v1 |
| `GET` | `/api/v1/reports/top-selling-items` | Top N items by revenue/qty. | v1 |
| `GET` | `/api/v1/reports/inventory-position` | Current stock by outlet (defer if no GRN flow in v1). | v2 |
| `GET` | `/api/v1/month-ends` | List periods + state by outlet. | v1 |
| `POST` | `/api/v1/month-ends/open` | Open period. | v1 |
| `POST` | `/api/v1/month-ends/close` | Close (lock) period. | v1 |
| `POST` | `/api/v1/gl/journal-import` | Trigger GL import job (queued). | v2 |
| `POST` | `/api/v1/gl/transfer` | Push to external GL. | v2 |
| `GET` | `/api/v1/gl/transfer-audit` | GL transfer audit log. | v2 |

---

## 8. Deferred contexts (all v2)

| Method | Path | Purpose | v1/v2 |
|---|---|---|---|
| `GET` | `/api/v1/hotel-rooms` | List rooms. | v2 |
| `POST` | `/api/v1/loyalty-cards/register` | Register customer card. | v2 |
| `POST` | `/api/v1/loyalty/earn` | Earn points (called from settlement). | v2 |
| `POST` | `/api/v1/loyalty/redeem` | Redeem points. | v2 |
| `GET` | `/api/v1/promotions/active` | Active promos for terminal. | v2 |
| `POST` | `/api/v1/promotions/evaluate` | Evaluate rule set against bill. | v2 |
| `POST` | `/api/v1/gift-vouchers/sell` | Sell voucher. | v2 |
| `POST` | `/api/v1/gift-vouchers/redeem` | Redeem voucher (partial allowed). | v2 |
| `GET` | `/api/v1/gift-vouchers/{number}` | Lookup balance + status. | v2 |

---

## Cross-cutting concerns

### Pagination
All list endpoints use **cursor pagination**. Never offset/limit on tenant DBs.

```
GET /api/v1/orders?limit=50&cursor=eyJpZCI6...
```
Response includes `next_cursor` (null at end).

### Filtering
Query parameter `?filter=field op value,field2 op2 value2`. Operators: `eq`, `ne`, `gt`, `gte`, `lt`, `lte`, `in`, `like`. E.g.:

```
?filter=status eq settled,total_amount gte 5000
```

### Sorting
Comma-separated, prefix `-` for descending:

```
?sort=-created_at,name
```

### Batch endpoints
Where mutation throughput matters:

| Path | Purpose |
|---|---|
| `POST /api/v1/products/batch` | Up to 500 products in one call. |
| `POST /api/v1/orders/{id}/lines/batch` | Bulk-add lines (waiter tablet sync). |
| `POST /api/v1/stock-on-hand/initialize` | Bulk opening balances. |

Batch responses use `207 Multi-Status` with per-item result array.

### Idempotency
Every `POST` that mutates money or stock **must** accept `Idempotency-Key` header (UUID). Server stores `(tenant_id, idempotency_key) → response` for 24h.

Mandatory on:
- `POST /orders`
- `POST /orders/{id}/payments`
- `POST /orders/{id}/settle`
- `POST /orders/{id}/void`
- `POST /grns`
- `POST /stock-adjustments`
- All webhook handlers

### Auth header
```
Authorization: Bearer <JWT>
X-Outlet-Id: <uuid>       (optional; defaults to user's primary outlet)
```

JWT claims required: `sub` (user_id), `tenant_id`, `roles`, `outlet_ids`.

### Error format
```json
{
  "error": {
    "code": "STOCK_INSUFFICIENT",
    "message": "Insufficient stock for product X at outlet Y",
    "details": {...}
  },
  "trace_id": "..."
}
```

### Rate limits
Per-tenant: 100 req/s burst, 50 req/s sustained. Per-IP signup: 5/hour.

### Versioning
URL-versioned (`/api/v1/`). Breaking changes require `v2` namespace. Additive fields are non-breaking.

---

## Open Questions

1. Should Print Agent use **long-poll** (`GET /kitchen-tickets/pending`) or **Server-Sent Events** (`GET /kitchen-tickets/stream`)? **Recommend** SSE for v2, long-poll for v1.
2. Does the cashier UI need a `GET /api/v1/orders/active-at-table/{table_id}` shortcut, or just filter `GET /orders`? **Recommend** dedicated endpoint — common path. [?]
3. Should `POST /orders/{id}/settle` be **synchronous** (returns receipt JSON) or **async with job_id**? **Recommend** sync, fast path. Stock-decrement and GL post happen async via outbox.
4. Should aggregator webhooks (`/from-aggregator`) be on the tenant API or the control plane? **Recommend** tenant API — tenant must be resolvable from the aggregator's `restaurant_id`.

