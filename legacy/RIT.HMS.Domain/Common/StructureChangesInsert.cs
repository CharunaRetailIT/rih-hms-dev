using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
namespace RIT.HMS.Domain.Common
{
    public class StructureChangesInsert
    {
        string Insertquery = "";
        string tableName = "";
        string Stringsqlconnection = "";
        public void RunInsert(string connectionString)
        {
            Stringsqlconnection = connectionString;
        
            
            #region InsertCashierFunctions Table Insert REPRINTDENIED
            tableName = "CashierFunctions";
            Insertquery = @"  INSERT INTO [dbo].[CashierFunctions] ([FunctionName],[FunctionDescription],[Order],[TypeID],[IsDelete],[IsValue]
                                ,[GroupOfCompanyID],[CreatedUser],[CreatedDate],[ModifiedUser],[ModifiedDate])
                                VALUES ('REPRINTDENIED','REPRINT PERMISSION GRANT',120,0,0,1,1,'Admin',GETDATE(),'Admin',GETDATE());";
            ExecuteInsert(Stringsqlconnection);
            #endregion InsertCashierFunctions Table Insert REPRINTDENIED



            #region InsertCashierFunctions Table Insert ALLOWNONTAX
            tableName = "CashierFunctions";
            Insertquery = @"  INSERT INTO [dbo].[CashierFunctions] ([FunctionName],[FunctionDescription],[Order],[TypeID],[IsDelete],[IsValue]
                                ,[GroupOfCompanyID],[CreatedUser],[CreatedDate],[ModifiedUser],[ModifiedDate])
                                VALUES ('ALLOWNONTAX','To Enable Service Chage and taxes',125,0,0,1,1,'Admin',GETDATE(),'Admin',GETDATE());";
            ExecuteInsert(Stringsqlconnection);
            #endregion InsertCashierFunctions Table Insert ALLOWNONTAX

            #region PayTypes Table Insert PickMe
            tableName = "PayTypes";
            Insertquery = @"INSERT INTO [dbo].[PayTypes]
           ([PaymentID]
           ,[Descrip]
           ,[IsSwipe]
           ,[Type]
           ,[Rate]
           ,[IsRefundable]
           ,[IsActive]
           ,[IsBillCopy]
           ,[PrintDescrip]
           ,[PreFix]
           ,[MaxLength]) VALUES (62,'PickMe',0,8,0.00,0,1,1,'PickMe','',0);";
            ExecuteInsert(Stringsqlconnection);
            #endregion PayTypes Table Insert PickMe

            #region ReportInfoes Table Insert Rafales
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Bill Listing Cancelled Bill Report'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Bill Listing Cancelled Report',
           'NULL',
           'NULL',
           'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_BillListingCancelledBillReport&rs:Command=Render',
           86,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion ReportInfoes Table Insert Rafales

            #region ReportInfoes Table Insert Rafales
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Bill Listing Summary'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
            VALUES
           (2,
           'Bill Listing Summary',
           'NULL',
           'NULL',
           'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_BillListingSummaryReport&rs:Command=Render',
           87,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion ReportInfoes Table Insert Rafales

            #region ReportInfoes Table Insert Rafales
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Bill Listing Detail'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Bill Listing Detail',
           'NULL',
           'NULL',
           'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_BillListingDetailReport&rs:Command=Render',
           88,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion ReportInfoes Table Insert Rafales

            #region ReportInfoes Table Insert Rafales
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Item Wise Sales Report'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Item Wise Sales Report',
           'NULL',
           'NULL',
           'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_ItemWiseSalesReport&rs:Command=Render',
           89,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion ReportInfoes Table Insert Rafales

            #region ReportInfoes Table Insert Rafales
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Item Wise Sales Summary Report'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Item Wise Sales Summary Report',
           'NULL',
           'NULL',
           'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_ItemWiseSalesSummaryReport&rs:Command=Render',
           90,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion ReportInfoes Table Insert Rafales

            #region ReportInfoes Table Insert Rafales
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Staff Usage Note Detail'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Staff Usage Note Detail',
           'NULL',
           'NULL',
           'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_StaffUsageNoteDetailReport&rs:Command=Render',
           91,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion ReportInfoes Table Insert Rafales

            #region ReportInfoes Table Insert Rafales
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Staff Usage Note Summary'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Staff Usage Note Summary',
           'NULL',
           'NULL',
           'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_StaffUsageNoteSummaryReport&rs:Command=Render',
           92,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion ReportInfoes Table Insert Rafales

            #region ReportInfoes Table Insert Rafales
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Daily (Hourly) Sales Analysis)')
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Daily (Hourly) Sales Analysis',
           'NULL',
           'NULL',
           'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_Daily(Hourly)SalesAnalysis&rs:Command=Render',
           93,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion ReportInfoes Table Insert Rafales

            #region ReportInfoes Table Insert Rafales
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Category Wise GRN'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Category Wise GRN',
           'NULL',
           'NULL',
           'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_CategoryWiseGRN&rs:Command=Render',
           94,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion ReportInfoes Table Insert Rafales

            #region ReportInfoes Table Insert Rafales
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Hourly Sales Report (Location Wise) - With Items'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Hourly Sales Report (Location Wise) - With Items',
           'NULL',
           'NULL',
           'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_HourlySales(Locationwise)Item&rs:Command=Render',
           95,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion ReportInfoes Table Insert Rafales

            #region ReportInfoes Table Insert Rafales
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Hourly Sales Report (Location Wise) - Without Items'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Hourly Sales Report (Location Wise) - Without Items',
           'NULL',
           'NULL',
           'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_HourlySales(Locationwise)WithoutItem&rs:Command=Render',
           96,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion ReportInfoes Table Insert Rafales

            #region ReportInfoes Table Insert Rafales
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Cancel KOT/BOT Listing Report'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Cancel KOT/BOT Listing Report',
           'NULL',
           'NULL',
           'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_CancelKOT_BOTListing&rs:Command=Render',
           97,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion ReportInfoes Table Insert Rafales

            #region ReportInfoes Table Insert Rafales
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Category Wise Gross Profit Report'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Category Wise Gross Profit Report',
           'NULL',
           'NULL',
           'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2rpt_CategoryWiseGrossProfitReport&rs:Command=Render',
           98,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion ReportInfoes Table Insert Rafales

            #region ReportInfoes Table Insert Rafales
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Item Re-Order Level Report'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Item Re-Order Level Report',
           'NULL',
           'NULL',
           'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_ItemReorderReport&rs:Command=Render',
           99,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion ReportInfoes Table Insert Rafales

            #region ReportInfoes Table Insert Rafales
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Department Wise Stock Variance'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Department Wise Stock Variance',
           'NULL',
           'NULL',
           'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_DepartmentWiseStockVariance&rs:Command=Render',
           100,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion ReportInfoes Table Insert Rafales

            #region ReportInfoes Table Insert Rafales
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Item Target Wise Sales'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Item Target Wise Sales',
           'NULL',
           'NULL',
           'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_ItemTargetWiseSalesReport&rs:Command=Render',
           101,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion ReportInfoes Table Insert Rafales

            #region ReportInfoes Table Insert Rafales
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Item Price List Report'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Item Price List Report',
           'NULL',
           'NULL',
           'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_ItemPriceListReport&rs:Command=Render',
           102,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion ReportInfoes Table Insert Rafales

            #region ReportInfoes Table Insert Rafales
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Bill Listing Summary'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
            VALUES
           (2,
           'Bill Listing Summary',
           'NULL',
           'NULL',
           'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_BillListingSummaryReport&rs:Command=Render',
           87,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion ReportInfoes Table Insert Rafales

            #region ReportInfoes Table Insert Rafales
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Department Wise Gross Grofit Report'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Department Wise Gross Grofit Report',
           'NULL',
           'NULL',
           'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_DepartmentWiseGrossGrofitReport&rs:Command=Render',
           103,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion ReportInfoes Table Insert Rafales

            #region Menu Item Stock Summary Report
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Menu Item Stock Summary Report'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Menu Item Stock Summary Report',
           'NULL',
           'NULL',
          'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_MenuItemStockSummaryReport&rs:Command=Render',
           104,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion Menu Item Stock Summary Report

            #region Menu Item Stock Detail Report
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Menu Item Stock Detail Report'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Menu Item Stock Detail Report',
           'NULL',
           'NULL',
          'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_MenuItemStockDetailReport&rs:Command=Render',
           105,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion Menu Item Stock Detail Report

            #region Item Wise GRN Report
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Item Wise GRN Report'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Item Wise GRN Report',
           'NULL',
           'NULL',
          'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_ItemWiseGRNReport&rs:Command=Render',
           106,
           1)
            END";
            #region ReportInfoes Table Insert Rafales
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Sales Listing Report'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Sales Listing Report',
           'NULL',
           'NULL',
           'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_SalesListingReport&rs:Command=Render',
           107,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion ReportInfoes Table Insert Rafales
            ExecuteInsert(Stringsqlconnection);

            #endregion Menu Item Stock Detail Report

            #region Invoice Reprint Report
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Invoice Reprint Report'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Invoice Reprint Report',
           'NULL',
           'NULL',
          'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_InvoiceReprintReport&rs:Command=Render',
           107,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion Invoice Reprint Report

            #region Order Sales Detail Report
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Order Sales Detail Report'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Order Sales Detail Report',
           'NULL',
           'NULL',
          'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_ProductWiseOrderSalesDetail&rs:Command=Render',
           108,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion Invoice Reprint Report

            #region ReportInfoes Table Insert Rafales
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Bill Listing Summary'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
            VALUES
           (2,
           'Bill Listing Summary',
           'NULL',
           'NULL',
           'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_BillListingSummaryReport&rs:Command=Render',
           87,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion ReportInfoes Table Insert Rafales

            #region Order Sales Summary Report
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Order Sales Summary Report'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Order Sales Summary Report',
           'NULL',
           'NULL',
          'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_ProductWiseOrderSalesSummary&rs:Command=Render',
           109,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion Invoice Reprint Report

            #region Order Sales Summary Report
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Price Change Item Report'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Price Change Item Report',
           'NULL',
           'NULL',
          'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_PriceChnageItemReport&rs:Command=Render',
           113,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion Invoice Reprint Report

            #region Receipt Summary Report
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Receipt Summary Report'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Receipt Summary Report',
           'NULL',
           'NULL',
          'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_CashInCashOutReceiptSummaryReport&rs:Command=Render',
           114,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion Receipt Summary Report

            #region User Permissions Report
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'User Permissions Report'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'User Permissions Report',
           'NULL',
           'NULL',
          'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_UserpermissionsReport&rs:Command=Render',
           115,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion User Permissions Report

            #region Withdrawal Summary Report
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Withdrawal Summary Report'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Withdrawal Summary Report',
           'NULL',
           'NULL',
          'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_CashOutWithdrawalSummaryReport&rs:Command=Render',
           116,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion Withdrawal Summary Report

            #region Product Wise Sales - Best Selling Products Report - II
            tableName = "ReportInfoes";
            Insertquery = @"
            IF (NOT EXISTS (SELECT * FROM ReportInfoes WHERE  ReportName = 'Product Wise Sales - Best Selling Products Report - II'))
            BEGIN
            INSERT INTO [dbo].[ReportInfoes]
           ([ReportCategoryId]
           ,[ReportName]
           ,[ReportPath]
           ,[ReportFileName]
           ,[ReportURL]
           ,[OrderId]
           ,[CompanyID])
     VALUES
           (2,
           'Product Wise Sales - Best Selling Products Report - II',
           'NULL',
           'NULL',
          'http://SERVER/ReportServer/Pages/ReportViewer.aspx?%2fReport+Project1%2frpt_ProductWiseSales_LocWise2&rs:Command=Render',
           116,
           1)
            END";
            ExecuteInsert(Stringsqlconnection);

            #endregion Withdrawal Summary Report


            #region 


            Insertquery = @" -- Batch insert for multiple records
INSERT INTO SysUserFunctions (
    FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId
)
SELECT 'Customer', 'Customer SSRS Report', 1, 1, 0, 1, 1, 1, 1, 'admin', '2024-09-13 09:44:56.640', 'admin', '2024-09-13 09:44:56.640', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Customer'
      AND FunctionDescription = 'Customer SSRS Report'
      AND [Order] = 1
      AND TypeID = 1
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 09:44:56.640'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 09:44:56.640'
      AND DataTransfer = 1
      AND FormId = 999
)
UNION ALL
SELECT 'Supplier', 'Supplier SSRS Report', 1, 1, 0, 1, 1, 1, 1, 'admin', '2024-09-13 09:44:56.640', 'admin', '2024-09-13 09:44:56.640', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Supplier'
      AND FunctionDescription = 'Supplier SSRS Report'
      AND [Order] = 1
      AND TypeID = 1
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 09:44:56.640'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 09:44:56.640'
      AND DataTransfer = 1
      AND FormId = 999
);

SELECT 'Employee', 'Employee SSRS Report', 1, 1, 0, 1, 1, 1, 1, 'admin', '2024-09-13 09:44:56.640', 'admin', '2024-09-13 09:44:56.640', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Employee'
      AND FunctionDescription = 'Employee SSRS Report'
      AND [Order] = 1
      AND TypeID = 1
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 09:44:56.640'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 09:44:56.640'
      AND DataTransfer = 1
      AND FormId = 999
);


-- Insert if not exists for Category
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Category', 'Category SSRS Report', 1, 1, 0, 1, 1, 1, 1, 'admin', '2024-09-13 09:59:58.593', 'admin', '2024-09-13 09:59:58.593', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Category'
      AND FunctionDescription = 'Category SSRS Report'
      AND [Order] = 1
      AND TypeID = 1
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 09:59:58.593'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 09:59:58.593'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Department
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Department', 'Department SSRS Report', 1, 1, 0, 1, 1, 1, 1, 'admin', '2024-09-13 09:59:58.593', 'admin', '2024-09-13 09:59:58.593', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Department'
      AND FunctionDescription = 'Department SSRS Report'
      AND [Order] = 1
      AND TypeID = 1
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 09:59:58.593'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 09:59:58.593'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Employee Group
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Employee Group', 'Employee Group SSRS Report', 1, 1, 0, 1, 1, 1, 1, 'admin', '2024-09-13 09:59:58.593', 'admin', '2024-09-13 09:59:58.593', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Employee Group'
      AND FunctionDescription = 'Employee Group SSRS Report'
      AND [Order] = 1
      AND TypeID = 1
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 09:59:58.593'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 09:59:58.593'
      AND DataTransfer = 1
      AND FormId = 999
);


-- Insert if not exists for Sub Category
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Sub Category', 'Sub Category SSRS Report', 1, 1, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:21:59.903', 'admin', '2024-09-13 10:21:59.903', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Sub Category'
      AND FunctionDescription = 'Sub Category SSRS Report'
      AND [Order] = 1
      AND TypeID = 1
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:21:59.903'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:21:59.903'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Supplier Group
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Supplier Group', 'Supplier Group SSRS Report', 1, 1, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:21:59.903', 'admin', '2024-09-13 10:21:59.903', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Supplier Group'
      AND FunctionDescription = 'Supplier Group SSRS Report'
      AND [Order] = 1
      AND TypeID = 1
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:21:59.903'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:21:59.903'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Table Master
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Table Master', 'Table Master SSRS Report', 1, 1, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:21:59.903', 'admin', '2024-09-13 10:21:59.903', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Table Master'
      AND FunctionDescription = 'Table Master SSRS Report'
      AND [Order] = 1
      AND TypeID = 1
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:21:59.903'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:21:59.903'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Tax Master
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Tax Master', 'Tax Master SSRS Report', 1, 1, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:21:59.903', 'admin', '2024-09-13 10:21:59.903', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Tax Master'
      AND FunctionDescription = 'Tax Master SSRS Report'
      AND [Order] = 1
      AND TypeID = 1
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:21:59.903'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:21:59.903'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Vehicle
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Vehicle', 'Vehicle SSRS Report', 1, 1, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:21:59.903', 'admin', '2024-09-13 10:21:59.903', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Vehicle'
      AND FunctionDescription = 'Vehicle SSRS Report'
      AND [Order] = 1
      AND TypeID = 1
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:21:59.903'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:21:59.903'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Products Master
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Products Master', 'Products Master SSRS Report', 1, 1, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:21:59.903', 'admin', '2024-09-13 10:21:59.903', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Products Master'
      AND FunctionDescription = 'Products Master SSRS Report'
      AND [Order] = 1
      AND TypeID = 1
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:21:59.903'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:21:59.903'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for ChairMaster
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'ChairMaster', 'ChairMaster SSRS Report', 1, 1, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:21:59.903', 'admin', '2024-09-13 10:21:59.903', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'ChairMaster'
      AND FunctionDescription = 'ChairMaster SSRS Report'
      AND [Order] = 1
      AND TypeID = 1
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:21:59.903'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:21:59.903'
      AND DataTransfer = 1
      AND FormId = 999
);
-- Insert if not exists for Currencies
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Currencies', 'Currencies SSRS Report', 1, 1, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:38:06.077', 'admin', '2024-09-13 10:38:06.077', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Currencies'
      AND FunctionDescription = 'Currencies SSRS Report'
      AND [Order] = 1
      AND TypeID = 1
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:38:06.077'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:38:06.077'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Payment Method
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Payment Method', 'Payment Method SSRS Report', 1, 1, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:38:06.077', 'admin', '2024-09-13 10:38:06.077', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Payment Method'
      AND FunctionDescription = 'Payment Method SSRS Report'
      AND [Order] = 1
      AND TypeID = 1
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:38:06.077'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:38:06.077'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Room Master
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Room Master', 'Room Master SSRS Report', 1, 1, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:38:06.077', 'admin', '2024-09-13 10:38:06.077', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Room Master'
      AND FunctionDescription = 'Room Master SSRS Report'
      AND [Order] = 1
      AND TypeID = 1
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:38:06.077'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:38:06.077'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Room Type Rates
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Room Type Rates', 'Room Type Rates SSRS Report', 1, 1, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:38:06.077', 'admin', '2024-09-13 10:38:06.077', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Room Type Rates'
      AND FunctionDescription = 'Room Type Rates SSRS Report'
      AND [Order] = 1
      AND TypeID = 1
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:38:06.077'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:38:06.077'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Room Types
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Room Types', 'Room Types SSRS Report', 1, 1, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:38:06.077', 'admin', '2024-09-13 10:38:06.077', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Room Types'
      AND FunctionDescription = 'Room Types SSRS Report'
      AND [Order] = 1
      AND TypeID = 1
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:38:06.077'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:38:06.077'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for PO Summary
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'PO Summary', 'PO Summary SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:38:06.077', 'admin', '2024-09-13 10:38:06.077', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'PO Summary'
      AND FunctionDescription = 'PO Summary SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:38:06.077'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:38:06.077'
      AND DataTransfer = 1
      AND FormId = 999
);
-- Insert if not exists for PRN Summary
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'PRN Summary', 'PRN Summary SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:39:05.893', 'admin', '2024-09-13 10:39:05.893', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'PRN Summary'
      AND FunctionDescription = 'PRN Summary SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:39:05.893'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:39:05.893'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for TOG Summary
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'TOG Summary', 'TOG Summary SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:39:05.893', 'admin', '2024-09-13 10:39:05.893', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'TOG Summary'
      AND FunctionDescription = 'TOG Summary SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:39:05.893'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:39:05.893'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Request Note Summary
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Request Note Summary', 'Request Note Summary SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:39:05.893', 'admin', '2024-09-13 10:39:05.893', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Request Note Summary'
      AND FunctionDescription = 'Request Note Summary SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:39:05.893'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:39:05.893'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Request Note Acceptance Summary
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Request Note Acceptance Summary', 'Request Note Acceptance Summary SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:39:05.893', 'admin', '2024-09-13 10:39:05.893', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Request Note Acceptance Summary'
      AND FunctionDescription = 'Request Note Acceptance Summary SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:39:05.893'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:39:05.893'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for GRN Detail
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'GRN Detail', 'GRN Detail SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:39:05.893', 'admin', '2024-09-13 10:39:05.893', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'GRN Detail'
      AND FunctionDescription = 'GRN Detail SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:39:05.893'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:39:05.893'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for PRN Detail
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'PRN Detail', 'PRN Detail SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:39:05.893', 'admin', '2024-09-13 10:39:05.893', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'PRN Detail'
      AND FunctionDescription = 'PRN Detail SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:39:05.893'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:39:05.893'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Stock Adjustment
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Stock Adjustmennt', 'Stock Adjustmennt SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:39:05.893', 'admin', '2024-09-13 10:39:05.893', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Stock Adjustmennt'
      AND FunctionDescription = 'Stock Adjustmennt SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:39:05.893'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:39:05.893'
      AND DataTransfer = 1
      AND FormId = 999
);
-- Insert if not exists for Purchase Order Detail
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Purchase Order Detail', 'Purchase Order Detail SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:40:44.210', 'admin', '2024-09-13 10:40:44.210', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Purchase Order Detail'
      AND FunctionDescription = 'Purchase Order Detail SSRS Report '
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:40:44.210'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:40:44.210'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Production Note Summary
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Production Note Summary', 'Production Note Summary SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:40:44.210', 'admin', '2024-09-13 10:40:44.210', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Production Note Summary'
      AND FunctionDescription = 'Production Note Summary SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:40:44.210'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:40:44.210'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Production Note Detail
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Production Note Detail', 'Production Note Detail SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:40:44.210', 'admin', '2024-09-13 10:40:44.210', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Production Note Detail'
      AND FunctionDescription = 'Production Note Detail SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:40:44.210'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:40:44.210'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Supplier Wise Stock Balance
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Supplier Wise Stock Balance', 'Supplier Wise Stock Balance SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:40:44.210', 'admin', '2024-09-13 10:40:44.210', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Supplier Wise Stock Balance'
      AND FunctionDescription = 'Supplier Wise Stock Balance SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:40:44.210'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:40:44.210'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Department Wise Stock Balance
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Department Wise Stock Balance', 'Department Wise Stock Balance SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:40:44.210', 'admin', '2024-09-13 10:40:44.210', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Department Wise Stock Balance'
      AND FunctionDescription = 'Department Wise Stock Balance SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:40:44.210'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:40:44.210'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Category Wise Stock Balance
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Category Wise Stock Balance', 'Category Wise Stock Balance SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:40:44.210', 'admin', '2024-09-13 10:40:44.210', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Category Wise Stock Balance'
      AND FunctionDescription = 'Category Wise Stock Balance SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:40:44.210'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:40:44.210'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Sub-Category Wise Stock Balance
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Sub-Category Wise Stock Balance', 'Sub-Category Wise Stock Balance SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:40:44.210', 'admin', '2024-09-13 10:40:44.210', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Sub-Category Wise Stock Balance'
      AND FunctionDescription = 'Sub-Category Wise Stock Balance SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:40:44.210'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:40:44.210'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Department Wise Sales
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Department Wise Sales', 'Department Wise Sales SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:40:44.210', 'admin', '2024-09-13 10:40:44.210', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Department Wise Sales'
      AND FunctionDescription = 'Department Wise Sales SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:40:44.210'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:40:44.210'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Category Wise Sales
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Category Wise Sales', 'Category Wise Sales SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:40:44.210', 'admin', '2024-09-13 10:40:44.210', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Category Wise Sales'
      AND FunctionDescription = 'Category Wise Sales SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:40:44.210'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:40:44.210'
      AND DataTransfer = 1
      AND FormId = 999
);
-- Insert if not exists for Customer Wise Sales
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Customer Wise Sales', 'Customer Wise Sales SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:42:20.553', 'admin', '2024-09-13 10:42:20.553', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Customer Wise Sales'
      AND FunctionDescription = 'Customer Wise Sales SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:42:20.553'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:42:20.553'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Supplier Wise Sales
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Supplier Wise Sales', 'Supplier Wise Sales SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:42:20.553', 'admin', '2024-09-13 10:42:20.553', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Supplier Wise Sales'
      AND FunctionDescription = 'Supplier Wise Sales SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:42:20.553'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:42:20.553'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Product Wise Sales
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Product Wise Sales', 'Product Wise Sales SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:42:20.553', 'admin', '2024-09-13 10:42:20.553', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Product Wise Sales'
      AND FunctionDescription = 'Product Wise Sales SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:42:20.553'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:42:20.553'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Location Wise Sales
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Location Wise Sales', 'Location Wise Sales SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:42:20.553', 'admin', '2024-09-13 10:42:20.553', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Location Wise Sales'
      AND FunctionDescription = 'Location Wise Sales SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:42:20.553'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:42:20.553'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Location Wise Stock
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Location Wise Stock', 'Location Wise Stock SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:42:20.553', 'admin', '2024-09-13 10:42:20.553', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Location Wise Stock'
      AND FunctionDescription = 'Location Wise Stock SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:42:20.553'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:42:20.553'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Menu Item Price List
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Menu Item Price List', 'Menu Item Price List SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:42:20.553', 'admin', '2024-09-13 10:42:20.553', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Menu Item Price List'
      AND FunctionDescription = 'Menu Item Price List SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:42:20.553'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:42:20.553'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Menu Item Recipe
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Menu Item Recipe', 'Menu Item Recipe SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:42:20.553', 'admin', '2024-09-13 10:42:20.553', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Menu Item Recipe'
      AND FunctionDescription = 'Menu Item Recipe SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:42:20.553'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:42:20.553'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Item Reorder Level
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Item Reorder Level', 'Item Reorder Level SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:42:20.553', 'admin', '2024-09-13 10:42:20.553', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Item Reorder Level'
      AND FunctionDescription = 'Item Reorder Level SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:42:20.553'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:42:20.553'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Stock Valuation Report
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Stock Valuation Report', 'Stock Valuation SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:42:20.553', 'admin', '2024-09-13 10:42:20.553', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Stock Valuation Report'
      AND FunctionDescription = 'Stock Valuation SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:42:20.553'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:42:20.553'
      AND DataTransfer = 1
      AND FormId = 999
);
-- Insert if not exists for Cashier Wise Sales
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Cashier Wise Sales', 'Cashier Wise Sales SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:43:43.630', 'admin', '2024-09-13 10:43:43.630', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Cashier Wise Sales'
      AND FunctionDescription = 'Cashier Wise Sales SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:43:43.630'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:43:43.630'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Wastage Note Report
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Wastage Note Report', 'Wastage Note SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:43:43.630', 'admin', '2024-09-13 10:43:43.630', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Wastage Note Report'
      AND FunctionDescription = 'Wastage Note SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:43:43.630'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:43:43.630'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Void Item Detail Report
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Void Item Detail Report', 'Void Item Detail SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:43:43.630', 'admin', '2024-09-13 10:43:43.630', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Void Item Detail Report'
      AND FunctionDescription = 'Void Item Detail SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:43:43.630'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:43:43.630'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Invoice Cancellation Report
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Invoice Cancellation Report', 'Invoice Cancellation SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:43:43.630', 'admin', '2024-09-13 10:43:43.630', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Invoice Cancellation Report'
      AND FunctionDescription = 'Invoice Cancellation SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:43:43.630'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:43:43.630'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Customer Wise Favorite Product List
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Customer Wise Favorite Product List', 'Customer Wise Favorite Product List SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:43:43.630', 'admin', '2024-09-13 10:43:43.630', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Customer Wise Favorite Product List'
      AND FunctionDescription = 'Customer Wise Favorite Product List SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:43:43.630'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:43:43.630'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Customer Wise Invoice Details
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Customer Wise Invoice Details', 'Customer Wise Invoice Details SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:43:43.630', 'admin', '2024-09-13 10:43:43.630', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Customer Wise Invoice Details'
      AND FunctionDescription = 'Customer Wise Invoice Details SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:43:43.630'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:43:43.630'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Fast Slow Movement
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Fast Slow Movement', 'Fast Slow Movement SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:43:43.630', 'admin', '2024-09-13 10:43:43.630', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Fast Slow Movement'
      AND FunctionDescription = 'Fast Slow Movement SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:43:43.630'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:43:43.630'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Catering Mode Wise Sales
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Catering Mode Wise Sales', 'Catering Mode Wise Sales SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:43:43.630', 'admin', '2024-09-13 10:43:43.630', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Catering Mode Wise Sales'
      AND FunctionDescription = 'Catering Mode Wise Sales SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:43:43.630'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:43:43.630'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Free Item Issue Details
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Free Item Issue Details', 'Free Item Issue Details SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:43:43.630', 'admin', '2024-09-13 10:43:43.630', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Free Item Issue Details'
      AND FunctionDescription = 'Free Item Issue Details SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:43:43.630'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:43:43.630'
      AND DataTransfer = 1
      AND FormId = 999
);
-- Insert if not exists for Steward Wise Sales
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Steward Wise Sales', 'Steward Wise Sales SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:45:13.530', 'admin', '2024-09-13 10:45:13.530', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Steward Wise Sales'
      AND FunctionDescription = 'Steward Wise Sales SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:45:13.530'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:45:13.530'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Stock Balance
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Stock Balance', 'Stock Balance SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:45:13.530', 'admin', '2024-09-13 10:45:13.530', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Stock Balance'
      AND FunctionDescription = 'Stock Balance SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:45:13.530'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:45:13.530'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Suspended Bill Details
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Suspended Bill Details', 'Suspended Bill Details SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:45:13.530', 'admin', '2024-09-13 10:45:13.530', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Suspended Bill Details'
      AND FunctionDescription = 'Suspended Bill Details SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:45:13.530'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:45:13.530'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Value Based Fast Slow Movement
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Value Based Fast Slow Movement', 'Value Based Fast Slow Movement SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:45:13.530', 'admin', '2024-09-13 10:45:13.530', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Value Based Fast Slow Movement'
      AND FunctionDescription = 'Value Based Fast Slow Movement SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:45:13.530'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:45:13.530'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Basket/ Bill Count
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Basket/ Bill Count', 'Basket/ Bill Count SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:45:13.530', 'admin', '2024-09-13 10:45:13.530', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Basket/ Bill Count'
      AND FunctionDescription = 'Basket/ Bill Count SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:45:13.530'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:45:13.530'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Department Wise Purchasing Report
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Department Wise Purchasing Report', 'Department Wise Purchasing SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:45:13.530', 'admin', '2024-09-13 10:45:13.530', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Department Wise Purchasing Report'
      AND FunctionDescription = 'Department Wise Purchasing SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:45:13.530'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:45:13.530'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Cashier wise efficiency
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Cashier wise efficiency', 'Cashier wise efficiency SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:45:13.530', 'admin', '2024-09-13 10:45:13.530', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Cashier wise efficiency'
      AND FunctionDescription = 'Cashier wise efficiency SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:45:13.530'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:45:13.530'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Bin Card
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Bin Card', 'Bin Card SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:45:13.530', 'admin', '2024-09-13 10:45:13.530', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Bin Card'
      AND FunctionDescription = 'Bin Card SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:45:13.530'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:45:13.530'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Refund Items Report
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Refund Items Report', 'Refund Items SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:45:13.530', 'admin', '2024-09-13 10:45:13.530', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Refund Items Report'
      AND FunctionDescription = 'Refund Items SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:45:13.530'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:45:13.530'
      AND DataTransfer = 1
      AND FormId = 999
);
-- Insert if not exists for Paymode Wise Sales Report
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Paymode Wise Sales Report', 'Paymode Wise Sales SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:46:44.283', 'admin', '2024-09-13 10:46:44.283', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Paymode Wise Sales Report'
      AND FunctionDescription = 'Paymode Wise Sales SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:46:44.283'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:46:44.283'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Supplier wise Products
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Supplier wise Products', 'Supplier wise Products SSRS Report', 1, 1, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:46:44.283', 'admin', '2024-09-13 10:46:44.283', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Supplier wise Products'
      AND FunctionDescription = 'Supplier wise Products SSRS Report'
      AND [Order] = 1
      AND TypeID = 1
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:46:44.283'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:46:44.283'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Bill Listing Cancelled Bill Report
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Bill Listing Cancelled Bill Report', 'Bill Listing Cancelled Bill SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:46:44.283', 'admin', '2024-09-13 10:46:44.283', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Bill Listing Cancelled Bill Report'
      AND FunctionDescription = 'Bill Listing Cancelled Bill SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:46:44.283'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:46:44.283'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Bill Listing Summary
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Bill Listing Summary', 'Bill Listing Summary SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:46:44.283', 'admin', '2024-09-13 10:46:44.283', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Bill Listing Summary'
      AND FunctionDescription = 'Bill Listing Summary SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:46:44.283'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:46:44.283'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Bill Listing Detail
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Bill Listing Detail', 'Bill Listing Detail SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:46:44.283', 'admin', '2024-09-13 10:46:44.283', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Bill Listing Detail'
      AND FunctionDescription = 'Bill Listing Detail SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:46:44.283'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:46:44.283'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Item Wise Sales Report
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Item Wise Sales Report', 'Item Wise Sales SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:46:44.283', 'admin', '2024-09-13 10:46:44.283', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Item Wise Sales SSRS Report'
      AND FunctionDescription = 'Item Wise Sales Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:46:44.283'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:46:44.283'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Item Wise Sales Summary Report
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Item Wise Sales Summary Report', 'Item Wise Sales Summary SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:46:44.283', 'admin', '2024-09-13 10:46:44.283', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Item Wise Sales Summary Report'
      AND FunctionDescription = 'Item Wise Sales Summary SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:46:44.283'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:46:44.283'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Staff Usage Note Detail
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Staff Usage Note Detail', 'Staff Usage Note Detail SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:46:44.283', 'admin', '2024-09-13 10:46:44.283', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Staff Usage Note Detail'
      AND FunctionDescription = 'Staff Usage Note Detail SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:46:44.283'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:46:44.283'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Staff Usage Note Summary
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Staff Usage Note Summary', 'Staff Usage Note Summary SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:46:44.283', 'admin', '2024-09-13 10:46:44.283', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Staff Usage Note Summary'
      AND FunctionDescription = 'Staff Usage Note Summary SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:46:44.283'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:46:44.283'
      AND DataTransfer = 1
      AND FormId = 999
);
-- Insert if not exists for Hourly Sales Report (Location Wise) - With Items
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Hourly Sales Report (Location Wise) - With Items', 'Hourly Sales Report (Location Wise) - With Items SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:48:02.203', 'admin', '2024-09-13 10:48:02.203', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Hourly Sales Report (Location Wise) - With Items'
      AND FunctionDescription = 'Hourly Sales Report (Location Wise) - With Items SSRS Report '
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:48:02.203'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:48:02.203'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Hourly Sales Report (Location Wise) - Without Items
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Hourly Sales Report (Location Wise) - Without Items', 'Hourly Sales Report (Location Wise) - Without Items SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:48:02.203', 'admin', '2024-09-13 10:48:02.203', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Hourly Sales Report (Location Wise) - Without Items'
      AND FunctionDescription = 'Hourly Sales Report (Location Wise) - Without Items SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:48:02.203'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:48:02.203'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Cancel KOT/BOT Listing Report
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Cancel KOT/BOT Listing Report', 'Cancel KOT/BOT Listing SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:48:02.203', 'admin', '2024-09-13 10:48:02.203', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Cancel KOT/BOT Listing Report'
      AND FunctionDescription = 'Cancel KOT/BOT Listing SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:48:02.203'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:48:02.203'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Category Wise Gross Profit Report
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Category Wise Gross Profit Report', 'Category Wise Gross Profit SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:48:02.203', 'admin', '2024-09-13 10:48:02.203', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Category Wise Gross Profit Report'
      AND FunctionDescription = 'Category Wise Gross Profit SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:48:02.203'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:48:02.203'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Item Re-Order Level Report
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Item Re-Order Level Report', 'Item Re-Order Level SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:48:02.203', 'admin', '2024-09-13 10:48:02.203', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Item Re-Order Level Report'
      AND FunctionDescription = 'Item Re-Order Level SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:48:02.203'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:48:02.203'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Department Wise Stock Variance
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Department Wise Stock Variance', 'Department Wise Stock Variance SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:48:02.203', 'admin', '2024-09-13 10:48:02.203', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Department Wise Stock Variance'
      AND FunctionDescription = 'Department Wise Stock Variance SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:48:02.203'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:48:02.203'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Item Target Wise Sales
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Item Target Wise Sales', 'Item Target Wise Sales SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:48:02.203', 'admin', '2024-09-13 10:48:02.203', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Item Target Wise Sales'
      AND FunctionDescription = 'Item Target Wise Sales SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:48:02.203'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:48:02.203'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Item Price List Report
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Item Price List Report', 'Item Price List SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:48:02.203', 'admin', '2024-09-13 10:48:02.203', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Item Price List Report'
      AND FunctionDescription = 'Item Price List SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:48:02.203'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:48:02.203'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Department Wise Gross Profit Report
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Department Wise Gross Grofit Report', 'Department Wise Gross Grofit SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:48:02.203', 'admin', '2024-09-13 10:48:02.203', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Department Wise Gross Grofit Report'
      AND FunctionDescription = 'Department Wise Gross Grofit SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:48:02.203'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:48:02.203'
      AND DataTransfer = 1
      AND FormId = 999
);
-- Insert if not exists for Menu Item Stock Detail Report
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Menu Item Stock Detail Report', 'Menu Item Stock Detail SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:49:15.590', 'admin', '2024-09-13 10:49:15.590', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Menu Item Stock Detail Report'
      AND FunctionDescription = 'Menu Item Stock Detail SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:49:15.590'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:49:15.590'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Sales Listing Report
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Sales Listing Report', 'Sales Listing SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:49:15.590', 'admin', '2024-09-13 10:49:15.590', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Sales Listing Report'
      AND FunctionDescription = 'Sales Listing SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:49:15.590'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:49:15.590'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Invoice Reprint Report
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Invoice Reprint Report', 'Invoice Reprint SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:49:15.590', 'admin', '2024-09-13 10:49:15.590', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Invoice Reprint Report'
      AND FunctionDescription = 'Invoice Reprint SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:49:15.590'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:49:15.590'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Order Sales Detail Report
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Order Sales Detail Report', 'Order Sales Detail SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:49:15.590', 'admin', '2024-09-13 10:49:15.590', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Order Sales Detail Report'
      AND FunctionDescription = 'Order Sales Detail SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:49:15.590'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:49:15.590'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Order Sales Summary Report
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Order Sales Summary Report', 'Order Sales Summary SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:49:15.590', 'admin', '2024-09-13 10:49:15.590', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Order Sales Summary Report'
      AND FunctionDescription = 'Order Sales Summary SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:49:15.590'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:49:15.590'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Price Change Item Report
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Price Change Item Report', 'Price Change Item SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:49:15.590', 'admin', '2024-09-13 10:49:15.590', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Price Change Item Report'
      AND FunctionDescription = 'Price Change Item SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:49:15.590'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:49:15.590'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Receipt Summary Report
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Receipt Summary Report', 'Receipt Summary SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:49:15.590', 'admin', '2024-09-13 10:49:15.590', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Receipt Summary Report'
      AND FunctionDescription = 'Receipt Summary SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:49:15.590'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:49:15.590'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for User Permissions Report
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'User Permissions Report', 'User Permissions SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:49:15.590', 'admin', '2024-09-13 10:49:15.590', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'User Permissions Report'
      AND FunctionDescription = 'User Permissions SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:49:15.590'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:49:15.590'
      AND DataTransfer = 1
      AND FormId = 999
);

-- Insert if not exists for Withdrawal Summary Report
INSERT INTO SysUserFunctions (FunctionName, FunctionDescription, [Order], TypeID, IsDelete, IsValue, GroupOfCompanyID, CompanyID, LocationId, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, FormId)
SELECT 'Withdrawal Summary Report', 'Withdrawal Summary SSRS Report', 1, 2, 0, 1, 1, 1, 1, 'admin', '2024-09-13 10:49:15.590', 'admin', '2024-09-13 10:49:15.590', 1, 999
WHERE NOT EXISTS (
    SELECT 1
    FROM SysUserFunctions
    WHERE FunctionName = 'Withdrawal Summary Report'
      AND FunctionDescription = 'Withdrawal Summary SSRS Report'
      AND [Order] = 1
      AND TypeID = 2
      AND IsDelete = 0
      AND IsValue = 1
      AND GroupOfCompanyID = 1
      AND CompanyID = 1
      AND LocationId = 1
      AND CreatedUser = 'admin'
      AND CreatedDate = '2024-09-13 10:49:15.590'
      AND ModifiedUser = 'admin'
      AND ModifiedDate = '2024-09-13 10:49:15.590'
      AND DataTransfer = 1
      AND FormId = 999
);




";
ExecuteInsert(Stringsqlconnection);


            ;



            #endregion



        }
        public void ExecuteInsert(string con)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(con))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(Insertquery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {

            }

        }
    }
}
