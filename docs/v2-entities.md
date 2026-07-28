# RIT HMS v2 — Entity Model

This document defines the complete v2 entity model. Entities are grouped by bounded context. v2 is an opinionated rewrite: god entities are decomposed, denormalized snapshots are dropped, typos are fixed, and string-typed soft-enums are replaced with real enums.

Related: [v2 API surface](./v2-api-surface.md) · [v2 Business rules](./v2-business-rules.md) · [v2 Multi-tenancy](./v2-multi-tenancy.md) · [v2 v1 scope](./v2-v1-scope.md).

## Typo & rename map

| Legacy name | v2 name |
|---|---|
| `Receipes` | `Recipe` |
| `Categoty$` / `RstCategories` | `Category` |
| `CateringMood` / `CateringMoods` | `CateringMode` |
| `LoyaltyCardSchems` | `LoyaltyCardScheme` |
| `Expiary*` / `ExpiryPoints1` | `Expiry*` |
| `CustomoerPreviousVisits` | `CustomerVisit` (deferred) |
| `RstDepartments` + `InterDepartments` | `Department` (with `department_type` enum) |
| `Customers` (68 cols) | `Customer` + `CustomerAddress` + `CustomerIdentifier` + `CustomerLoyaltyProfile` |
| `Employees` + `DeliveryPersons` + `StewardsMasters` | `Employee` (+ `role_type` enum) |
| `SuspendHeds` + `PaymentDets` (header bits) + `TransactionDets` (header bits) | `Order` |
| `TransactionDets` (120 cols) | `OrderLine` |
| `InvSales` (77 cols) | Materialized view, not a table |
| `ProductionNoteHeaders`/`Details` | `KitchenTicket` + `KitchenTicketLine` |
| `Tax` + `LocationTaxes` + `ProductTaxes` + `PayTypeTaxes` + `CateringModeTaxes` | `Tax` + 4 typed junctions with `tax_sequence` |

---

## 1. Tenancy & Identity context

| Entity | Source legacy | Purpose |
|---|---|---|
| `Tenant` | `SysGroupOfCompanies`, `SysCompanies` | A paying customer. One Postgres DB per tenant. |
| `Subscription` | (new) | Billing state, plan, trial window, period dates. |
| `SubscriptionPlan` | (new) | Catalogue of plans (Starter / Pro / Enterprise). |
| `Outlet` | `SysLocations`, `SYSLOC` | A physical restaurant location. v1: 1 per tenant. |
| `User` | `SysUserMasters`, `ApplicationUser` | A login. No password (Entra). |
| `Role` | `SysUserGroups`, `EmployeeGroups`, `CashierGroups`, `POSUserGroups` | RBAC role. v1: owner/manager/cashier. |
| `Permission` | `SysUserPermissions`, `SysUserGroupPermissions`, `CashierPermissions` | Feature grant (and optional limit). |
| `FeatureFlag` | (new) | Per-tenant feature toggle. |
| `TenantConfig` | (new) | Per-tenant JSON config (NOT code branch per customer). |

### `Tenant`
| Field | Type | Notes |
|---|---|---|
| `tenant_id` | `uuid` (v7) | PK |
| `name` | `text` | |
| `email` | `text` | Billing contact |
| `vat_number` | `text` | |
| `country_code` | `text` | ISO-3166 (default `LK`) |
| `is_active` | `bool` | |

Audit cols apply. Stored in **control plane** (`rit_control`), NOT a tenant DB.

### `Subscription`
| Field | Type | Notes |
|---|---|---|
| `subscription_id` | `uuid` | PK |
| `tenant_id` | `uuid` | FK → Tenant |
| `plan_id` | `uuid` | FK → SubscriptionPlan |
| `status` | `enum` | `trialing` \| `active` \| `past_due` \| `cancelled` |
| `trial_end_at` | `timestamptz` | |
| `current_period_start` | `timestamptz` | |
| `current_period_end` | `timestamptz` | |
| `payment_provider` | `enum` | `stripe` \| `payhere` |
| `external_subscription_id` | `text` | Stripe/PayHere ref |

Control plane.

### `Outlet`
| Field | Type | Notes |
|---|---|---|
| `outlet_id` | `uuid` | PK |
| `code` | `text` | Short code |
| `name` | `text` | |
| `address` | `text` | |
| `phone` | `text` | |
| `email` | `text` | |
| `is_vat` | `bool` | |
| `is_head_office` | `bool` | |
| `cost_center` | `text` | |

Drop `LocationIP`, `LocationPrefixCode`, `DataTransfer`.

### `User`
| Field | Type | Notes |
|---|---|---|
| `user_id` | `uuid` | PK |
| `email` | `text` | Unique within tenant |
| `name` | `text` | |
| `is_active` | `bool` | |
| `role_id` | `uuid` | FK → Role |
| `outlet_ids` | `uuid[]` | Scoped outlets |

No passwords. Entra is source of truth. This is local cache for display/scope.

### `Role`
| Field | Type | Notes |
|---|---|---|
| `role_id` | `uuid` | PK |
| `code` | `enum` | `OWNER` \| `MANAGER` \| `CASHIER` \| `KITCHEN` \| `WAITER` \| `ACCOUNTANT` \| `VIEWER` |
| `name` | `text` | |

### `Permission`
| Field | Type | Notes |
|---|---|---|
| `permission_id` | `uuid` | PK |
| `role_id` | `uuid` | FK → Role |
| `feature_key` | `text` | E.g. `void_invoice`, `approve_transfer` |
| `access_level` | `enum` | `allow` \| `deny` |
| `limit_value` | `numeric(15,4)` | E.g. max discount %, max void amount |

---

## 2. Master Data context

| Entity | Source legacy | Purpose |
|---|---|---|
| `Customer` | `Customers` (decomposed) | Core identity only |
| `CustomerAddress` | `Customers` (BillingAddress*, DeliveryAddress*, WorkAddres*) | Multiple typed addresses |
| `CustomerIdentifier` | `Customers` (NIC, Passport) | KYC-bearing fields |
| `CustomerLoyaltyProfile` | `Customers` (loyalty subset) | (v2) |
| `CustomerCategory` | `CustomerCategories` | Segment (VIP, walk-in, corporate) |
| `CustomerDiscount` | `CustomerDiscounts` | Per-customer product price overrides |
| `Employee` | `Employees` + `DeliveryPersons` + `StewardsMasters` | Unified staff master |
| `EmployeeCompensation` | `StewardsMasters.Commission/Target` | Time-bounded comp records |
| `Department` | `RstDepartments` + `InterDepartments` | Operational dept; `department_type` enum |
| `Category` | `RstCategories` | Menu/product middle level |
| `SubCategory` | `RstSubCategories` | Menu/product leaf level |
| `MealType` | `RstMealTypes` | Breakfast/Lunch/Dinner (v2) |
| `Currency` | `Currencies` | Multi-currency master |
| `CurrencyHistory` | `CurrencyHistories` | Rate history |
| `Tax` | `Taxes` | Base tax definitions |
| `ProductTax` | `ProductTaxes` | Product-tax junction with `tax_sequence` |
| `OutletTax` | `LocationTaxes` | Outlet override |
| `PaymentMethodTax` | `PayTypeTaxes` | Payment-method tax |
| `CateringModeTax` | `CateringModeTaxes` | Catering-mode tax (v2) |
| `Bank` | `Banks` | Payment processing |
| `BankBin` | `BankBins` | Card bin → bank → surcharge (v2 rules) |
| `Vehicle` | `Vehicles` | Delivery fleet (v2) |
| `CateringEvent` | `Events`, `EventProducts` | Banquet/catering menus (v2) |
| `Partner` | `TourAgents`, `TourAgentCompanies` | Tour partners (v2) |
| `Steward` | `StewardsMasters` | Drop as separate entity; folded into `Employee` |

### `Customer`
| Field | Type |
|---|---|
| `customer_id` | `uuid` |
| `code` | `text` |
| `name` | `text` |
| `category_id` | `uuid` FK |
| `date_of_birth` | `date` |
| `email` | `text` |
| `telephone` | `text` |
| `mobile` | `text` |
| `credit_limit` | `numeric(15,4)` |
| `customer_since` | `date` |
| `is_active` | `bool` |

Drop: `CustomerTitle`, `CustomerPicture*`, `Gender` (move to enum if kept), `Profession`, `Religion`, `Race`, `CivilStatus`, `SpecialDayType`, all `LOG*` audit-mirror tables.

### `CustomerAddress`
| Field | Type |
|---|---|
| `address_id` | `uuid` |
| `customer_id` | `uuid` FK |
| `address_type` | `enum` `billing` \| `delivery` \| `work` |
| `line1` / `line2` / `line3` | `text` |
| `city` | `text` |
| `postal_code` | `text` |

### `Employee` (unified)
| Field | Type | Notes |
|---|---|---|
| `employee_id` | `uuid` | PK |
| `code` | `text` | |
| `name` | `text` | |
| `role_type` | `enum` | `cashier` \| `manager` \| `steward` \| `delivery_driver` \| `kitchen` \| `waiter` \| `accountant` |
| `department_id` | `uuid` FK | |
| `nic` | `text` | |
| `passport` | `text` | |
| `epf_no` | `text` | |
| `email` | `text` | |
| `mobile` | `text` | |
| `is_active` | `bool` | |

Drop `EmployeePicture*` (use asset store), `IsDeliveryPerson`/`IsKarokeGirl` (now `role_type`), separate `DeliveryPersons` and `StewardsMasters` tables.

### `Tax` (and its 4 junctions)
| Tax | Field | Type |
|---|---|---|
| `Tax` | `tax_id` | `uuid` |
|  | `code` | `text` |
|  | `name` | `text` |
|  | `percentage` | `numeric(15,4)` |
|  | `is_tax_on_tax` | `bool` |
|  | `is_service_charge` | `bool` |
|  | `is_purchasing_tax` | `bool` |
|  | `is_selling_tax` | `bool` |
| `ProductTax` | `(product_id, tax_id, tax_sequence)` | composite |
| `OutletTax` | `(outlet_id, tax_id, tax_sequence, override_pct)` | composite |
| `PaymentMethodTax` | `(payment_method_id, tax_id, tax_sequence)` | composite |
| `CateringModeTax` | `(catering_mode, tax_id, tax_sequence)` | composite |

`tax_sequence INT` controls stacking order. Drop `POProductTaxes` and `TempItemTaxes` — transient.

### `Currency`
| Field | Type |
|---|---|
| `currency_id` | `uuid` |
| `code` | `text` ISO-4217 |
| `symbol` | `text` |
| `buying_rate` | `numeric(15,4)` |
| `selling_rate` | `numeric(15,4)` |
| `as_of_date` | `timestamptz` |

Drop `CurrencyFormat` (UI concern).

**Drop entirely**: `ReferenceTypes` (replace with C# enums), all `*$` Excel import staging tables, all `LOG*` audit-mirror tables (use event log instead), `Categoty$`, `Department$`, `Unit_Of_Measure$`, `Receipes12`, `Receipes3`, `tmpRecipes`, `ReportInfoes27052022`, `SuspendHedBackups`, `SuspendDetBackups`, `AutoGenerateInfo` / `AutoGenerateInfoes`.

---

## 3. Inventory / Procurement context

| Entity | Source legacy | Purpose |
|---|---|---|
| `Product` | `Products` | Item master (raw + finished) |
| `ProductServingUnit` | `ProductServingUnits` | Per-product serving variant + price |
| `ProductStock` | `ProductStockMasters` | Per-outlet stock-on-hand cache |
| `Supplier` | `Suppliers` | Vendor master |
| `SupplierGroup` | `SupplierGroups` | Vendor grouping |
| `SupplierType` | `SupplierTypes` | Vendor classification |
| `UnitOfMeasure` | `UnitOfMeasures` | Global UOM dictionary (shared, NOT tenant) |
| `UnitConversion` | `UnitConversions` | UOM hierarchy (1 kg = 1000 g) |
| `PurchaseOrder` + `PurchaseOrderLine` | `PurchaseOrderHeaders/Details` | PO doc (v2) |
| `GoodsReceiptNote` + `GoodsReceiptLine` | `PurchaseHeaders` (DocumentID=2, IsGRN=true) | GRN (v2) |
| `Recipe` + `RecipeIngredient` | `Receipes` | BOM (renamed) |
| `StockAdjustment` + `StockAdjustmentLine` | `StockAdjustmentHeaders/Details` | Manual corrections (v2) |
| `StockTransfer` + `StockTransferLine` | `TransferNoteHeaders/Details` | Inter-outlet move (v2) |
| `RequestNote` + `RequestNoteLine` | `RequestNote*` | Internal request (v2) |
| `Department` | `RstDepartments` | (See Master Data) |
| `Category` / `SubCategory` | `RstCategories` / `RstSubCategories` | (See Master Data) |

### `Product`
| Field | Type | Notes |
|---|---|---|
| `product_id` | `uuid` | PK |
| `code` | `text` | SKU |
| `name` | `text` | |
| `name_sinhala` | `text` | Localization |
| `is_raw_material` | `bool` | |
| `is_countable` | `bool` | |
| `is_scale_item` | `bool` | Weighable |
| `is_expiry` | `bool` | Triggers batch tracking |
| `department_id` | `uuid` FK | |
| `category_id` | `uuid` FK | |
| `sub_category_id` | `uuid` FK | |
| `purchasing_unit_id` | `uuid` FK → UnitOfMeasure | |
| `barcode` | `text` | |
| `re_order_level` | `numeric(15,4)` | |
| `re_order_quantity` | `numeric(15,4)` | |
| `wastage_prc` | `numeric(15,4)` | |
| `is_active` | `bool` | |

Drop image columns (asset store), `Products_Temp`, `LocationWiseStock`, all denormalized location/company FKs (use `tenant_id`), `DataTransfer`, kitchen routing fields (now in Kitchen context).

### `ProductStock`
| Field | Type | Notes |
|---|---|---|
| `product_stock_id` | `uuid` | PK |
| `product_id` | `uuid` FK | |
| `outlet_id` | `uuid` FK | |
| `stock_on_hand` | `numeric(15,4)` | |
| `cost_price` | `numeric(15,4)` | Moving-average |
| `selling_price` | `numeric(15,4)` | |

Drop all denormalized product fields (`ProductCode`, `ProductName`, `Barcode`, etc.) — join to `products`. Drop `AvgCost`, `OpenBal`, `InitSIH`, `InitCost`, `AdjQty` (recompute or event-source).

### `Recipe` + `RecipeIngredient`
| Recipe | Field | Type |
|---|---|---|
|  | `recipe_id` | `uuid` |
|  | `finished_product_id` | `uuid` FK |
|  | `product_serving_unit_id` | `uuid` FK |
|  | `output_qty` | `numeric(15,4)` |
|  | `cost_price` | `numeric(15,4)` |
|  | `selling_price` | `numeric(15,4)` |
| `RecipeIngredient` | `ingredient_id` | `uuid` |
|  | `recipe_id` | `uuid` FK |
|  | `raw_material_id` | `uuid` FK → Product |
|  | `material_qty` | `numeric(15,4)` |
|  | `unit_of_measure_id` | `uuid` FK |

### `Supplier`
| Field | Type |
|---|---|
| `supplier_id` | `uuid` |
| `code` | `text` |
| `name` | `text` |
| `contact_person_name` | `text` |
| `email` | `text` |
| `credit_limit` | `numeric(15,4)` |
| `credit_period` | `int` (days) |
| `payment_term_id` | `uuid` FK |
| `supplier_group_id` | `uuid` FK |
| `supplier_type_id` | `uuid` FK |
| `tax_registration_no` | `text` |
| `is_blocked` | `bool` |

Drop 5 `TaxNo*` fields (consolidate), 6 address fields (→ Address entity), `SupplierPicture*`, `EmailSubject`/`EmailBoday`, `ChequeLimit`/`ChequePeriod`.

### `UnitOfMeasure` (shared reference, NOT tenant-scoped)
| Field | Type |
|---|---|
| `unit_of_measure_id` | `uuid` |
| `code` | `text` (e.g. `KG`) |
| `name` | `text` |
| `is_active` | `bool` |

### `UnitConversion` (shared)
| Field | Type |
|---|---|
| `unit_conversion_id` | `uuid` |
| `base_unit_of_measure_id` | `uuid` FK |
| `sub_unit` | `text` (e.g. `GRAM`) |
| `sub_unit_symbol` | `text` |
| `sub_unit_value` | `numeric(15,4)` |

---

## 4. POS / Sales / Settlement context

| Entity | Source legacy | Purpose |
|---|---|---|
| `Order` | `SuspendHeds` + header fields of `PaymentDets` + `TransactionDets` | Aggregate root |
| `OrderLine` | `TransactionDets` (slimmed) | Settled sale line |
| `OrderPayment` | `PaymentDets` | Payment line (multi-tender) |
| `SuspendOrder` | `SuspendHeds` + `SuspendPaymentDets` | Open / held order |
| `AdvancePayment` + `AdvancePaymentLine` | `InvAdvanceNoteHeds/Dets`, `InvAdvancePaymentDets` | Pre-paid deposit (v2) |
| `OrderDiscount` | `TransactionDets.IDI*` columns flattened | Discount line |
| `OrderPromotionLine` | `IsPromotion*` columns flattened | Applied promo (v2) |
| `TransactionLog` | `TransactionLogs` | Immutable audit log |
| `CateringMode` | `CateringMood/CateringMoods` | dine-in / takeaway / delivery (typo fixed) |
| `Table` | `TableMasters` | Dine-in table |
| `Chair` | `ChairMasters` | Per-seat split (v2) |

### `Order` (aggregate root)
| Field | Type | Notes |
|---|---|---|
| `order_id` | `uuid` | PK |
| `order_no` | `text` | Human-readable |
| `receipt_no` | `text` | Receipt printout no |
| `outlet_id` | `uuid` FK | |
| `table_id` | `uuid` FK | nullable |
| `customer_id` | `uuid` FK | nullable |
| `cashier_id` | `uuid` FK → User | |
| `catering_mode` | `enum` | `dine_in` \| `takeaway` \| `room_service` \| `delivery` |
| `order_source` | `enum` | `pos` \| `uber_eats` \| `pickme` |
| `order_status` | `enum` | `draft` \| `confirmed` \| `posted` \| `void` |
| `payment_status` | `enum` | `unpaid` \| `partial` \| `paid` |
| `total_amount` | `numeric(15,4)` | |
| `discount_amount` | `numeric(15,4)` | |
| `tax_amount` | `numeric(15,4)` | |
| `service_charge_amount` | `numeric(15,4)` | |
| `net_amount` | `numeric(15,4)` | |
| `currency_id` | `uuid` FK | |
| `currency_rate` | `numeric(15,4)` | Locked at settlement |
| `shift_no` | `text` | |
| `z_no` | `text` | |
| `opened_at` | `timestamptz` | |
| `closed_at` | `timestamptz` | |

### `OrderLine`
| Field | Type |
|---|---|
| `order_line_id` | `uuid` |
| `order_id` | `uuid` FK |
| `line_no` | `int` |
| `product_id` | `uuid` FK |
| `product_serving_unit_id` | `uuid` FK |
| `quantity` | `numeric(15,4)` |
| `unit_price` | `numeric(15,4)` |
| `cost_price` | `numeric(15,4)` |
| `discount_amount` | `numeric(15,4)` |
| `tax_amount` | `numeric(15,4)` |
| `nett_amount` | `numeric(15,4)` |
| `batch_no` | `text` | nullable |
| `expiry_date` | `date` | nullable |
| `kitchen_station_id` | `uuid` FK | for KOT routing |
| `item_comment` | `text` | Custom instructions |
| `is_void` | `bool` | |

Drop ~100 columns from legacy `TransactionDets`: denormalized cashier/customer/salesman names, tour agent columns, 5×3 multi-discount columns (IDI1-5, IDis1-5, IDiscount1-5 → flatten), all `IsPrinted`/`IsPacked` flags (use events), `CopperratePrice*`, `WarrantyPeriod*`, `NextBillDate`.

### `OrderPayment`
| Field | Type |
|---|---|
| `order_payment_id` | `uuid` |
| `order_id` | `uuid` FK |
| `payment_method_id` | `uuid` FK |
| `row_no` | `int` |
| `amount` | `numeric(15,4)` |
| `balance` | `numeric(15,4)` |
| `currency_id` | `uuid` FK |
| `currency_rate` | `numeric(15,4)` |
| `ref_no` | `text` | Card auth code / cheque no |
| `bank_id` | `uuid` FK | nullable |
| `cheque_date` | `date` | nullable |
| `paid_at` | `timestamptz` | |

### `SuspendOrder`
| Field | Type |
|---|---|
| `suspend_order_id` | `uuid` |
| `order_id` | `uuid` FK |
| `outlet_id` | `uuid` FK |
| `table_id` | `uuid` FK |
| `token_number` | `text` |
| `cashier_id` | `uuid` FK |
| `suspended_at` | `timestamptz` |
| `recalled_at` | `timestamptz` |

### `TransactionLog` (immutable, append-only)
| Field | Type |
|---|---|
| `transaction_log_id` | `uuid` |
| `event_type` | `enum` |
| `entity_type` | `text` |
| `entity_id` | `uuid` |
| `payload` | `jsonb` |
| `user_id` | `uuid` |
| `occurred_at` | `timestamptz` |

**Drop entirely**: `InvSales` (77-col denormalized snapshot) — replace with materialized view over `order_lines + products + outlets + customers`.

---

## 5. Kitchen / KOT-BOT context

| Entity | Source legacy | Purpose |
|---|---|---|
| `KitchenStation` | `KitchenMasters` | Physical station (Kitchen/Bar/Pizza) |
| `PrinterType` | `PrinterTypes` | Replace with enum |
| `ProductStationMapping` | `ProductKitchenMappers` | Product → multiple stations |
| `KitchenTicket` | `ProductionNoteHeaders` | KOT/BOT ticket |
| `KitchenTicketLine` | `ProductionNoteDetails` | Line + recipe deduction |
| `ServingUnit` | `ServingUnits` | Single/Double/Half/Combo |
| `Addon` | `Addons` | Optional modifier |
| `AddonCategory` | `AddonCategoryMasters` | Modifier group |
| `ProductInstruction` | `ProductInstructions` | Free-text kitchen note (default) |

### `KitchenStation`
| Field | Type | Notes |
|---|---|---|
| `kitchen_station_id` | `uuid` | PK |
| `code` | `text` | |
| `description` | `text` | |
| `outlet_id` | `uuid` FK | |
| `printer_config` | `jsonb` | `{type, ip, mac, serial_port}` |
| `is_active` | `bool` | |

### `KitchenTicket`
| Field | Type |
|---|---|
| `kitchen_ticket_id` | `uuid` |
| `ticket_no` | `text` |
| `order_id` | `uuid` FK |
| `kitchen_station_id` | `uuid` FK |
| `status` | `enum` `pending` \| `printed` \| `reprinted` \| `voided` |
| `printed_at` | `timestamptz` nullable |

### `KitchenTicketLine`
| Field | Type |
|---|---|
| `kitchen_ticket_line_id` | `uuid` |
| `kitchen_ticket_id` | `uuid` FK |
| `order_line_id` | `uuid` FK |
| `product_id` | `uuid` FK |
| `quantity` | `numeric(15,4)` |
| `serving_unit` | `text` |
| `modifiers` | `jsonb` | Addon list |
| `instructions` | `text` |

### `Addon`
| Field | Type |
|---|---|
| `addon_id` | `uuid` |
| `parent_product_id` | `uuid` FK |
| `addon_product_id` | `uuid` FK |
| `addon_category_id` | `uuid` FK nullable |
| `selling_price_override` | `numeric(15,4)` |
| `quantity` | `numeric(15,4)` |
| `is_show_on_bill` | `bool` |

---

## 6. Finance / GL / Reports context

| Entity | Source legacy | Purpose |
|---|---|---|
| `PaymentMethod` | `PayTypes` + `PaymentMethods` (unified) | Cash / Card / Online / Uber / PickMe |
| `PaymentTerm` | `PaymentTerms` | Credit terms |
| `PaidInType` | `PaidInTypes` | Cash deposit category |
| `PaidOutType` | `PaidOutTypes` | Cash withdrawal category |
| `MonthEnd` | `MonthEnds` | Period closing lock |
| `JournalEntry` | `ImportJournalDetails` | GL staging |
| `JournalEntryBatch` | `ImportJournalDetailsLogs` | Batch audit |
| `GLTransferLog` | (new) | External GL POST audit |
| `OutboxEvent` | (new) | Reliable event delivery |

### `PaymentMethod` (unified)
| Field | Type | Notes |
|---|---|---|
| `payment_method_id` | `uuid` | PK |
| `code` | `text` | |
| `name` | `text` | |
| `type` | `enum` | `cash` \| `card` \| `wallet` \| `loyalty` \| `advance` \| `online` \| `aggregator` |
| `commission_rate` | `numeric(15,4)` | |
| `is_swipe` | `bool` | |
| `is_refundable` | `bool` | |
| `is_aggregator` | `bool` | |
| `aggregator_provider` | `enum` nullable | `uber_eats` \| `pickme` |
| `is_active` | `bool` | |

Legacy `PayTypes.PaymentID` hardcoded values (1=Cash, 2=Card, 3=Visa, 4=Amex, 58=Online, 59=Uber, 62=PickMe) are dropped — use UUID + `type` enum.

### `MonthEnd`
| Field | Type | Notes |
|---|---|---|
| `month_end_id` | `uuid` | PK |
| `outlet_id` | `uuid` FK | |
| `year` | `int` | |
| `month` | `int` | |
| `state` | `enum` | `open` \| `locking` \| `locked` \| `closed` |
| `closed_at` | `timestamptz` nullable | |
| `closed_by` | `uuid` FK | |

Replace confusing `LocStatus` + `LocIsClose` bool pair with single `state` enum.

### `JournalEntry`
| Field | Type | Notes |
|---|---|---|
| `journal_entry_id` | `uuid` | PK |
| `batch_id` | `uuid` FK | |
| `transaction_type` | `text` | `'08'` for sales |
| `doc_no` | `text` | |
| `entry_date` | `date` | |
| `seq_no` | `int` | |
| `account_code` | `text` | GL account |
| `cost_center` | `text` | |
| `dr_cr` | `enum` | `dr` \| `cr` |
| `amount` | `numeric(15,4)` | |
| `description` | `text` | |
| `gl_posted` | `bool` | |
| `gl_posted_at` | `timestamptz` | |

### `OutboxEvent` (used by GL, KOT, stock decrement, loyalty)
| Field | Type | Notes |
|---|---|---|
| `outbox_event_id` | `uuid` | PK |
| `event_type` | `text` | E.g. `OrderSettled`, `PaymentReconciled` |
| `aggregate_id` | `uuid` | |
| `payload` | `jsonb` | |
| `created_at` | `timestamptz` | |
| `processed_at` | `timestamptz` nullable | |
| `idempotency_key` | `text` unique | |

---

## 7. Hotel / Loyalty / Promotions / Gift Vouchers (v2)

All four sub-domains are **deferred to v2**. Listed here for schema continuity. See [v1 scope](./v2-v1-scope.md).

| Entity | Source legacy | v1 / v2 |
|---|---|---|
| `HotelRoom` | `RstRoomMasters` | v2 |
| `RoomType` | `RstRoomTypes` | v2 |
| `RoomTypeRate` | `RstRoomTypeRates` | v2 |
| `LoyaltyCard` | `CardMasters`, `CardTypes` | v2 |
| `LoyaltyCardScheme` | `LoyaltyCardSchems` (typo fix) | v2 |
| `LoyaltyCustomer` | `LoyaltyCustomers` | v2 |
| `LoyaltyTransaction` | `InvLoyaltyTransactions` | v2 |
| `PointsAudit` | (new — replaces CPoints/EPoints/RPoints columns) | v2 |
| `PointsExpiry` | `PointsExpirations` (typo fix) | v2 |
| `Promotion` | `InvPromotionMasters` + 7 detail tables | v2 |
| `PromotionRule` | (new — replaces 7 detail tables with rule engine) | v2 |
| `GiftVoucher` | `InvGiftVoucherMasters` | v2 |
| `GiftVoucherTransaction` | (new — replaces denormalized state columns) | v2 |

---

## v2 Postgres conventions

Apply to **every tenant-scoped table**:

| Column | Type | Default | Notes |
|---|---|---|---|
| `id` (or `<entity>_id`) | `uuid` | `uuidv7()` | Primary key, time-ordered |
| `tenant_id` | `uuid` | (from session) | FK → control plane `tenants` |
| `created_at` | `timestamptz` | `now()` | |
| `updated_at` | `timestamptz` | `now()` | Refreshed on every write |
| `created_by` | `uuid` | (current user) | FK → `users` |
| `updated_by` | `uuid` | (current user) | FK → `users` |
| `is_deleted` | `bool` | `false` | Soft-delete marker; partial index `WHERE is_deleted = false` |

**Type conventions**
- All monetary amounts: `numeric(15,4)`
- All percentages: `numeric(15,4)` (store as `0.15` for 15%)
- All rates / multipliers: `numeric(15,4)`
- All quantities: `numeric(15,4)`
- All primary and foreign keys: `uuid` (v7)
- All timestamps: `timestamptz`
- All enums: real Postgres `ENUM` types, mirrored in C# domain enums

**Row-Level Security**: every tenant-scoped table has an RLS policy `tenant_id = current_setting('app.tenant_id')::uuid`. Defence-in-depth alongside EF Core `HasQueryFilter`.

**Reference data exception**: `UnitOfMeasure`, `UnitConversion`, `Currency` (master only) live in a shared schema, NOT per-tenant. They have no `tenant_id`.

**Control plane exception**: `Tenant`, `Subscription`, `SubscriptionPlan` live in the `rit_control` database (not in a tenant DB). See [v2-multi-tenancy.md](./v2-multi-tenancy.md).

---

## Open Questions

1. Does v1 need `CustomerLoyaltyProfile` table created (empty), or fully deferred? **Recommend**: defer.
2. Should `UnitOfMeasure` be tenant-scoped or shared globally? **Recommend**: shared (UI shows global list).
3. Are batch/expiry fields (`batch_no`, `expiry_date`) v1 columns on `OrderLine` and `ProductStock`, or v2 additions? **Recommend**: column exists from v1, populated only when `Product.is_expiry = true`. [?]
4. Should `kitchen_station_id` be denormalized onto `OrderLine`, or computed from `ProductStationMapping` at KOT time? **Recommend**: compute at KOT time, do not store on `OrderLine`.
5. Are `service_charge_amount` and `tax_amount` derived (compute at query time) or stored on `Order`? **Recommend**: stored at settlement, recomputable as audit check.
6. What's the cardinality of `Address` for a customer — multiple billing/delivery, or one each? **Recommend**: multiple of each type allowed (set one `is_default`).
7. Should `OrderPromotionLine` be created in v1 schema (empty) for forward-compat? **Recommend**: yes; cheap.

