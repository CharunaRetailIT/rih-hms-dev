# SMART HMS — Database Documentation

> Source of truth: [`/db/schema/HMS_DATABASE_SCRIPT.sql`](../db/schema/HMS_DATABASE_SCRIPT.sql)
> (schema-only, no customer data).

## Engine & scale

- **Microsoft SQL Server**, schema `dbo`, UTF-16 LE script
- **207 tables**, **48 stored procedures**, 0 views, 0 functions, 2 triggers
- ~10 declared foreign keys (4% coverage — referential integrity is app-layer)
- 1.4 MB schema-only script, no data

## Bounded contexts (207 tables → 8 logical contexts)

| Context | ~Tables | Anchor entities |
|---|---|---|
| **Security & Tenancy** | 9 | `SysGroupOfCompanies` → `SysCompanies` → `SysLocations` → `SysUserMaster`, `SysUserGroups`, `SysUserPermissions` |
| **Restaurant Operations** | 26 | `RstCategories`, `RstDepartments`, `RstRoomMasters`, `RstRoomTypes/Rates`, `KitchenMasters`, `TableMasters`, `ChairMasters`, `Receipes` (sic) |
| **Inventory & Stock** | 30 | `Products`, `ProductStockMasters`, `PurchaseHeaders/Details`, `PurchaseOrderHeaders/Details`, `RequestNoteHeaders/Details`, `TransferNoteHeaders/Details`, `StockAdjustmentHeaders/Details`, `UnitOfMeasures`, `UnitConversions` |
| **POS / Sales** | 18 | `SuspendHeds`, `SuspendDets`, `PaymentDets`, `TransactionDets`, `TransactionLogs`, `InvSales`, `InvAdvanceNoteHeds/Dets`, `InvAdvancePaymentDets` |
| **Loyalty & Promotions** | 26 | `LoyaltyCardSchems`, `LoyaltyCardIssueHeaders/Details`, `LoyaltyCardGenerationHeaders/Details`, `InvLoyaltyTransactions`, `InvPromotionMasters`, `InvGiftVoucher*` (10 tables), `PointsExpiration*` |
| **Finance / GL** | 12 | `PayTypes`, `PaymentMethods`, `PaymentTerms`, `PaidInTypes`, `PaidOutTypes`, `Banks`, `Currencies`, `LoanPaymentDetails`, `MonthEnds`, `ImportJournalDetails`, `Taxes`, `LocationTaxes` |
| **Master / Reference Data** | 26 | `Customers`, `CustomerCategories`, `Suppliers`, `SupplierGroups`, `Employees`, `EmployeeGroups`, `DeliveryPersons`, `Vehicles`, `Events`, `TourAgents`, `Departments`, `ReferenceTypes` |
| **Audit, Reports & Junk** | 60+ | `LOG*` mirror tables (11), `*Backup`, `Temp*` / `tmp*` (10+), `ReportInfoes`, `ReportInfoes27052022`, `Receipes12`, `Receipes3`, `RECIPES_2$`, Excel-import leftovers (`PRODUCT$`, `Categoty$`, `Department$`, `Sub_Category$`, `Unit_Of_Measure$`, `RECIPES_2$`) |

## Critical tables (don't break these)

### `SuspendHeds` / `SuspendDets`
The "cart" while an order is open at a table. Recalled by cashier to settle.
**Lives in main DB**, written by both web apps.

### `PaymentDets`
Each payment line (cash / card / voucher / wallet). **`AFTER INSERT` trigger
`Trg_GenProductionNotes`** fires `genProductionNotes` stored proc to print
kitchen tickets.

### `TransactionDets`
Final settled sale lines. **`AFTER INSERT` trigger `Trigger_UpdateStockInHeadOffice`**
decrements `ProductStockMasters.Stock` when:
- `DocumentID IN (1, 3)` (sale or sale-with-tax)
- `BillTypeID = 1` (POS)
- `SaleTypeID = 1`
- `TransStatus = 1`
- `Status = 1`

> **Implications for any new integration**: if you insert into `TransactionDets`,
> stock decrements automatically. If you want delivery orders to *not* decrement
> until "order delivered" status, you must use a new `DocumentID` that the
> trigger does not match, OR change the trigger.

### `ProductStockMasters`
Per-location stock-on-hand. Updated by the trigger above plus by the various
stock-adjustment, GRN, transfer, and PO controllers.

### `STOS_TabOrderHeader` / `STOS_TabOrderDetail`
The HMSOrderTaker app's view of open table orders. Eventually flushed into
`SuspendHeds` / `SuspendDets` when the cashier recalls.

## Schema debris worth knowing about

| Name | Status |
|---|---|
| `Receipes` | Active recipe table (note typo) |
| `Receipes3`, `Receipes12`, `tmpRecipes`, `RECIPES_2$` | Legacy / orphaned. Confirm before deleting. |
| `PRODUCT$`, `Categoty$` (sic), `Department$`, `Sub_Category$`, `Unit_Of_Measure$`, `Sub_Category_1$` | Excel-import staging tables. Likely orphaned. |
| `Products_Temp` | Likely orphaned |
| `SuspendHedBackups`, `SuspendDetBackups` | Hand-rolled backup pairs |
| `ReportInfoes`, `ReportInfoes27052022` | Two copies of the same table, one dated |
| `TmpMatCons`, `TmpProductStockDetails`, `TmpProductTrans`, `TmpStockTrans`, `tmpItem`, `tmpMonthEnds`, `tmpUom` | Working tables (some are SQL `#temp` tables that should not be persisted) |
| `Categoty$` | Typo of `Category$` |

**Don't drop any of these without testing on a restored DB first.**
The legacy app may quietly reference one of them in a corner.

## Migration history

- EF6 Code-First with `__MigrationHistory` table
- **783 migration files** in `src/HospitalityManagement/Migrations/`
- Date range: 2018-07-07 → 2025-05 (active development)
- Many migrations named after individual developers (e.g. `Hasanka 001`,
  `hasanka002`, `chamodi_*`) — weak governance, no semantic naming
- Some hour-apart migrations (`36.0-4`, `36.0-5`, `36.0-6`) suggest band-aid
  patching rather than planned schema changes

> Running `Update-Database` on a fresh DB is high-risk. The recommended path
> is to restore from the schema script then apply only new migrations from
> a known good state.

## Stored procedures of note

- `SP_StockCAL` — stock recalculation
- `SP_DailySales`, `SP_GivenDateSales`, `SP_DB_HourlySales*`, `SP_DB_OrderTypeWiseSales`, `SP_DB_FoodCostEstimate*` — reporting
- `genProductionNotes` — fires from PaymentDets trigger to print KOTs
- `SP_HMSLoyaltyPointsExpirationSchedular` — points expiration job (no scheduler exists; appears to be called ad-hoc)
- `spImportJournalDetails`, `SpTransferToGL` — GL bridge
- `spRegisterLoyaltyCustomer` — loyalty signup

## Triggers

| Trigger | Fires on | Effect |
|---|---|---|
| `Trg_GenProductionNotes` | `PaymentDets` AFTER INSERT | Calls `genProductionNotes` to print kitchen tickets |
| `Trigger_UpdateStockInHeadOffice` | `TransactionDets` AFTER INSERT | Decrements `ProductStockMasters.Stock` |

These are the two most important objects in the database. Treat changes to them
as you would changes to production code — with tests on a restored DB.
