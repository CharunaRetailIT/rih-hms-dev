using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Common
{
    public class StrutureChangesSP
    {
        string tableName = "";
        string query = "";
        string spName = "";
        string ViewName = "";
        string ColumnName = "";
        public bool status = true;
        string Stringsqlconnection = "";
        string CheckspName = "";

        private void CheckSP(string sp)
        {
            CheckspName = string.Format(@"SELECT 1 FROM sys.procedures  WHERE Name ='{0}'
                                        BEGIN
                                            DROP PROCEDURE {1}
                                        END", sp, sp);
            ExecuteSPCheckQuery(Stringsqlconnection);

        }

        private string CheckAlterTableAddColumn(string TableName, string ColumnName, string qry)
        {
            var sqlQuery = string.Format(@"IF (NOT EXISTS ( SELECT column_name, data_type, character_maximum_length    
                                                            FROM INFORMATION_SCHEMA.COLUMNS  
                                                            WHERE table_name = '{0}' AND column_name = '{1}')) 
                                              BEGIN
                                                 {2}                               
                                              END", TableName, ColumnName, query);
            return sqlQuery;
        }

        public void RunSp(string connectionString)
        {
            Stringsqlconnection = connectionString;
            #region Stored Procedures

            #region spRegisterLoyaltyCustomer
            spName = "spRegisterLoyaltyCustomer";
            query = @"CREATE PROCEDURE [dbo].[spRegisterLoyaltyCustomer]
    @CardNo			NVARCHAR(MAX)  = '',
    @CardType		BIGINT  = 1,
    @CashierID		BIGINT  = 1,
    @LocationID		INT  = 1,    
    @Name			VARCHAR(60) = '',
    @Code			VARCHAR(20) = '',
    
    @NicNo			VARCHAR(20) = '',
    @DOB			DATETIME='9999-12-31',
    @Address		VARCHAR(50) = '',
    @Address2		VARCHAR(50) = '',    
    @Address3		VARCHAR(50) = '',
    @MobileNumber	VARCHAR(20) = '',
    @Email			VARCHAR(200) = '',    
    @Gender			INT = 1,
    @Organization	VARCHAR(50) = '',
    @Occupation		VARCHAR(50) = '',
    @LastName		VARCHAR(200) = '',
    @Country		VARCHAR(50) = ''  
           
AS 
    DECLARE @GroupOfCompanyID INT ,
        @Title INT ,
        @CardMasterID INT,@CompanyID int,
        @CustomerID int
	 
    SET NOCOUNT ON

    BEGIN TRY
        BEGIN TRANSACTION 

        IF NOT EXISTS ( SELECT  CardNo
                        FROM    LoyaltyCustomers
                        WHERE   CardNo = @CardNo
                                --OR NicNo = @NicNo 
                                ) 
            BEGIN


                IF LEFT(@CardNo, 1) = '9'
                    BEGIN

                        SET @CardType = 3
                        SELECT TOP 1
                                @CardMasterID = CardMasterID
                        FROM    CardMasters
                        WHERE   CardType = @CardType
                                AND Discount = 0
                        ORDER BY PointValue ASC

                    END
                ELSE 
                    BEGIN

                        SET @CardType = 2
                        SELECT TOP 1
                                @CardMasterID = CardMasterID
                        FROM    CardMasters
                        WHERE   CardType = @CardType
                                AND Discount = 0
                        ORDER BY PointValue ASC

                    END
	
                SELECT  @GroupOfCompanyID = SysGroupOfCompanyId
                FROM    SysGroupOfCompanies
                
                SELECT  @CompanyID = CompanyID
                FROM    SysLocations where SysLocationID=@LocationID
                
                SELECT  @CustomerID = CustomerID
                FROM    Customers where CustomerCode=@Code and CompanyID=@CompanyID
	

                SELECT TOP 1 @Title = LookupKey
                FROM    ReferenceTypes
                WHERE   LookupType = 1
                ORDER BY LookupValue ASC
	

                INSERT  INTO LoyaltyCustomers
                        ( [CardNo] ,[CustomerId] ,[CardMasterID] ,[CardIssued] ,[IssuedOn] ,[ExpiryDate] ,
                          [RenewedOn] ,[LedgerId] ,[LedgerId2] ,[CreditLimit] , [CreditPeriod] ,
                          [GroupOfCompanyID] , [CreatedDate] , [ModifiedDate] , [DataTransfer] ,
                          [CPoints] , [EPoints] ,  [RPoints] , [IsReDimm] , [AcitiveDate] ,
                          [CashierID] ,LocationID ,LoyaltyType ,
                          NameOnCard,IsCardIssued,ExpiryPoints,ExpiryPoints1,IsSold,Discount,
                          LastUpdatedLocId,Status,CompanyId )
                VALUES  ( @CardNo , @CustomerID , @CardMasterID , 1 ,GETDATE() , DATEADD(mm, 12, GETDATE()) ,
                          GETDATE() ,0,0,0,0,
                          @GroupOfCompanyID ,GETDATE() ,GETDATE() ,0 ,
                          0 ,0,0,1,GETDATE() ,
                          @CashierID ,@LocationID ,@CardType ,
                          @Name,1,0,0,1,0,
                          @LocationID,1,@CompanyID )
            END
        COMMIT TRANSACTION;
        SELECT  '0' AS Result
    END TRY
  
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            BEGIN
                ROLLBACK TRANSACTION
                SELECT  ERROR_MESSAGE() AS Result

            END
        ELSE 
            BEGIN
                SELECT  ERROR_MESSAGE() AS Result
            END
    END CATCH  ";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion spRegisterLoyaltyCustomer

            #region DayEnd_DateRange
            spName = "DayEnd_DateRange";
            query = @"CREATE PROCEDURE [dbo].[DayEnd_DateRange]
@DateFrom DATE,
@DateTo DATE

AS
BEGIN

    DELETE  FROM InvSales
    WHERE   CAST(DocumentDate AS DATE) BETWEEN @DateFrom
                                       AND     @DateTo

 ------------------   Dileepa Fcurruncy Update -------------------------
    
    
    UPDATE td SET SellingCopperratePrice = Price , AmountCopperratePrice = Amount , NettCopperratePrice = Nett , RateCopperratePrice = Rate 
    FROM dbo.TransactionDets td
    INNER JOIN SysLocations l on td.LocationID = l.SysLocationID
    WHERE IsDayEnd = 0 AND td.IsCopperratePriceEnable=1 

    UPDATE td SET Price = (Price * CopperratePrice) , Amount = (Amount*CopperratePrice) ,Nett = (Nett * CopperratePrice) , Rate = (Rate*CopperratePrice) 
    FROM dbo.TransactionDets td
    INNER JOIN SysLocations l on td.LocationID = l.SysLocationID
    WHERE IsDayEnd = 0 AND td.IsCopperratePriceEnable = 1 
    
    UPDATE pd SET AmountCopperratePrice = Amount , BalanceCopperratePrice = Balance 
    FROM dbo.PaymentDets pd
    INNER JOIN SysLocations l on pd.LocationID = l.SysLocationID
    WHERE IsDayEnd = 0 AND pd.IsCopperratePriceEnable = 1 
    
    UPDATE pd SET Amount = (Amount * CopperratePrice) , Balance = (Balance*CopperratePrice)
    FROM dbo.PaymentDets pd
    INNER JOIN SysLocations l on pd.LocationID = l.SysLocationID
    WHERE IsDayEnd = 0 AND pd.IsCopperratePriceEnable = 1 
    
 -------------------------------------------------------------------------------------------------------------------------------

    INSERT  INTO dbo.InvSales ( SalesID, CompanyID, CompanyCode, CompanyName,
                                LocationID, LocationCode, LocationName,
                                CostCentreId, DocumentID, DocumentNo,
                                ReferenceNo, DocumentDate, TransactionTime,
                                CustomerType, CustomerID, CustomerCode,
                                CustomerName, SupplierID, SupplierCode,
                                SupplierName, SalesPersonID, SalesPersonCode,
                                SalesPersonName, GrossAmount,
                                DiscountPercentage, DiscountAmount, NetAmount,
                                SubTotalDiscountPercentage,
                                SubTotalDiscountAmount, CurrencyID,
                                CurrencyRate, DepartmentCode, DepartmentName,
                                CategoryCode, CategoryName, SubCategoryCode,
                                SubCategoryName, ProductID, ProductCode,
                                ProductName, BarCode, BatchNo, ExpiryDate, Qty,
                                UnitOfMeasureID, UnitOfMeasureName, PackSize,
                                SellingPrice, WholeSalePrice, CostPrice,
                                AverageCost, DocumentStatus, IsFreeIssue,
                                TerminalNo, IsDispatch, IsUpLoad, IsDelete,
                                GroupOfCompanyID, CreatedUser, CreatedDate,
                                ModifiedUser, ModifiedDate, DataTransfer,
                                UnitNo, Zno, DepartmentID, CategoryID,
                                SubCategoryID, SerialNo, CorporatePrice )
                                
            SELECT  td.TransactionDetID, l.CompanyID, com.CompanyCode,
                    com.CompanyName, td.LocationID, l.LocationCode,
                    l.LocationName, l.SysLocationId, td.DocumentID, td.Receipt,
                    td.RefCode, TD.RecDate, td.StartTime, td.CustomerType,
                    td.CustomerID, '', '', ISNULL(0, ''), '', '',
                    ISNULL(td.SalesmanID, 0), '', '', td.Amount, 0 AS disper,
                    ( td.IDiscount1 + td.IDiscount2 + td.IDiscount3
                      + td.IDiscount4 + td.IDiscount5 ) AS disamt, td.Nett, 0,
                    0, 0 AS curid, 0 AS curRate, '', '', '', '', '',
                    '', pm.ProductId, ISNULL(pm.ProductCode, ''),
                    ISNULL(pm.ProductName, ''), 0 AS barcode, td.BatchNo,
                    ISNULL(td.ExpiryDate,td.RecDate), td.Qty, td.UnitOfMeasureID,
                    td.UnitOfMeasureName, pm.PackSize, td.Price,
                    0, td.Cost, td.AvgCost,
                    td.Status AS status, 0 AS freeissue, td.UnitNo,
                    0 AS dispatch, 0 AS isupload, 0 isdelete,
                    td.GroupOfCompanyID, td.Cashier, -- em.JournalName ,
                    td.StartTime, td.Cashier, --em.JournalName ,
                    td.StartTime, 0 AS datatransfer, td.UnitNo, td.ZNo,
                    pm.DepartmentID, pm.CategoryID, pm.SubCategoryID,
                    td.ItemSerial,td.CopperratePrice
            FROM    TransactionDets td ( NOLOCK )
                    INNER JOIN Products pm ( NOLOCK ) ON td.ProductID = pm.ProductId
                    INNER JOIN SysLocations l ( NOLOCK ) ON l.SysLocationID = td.LocationID
                    INNER JOIN dbo.SysCompanies com ( NOLOCK ) ON com.SysCompanyID = l.CompanyID
                    --LEFT OUTER JOIN InvSerial se ON td.ItemSerial = se.InvSerialID AND td.ProductCode = se.ProductCode
            WHERE   ( DocumentID = 1
                      OR DocumentID = 3
                    )
                    AND Status = 1
                    AND TransStatus = 1
                    AND SaleTypeID = 1
                    AND BillTypeID = 1
                    
                    AND CAST(TD.RecDate AS DATE) BETWEEN @DateFrom AND @DateTo
            
    INSERT  INTO dbo.InvSales ( 
    
								SalesID, 
								CompanyID, 
								CompanyCode, 
								CompanyName,    
								LocationID, 
								LocationCode, 
								LocationName,
								CostCentreId, 
								DocumentID, 
								DocumentNo,
								ReferenceNo, 
								DocumentDate, 
								TransactionTime,
                                CustomerType, 
                                CustomerID, 
                                CustomerCode,
                                CustomerName,
                                SupplierID, 
                                SupplierCode,
                                SupplierName, 
                                SalesPersonID, 
                                SalesPersonCode,
                                SalesPersonName, 
                                GrossAmount,
                                DiscountPercentage, 
                                DiscountAmount, 
                                NetAmount,
                                SubTotalDiscountPercentage,
                                SubTotalDiscountAmount, 
                                CurrencyID,
                                CurrencyRate, 
                                DepartmentCode, 
                                DepartmentName,
                                CategoryCode, 
                                CategoryName, 
                                SubCategoryCode,
                                SubCategoryName, 
                                ProductID, 
                                ProductCode,
                                ProductName, 
                                BarCode, 
                                BatchNo, 
                                ExpiryDate, 
                                Qty,
                                UnitOfMeasureID, 
                                UnitOfMeasureName, 
                                PackSize,
                                SellingPrice, 
                                WholeSalePrice, 
                                CostPrice,
                                AverageCost, 
                                DocumentStatus, 
                                IsFreeIssue,
                                TerminalNo, 
                                IsDispatch, 
                                IsUpLoad, 
                                IsDelete,
                                GroupOfCompanyID, 
                                CreatedUser, 
                                CreatedDate,
                                ModifiedUser, 
                                ModifiedDate, 
                                DataTransfer,
                                UnitNo, 
                                Zno, 
                                DepartmentID, 
                                CategoryID,
                                SubCategoryID,                                 
                                IsBackOffice, 
                                SerialNo, 
                                CorporatePrice
                                 
                                )
            SELECT  
            
					td.TransactionDetID, 
					l.CompanyID, 
					com.CompanyCode,
                    com.CompanyName, 
                    td.LocationID, 
                    l.LocationCode,
                    l.LocationName, 
                    l.SysLocationId, 
                    td.DocumentID, 
                    td.Receipt,
                    td.RefCode, 
                    TD.RecDate, 
                    td.StartTime, 
                    td.CustomerType,
                    td.CustomerID, 
                    '', 
                    '', 
                    ISNULL('', ''),
                     '', 
                     '',
                    ISNULL(td.SalesmanID, 0), 
                    '', 
                    '',
                     -1 * ( td.Amount ),
                    0 AS disper,
                    -1 * ( td.IDiscount1 + td.IDiscount2 + td.IDiscount3 + td.IDiscount4 + td.IDiscount5 ),
                    -1 * ( td.Nett ),
                    -1 * ( td.SDIs ), 
                    -1 * ( td.SDiscount ), 
                    0 AS curid,
                    0 AS curRate,
                     '', 
                     '', 
                     '', 
                     '', 
                     '', 
                     '', 
                    
                    pm.ProductId, 
                    ISNULL(pm.ProductCode, ''),
                    ISNULL(pm.ProductName, ''), 
                    0 AS barcode, 
                    td.BatchNo,
                    ISNULL(td.ExpiryDate,td.RecDate), 
                    -1 * ( td.Qty ), 
                    td.UnitOfMeasureID,
                    td.UnitOfMeasureName, 
                    pm.PackSize, 
                    td.Price,
                    0, 
                    td.Cost, 
                    td.AvgCost, 
                    td.Status,
                    0 AS freeissue, 
                    td.UnitNo, 
                    0 AS dispatch, 
                    0 AS isupload,
                    0 isdelete, 
                    td.GroupOfCompanyID, 
                    td.cashier, --em.JournalName ,
                    GETDATE(), 
                    td.cashier, --em.JournalName ,
                    GETDATE(), 
                    0 AS datatransfer, 
                    td.UnitNo, 
                    td.ZNo,
                    pm.DepartmentID, 
                    pm.CategoryID, 
                    pm.SubCategoryID,
                    'FALSE',                            
                    td.ItemSerial,
                    td.CopperratePrice
                    
            FROM    TransactionDets td ( NOLOCK )
                    INNER JOIN Products pm ( NOLOCK ) ON td.ProductID = pm.ProductId
                    INNER JOIN SysLocations l ( NOLOCK ) ON l.SysLocationID = td.LocationID
                    INNER JOIN dbo.SysCompanies com ( NOLOCK ) ON com.SysCompanyID = l.CompanyID
                    --LEFT OUTER JOIN InvSerial se ON td.ItemSerial = se.InvSerialID AND td.ProductCode = se.ProductCode
            WHERE   ( DocumentID = 2
                      OR DocumentID = 4
                    )
                    AND Status = 1
                    AND TransStatus = 1
                    AND SaleTypeID = 1
                    AND BillTypeID = 1
                    AND CAST(TD.RecDate AS DATE) BETWEEN @DateFrom AND @DateTo
 
				-- UPDATE DEPARTMENT                            
    UPDATE  b
    SET     b.DepartmentCode = T.DepartmentCode,
            b.DepartmentName = T.DepartmentName
    FROM    dbo.InvSales b ( NOLOCK )
            INNER JOIN RstDepartments t ( NOLOCK ) ON b.DepartmentID = t.RstDepartmentID
    WHERE   CAST(b.DocumentDate AS DATE) BETWEEN @DateFrom
                                         AND     @DateTo
                        
                         -- UPDATE CATEGORY
    UPDATE  b
    SET     b.CategoryCode = T.RstCategoryCode, b.CategoryName = T.RstCategoryCode
    FROM    dbo.InvSales b ( NOLOCK )
            INNER JOIN RstCategories t ( NOLOCK ) ON b.CategoryID = t.RstCategoryID
    WHERE   CAST(b.DocumentDate AS DATE) BETWEEN @DateFrom
                                         AND     @DateTo    
                         -- UPDATE SUBCATEGORY
            
    UPDATE  b
    SET     b.SubCategoryCode = T.RstSubCategoryCode,
            b.SubCategoryName = T.RstSubCategoryName
    FROM    dbo.InvSales b ( NOLOCK )
            INNER JOIN RstSubCategories t ( NOLOCK ) ON b.SubCategoryID = t.RstSubCategoryID
    WHERE   CAST(b.DocumentDate AS DATE) BETWEEN @DateFrom
                                         AND     @DateTo    

    UPDATE  b
    SET     b.SupplierCode = T.SupplierCode, b.SupplierName = T.SupplierName
    FROM    dbo.InvSales b ( NOLOCK )
            INNER JOIN dbo.Suppliers t ( NOLOCK ) ON b.SupplierID = t.SupplierID
    WHERE   CAST(b.DocumentDate AS DATE) BETWEEN @DateFrom
                                         AND     @DateTo    
                         -- UPDATE Customer
    UPDATE  b
    SET     b.CustomerCode = T.CustomerCode, b.CustomerName = T.CustomerName
    FROM    dbo.InvSales b ( NOLOCK )
            INNER JOIN dbo.Customers t ( NOLOCK ) ON b.CustomerId = t.CustomerID
    WHERE   CAST(b.DocumentDate AS DATE) BETWEEN @DateFrom
                                         AND     @DateTo  
  
		INSERT  INTO dbo.InvSales ( SalesID, CompanyID, CompanyCode, CompanyName,
		                            LocationID, LocationCode, LocationName,
		                            CostCentreId, DocumentID, DocumentNo,
		                            ReferenceNo, DocumentDate, TransactionTime,
		                            CustomerType, CustomerID, CustomerCode,
		                            CustomerName, SupplierID, SupplierCode,
		                            SupplierName, SalesPersonID, SalesPersonCode,
		                            SalesPersonName, GrossAmount,
		                            DiscountPercentage, DiscountAmount, NetAmount,
		                            SubTotalDiscountPercentage,
		                            SubTotalDiscountAmount, CurrencyID,
		                            CurrencyRate, DepartmentCode, DepartmentName,
		                            CategoryCode, CategoryName, SubCategoryCode,
		                            SubCategoryName,  ProductID, ProductCode,
		                            ProductName, BarCode, BatchNo, ExpiryDate, Qty,
		                            UnitOfMeasureID, UnitOfMeasureName, PackSize,
		                            SellingPrice, WholeSalePrice, CostPrice,
		                            AverageCost, DocumentStatus, IsFreeIssue,
		                            TerminalNo, IsDispatch, IsUpLoad, IsDelete,
		                            GroupOfCompanyID, CreatedUser, CreatedDate,
		                            ModifiedUser, ModifiedDate, DataTransfer,
		                            UnitNo, Zno, DepartmentID, CategoryID,
		                            SubCategoryID, IsBackOffice, SerialNo, CorporatePrice )
		        SELECT  td.TransactionDetID, l.CompanyID, com.CompanyCode,
		                com.CompanyName, td.LocationID, l.LocationCode,
		                l.LocationName, l.SysLocationId, 6, td.Receipt,
		                td.RefCode, TD.RecDate, td.StartTime, td.CustomerType,
		                td.CustomerID, '', '', 0, '', '', ISNULL(td.SalesmanID, 0),
		                '', '', 0 , 0 AS disper,
		                 ( SDiscount )AS disamt,
		                ( -1 )* ( SDiscount )AS Nett, 0, 0, 0 AS curid, 0 AS curRate, '',
		                '', '', '', '', '', 0, '', 'Subtotal Discount',
		                0 AS barcode, td.BatchNo, ISNULL(td.ExpiryDate,td.RecDate), 0 Qty,
		                td.UnitOfMeasureID, td.UnitOfMeasureName, 0, 0, 0, 0, 0,
		                td.Status AS status, 0 AS freeissue, td.UnitNo,
		                0 AS dispatch, 0 AS isupload, 0 isdelete,
		                td.GroupOfCompanyID, td.Cashier, -- em.JournalName ,
		                td.StartTime, td.Cashier, --em.JournalName ,
		                td.StartTime, 0 AS datatransfer, td.UnitNo, td.ZNo, 0, 0,
		                0, 'FALSE', 0, 0
		        FROM    TransactionDets td ( NOLOCK )
		                INNER JOIN SysLocations l ( NOLOCK ) ON l.SysLocationID = td.LocationID
		                INNER JOIN dbo.SysCompanies com ( NOLOCK ) ON com.SysCompanyID = l.CompanyID
		        WHERE   DocumentID = 6
		                AND td.ProductID = 0
		                AND Status = 1
		                AND TransStatus = 1
		                AND SaleTypeID = 1
		                AND BillTypeID = 1
		                
		                
		                AND CAST(TD.RecDate AS DATE) BETWEEN @DateFrom AND @DateTo
	                    
	 -------- Update Sale Stock Meater ---------------
	 
     DECLARE @LocationID INT ,
        @StockCode VARCHAR(100) ,
        @BatchNo VARCHAR(20) ,
        @Qty DECIMAL(18, 2) ,
        @TransactionDetsID INT ,
        @CurrentStock DECIMAL(18, 2) ,
        @CLocationID INT
      
     DECLARE db_cursorLocation CURSOR
     FOR
      
        SELECT  SyslocationID
        FROM    dbo.SysLocations
        
     OPEN db_cursorLocation   
     FETCH NEXT FROM db_cursorLocation INTO @CLocationID
     WHILE @@FETCH_STATUS = 0 
        BEGIN    
	  
            DECLARE db_cursor CURSOR
            FOR
                SELECT  td.TransactionDetID, td.LocationID, td.ProductCode,
                        td.BatchNo, SUM(CASE DocumentID
                                          WHEN 1 THEN -Qty
                                          WHEN 3 THEN -Qty
                                          WHEN 2 THEN Qty
                                          WHEN 4 THEN Qty
                                          ELSE 0
                                        END)
                FROM    TransactionDets AS td
                WHERE   LocationID = @CLocationID
                        AND [Status] = 1
                        AND TransStatus = 1
                        AND ( DocumentID IN ( 1, 2, 3, 4 ) )
                        AND IsDayEnd = 0
                        AND CAST(TD.RecDate AS DATE) BETWEEN @DateFrom
                                                     AND     @DateTo
                GROUP BY td.TransactionDetID, td.LocationID, td.ProductCode,
                        td.BatchNo
            OPEN db_cursor   
            FETCH NEXT FROM db_cursor INTO @TransactionDetsID, @LocationID, @StockCode, @BatchNo, @Qty 
            WHILE @@FETCH_STATUS = 0 
                BEGIN 
            
                    SET @CurrentStock = 0;
                    SELECT  @CurrentStock = ISNULL(Stock, 0)
                    FROM    ProductStockMasters
                    WHERE   StockCode = @StockCode
                            AND LocationID = @LocationID
                    UPDATE  ProductStockMasters
                    SET     Stock = ( @CurrentStock + @Qty )
                    WHERE   StockCode = @StockCode
                            AND LocationID = @LocationID
                    UPDATE  TransactionDets
                    SET     IsDayEnd = 1
                    WHERE   TransactionDetID = @TransactionDetsID
            
                    PRINT @CurrentStock
                    PRINT @StockCode
                    PRINT @CLocationID
            
                    FETCH NEXT FROM db_cursor INTO @TransactionDetsID,
                        @LocationID, @StockCode, @BatchNo, @Qty   
                END   

            CLOSE db_cursor   
            DEALLOCATE db_cursor 
        
            FETCH NEXT FROM db_cursorLocation INTO @CLocationID  
        END   

     CLOSE db_cursorLocation   
     DEALLOCATE db_cursorLocation 


UPDATE dbo.TransactionDets SET IsDayEnd = 1 WHERE CAST(RecDate AS Date) BETWEEN @DateFrom AND @DateTo  

END ";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion DayEnd_DateRange

            #region GenProductionNotes
            spName = "GenProductionNotes";
            query = @"CREATE PROCEDURE [dbo].[GenProductionNotes] 
				@ReceiptLocID int, @R_Zno int,
 @ReceiptNo nvarchar(40), @UnitNo int 
As
Begin

	select distinct StockLocationID  into #tmpStockLocs From TransactionDets 
	where LocationID  =@ReceiptLocID and Zno=@R_Zno and Receipt=@ReceiptNo
	and UnitNo = @UnitNo AND StockLocationID <>0
    print 'Stock locaitons selected'
    --SELECT * FROM #tmpStockLocs 
    
	declare LocCursor cursor for 
	select stockLocationid From #tmpStockLocs
	declare @tmpStockLoc int

	declare @PreFix nvarchar(40)
	declare @ProductionNoteNo nvarchar(40)
	declare @CodeLen int 
	declare @DocNo int
	declare @ProductNoteID int
	declare @ProductID bigint
	declare @Qty decimal(18,2)
	declare @ServingUnitID int
	declare @CNT int=0
	declare @MaterialQty decimal(18,2)
	declare @MaterialId bigint
	declare @MatQty decimal(18,2)
	declare @ProductionLocId int
	declare @ProdLocId int
	declare @ProductCost decimal(18,2)

	open locCursor

	fetch next from locCursor into @tmpStockLoc 
	while @@fetch_status=0
	begin
	
	if (Select isnull(COUNT(*),0) From [ProductionNoteHeaders] 
	               where ProductionLocId =@tmpStockLoc and R_Zno=@R_Zno 
	               and ReceiptNo =@ReceiptNo and UnitNo = @UnitNo) > 0
	    begin
	    print 'Production note exist for Rec:' + @ReceiptNO + ' StockLoc:' + convert(varchar(20),@tmpStockLoc) + ' UnitNo:' 
	             + convert(varchar(20),@UnitNo) + ' Zno:' + convert(varchar(20),@R_Zno)
	    end
	else
	    Begin	
		select @PreFix = PreFix, @CodeLen=CodeLength From AutoGenerateInfoes where documentID =7
		select @DocNo =  DocumentNo +1 From DocumentNumbers where documentID=7 and locationid = @ReceiptLocID 
		set @PreFix =  @PreFix -- + CONVERT(varchar(10), @ReceiptLocID )


		set @ProductionNoteNo = @PreFix + replicate('0',@CodeLen -len(@DocNo)) + convert(nvarchar(30),@DocNo)
	--	+ replicate('0',@codelen -len(@PreFix) - len(convert(nvarchar(30),@DocNo))) 
	--	+ convert(nvarchar(30),@DocNo)
		  
		  print @ProductionNoteNo
		  
		INSERT INTO [dbo].[ProductionNoteHeaders]
				   ([DocumentNo],[ProductionLocId],[Remark],[ProductId],[ProductCostPrice],[ProductSellingPrice],[ProductDiscounts]
				   ,[GroupOfCompanyID],[CompanyID],[LocationId],[CreatedUser],[CreatedDate],[ModifiedUser],[ModifiedDate]
				   ,[DataTransfer],[ProductQty],[IsTempPN],[IsFinished],[DocumentId],[ReceiptLocID],[R_Zno],[ReceiptNo],[UnitNo])

		select @ProductionNoteNo ,td.stocklocationid,'Sales Production',0 ,0,0,0,1,1,@tmpStockLoc ,td.Cashier ,GETDATE(),
		td.Cashier ,GETDATE(),0,0,0,1,7, td.stocklocationid, td.ZNo ,td.Receipt ,td.UnitNo 
		 from transactiondets TD
		inner join Receipes R on TD.ProductID = R.ProductID
	--	and ((TD.Qty=R.ProductQty and R.ProductQty>1)
	--		  Or
		--	  R.productQty=1 
		--	 )
		inner join products P on P.productID = R.ProductID 
		and isnull(P.isRowMaterial,0)=0 and td.stockLocationID=@tmpStockLoc 
		where td.LocationID   =@ReceiptLocID and td.Zno=@R_Zno and td.Receipt=@ReceiptNo
		and td.UnitNo = @UnitNo AND TD.DocumentID IN (1,3)

		group by td.stocklocationid,td.unitno,td.Cashier ,td.ZNo,td.receipt

	    
		if @@rowcount>0 --if production header inserted 
		   Begin
		   
		   print 'Header inserted'
		   set @ProductNoteID =@@IDENTITY --get last insert identity value of producitonnoteid
		   update DocumentNumbers set DocumentNo =  DocumentNo +1 From DocumentNumbers 
		   where documentID=7 and locationid = @ReceiptLocID 
		  
			DECLARE cur CURSOR
			FOR
				select  td.ProductID,sum(td.Qty),td.ServingUnitId from TransactionDets td 
				inner join products P on P.productID = td.ProductID
				where td.stockLocationID=@tmpStockLoc
				and td.LocationID   =@ReceiptLocID
				and td.Zno=@R_Zno and td.Receipt=@ReceiptNo
				and td.UnitNo = @UnitNo 
				AND (P.AutoProduction = 1)
				group by  td.ProductID,td.ServingUnitId

			OPEN cur
			FETCH NEXT FROM cur INTO @ProductID,@Qty,@ServingUnitID

			WHILE @@fetch_status = 0 
				BEGIN
					set @CNT=0
					set @CNT=(SELECT count(*) FROM Receipes WHERE (ProductId = @ProductID) and ProductServingUnitId=@ServingUnitID AND (ProductQty = @Qty) and LocationId=@ReceiptLocID)
					
					if @CNT > 0
					begin
						INSERT INTO [ProductionNoteDetails]
						([ProductionNoteHeaderId],[MaterialId],[MaterialName],[MaterialQty],[SellingPrice],[CostPrice]
						,[AvgCost],[ProductId],[ProductQty],[ProductName],[ProductCostPrice],[ProductSellingPrice]
						,[ServingUnitId])
						select @ProductNoteID  ,R.MaterialID,PRM.ProductName, 

						--CONVERT ACCORDING TO PURCHASE QTY  
						---divide by receipe unit qty to get purchasing unit wise actual qty
						--(case when R.ProductQty>1 then R.Quantity else td.Qty* R.Quantity end) / (case when isnull( uc.SubUnitValue ,1)=0 then 1 else isnull( uc.SubUnitValue,1) end)  ,
						(R.Quantity/uc.SubUnitValue),
						PSM.SellingPrice,R.CostPrice*TD.Qty,R.CostPrice*TD.Qty,TD.ProductID,
						TD.Qty , --Production Qty Only put in first row
						P.ProductName,PSMFinProd.CostPrice,PSMFinProd.SellingPrice, td.ProductServingUnitId

						from (Select TD1.ProductID,
						StockLocationID,TD1.locationID,Zno,Receipt,UnitNo,PSU.ProductServingUnitID,
						sum(Qty) Qty From transactiondets TD1
						inner join ProductServingUnits PSU on psu.ServingUnit = TD1.ServingUnit and psu.productid=TD1.ProductID 
						WHERE  TD1.DocumentID IN (1,3)
						group by TD1.ProductID,StockLocationID,TD1.locationID,Zno,Receipt,UnitNo,PSU.ProductServingUnitID ) TD
						inner join Receipes R on TD.ProductID = R.ProductID and TD.ProductServingUnitId=R.ProductServingUnitId
						and (TD.Qty=R.ProductQty)  and R.locationID=@ReceiptLocID
						--	  Or R.productQty=1 )
						inner join products PRM on PRM.productID = R.MaterialID --for raw materials
						inner join Products P on P.ProductID = R.ProductID --for finished product
						inner join productStockMasters PSM on PSM.productID = R.MaterialID and PSM.locationID= @tmpStockLoc -- for raw material
						inner join productStockMasters PSMFinProd on PSMFinProd.ProductID =TD.ProductID and PSMFinProd.locationID =@tmpStockLoc --for finishproduct
						Left join UnitConversions UC 
						on PRM.PurchasingUnit = uc.UnitOfMeasureId and PRM.WeightPerUnit = uc.UnitConversionId --Join with unit convertion   
						where td.stockLocationID=@tmpStockLoc
						and td.LocationID   =@ReceiptLocID and td.Zno=@R_Zno and td.Receipt=@ReceiptNo
						and td.UnitNo = @UnitNo AND (P.AutoProduction = 1) and td.productID=@ProductID and td.qty=@Qty
						and td.ProductServingUnitId=@ServingUnitID
			 
					end
				
					else
					begin
						set @CNT=(SELECT count(*) FROM Receipes WHERE (ProductId = @ProductID) AND (ProductQty = 1))
						if @CNT >0
						begin
							INSERT INTO [ProductionNoteDetails]
							([ProductionNoteHeaderId],[MaterialId],[MaterialName],[MaterialQty],[SellingPrice],[CostPrice]
							,[AvgCost],[ProductId],[ProductQty],[ProductName],[ProductCostPrice],[ProductSellingPrice]
							,[ServingUnitId])
							select @ProductNoteID  ,R.MaterialID,PRM.ProductName, 

							--CONVERT ACCORDING TO PURCHASE QTY  
							---divide by receipe unit qty to get purchasing unit wise actual qty
							--(case when R.ProductQty>1 then R.Quantity else td.Qty* R.Quantity end) / (case when isnull( uc.SubUnitValue ,1)=0 then 1 else isnull( uc.SubUnitValue,1) end)  ,
							(R.Quantity/uc.SubUnitValue)*TD.Qty,
							PSM.SellingPrice,R.CostPrice*TD.Qty,R.CostPrice*TD.Qty,TD.ProductID,
							TD.Qty , --Production Qty Only put in first row
							P.ProductName,PSMFinProd.CostPrice,PSMFinProd.SellingPrice, td.ProductServingUnitId

							from (Select TD1.ProductID,
							StockLocationID,TD1.locationID,Zno,Receipt,UnitNo,PSU.ProductServingUnitID,
							sum(Qty) Qty From transactiondets TD1
							inner join ProductServingUnits PSU on psu.ServingUnit = TD1.ServingUnit and psu.productid=TD1.ProductID 
							WHERE  TD1.DocumentID IN (1,3)
							group by TD1.ProductID,StockLocationID,TD1.locationID,Zno,Receipt,UnitNo,PSU.ProductServingUnitID ) TD
							inner join Receipes R on TD.ProductID = R.ProductID and TD.ProductServingUnitId=R.ProductServingUnitId
							and (R.ProductQty=1)  and R.locationID=@ReceiptLocID
							--	  Or R.productQty=1 )
							inner join products PRM on PRM.productID = R.MaterialID --for raw materials
							inner join Products P on P.ProductID = R.ProductID --for finished product
							inner join productStockMasters PSM on PSM.productID = R.MaterialID and PSM.locationID= @tmpStockLoc -- for raw material
							inner join productStockMasters PSMFinProd on PSMFinProd.ProductID =TD.ProductID and PSMFinProd.locationID =@tmpStockLoc --for finishproduct
							Left join UnitConversions UC 
							on PRM.PurchasingUnit = uc.UnitOfMeasureId and PRM.WeightPerUnit = uc.UnitConversionId --Join with unit convertion   
							where td.stockLocationID=@tmpStockLoc
							and td.LocationID   =@ReceiptLocID and td.Zno=@R_Zno and td.Receipt=@ReceiptNo
							and td.UnitNo = @UnitNo AND (P.AutoProduction = 1) and td.productID=@ProductID and td.qty=@Qty
							and td.ProductServingUnitId=@ServingUnitID
						end --if @CNT >0
						else
						Begin
						-----------------------------------------------------------
						
						set @CNT=(SELECT count(*) FROM Receipes WHERE (ProductId = @ProductID) and ProductServingUnitId=@ServingUnitID and LocationId=@ReceiptLocID)
						if @CNT >0
						begin
							INSERT INTO [ProductionNoteDetails]
							([ProductionNoteHeaderId],[MaterialId],[MaterialName],[MaterialQty],[SellingPrice],[CostPrice]
							,[AvgCost],[ProductId],[ProductQty],[ProductName],[ProductCostPrice],[ProductSellingPrice]
							,[ServingUnitId])
							select @ProductNoteID  ,R.MaterialID,PRM.ProductName, 

							--CONVERT ACCORDING TO PURCHASE QTY  
							---divide by receipe unit qty to get purchasing unit wise actual qty
							--(case when R.ProductQty>1 then R.Quantity else td.Qty* R.Quantity end) / (case when isnull( uc.SubUnitValue ,1)=0 then 1 else isnull( uc.SubUnitValue,1) end)  ,
							((R.Quantity/R.ProductQty)/uc.SubUnitValue)*TD.Qty ,-- mqTY
							PSM.SellingPrice,
							--(R.CostPrice/R.ProductQty)*TD.Qty,
							--(R.CostPrice/R.ProductQty)*TD.Qty,
							(R.CostPrice/R.ProductQty),
							(R.CostPrice/R.ProductQty),
							TD.ProductID,
							TD.Qty , --Production Qty Only put in first row
							P.ProductName,PSMFinProd.CostPrice,PSMFinProd.SellingPrice, td.ProductServingUnitId

							from (Select TD1.ProductID,
							StockLocationID,TD1.locationID,Zno,Receipt,UnitNo,PSU.ProductServingUnitID,
							sum(Qty) Qty From transactiondets TD1
							inner join ProductServingUnits PSU on psu.ServingUnit = TD1.ServingUnit and psu.productid=TD1.ProductID 
							WHERE  TD1.DocumentID IN (1,3)
							group by TD1.ProductID,StockLocationID,TD1.locationID,Zno,Receipt,UnitNo,PSU.ProductServingUnitID ) TD
							inner join Receipes R on TD.ProductID = R.ProductID and TD.ProductServingUnitId=R.ProductServingUnitId
							--and (R.ProductQty=1)  
							and R.locationID=@ReceiptLocID
							--	  Or R.productQty=1 )
							inner join products PRM on PRM.productID = R.MaterialID --for raw materials
							inner join Products P on P.ProductID = R.ProductID --for finished product
							inner join productStockMasters PSM on PSM.productID = R.MaterialID and PSM.locationID= @tmpStockLoc -- for raw material
							inner join productStockMasters PSMFinProd on PSMFinProd.ProductID =TD.ProductID and PSMFinProd.locationID =@tmpStockLoc --for finishproduct
							Left join UnitConversions UC 
							on PRM.PurchasingUnit = uc.UnitOfMeasureId and PRM.WeightPerUnit = uc.UnitConversionId --Join with unit convertion   
							where td.stockLocationID=@tmpStockLoc
							and td.LocationID   =@ReceiptLocID and td.Zno=@R_Zno and td.Receipt=@ReceiptNo
							and td.UnitNo = @UnitNo AND (P.AutoProduction = 1) and td.productID=@ProductID and td.qty=@Qty
							and td.ProductServingUnitId=@ServingUnitID
						end
						------------------------------------------------------------------------
						End  --else end
						
					end
				FETCH NEXT FROM cur INTO @ProductID,@Qty,@ServingUnitID
				end
				
			close cur
			deallocate cur
			
		   print 'Detail inserted'
		   
		   select ProductID,ServingUnitId ,MIN(ProductionNoteDetailID) as ProductionNoteDetailID
		   into #tmpProductionItemFilter
		   from ProductionNoteDetails where ProductionNoteHeaderId =@ProductNoteID
		   group by ProductId,ServingUnitId
		   
		   update PND set pnd.ProductQty = 0  --Set only one production items qty value ,zero all other product qty as required in structure 
		   from ProductionNoteDetails PND inner join #tmpProductionItemFilter 
		   on pnd.ProductID= #tmpProductionItemFilter.ProductId 
		      and Pnd.ServingUnitId = #tmpProductionItemFilter.ServingUnitId 
		      and pnd.ProductionNoteDetailId <> #tmpProductionItemFilter.ProductionNoteDetailID
		      and pnd.ProductionNoteHeaderId =@ProductNoteID 
		   
	       DROP TABLE #tmpProductionItemFilter

			DECLARE cur1 CURSOR
			FOR
				select PNH.ProductionLocId as ProductionLocId, PND.MaterialId as MaterialId,sum(pnd.MaterialQty) as MatQty from 
			   ProductionNoteDetails PND  Inner join ProductionNoteHeaders PNH 
			   on PNH.ProductionNoteHeaderId = PND.ProductionNoteHeaderId  
			   inner join ProductStockMasters PS 
			   on PS.ProductId =PND.MaterialId and ps.LocationId = PNH.ProductionLocId 
			   and PNH.ProductionNoteHeaderId =@ProductNoteID  group by PNH.ProductionLocId,PND.MaterialId

			OPEN cur1
			FETCH NEXT FROM cur1 INTO @ProductionLocId,@MaterialId,@MatQty

			WHILE @@fetch_status = 0 
				BEGIN

					update ProductStockMasters set Stock = Stock - @MatQty where LocationId=@ProductionLocId and ProductId=@MaterialId
								
					FETCH NEXT FROM cur1 INTO @ProductionLocId,@MaterialId,@MatQty
				end
				
			close cur1
			deallocate cur1
	       
		   
		   print 'Raw materials deducted'

		   	DECLARE cur2 CURSOR
			FOR
			select PND.ProductId as ProductId, PND.ProductQty as ProductQty,
			PND.CostPrice as CostPrice,ProductionLocId from 
		   ProductionNoteDetails PND  Inner join ProductionNoteHeaders PNH 
		   on PNH.ProductionNoteHeaderId = PND.ProductionNoteHeaderId  
		   inner join ProductStockMasters PS 
		   on PS.ProductId =PND.ProductId  and ps.LocationId = PNH.ProductionLocId 
			and PNH.ProductionNoteHeaderId =@ProductNoteID and pnd.ProductQty >0  

			OPEN cur2
			FETCH NEXT FROM cur2 INTO @ProductId,@Qty,@ProductCost,@ProdLocId

			WHILE @@fetch_status = 0 
				BEGIN

					--update ProductStockMasters set Stock = Stock + @Qty where LocationId=@ProdLocId and ProductId=@ProductId
								
					update ProductStockMasters set  AvgCost = case when (stock + @Qty)>0 then
					((AvgCost * Stock) + (@MatQty * @ProductCost))/(Stock + @Qty)
										else AvgCost End
										where LocationId=@ReceiptLocID and ProductId=@ProductId

					FETCH NEXT FROM cur2 INTO @ProductId,@Qty,@ProductCost,@ProdLocId
				end
				
			close cur2
			deallocate cur2

		   print 'Finish product added and Avg Cost Caled'
	       
		   update PND set  PND.AvgCost =PS.AvgCost from 
		   ProductionNoteDetails PND  Inner join ProductionNoteHeaders PNH 
		   on PNH.ProductionNoteHeaderId = PND.ProductionNoteHeaderId  
		   inner join ProductStockMasters PS 
		   on PS.ProductId =PND.ProductId and ps.LocationId = PNH.ProductionLocId 
		   and PNH.ProductionNoteHeaderId =@ProductNoteID and pnd.ProductQty >0
		   print 'Avg Cost updated to production note-finish product'
	       
	       update ProductionNoteHeaders 
	       set ProductCostPrice =(case when
 (select SUM(CostPrice) from ProductionNoteDetails where ProductionNoteHeaderId=@ProductNoteID GROUP by ProductionNoteHeaderId) IS NULL then 0 else (select SUM(CostPrice) from ProductionNoteDetails where ProductionNoteHeaderId=@ProductNoteID group by ProductionNoteHeaderId) end ) where ProductionNoteHeaderId =@ProductNoteID
	       
		   End --Header inserted ? if
       End --Production not exist ?  if
	fetch next from locCursor into @tmpStockLoc 
	end 

	close LocCursor
	deallocate LocCursor


End";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion GenProductionNotes


            #region SP_AsAtStockBal
            spName = "SP_AsAtStockBal";
            query = @"CREATE PROCEDURE [dbo].[SP_AsAtStockBal]
    @CompanyId INT ,
    @SelectedLocationID INT ,
    @ToDate dateTime,
	@ProductID NVARCHAR(MAX),
    @DepartmentID NVARCHAR(mAX),
	@WithZeroBal int =1

AS 
     
    BEGIN
      
set dateformat dmy
  
		Print 'AA'
        CREATE TABLE #TmpStockTrans  
            (
              [LocationID] [bigint] NOT NULL ,
              [ToLocationName] [nvarchar](50) NULL ,
              [StockCode] [nvarchar](25)  NULL ,
              [BatchNo] [nvarchar](25) NULL ,
              [Qty] [decimal](18, 2) NOT NULL ,
              [CostPrice] [decimal](18, 2) ,
              [SellingPrice] [decimal](18, 2) ,
              [TransactionType] [nvarchar](50) NULL ,
              [TransactionNo] [nvarchar](20) NULL ,
              [TransactionDate] [DateTime],
              [ZNo] [int] NULL ,
              [UnitNo] [int] NULL 
            )
	
	        CREATE TABLE #tmpSelProducts
            (
              [item] [nvarchar](25) NULL 
            )
            
      CREATE TABLE #TmpSelDepartments
            (
              [item] [nvarchar](25) NULL 
            )
            
        if @ProductID <>'0'
        begin
        	insert into #tmpSelProducts Select distinct CONVERT(Nvarchar(50), ProductID) as item From
			Products where ',' + @ProductID + ',' like
			'%,' + Convert(Nvarchar(50),ProductID) + ',%'
        end
        else
        begin	--all items
			insert into #tmpSelProducts Select distinct CONVERT(Nvarchar(50), ProductID) as item From
			Products 
        end

        if @DepartmentID <>'0'
        begin
        insert into #TmpSelDepartments Select distinct CONVERT(Nvarchar(50), RstDepartmentID  ) as item From
        RstDepartments where ',' + @DepartmentID  + ',' like
        '%,' + Convert(Nvarchar(50),RstDepartmentID) + ',%'
        end
        else
        begin     --all items
             insert into #TmpSelDepartments Select distinct CONVERT(Nvarchar(50), RstDepartmentID  ) as item From
			RstDepartments
        end
      

		/*--GRN  */ 
                INSERT  INTO #TmpStockTrans 
                        ( LocationID ,
                          StockCode ,
                          BatchNo ,
                          Qty ,
                          TransactionType ,
                          TransactionNo ,
                          TransactionDate ,
                          CostPrice ,
                          SellingPrice ,
                          ZNo ,
                          UnitNo
                        )
                        SELECT  ph.GRNLocationId  ,
                                pd.StockCode ,
                                pd.BatchNo ,
                                ( pd.GRNQuantity + pd.FreeQty ) AS QTY ,
                                'GRN' ,
                                ph.DocumentNo ,
                                --DATEADD(day, 0, DATEDIFF(day, 0, ph.DocumentDate)) + DATEADD(day, 0 - DATEDIFF(day, 0, ph.CreatedDate), ph.CreatedDate),
                                ph.DocumentDate,
                                pd.CostPrice ,
                                pd.SellingPrice ,
                                0 ,
                                0
                        FROM    PurchaseDetails pd
                                INNER JOIN PurchaseHeaders ph ON pd.PurchaseHeaderID = ph.PurchaseHeaderId
                                                              AND pd.DocumentNo = ph.DocumentNo
                        WHERE   ph.DocumentID = 4
                                AND ph.GRNLocationId = @SelectedLocationID
                                AND CAST(ph.DocumentDate AS DATE) <= @ToDate and ph.DocumentStatus=3
								AND convert(nvarchar(25),pd.Productid) in (select item from #tmpSelProducts )
                                and Pd.ProductID in (select ProductID From Products where CONVERT(NVARCHAR(25), DepartmentId) in (select item from #TmpSelDepartments  ) )
                                
                        ORDER BY ph.DocumentDate
		Print 'A'
 
		/*--Purchase Returns  */
                INSERT  INTO #TmpStockTrans 
                        ( LocationID ,
                          StockCode ,
                          BatchNo ,
                          Qty ,
                          TransactionType ,
                          TransactionNo ,
                          TransactionDate ,
                          CostPrice ,
                          SellingPrice ,
                          ZNo ,
                          UnitNo
                        )
                        SELECT  ph.GRNLocationId ,
                                pd.StockCode ,
                                pd.BatchNo ,
                                ( ( pd.GRNQuantity + pd.FreeQty ) * -1 ) AS QTY ,
                                'Purchase Returns' ,
                                ph.DocumentNo ,
                                --DATEADD(day, 0, DATEDIFF(day, 0, ph.DocumentDate)) + DATEADD(day, 0 - DATEDIFF(day, 0, ph.CreatedDate), ph.CreatedDate),
                                ph.DocumentDate ,
                                pd.CostPrice ,
                                pd.SellingPrice ,
                                0 ,
                                0
                        FROM    PurchaseDetails pd
                                INNER JOIN PurchaseHeaders ph ON pd.PurchaseHeaderID = ph.PurchaseHeaderId
                                                              AND pd.DocumentID = ph.DocumentID
                        WHERE    CAST(ph.DocumentDate  AS DATE) <=@ToDate                              
								AND ph.DocumentID = 6
								and ph.GRNLocationId =@SelectedLocationID  and ph.DocumentStatus=3
								AND convert(nvarchar(25),pd.Productid) in (select item from #tmpSelProducts )
                                and Pd.ProductID in (select ProductID From Products where CONVERT(NVARCHAR(25), DepartmentId) in (select item from #TmpSelDepartments  ) )
								
                        ORDER BY ph.DocumentDate
                        
		/*--TOG IN  */
                INSERT  INTO #TmpStockTrans 
                        ( LocationID ,
                          StockCode ,
                          BatchNo ,
                          Qty ,
                          TransactionType ,
                          TransactionNo ,
                          TransactionDate ,
                          CostPrice ,
                          SellingPrice ,
                          ToLocationName ,
                          ZNo ,
                          UnitNo
                        )
                        SELECT  @SelectedLocationID  ,
                                td.StockCode ,
                                td.BatchNo ,
                               td.OrderQty   ,
                                'TOG IN'  ,
                                th.DocumentNo ,
                                --DATEADD(day, 0, DATEDIFF(day, 0, th.DocumentDate)) + DATEADD(day, 0 - DATEDIFF(day, 0, th.CreatedDate), th.CreatedDate),
                                th.DocumentDate ,
                                td.CostPrice ,
                                td.SellingPrice ,
                                ISNULL(l.LocationName, '') ,
                                0,
                                0
                        FROM   TransferNoteDetails AS td INNER JOIN
                         TransferNoteHeaders AS th ON th.TransferNoteHeaderID = td.TransferNoteHeaderID INNER JOIN
                         SysLocations l ON th.ToLocationID = l.SysLocationID
                        WHERE   th.DocumentStatus=3 and
				  ( th.ToLocationID  = @SelectedLocationID )
                              and  CAST(th.DocumentDate AS DATE)  <=@ToDate
							  AND convert(nvarchar(25),td.Productid) in (select item from #tmpSelProducts )
                                and td.ProductID in (select ProductID From Products where CONVERT(NVARCHAR(25), DepartmentId) in (select item from #TmpSelDepartments  ) )
                                
                        ORDER BY th.DocumentDate

							/*--TOG OUT  */
                INSERT  INTO #TmpStockTrans 
                        ( LocationID ,
                          StockCode ,
                          BatchNo ,
                          Qty ,
                          TransactionType ,
                          TransactionNo ,
                          TransactionDate ,
                          CostPrice ,
                          SellingPrice ,
                          ToLocationName ,
                          ZNo ,
                          UnitNo
                        )
                        SELECT  @SelectedLocationID  ,
                                td.StockCode ,
                                td.BatchNo ,
                               td.OrderQty*-1   ,
                                'TOG OUT'  ,
                                th.DocumentNo ,
                                --DATEADD(day, 0, DATEDIFF(day, 0, th.DocumentDate)) + DATEADD(day, 0 - DATEDIFF(day, 0, th.CreatedDate), th.CreatedDate),
                                th.DocumentDate ,
                                td.CostPrice ,
                                td.SellingPrice ,
                                ISNULL(l.LocationName, '') ,
                                0,
                                0
                        FROM   TransferNoteDetails AS td INNER JOIN
                         TransferNoteHeaders AS th ON th.TransferNoteHeaderID = td.TransferNoteHeaderID INNER JOIN
                         SysLocations l ON th.FromLocationId = l.SysLocationID
                        WHERE   th.DocumentStatus=3 and
				  ( th.FromLocationId  = @SelectedLocationID )
                              and  CAST(th.DocumentDate AS DATE)  <=@ToDate
							  AND convert(nvarchar(25),td.Productid) in (select item from #tmpSelProducts )
                                and td.ProductID in (select ProductID From Products where CONVERT(NVARCHAR(25), DepartmentId) in (select item from #TmpSelDepartments  ) )
                                
                        ORDER BY th.DocumentDate
							
		print 'TOG'
								
		/*--Stock Adjustment (ADD, reduce and override all types) */
                INSERT  INTO #TmpStockTrans 
                        ( LocationID ,
                          StockCode ,
                          BatchNo ,
                          Qty ,
                          TransactionType ,
                          TransactionNo ,
                          TransactionDate ,
                          CostPrice ,
                          SellingPrice ,
                          ZNo ,
                          UnitNo
                        )
                        SELECT  SH.StockLocationId ,
                                PSM.StockCode  ,
                                0 ,
                                Case when ad.BaseType ='Add' then ad.AdjustStock
                                     when ad.BaseType ='Reduce' then -1* ad.AdjustStock 
                                     when ad.BaseType ='Override' then (ad.AdjustStock- ad.currentstock)
                                     else ad.AdjustStock  end
                                     
                                 ,
                                case when Ad.BaseType ='Add' then 'Stock Adjustment (ADD)' 
                                     when Ad.BaseType ='Reduce' then 'Stock Adjustment (Reduce)'
                                     When Ad.BaseType = 'Override' then 'Stock Adjustment (Override)' 
                                     else 'NA' end                    
                                      ,
                                sh.DocumentNo ,
								SH.CreatedDate ,
                                ad.CostPrice ,
                                ad.SellingPrice ,
                                0 ,
                                0
                        FROM    StockAdjustmentDetails ad
                                INNER JOIN StockAdjustmentHeaders sh ON sh.StockAdjustmentHeaderID = ad.StockAdjustmentHeaderID
                                INNER JOIN ProductStockMasters psm ON AD.ProductId = PSM.ProductId AND PSM.LocationId = SH.LocationId 
                        WHERE   
                        --ad.BaseType ='Add' and
                             sh.DocumentStatus=3 and
                                 sh.StockLocationId = @SelectedLocationID
                                and CAST(sh.CreatedDate  AS DATE)  <=@ToDate
								AND convert(nvarchar(25),ad.Productid) in (select item from #tmpSelProducts )
                                and ad.ProductID in (select ProductID From Products where CONVERT(NVARCHAR(25), DepartmentId) in (select item from #TmpSelDepartments  ) )
                               
                        ORDER BY sh.CreatedDate 
						print 'Stock Adjustments'
						
							 
		 						
		/*--Sales & Returns */	
                INSERT  INTO #TmpStockTrans 
                        ( LocationID ,
                          StockCode ,
                          BatchNo ,
                          Qty ,
                          TransactionType ,
                          TransactionNo ,
                          TransactionDate ,
                          CostPrice ,
                          SellingPrice ,
                          ZNo ,
                          UnitNo
                        )
                        SELECT  td.StockLocationID,
                                td.ProductCode ,
                                td.BatchNo ,
                                SUM(CASE DocumentID
                                      WHEN 1 THEN -(Qty)
                                      WHEN 3 THEN -(Qty)
                                      WHEN 2 THEN Qty
                                      WHEN 4 THEN Qty
                                      ELSE 0
                                    END) ,
                                'Sales & Returns',
                                td.Receipt ,
                                 
                                dateadd(second,datepart(second,td.endtime),
                                DATEADD(MINUTE ,DATEPART(MINUTE ,td.EndTime), 
                                dateadd(hh,DATEPART(hh,td.EndTime), td.RecDate))), --add end time to recdate
                                
                                td.Cost ,
                                td.Price ,
                                td.ZNo ,
                                td.UnitNo
                        FROM    TransactionDets td
                        WHERE  CAST(td.RecDate AS DATE)  <=@ToDate
								     
                                and td.StockLocationID = @SelectedLocationID 
								AND convert(nvarchar(25),td.Productid) in (select item from #tmpSelProducts )
                                and td.ProductID in (select ProductID From Products where CONVERT(NVARCHAR(25), DepartmentId) in (select item from #TmpSelDepartments  ) )
                               
                        GROUP BY td.StockLocationID ,
                                td.ProductCode ,
                                td.BatchNo ,
                                    dateadd(second,datepart(second,td.endtime),
                                DATEADD(MINUTE ,DATEPART(MINUTE ,td.EndTime), 
                                dateadd(hh,DATEPART(hh,td.EndTime), td.RecDate))) ,
                                td.Cost ,
                                td.Price ,
                                td.Receipt ,
                                td.ZNo ,
                                td.UnitNo
                        ORDER BY dateadd(second,datepart(second,td.endtime),
                                DATEADD(MINUTE ,DATEPART(MINUTE ,td.EndTime), 
                                dateadd(hh,DATEPART(hh,td.EndTime), td.RecDate)))	
                                
                                
                                
/*--PRODUCTION NOTE ADD  */ 
                INSERT  INTO #TmpStockTrans 
                        ( LocationID ,
                          StockCode ,
                          BatchNo ,
                          Qty ,
                          TransactionType ,
                          TransactionNo ,
                          TransactionDate ,
                          CostPrice ,
                          SellingPrice ,
                          ZNo ,
                          UnitNo
                        )
                        SELECT distinct ph.ProductionLocId    ,
                                p.ProductCode  ,
                                '',
                                pd.ProductQty  AS QTY ,
                                'PRODUCTION-ADD' ,
                                ph.DocumentNo ,
                                --DATEADD(day, 0, DATEDIFF(day, 0, ph.DocumentDate)) + DATEADD(day, 0 - DATEDIFF(day, 0, ph.CreatedDate), ph.CreatedDate),
                                ph.CreatedDate ,
                                pd.ProductCostPrice  ,
                                pd.ProductSellingPrice  ,
                                0 ,
                                0
                        FROM    (select  PD.ProductionNoteHeaderId, ProductID ,pd.ProductCostPrice  ,
                                 pd.ProductSellingPrice  , sum(ProductQty) as ProductQty 
                                 From ProductionNoteDetails PD
                                 where PD.productQty<>0
                                 group by pd.ProductionNoteHeaderId, PD.productID,
                                 pd.ProductCostPrice  ,pd.ProductSellingPrice 
                                 ) PD inner join 
                                ProductionNoteHeaders ph 
                                on pd.ProductionNoteHeaderId =ph.ProductionNoteHeaderId 
                                inner join Products P on p.ProductId = pd.ProductId 
                        WHERE ph.ProductionLocId   = @SelectedLocationID
                                AND CAST(ph.CreatedDate AS DATE) <= @ToDate
								AND convert(nvarchar(25),PD.Productid) in (select item from #tmpSelProducts )
                                and PD.ProductID in (select ProductID From Products where CONVERT(NVARCHAR(25), DepartmentId) in (select item from #TmpSelDepartments  ) )
                    
                        ORDER BY ph.CreatedDate                                 	
	
	/*--PRODUCTION NOTE REDUCE  */ 
                INSERT  INTO #TmpStockTrans 
                        ( LocationID ,
                          StockCode ,
                          BatchNo ,
                          Qty ,
                          TransactionType ,
                          TransactionNo ,
                          TransactionDate ,
                          CostPrice ,
                          SellingPrice ,
                          ZNo ,
                          UnitNo
                        )
                        SELECT distinct ph.ProductionLocId    ,
                                p.ProductCode  ,
                                '',
                                pd.MaterialQty * -1  AS QTY ,
                                'PRODUCTION-CONSUME' ,
                                ph.DocumentNo ,
                                --DATEADD(day, 0, DATEDIFF(day, 0, ph.DocumentDate)) + DATEADD(day, 0 - DATEDIFF(day, 0, ph.CreatedDate), ph.CreatedDate),
                                ph.CreatedDate ,
                                pd.CostPrice ,
                                pd.SellingPrice ,
                                0 ,
                                0
                        FROM    (select  PD.ProductionNoteHeaderId, MaterialId  ,pd.CostPrice ,
                                 pd.SellingPrice , sum(MaterialQty) as MaterialQty 
                                 From ProductionNoteDetails PD
                                 where PD.MaterialQty <>0
                                 group by pd.ProductionNoteHeaderId, PD.MaterialId ,
                                 pd.CostPrice  ,pd.SellingPrice
                                 ) PD inner join 
                                ProductionNoteHeaders ph 
                                on pd.ProductionNoteHeaderId =ph.ProductionNoteHeaderId 
                                inner join Products P on p.ProductId = pd.MaterialId  
                        WHERE ph.ProductionLocId   = @SelectedLocationID
                                AND CAST(ph.CreatedDate AS DATE) <= @ToDate
								AND convert(nvarchar(25),PD.MaterialId) in (select item from #tmpSelProducts )
                                and PD.MaterialId in (select ProductID From Products where CONVERT(NVARCHAR(25), DepartmentId) in (select item from #TmpSelDepartments  ) )
 
                        ORDER BY ph.CreatedDate 	


				if @WithZeroBal=1
				begin
                  SELECT (select LocationName from SysLocations where SysLocationID=@SelectedLocationID) as Location, 
				  p.ProductCode  ,p.PRODUCTNAME,SUM(ISNULL(stock.Qty,0)) AS STOCK 
		          FROM PRODUCTS p LEFT JOIN #TmpStockTrans STOCK ON P.ProductCode  = STOCK.StockCode
		          GROUP BY  p.ProductCode ,p.PRODUCTNAME
				end
				else
				begin
					  SELECT (select LocationName from SysLocations where SysLocationID=@SelectedLocationID) as Location, 
					  p.ProductCode   ,p.PRODUCTNAME  ,SUM(ISNULL(stock.Qty,0)) AS STOCK 
		          FROM PRODUCTS p LEFT JOIN #TmpStockTrans STOCK ON P.ProductCode  = STOCK.StockCode
		          GROUP BY  p.ProductCode ,p.PRODUCTNAME
				  having SUM(ISNULL(stock.Qty,0))>0
				end

				END";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_AsAtStockBal

            #region sp_CheckProductTransactionCount
            spName = "sp_CheckProductTransactionCount";
            query = @"CREATE PROCEDURE [dbo].[sp_CheckProductTransactionCount]
	@ProductCode  NVARCHAR(50) ,
    @CompanyID INT 
AS
DECLARE @Count INT
BEGIN
	
SET NOCOUNT ON;   
SELECT @Count = 0
IF OBJECT_ID('tempdb..#t0') IS NOT NULL 
BEGIN
	DROP TABLE #t0
END 
CREATE TABLE #t0
(
 TranCount Int
)

SELECT @Count =@Count + count(*) FROM PurchaseHeaders ph inner join PurchaseDetails pd
on ph.PurchaseHeaderId = pd.PurchaseHeaderId
inner join Products p on pd.productid = p.productid
where P.productcode = @ProductCode AND P.CompanyID = @CompanyID

SELECT @Count = @Count+ count(*) FROM RequestNoteHeaders ph inner join RequestNoteDetails pd
on ph.RequestnoteHeaderId = pd.RequestnoteHeaderId
inner join Products p on pd.productid = p.productid
where P.productcode = @ProductCode AND P.CompanyID = @CompanyID

SELECT @Count = @Count+ count(*) FROM TransferNoteHeaders ph inner join TransferNoteDetails pd
on ph.TransferNoteHeaderID = pd.TransferNoteHeaderID
inner join Products p on pd.productid = p.productid
where P.productcode = @ProductCode AND P.CompanyID = @CompanyID

SELECT @Count = @Count+ count(*) FROM StockAdjustmentHeaders ph inner join StockAdjustmentDetails pd
on ph.StockAdjustmentHeaderId = pd.StockAdjustmentHeaderId
inner join Products p on pd.productid = p.productid
where P.productcode = @ProductCode AND P.CompanyID = @CompanyID

SELECT @Count = @Count+ count(*) FROM PurchaseOrderHeaders ph inner join PurchaseOrderDetails pd
on ph.PurchaseOrderHeaderId = pd.PurchaseOrderHeaderId
inner join Products p on pd.productid = p.productid
where P.productcode = @ProductCode AND P.CompanyID = @CompanyID

SELECT @Count = @Count+ count(*) FROM TransactionDets ph inner join Products p on ph.productid = p.productid
where P.productcode = @ProductCode AND P.CompanyID = @CompanyID

SELECT @Count = @Count+ count(*) FROM ProductionNoteHeaders ph inner join ProductionNoteDetails pd
on ph.ProductionNoteHeaderId = pd.ProductionNoteHeaderId
inner join Products p on pd.productid = p.productid
where P.productcode = @ProductCode AND P.CompanyID = @CompanyID

insert into #t0 
select @Count
select TranCount from #t0 

END";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion sp_CheckProductTransactionCount

            #region SP_DailySales
            spName = "SP_DailySales";
            query = @"CREATE PROCEDURE [dbo].[SP_DailySales]
@Date Datetime='',
@LocationId int=0
AS
BEGIN



SELECT distinct t.Receipt,t.LocationID,t.UnitNo,t.ZNo,
--t.SerialNo,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'FS',t.RecDate) as FoodSale,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'BS',t.RecDate) as BevSale,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'NS',t.RecDate) as NonSale,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'CH',t.RecDate) as Cash,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'CD',t.RecDate) as Card,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'OT',t.RecDate) as Others,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'Ol',t.RecDate) as Online,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'UB',t.RecDate) as UBER,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'PM',t.RecDate) as PICKME,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'SC',t.RecDate) as ServCharge,
0.0 as ChiliPaste,
dbo.GetDailySalesReportValues(RTRIM(t.Receipt),t.ZNo,t.UnitNo,t.LocationID,'VT',t.RecDate) as VAT,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'D',t.RecDate) as Discount,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'NB',t.RecDate) as NBT,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'TD',t.RecDate) as TDL,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'GR',t.RecDate) as Gross,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'NT',t.RecDate)+dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'SC',t.RecDate)+dbo.GetDailySalesReportValues(RTRIM(t.Receipt),t.ZNo,t.UnitNo,t.LocationID,'VT',t.RecDate)+dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'TD',t.RecDate)+dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'NB',t.RecDate)-dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'D',t.RecDate) as TNet,
--dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'NT',t.RecDate) as TNet,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'CR',t.RecDate) as Credit,
'NA' as HoldersName

FROM TransactionDets t 
where
cast(t.RecDate as date)=CAST(@Date as date)
and t.LocationID=@LocationId and t.Status=1

END";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_DailySales

            #region sp_DataUpload
            spName = "sp_DataUpload";
            query = @"CREATE PROCEDURE [dbo].[sp_DataUpload]

as

Select * From SysCompanies

Select * From SysLocations

Select * From RstDepartments


Select * From Final_HMS.dbo.RstDepartments";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion sp_DataUpload

            #region SP_DB_DepartmentWiseSales
            spName = "SP_DB_DepartmentWiseSales";
            query = @"CREATE PROCEDURE [dbo].[SP_DB_DepartmentWiseSales]
	@FromDate date,
	@ToDate date,
    @LocationID INT ,
    @CustCatID int, --customer group
    @CompanyID int

AS 
    SET NOCOUNT ON
    SET XACT_ABORT ON

    BEGIN TRY
			
		IF OBJECT_ID('tempdb..#t0') IS NOT NULL 
		BEGIN
			DROP TABLE #t0
		END 
		CREATE TABLE #t0
		(DeptID int,DeptName varchar(100),Nett decimal,Cost decimal)
			
			if @CustCatID<>0
				begin
					if @LocationID=0
					begin
						insert into #t0 
						select rstdepartmentid as DeptID,departmentname as DeptName,
						 case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
						case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
						where recDATE between @FromDate and @ToDate and 
						customercategoryid=@CustCatID and CompanyID=@CompanyID
						group by rstdepartmentid,departmentname
					end
					else
					begin
						insert into #t0 
						select rstdepartmentid as DeptID,departmentname as DeptName,
						  case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
						case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
						where recDATE between @FromDate and @ToDate and 
						locationID=@LocationID and customercategoryid=@CustCatID 
						group by rstdepartmentid,departmentname
					end
				end
			else
				begin
					if @LocationID=0
					begin
						insert into #t0 
						select rstdepartmentid as DeptID,departmentname as DeptName,
						  case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
						case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
						where recDATE between @FromDate and @ToDate    and CompanyID=@CompanyID
						 group by rstdepartmentid,departmentname
					end
					else
					begin
						insert into #t0 
						select rstdepartmentid as DeptID,departmentname as DeptName,
						  case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
						case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
						where recDATE between @FromDate and @ToDate and 
						locationID=@LocationID  group by rstdepartmentid,departmentname
					end
				end

		SELECT DeptID,DeptName,Nett,Cost  FROM  #t0
    END TRY
  
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            BEGIN
                --ROLLBACK TRANSACTION
                SELECT  ERROR_MESSAGE() AS Result

            END
        ELSE 
            BEGIN
                SELECT  ERROR_MESSAGE() AS Result
            END
    END CATCH ";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_DB_DepartmentWiseSales

            #region SP_DB_DeptOrderTypeWiseSales
            spName = "SP_DB_DeptOrderTypeWiseSales";
            query = @"CREATE PROCEDURE [dbo].[SP_DB_DeptOrderTypeWiseSales]
	@FromDate date,
	@ToDate date,
    @LocationID INT ,
    @DeptID int ,
    @CustCatID int, --customer group
    @CateModeID int
AS 
    SET NOCOUNT ON
    SET XACT_ABORT ON

    BEGIN TRY

        DECLARE @reccount int = 0 ,@date date,@dateyear int,@datemonth int,@dateweek int,
        @MonthsCount int,@MonthNumber int,
        @WeeksCount int, @WeekNumber int,
        @strMonth varchar(50)
			
		IF OBJECT_ID('tempdb..#t0') IS NOT NULL 
		BEGIN
			DROP TABLE #t0
		END 
		CREATE TABLE #t0
		(DeptID int,DeptName varchar(100),OrderTypeID int,OrderType varchar(50),Nett decimal,Cost decimal)
			
	--	if @ChartType=1 --daily
	--	begin
	--		set @date = @FromDate
		
	--		while (@date <= @ToDate) 
	--		begin
				if @DeptID <>0
					begin--//
						if @CustCatID<>0
							begin
								if @LocationID=0
								begin
									insert into #t0 
									select rstdepartmentid as DeptID,departmentname as DeptName ,cateringmoodid as OrderTypeID,cateringmoodname as OrderType,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recDATE between @FromDate and @ToDate and 
									customercategoryid=@CustCatID and RstDepartmentid=@DeptID
									and cateringmoodid=@CateModeID
									group by cateringmoodname,cateringmoodid,rstdepartmentid,departmentname
								end
								else
								begin
									insert into #t0 
									select rstdepartmentid as DeptID,departmentname as DeptName ,cateringmoodid as OrderTypeID,cateringmoodname as OrderType,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recDATE between @FromDate and @ToDate and 
									locationID=@LocationID and customercategoryid=@CustCatID and RstDepartmentid=@DeptID
									and cateringmoodid=@CateModeID
									group by cateringmoodname,cateringmoodid,rstdepartmentid,departmentname
								end
							end
						else
							begin
								if @LocationID=0
								begin
									insert into #t0 
									select rstdepartmentid as DeptID,departmentname as DeptName ,cateringmoodid as OrderTypeID,cateringmoodname as OrderType,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recDATE between @FromDate and @ToDate and 
									RstDepartmentid=@DeptID and cateringmoodid=@CateModeID group by cateringmoodname,cateringmoodid,rstdepartmentid,departmentname
								end
								else
								begin
									insert into #t0 
									select rstdepartmentid as DeptID,departmentname as DeptName ,cateringmoodid as OrderTypeID,cateringmoodname as OrderType,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recDATE between @FromDate and @ToDate and 
									locationID=@LocationID and cateringmoodid=@CateModeID and RstDepartmentid=@DeptID group by cateringmoodname,cateringmoodid,rstdepartmentid,departmentname
								end
							end
					end--//
				else
					begin
						if @CustCatID<>0
							begin
								if @LocationID=0
								begin
									insert into #t0 
									select rstdepartmentid as DeptID,departmentname as DeptName ,cateringmoodid as OrderTypeID,cateringmoodname as OrderType,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recDATE between @FromDate and @ToDate and 
									customercategoryid=@CustCatID and cateringmoodid=@CateModeID  group by cateringmoodname,cateringmoodid,rstdepartmentid,departmentname
								end
								else
								begin
									insert into #t0 
									select rstdepartmentid as DeptID,departmentname as DeptName ,cateringmoodid as OrderTypeID,cateringmoodname as OrderType,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recDATE between @FromDate and @ToDate and 
									locationID=@LocationID and customercategoryid=@CustCatID and cateringmoodid=@CateModeID group by cateringmoodname,cateringmoodid,rstdepartmentid,departmentname
								end
							end
						else
							begin
							if @LocationID=0
								begin
									insert into #t0 
									select rstdepartmentid as DeptID,departmentname as DeptName ,cateringmoodid as OrderTypeID,cateringmoodname as OrderType,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recDATE between @FromDate and @ToDate and cateringmoodid=@CateModeID group by cateringmoodname,cateringmoodid,rstdepartmentid,departmentname
								end	
							else
								begin
									insert into #t0 
									select rstdepartmentid as DeptID,departmentname as DeptName ,cateringmoodid as OrderTypeID,cateringmoodname as OrderType,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recDATE between @FromDate and @ToDate	and locationID=@LocationID and cateringmoodid=@CateModeID group by cateringmoodname,cateringmoodid,rstdepartmentid,departmentname
								end						
							end
					end
					
	
		
		SELECT DeptID,DeptName,OrderTypeID,OrderType,Nett,Cost  FROM  #t0
    END TRY
  
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            BEGIN
                --ROLLBACK TRANSACTION
                SELECT  ERROR_MESSAGE() AS Result

            END
        ELSE 
            BEGIN
                SELECT  ERROR_MESSAGE() AS Result
            END
    END CATCH  ";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_DB_DeptOrderTypeWiseSales

            #region SP_DB_FoodCostEstimate
            spName = "SP_DB_FoodCostEstimate";
            query = @"CREATE PROCEDURE [dbo].[SP_DB_FoodCostEstimate]
	@FromDate date,
	@ToDate date,
	@CompanyID int ,
    @LocationID INT ,
    @IsDeptWise int

AS 
    SET NOCOUNT ON
    SET XACT_ABORT ON

    BEGIN TRY
    declare @date date
    
		if @IsDeptWise=0
			begin
				IF OBJECT_ID('tempdb..#t0') IS NOT NULL 
				BEGIN
					DROP TABLE #t0
				END 
				
				CREATE TABLE #t0
				(
				  RecDate varchar(50),
				  [value] [decimal](18, 2) NOT NULL 
				)
				
				set @date = @FromDate
		
				while (@date <= @ToDate) 
				begin
					if @LocationID=0
					begin
						insert into #t0 
						select @date  recdate,
						sum(qty*cost)/sum(nett)*100 as value from view_sales where 
						recdate =@date and CompanyID=@CompanyID
						group by recdate
					end
					else
					begin
						insert into #t0 
						select @date recdate ,
						sum(qty*cost)/sum(nett)*100 as value from view_sales where 
						recdate =@date and CompanyID=@CompanyID and locationid=@LocationID
						group by recdate
					end
					set @date = dateadd(DAY, 1, @date)
        		end
			SELECT RecDate,Value  FROM  #t0
			
		end
		if @IsDeptWise=1
		begin
			IF OBJECT_ID('tempdb..#t1') IS NOT NULL 
			BEGIN
				DROP TABLE #t1
			END 
			
			CREATE TABLE #t1
			(
			  [DeptID] [int] NOT NULL ,
			  [DeptName] [varchar](100) NOT NULL ,
			  [value] [decimal](18, 2) NOT NULL 
			)
			
				if @LocationID=0
				begin
					insert into #t1
					select rstdepartmentid as DeptID,departmentname as DeptName,
					sum(qty*cost)/sum(nett)*100 as value from view_sales where 
					recdate between @FromDate and @ToDate and CompanyID=@CompanyID
					group by rstdepartmentid ,departmentname 
				end
				else
				begin
					insert into #t1
					select rstdepartmentid as DeptID,departmentname as DeptName,
					sum(qty*cost)/sum(nett)*100 as value from view_sales where 
					recdate between @FromDate and @ToDate and CompanyID=@CompanyID and locationid=@LocationID
					group by rstdepartmentid ,departmentname
				end
				
			SELECT DeptID,DeptName,Value  FROM  #t1
		end
    END TRY
  
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            BEGIN
                --ROLLBACK TRANSACTION
                SELECT  ERROR_MESSAGE() AS Result

            END
        ELSE 
            BEGIN
                SELECT  ERROR_MESSAGE() AS Result
            END
    END CATCH ";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_DB_FoodCostEstimate

            #region SP_DB_FoodCostEstimate_New
            spName = "SP_DB_FoodCostEstimate_New";
            query = @"CREATE PROCEDURE [dbo].[SP_DB_FoodCostEstimate_New]
	@FromDate date,
	@ToDate date,
	@CompanyID int ,
    @LocationID INT ,
    @IsDeptWise int

AS 
    SET NOCOUNT ON
    SET XACT_ABORT ON

    BEGIN TRY
		if @IsDeptWise=0
			begin
				IF OBJECT_ID('tempdb..#t0') IS NOT NULL 
				BEGIN
					DROP TABLE #t0
				END 
				
				CREATE TABLE #t0
				(
				  [RecDate] date not null,
				  [value] [decimal](18, 2) NOT NULL 
				)
				
				if @LocationID=0
				begin
					insert into #t0 
					select recdate,
					sum(qty*cost)/sum(nett)*100 as value from view_sales where 
					recdate between @FromDate and @ToDate and CompanyID=@CompanyID
					group by recdate
				end
				else
				begin
					insert into #t0 
					select recdate,
					sum(qty*cost)/sum(nett)*100 as value from view_sales where 
					recdate between @FromDate and @ToDate and CompanyID=@CompanyID and locationid=@LocationID
					group by recdate
				end
        	
			SELECT RecDate,DeptID,DeptName,Value  FROM  #t0
			
		end
		if @IsDeptWise=1
		begin
			IF OBJECT_ID('tempdb..#t1') IS NOT NULL 
			BEGIN
				DROP TABLE #t1
			END 
			
			CREATE TABLE #t1
			(
			  [DeptID] [int] NOT NULL ,
			  [DeptName] [varchar](100) NOT NULL ,
			  [value] [decimal](18, 2) NOT NULL 
			)
			
				if @LocationID=0
				begin
					insert into #t1
					select recdate,rstdepartmentid as DeptID,departmentname as DeptName,
					sum(qty*cost)/sum(nett)*100 as value from view_sales where 
					recdate between @FromDate and @ToDate and CompanyID=@CompanyID
					group by recdate,rstdepartmentid ,departmentname 
				end
				else
				begin
					insert into #t1
					select recdate,rstdepartmentid as DeptID,departmentname as DeptName,
					sum(qty*cost)/sum(nett)*100 as value from view_sales where 
					recdate between @FromDate and @ToDate and CompanyID=@CompanyID and locationid=@LocationID
					group by recdate,rstdepartmentid ,departmentname
				end
				
			SELECT DeptID,DeptName,Value  FROM  #t1
		end
    END TRY
  
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            BEGIN
                --ROLLBACK TRANSACTION
                SELECT  ERROR_MESSAGE() AS Result

            END
        ELSE 
            BEGIN
                SELECT  ERROR_MESSAGE() AS Result
            END
    END CATCH ";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_DB_FoodCostEstimate_New

            #region SP_DB_FoodWasteAndMixedWastage
            spName = "SP_DB_FoodWasteAndMixedWastage";
            query = @"CREATE PROCEDURE [dbo].[SP_DB_FoodWasteAndMixedWastage]
	@FromDate date,
	@ToDate date,
    @LocationID INT ,
    @ChartType int, -- daily/weekly/monthly
	@OrderType int, -- 0-All / 1-Food Waste / 2-Mixed Waste
	@CompanyID int

AS 
    SET NOCOUNT ON
    SET XACT_ABORT ON

    BEGIN TRY

        DECLARE @reccount int = 0 ,@date date,@dateyear int,@datemonth int,@dateweek int,
        @MonthsCount int,@MonthNumber int,
        @WeeksCount int, @WeekNumber int,
        @strMonth varchar(50)
			
		IF OBJECT_ID('tempdb..#t0') IS NOT NULL 
		BEGIN
			DROP TABLE #t0
		END 
		CREATE TABLE #t0
		(recdate varchar(50),Weights decimal)
			
		if @ChartType=1 --daily
		begin
			set @date = @FromDate
		
			while (@date <= @ToDate) 
			begin
				if @OrderType=0  --all order types
				begin
					if @LocationID=0
					begin
						insert into #t0 
						SELECT @date recdate,case when SUM(AdjustStock) IS null then 0 else SUM(AdjustStock)end Weights 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						--inner join UnitConversions on Products.WeightPerUnit = UnitConversions.UnitConversionId
						WHERE cast(StockAdjustmentHeaders.CreatedDate as date)=@date 
						and (StockAdjustmentDetails.Reason = N'Wastage') and StockAdjustmentHeaders.CompanyID=@CompanyID
					end
					else
					begin
						insert into #t0 
						SELECT @date recdate,case when SUM(AdjustStock) IS null then 0 else SUM(AdjustStock)end Weights 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						--inner join UnitConversions on Products.WeightPerUnit = UnitConversions.UnitConversionId
						WHERE cast(StockAdjustmentHeaders.CreatedDate as date)=@date 
						and StockLocationId=@LocationID and (StockAdjustmentDetails.Reason = N'Wastage')
					end
				end
				else if @OrderType=1 -- product
				begin
					if @LocationID=0
					begin
						insert into #t0 
						SELECT @date recdate,case when SUM(AdjustStock) IS null then 0 else SUM(AdjustStock)end Weights 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						--inner join UnitConversions on Products.WeightPerUnit = UnitConversions.UnitConversionId
						WHERE cast(StockAdjustmentHeaders.CreatedDate as date)=@date 
						and Products.IsRowMaterial=0 and (StockAdjustmentDetails.Reason = N'Wastage')
						 and StockAdjustmentHeaders.CompanyID=@CompanyID
					end	
					else
					begin
						insert into #t0 
						SELECT @date recdate,case when SUM(AdjustStock) IS null then 0 else SUM(AdjustStock)end Weights 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						--inner join UnitConversions on Products.WeightPerUnit = UnitConversions.UnitConversionId
						WHERE cast(StockAdjustmentHeaders.CreatedDate as date)=@date 
						and Products.IsRowMaterial=0 and StockLocationId=@LocationID and (StockAdjustmentDetails.Reason = N'Wastage')
					end						
				end	
				else if @OrderType=2-- raw meterial 
				begin
					if @LocationID=0
					begin
						insert into #t0 
						SELECT @date recdate,case when SUM(AdjustStock) IS null then 0 else SUM(AdjustStock)end Weights 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						--inner join UnitConversions on Products.WeightPerUnit = UnitConversions.UnitConversionId
						WHERE cast(StockAdjustmentHeaders.CreatedDate as date)=@date 
						and Products.IsRowMaterial=1 and (StockAdjustmentDetails.Reason = N'Wastage')
						 and StockAdjustmentHeaders.CompanyID=@CompanyID
					end	
					else
					begin
						insert into #t0 
						SELECT @date recdate,case when SUM(AdjustStock) IS null then 0 else SUM(AdjustStock)end Weights 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						--inner join UnitConversions on Products.WeightPerUnit = UnitConversions.UnitConversionId
						WHERE cast(StockAdjustmentHeaders.CreatedDate as date)=@date 
						and Products.IsRowMaterial=1 and StockLocationId=@LocationID and (StockAdjustmentDetails.Reason = N'Wastage')
					end						
				end		
				set @date = dateadd(DAY, 1, @date)
			end 
			
		end
		if @ChartType=2  -- weekly
		begin
			set @date = @FromDate
			set @dateweek =(Select DatePart(week, @FromDate))
			set @dateyear = year(@FromDate)
			set @WeeksCount=(select ceiling(convert(float, abs(datediff(day, @FromDate,@ToDate))) / 7))+1
			set @WeekNumber=1
			
			while (@WeekNumber<=@WeeksCount ) 
			begin--@@

			set @dateweek=(Select DatePart(week, @date))
			if @OrderType=0  --all order types
				begin
					if @LocationID=0
					begin
						insert into #t0 
						SELECT @WeekNumber recdate,case when SUM(AdjustStock) IS null then 0 else SUM(AdjustStock)end Weights 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						--inner join UnitConversions on Products.WeightPerUnit = UnitConversions.UnitConversionId
						WHERE 
						YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and DATEPART(week, cast(StockAdjustmentHeaders.CreatedDate as date))=@dateweek 
						and cast(StockAdjustmentHeaders.CreatedDate as date) between @FromDate and @ToDate
						and (StockAdjustmentDetails.Reason = N'Wastage')
						 and StockAdjustmentHeaders.CompanyID=@CompanyID
					end
					else
					begin
						insert into #t0 
						SELECT @WeekNumber recdate,case when SUM(AdjustStock) IS null then 0 else SUM(AdjustStock)end Weights 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						--inner join UnitConversions on Products.WeightPerUnit = UnitConversions.UnitConversionId
						WHERE YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and DATEPART(week, cast(StockAdjustmentHeaders.CreatedDate as date))=@dateweek 
						and cast(StockAdjustmentHeaders.CreatedDate as date) between @FromDate and @ToDate 
						and StockLocationId=@LocationID and (StockAdjustmentDetails.Reason = N'Wastage')
					end
				end
			else if @OrderType=1 -- product
				begin
					if @LocationID=0
					begin
						insert into #t0 
						SELECT @WeekNumber recdate,case when SUM(AdjustStock) IS null then 0 else SUM(AdjustStock)end Weights 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						--inner join UnitConversions on Products.WeightPerUnit = UnitConversions.UnitConversionId
						WHERE YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and DATEPART(week, cast(StockAdjustmentHeaders.CreatedDate as date))=@dateweek 
						and cast(StockAdjustmentHeaders.CreatedDate as date) between @FromDate and @ToDate 
						and Products.IsRowMaterial=0 and (StockAdjustmentDetails.Reason = N'Wastage')
						 and StockAdjustmentHeaders.CompanyID=@CompanyID
					end	
					else
					begin
						insert into #t0 
						SELECT @WeekNumber recdate,case when SUM(AdjustStock) IS null then 0 else SUM(AdjustStock)end Weights 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						--inner join UnitConversions on Products.WeightPerUnit = UnitConversions.UnitConversionId
						WHERE YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and DATEPART(week, cast(StockAdjustmentHeaders.CreatedDate as date))=@dateweek 
						and cast(StockAdjustmentHeaders.CreatedDate as date) between @FromDate and @ToDate 
						and Products.IsRowMaterial=0 and StockLocationId=@LocationID and (StockAdjustmentDetails.Reason = N'Wastage')
					end						
				end	
			else if @OrderType=2 -- raw meterial 
				begin
					if @LocationID=0
					begin
						insert into #t0 
						SELECT @WeekNumber recdate,case when SUM(AdjustStock) IS null then 0 else SUM(AdjustStock)end Weights 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						--inner join UnitConversions on Products.WeightPerUnit = UnitConversions.UnitConversionId
						WHERE YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and DATEPART(week, cast(StockAdjustmentHeaders.CreatedDate as date))=@dateweek 
						and cast(StockAdjustmentHeaders.CreatedDate as date) between @FromDate and @ToDate 
						and Products.IsRowMaterial=1 and (StockAdjustmentDetails.Reason = N'Wastage')
						 and StockAdjustmentHeaders.CompanyID=@CompanyID
					end	
					else
					begin
						insert into #t0 
						SELECT @WeekNumber recdate,case when SUM(AdjustStock) IS null then 0 else SUM(AdjustStock)end Weights 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						--inner join UnitConversions on Products.WeightPerUnit = UnitConversions.UnitConversionId
						WHERE YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and DATEPART(week, cast(StockAdjustmentHeaders.CreatedDate as date))=@dateweek 
						and cast(StockAdjustmentHeaders.CreatedDate as date) between @FromDate and @ToDate 
						and Products.IsRowMaterial=1 and StockLocationId=@LocationID and (StockAdjustmentDetails.Reason = N'Wastage')
					end						
				end	
			
			set @date = dateadd(DAY, 7, @date)
			set @WeekNumber=@WeekNumber+1
			
			set @dateyear = year(@date)

			end--@@
		end
		
		if @ChartType=3 --monthly
		begin
			set @date = @FromDate
			set @dateyear = year(@FromDate)
			set @datemonth = month(@FromDate)
			
			set @strMonth=DATENAME(month, @FromDate)
			set @MonthsCount=(SELECT DATEDIFF(mm, @FromDate, @ToDate) +1)
			set @MonthNumber=1
			
			while (@MonthNumber<=@MonthsCount ) 
			begin

			set @strMonth=DATENAME(month, @date)
				if @OrderType=0  --all order types
				begin
					if @LocationID=0
					begin
						insert into #t0 
						SELECT @strMonth recdate,case when SUM(AdjustStock) IS null then 0 else SUM(AdjustStock)end Weights 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						--inner join UnitConversions on Products.WeightPerUnit = UnitConversions.UnitConversionId
						WHERE 
						YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and MONTH(cast(StockAdjustmentHeaders.CreatedDate as date))=@datemonth
						and (StockAdjustmentDetails.Reason = N'Wastage')
						 and StockAdjustmentHeaders.CompanyID=@CompanyID
					end
					else
					begin
						insert into #t0 
						SELECT @strMonth recdate,case when SUM(AdjustStock) IS null then 0 else SUM(AdjustStock)end Weights 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						--inner join UnitConversions on Products.WeightPerUnit = UnitConversions.UnitConversionId
						WHERE YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and MONTH(cast(StockAdjustmentHeaders.CreatedDate as date))=@datemonth 
						and StockLocationId=@LocationID and (StockAdjustmentDetails.Reason = N'Wastage')
					end
				end
				else if @OrderType=1 -- product
				begin
					if @LocationID=0
					begin
						insert into #t0 
						SELECT @strMonth recdate,case when SUM(AdjustStock) IS null then 0 else SUM(AdjustStock)end Weights 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						--inner join UnitConversions on Products.WeightPerUnit = UnitConversions.UnitConversionId
						WHERE YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and MONTH(cast(StockAdjustmentHeaders.CreatedDate as date))=@datemonth 
						and Products.IsRowMaterial=0 and (StockAdjustmentDetails.Reason = N'Wastage')
						 and StockAdjustmentHeaders.CompanyID=@CompanyID
					end	
					else
					begin
						insert into #t0 
						SELECT @strMonth recdate,case when SUM(AdjustStock) IS null then 0 else SUM(AdjustStock)end Weights 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						--inner join UnitConversions on Products.WeightPerUnit = UnitConversions.UnitConversionId
						WHERE YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and MONTH(cast(StockAdjustmentHeaders.CreatedDate as date))=@datemonth 
						and Products.IsRowMaterial=0 and StockLocationId=@LocationID and (StockAdjustmentDetails.Reason = N'Wastage')
					end						
				end
				else if @OrderType=2 -- raw meterial 
				begin
					if @LocationID=0
					begin
						insert into #t0 
						SELECT @strMonth recdate,case when SUM(AdjustStock) IS null then 0 else SUM(AdjustStock)end Weights 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						--inner join UnitConversions on Products.WeightPerUnit = UnitConversions.UnitConversionId
						WHERE YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and MONTH(cast(StockAdjustmentHeaders.CreatedDate as date))=@datemonth 
						and Products.IsRowMaterial=1 and (StockAdjustmentDetails.Reason = N'Wastage')
						 and StockAdjustmentHeaders.CompanyID=@CompanyID
					end	
					else
					begin
						insert into #t0 
						SELECT @strMonth recdate,case when SUM(AdjustStock) IS null then 0 else SUM(AdjustStock)end Weights 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						--inner join UnitConversions on Products.WeightPerUnit = UnitConversions.UnitConversionId
						WHERE YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and MONTH(cast(StockAdjustmentHeaders.CreatedDate as date))=@datemonth 
						and Products.IsRowMaterial=1 and StockLocationId=@LocationID and (StockAdjustmentDetails.Reason = N'Wastage')
					end						
				end	
			
			set @date = dateadd(MONTH, 1, @date)
			set @MonthNumber=@MonthNumber+1
			
			set @dateyear = year(@date)
			set @datemonth = month(@date)
			end

		end
		
		SELECT recdate,Weights  FROM  #t0
    END TRY
  
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            BEGIN
                --ROLLBACK TRANSACTION
                SELECT  ERROR_MESSAGE() AS Result

            END
        ELSE 
            BEGIN
                SELECT  ERROR_MESSAGE() AS Result
            END
    END CATCH ";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_DB_FoodWasteAndMixedWastage

            #region SP_DB_GRNProductDetailsEstimate
            spName = "SP_DB_GRNProductDetailsEstimate";
            query = @"CREATE PROCEDURE [dbo].[SP_DB_GRNProductDetailsEstimate]
	@FromDate date,
	@ToDate date,
	@CompanyID int ,
    @LocationID INT ,
    @IsDeptWise int,
    @SupplierID  int

AS 
    SET NOCOUNT ON
    SET XACT_ABORT ON

    BEGIN TRY
    declare @date date
    
    IF OBJECT_ID('tempdb..#t0') IS NOT NULL 
				BEGIN
					DROP TABLE #t0
				END 
				
				CREATE TABLE #t0
				(
				  GRNcode varchar(50),
				  Value [decimal](18, 2) NOT NULL 
				)
    
		if @SupplierID=0
			begin
				
				
				set @date = convert(date,@FromDate)
		
				while (convert(date,@date) <= convert(date,@ToDate))
				begin
					if @LocationID=0
					begin
						insert into #t0 
						select ph.DocumentNo as GRNcode,
						ph.TotCostPrice as Value 
						from PurchaseHeaders ph
						where convert(date,ph.GRNDate) =CONVERT(date,@date) and ph.CompanyID=@CompanyID
						and ph.DocumentID = 4 and ph.IsGRN = 1 --and ph.DocumentStatus = 3
						
					end
					else
					begin
						insert into #t0 
						select ph.DocumentNo as GRNcode,
						ph.TotCostPrice as Value 
						from PurchaseHeaders ph
						where convert(date,ph.GRNDate) =CONVERT(date,@date) and ph.CompanyID=@CompanyID
						and ph.LocationId = @LocationID and ph.DocumentID = 4 and ph.IsGRN = 1 --and ph.DocumentStatus = 3
						
					end
					set @date = dateadd(DAY, 1, @date)
        		end
			SELECT GRNcode,Value  FROM  #t0
			
		end
		else 
		begin
				
			
				set @date = convert(date,@FromDate)
		
				while (convert(date,@date) <= convert(date,@ToDate))
				begin
					if @LocationID=0
					begin
						insert into #t0 
						select ph.DocumentNo as GRNcode,
						ph.TotCostPrice as Value 
						from PurchaseHeaders ph
						where convert(date,ph.GRNDate) =CONVERT(date,@date) and ph.CompanyID=@CompanyID and ph.SupplierID = @SupplierID
						and ph.DocumentID = 4 and ph.IsGRN = 1 --and ph.DocumentStatus = 3
					end
					else
					begin
						insert into #t0 
						select ph.DocumentNo as GRNcode,
						ph.TotCostPrice as Value 
						from PurchaseHeaders ph
						where convert(date,ph.GRNDate) =CONVERT(date,@date) and ph.CompanyID=@CompanyID and ph.SupplierID = @SupplierID
						and ph.LocationId = @LocationID and ph.DocumentID = 4 and ph.IsGRN = 1 --and ph.DocumentStatus = 3
						
					end
					set @date = dateadd(DAY, 1, @date)
        		end
			SELECT GRNcode,Value  FROM  #t0
		end
    END TRY
  
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            BEGIN
                --ROLLBACK TRANSACTION
                SELECT  ERROR_MESSAGE() AS Result

            END
        ELSE 
            BEGIN
                SELECT  ERROR_MESSAGE() AS Result
            END
    END CATCH ";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_DB_GRNProductDetailsEstimate

            #region SP_DB_HourlySales
            spName = "SP_DB_HourlySales";
            query = @"CREATE PROCEDURE [dbo].[SP_DB_HourlySales]
		@LocationID INT ,
		@FromDate date,
		@FromTime time,
		@ToDate date,
		@ToTime time ,
		@CompanyID int

	AS 
		SET NOCOUNT ON
		SET XACT_ABORT ON

		BEGIN TRY
				
			DECLARE @reccount int = 0 ,
			@date datetime,@todatetime datetime,
			@dateyear int,@datemonth int,@dateweek int,
			@MonthNumber int,
			@DayNumber int,
			@HourCount int,@HourNumber int,@Hour int,
			@strMonth varchar(50),@SalesFound int,@SalesVal decimal
	        
			IF OBJECT_ID('tempdb..#t0') IS NOT NULL 
			BEGIN
				DROP TABLE #t0
			END 
			CREATE TABLE #t0
			(RecTime int,Nett decimal)
			
				set @date = CAST(@FromDate as DATETIME) + CAST(@FromTime as DATETIME)
				set @todatetime = CAST(@ToDate as DATETIME) + CAST(@ToTime as DATETIME)

				set @dateyear = year(@FromDate)
				set @MonthNumber= month(@FromDate)
				set @DayNumber = DAY(@FromDate)
				set @Hour=datepart(hour,@date)
			
				set @HourNumber= 1
				set @SalesFound=0
				set @SalesVal=0
				
				set @HourCount =DATEDIFF(hh, @date , @todatetime)
				
				while (@HourNumber<=@HourCount ) 
				begin--@@

					if @LocationID=0
						begin
						if @SalesFound=1
							begin
								insert into #t0 
								select @Hour as recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett
								from view_sales 
								where recyear=@dateyear and recmonth=@MonthNumber and day(recdate)=@DayNumber
								and datepart(hour,endTime)=@Hour and CompanyID=@CompanyID
							end
						else
							begin
								set @SalesVal=(select case when SUM(nett) IS null then 0 else SUM(nett)end Nett
								from view_sales 
								where recyear=@dateyear and recmonth=@MonthNumber and day(recdate)=@DayNumber
								and datepart(hour,endTime)=@Hour and companyid=@CompanyID) 
								
								if @SalesVal<>0
									begin
										insert into #t0 
										select @Hour as recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett
										from view_sales 
										where recyear=@dateyear and recmonth=@MonthNumber and day(recdate)=@DayNumber
										and datepart(hour,endTime)=@Hour
										
										set @SalesFound=1
									end				
							end
						end
					else
						begin
							if @SalesFound=1
								begin
									insert into #t0 
									select @Hour as recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett
									from view_sales 
									where recyear=@dateyear and recmonth=@MonthNumber and day(recdate)=@DayNumber
									and datepart(hour,endTime)=@Hour and LocationID=@LocationID
								end
							else
								begin
									set @SalesVal=(select case when SUM(nett) IS null then 0 else SUM(nett)end Nett
									from view_sales 
									where recyear=@dateyear and recmonth=@MonthNumber and day(recdate)=@DayNumber
									and datepart(hour,endTime)=@Hour and LocationID=@LocationID) 
									
									if @SalesVal<>0
										begin
											insert into #t0 
											select @Hour as recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett
											from view_sales 
											where recyear=@dateyear and recmonth=@MonthNumber and day(recdate)=@DayNumber
											and datepart(hour,endTime)=@Hour  and LocationID=@LocationID
											
											set @SalesFound=1
										end				
								end
						end

					set @date = dateadd(MINUTE, 60, @date)
					set @dateyear = year(@date)
					set @MonthNumber= month(@date)
					set @DayNumber = DAY(@date)
					set @Hour=datepart(hour,@date)
					set @HourNumber=@HourNumber+1

				end--@@

			SELECT RecTime,Nett  FROM  #t0
		END TRY
	  
		BEGIN CATCH
			IF @@TRANCOUNT > 0 
				BEGIN
					--ROLLBACK TRANSACTION
					SELECT  ERROR_MESSAGE() AS Result

				END
			ELSE 
				BEGIN
					SELECT  ERROR_MESSAGE() AS Result
				END
		END CATCH ";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_DB_HourlySales

            #region SP_FastSlowReport
            spName = "SP_FastSlowReport";
            query = @"CREATE PROCEDURE [dbo].[SP_FastSlowReport] --2024-06-28
    @CompanyID Int,
    @From DateTime,
    @To DateTime,
    @StartTime Time,
    @EndTime Time,
    @Location Varchar(100),
    @MovementType Int,
    @Method Int,
    @Customers Varchar(100),
    @FastSlowValue Int
AS
BEGIN
    -- Define the base query
    DECLARE @BaseQuery NVARCHAR(MAX) = '
        SELECT TOP(@FastSlowValue) 
            C.CustomerCode + ''-'' + C.CustomerName AS Customer, 
            TD.ProductCode, 
            P.ProductName, 
            SUM(TD.Qty) AS Qty, 
            SUM(TD.Amount) AS Amount
        FROM TransactionDets AS TD 
        INNER JOIN Customers C ON TD.CustomerID = C.CustomerID 
        INNER JOIN Products P ON TD.ProductID = P.ProductId
        WHERE 
            TD.LocationId IN (' + @Location + ') AND 
            (TD.RecDate BETWEEN @From AND @To) AND 
            (CAST(TD.EndTime AS TIME) BETWEEN @StartTime AND @EndTime) AND 
            TD.CustomerID IN (' + @Customers + ') AND 
            (TD.DocumentID IN (1, 3)) AND 
            (TD.Status = 1) AND 
            (TD.TransStatus = 1) AND 
            (TD.SaleTypeID = 1) AND 
            (TD.BillTypeID = 1)
        GROUP BY 
            C.CustomerCode, 
            C.CustomerName, 
            TD.ProductCode, 
            P.ProductName
        ORDER BY ';

    -- Add ORDER BY clause based on MovementType and Method
    IF (@MovementType = 1 AND @Method = 1)
    BEGIN
        SET @BaseQuery = @BaseQuery + 'Qty DESC';
    END
    ELSE IF (@MovementType = 1 AND @Method = 2)
    BEGIN
        SET @BaseQuery = @BaseQuery + 'Amount DESC';
    END
    ELSE IF (@MovementType = 2 AND @Method = 1)
    BEGIN
        SET @BaseQuery = @BaseQuery + 'Qty ASC';
    END
    ELSE IF (@MovementType = 2 AND @Method = 2)
    BEGIN
        SET @BaseQuery = @BaseQuery + 'Amount ASC';
    END

    -- Execute the dynamic query
    EXEC sp_executesql @BaseQuery, 
        N'@FastSlowValue INT, @From DATETIME, @To DATETIME, @StartTime TIME, @EndTime TIME', 
        @FastSlowValue, @From, @To, @StartTime, @EndTime;
END";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_FastSlowReport

            #region SP_MenuItemStockSummary
            spName = "SP_MenuItemStockSummary";
            query = @"CREATE PROCEDURE [dbo].[SP_MenuItemStockSummary] --2024-06-05 Modified
 
    @CompanyID INT, 
    @Location VARCHAR(100),
    @Supplier VARCHAR(100), 
    @Category VARCHAR(100), 
    @Status INT, 
    @StockLevel INT,
    @PriceMode INT,
    @Department VARCHAR(100)
AS
BEGIN
    BEGIN TRY

    DECLARE @PriceModeColumnName NVARCHAR(50);
    DECLARE @StatusMode NVARCHAR(50);
    DECLARE @StockLevelMode NVARCHAR(50);
    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @PriceModeLabel NVARCHAR(25);

    IF @PriceMode = 0 
    BEGIN
        SET @PriceModeColumnName = 'PSM.CostPrice';
        SET @PriceModeLabel = 'Cost Price';
    END
    ELSE
    BEGIN
        SET @PriceModeColumnName = 'PSM.SellingPrice';
        SET @PriceModeLabel = 'Selling Price';
    END
	SET @StatusMode = ''
    IF @Status = 0
    BEGIN
        SET @StatusMode = ' AND P.IsActive = 0';
    END
    ELSE IF @Status = 1
    BEGIN
        SET @StatusMode = ' AND P.IsActive = 1';
    END
	SET @StockLevelMode = ''
    IF @StockLevel = 0
    BEGIN
        SET @StockLevelMode = ' AND PSM.Stock != 0';
    END
    ELSE IF @StockLevel = 1
    BEGIN
        SET @StockLevelMode = ' AND PSM.Stock < 0';
    END
    ELSE IF @StockLevel = 2
    BEGIN
        SET @StockLevelMode = ' AND PSM.Stock > 0';
    END
    ELSE IF @StockLevel = 3
    BEGIN
        SET @StockLevelMode = ' AND PSM.Stock = 0';
    END

    -- Drop the temporary table if it already exists
    IF OBJECT_ID('tempdb..#temp_table') IS NOT NULL
    BEGIN
        DROP TABLE #temp_table;
    END

    -- Create temporary table
    CREATE TABLE #temp_table (
        Product VARCHAR(50),
        Descriptions VARCHAR(150),
		Department VARCHAR(100),
		Category VARCHAR(100),
        Locations VARCHAR(150),
        Unit VARCHAR(150),
        PriceMode DECIMAL(18, 2),
        StockInHand DECIMAL(18, 2),
        Amount DECIMAL(18, 2),
        PriceModeLabel VARCHAR(25)
    );


        -- Construct dynamic SQL
        SET @SQL = N'
            INSERT INTO #temp_table
            SELECT 
                PSM.ProductCode AS Product,
                PSM.ProductName AS Descriptions,
				D.DepartmentCode + ''-'' + D.DepartmentName As Department,
				C.RstCategoryCode + ''-'' + C.RstCategoryName As Category,
                L.LocationCode + ''-'' + L.LocationName AS Locations,
                UM.UnitOfMeasureName AS Unit,
                ' + @PriceModeColumnName + ' AS PriceMode,
                PSM.Stock AS StockInHand,
                ROUND(CAST(SUM(PSM.CostPrice * PSM.Stock) AS DECIMAL(10, 2)), 2) AS Amount,
                ''' + @PriceModeLabel + ''' AS PriceModeLabel
            FROM 
                ProductStockMasters AS PSM
            INNER JOIN 
                Products AS P ON PSM.ProductId = P.ProductId 
            INNER JOIN 
                UnitOfMeasures AS UM ON P.PurchasingUnit = UM.UnitOfMeasureId
			INNER JOIN
				RstDepartments As D ON P.DepartmentId = D.RstDepartmentID
			INNER JOIN
				RstCategories As C ON P.CategoryId = C.RstCategoryID
            INNER JOIN 
                SysLocations AS L ON PSM.LocationId = L.SysLocationID
            INNER JOIN
                SupplierProducts AS SP ON PSM.ProductId = SP.ProductId 
            INNER JOIN
                Suppliers AS SUP ON SP.SupplierId = SUP.SupplierID
            WHERE 
                PSM.LocationId IN (' + @Location + ') AND SUP.SupplierID IN(' + @Supplier + ') AND P.PurchasingUnit IN(' + @Category + ') AND P.DepartmentID IN(' + @Department + ')
                ' + @StatusMode + @StockLevelMode + '
            GROUP BY 
                PSM.ProductCode,
                PSM.ProductName,
				D.DepartmentCode,
				D.DepartmentName,
				C.RstCategoryCode,
				C.RstCategoryName,
                L.LocationCode,
                L.LocationName,
                UM.UnitOfMeasureName,
                PSM.CostPrice,
                PSM.SellingPrice,
                PSM.Stock
            ORDER BY 
                PSM.ProductCode ASC;';
 
        -- Execute the dynamic SQL
        EXEC sp_executesql @SQL;

        -- Select from temporary table
        SELECT 
            Product,
            Descriptions,
			Department,
			Category,
            Locations,
            Unit,
            PriceMode,
            StockInHand,
            Amount,
            PriceModeLabel
        FROM 
            #temp_table;

        -- Drop temporary table
        DROP TABLE #temp_table;
    END TRY
    BEGIN CATCH
        -- Error handling
        SELECT
            ERROR_NUMBER() AS ErrorNumber,
            ERROR_STATE() AS ErrorState,
            ERROR_SEVERITY() AS ErrorSeverity,
            ERROR_LINE() AS ErrorLine,
            ERROR_PROCEDURE() AS ErrorProcedure,
            ERROR_MESSAGE() AS ErrorMessage;
    END CATCH
END;";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_MenuItemStockSummary

            #region SP_DB_HourlySalesTable
            spName = "SP_DB_HourlySalesTable";
            query = @"CREATE PROCEDURE [dbo].[SP_DB_HourlySalesTable]
		@LocationID INT ,
		@FromDate date,
		@CompanyID int

	AS 
		SET NOCOUNT ON
		SET XACT_ABORT ON

		BEGIN TRY
			declare	
			@HourNumber int,@DayNumber int,--@Hour int,
			@SalesFound int,@SalesVal decimal,
			@Date date,
			@Date1 date,@Date2 date,@Date3 date,@Date4 date,@Date5 date,@Date6 date,@Date7 date,
			@RowId int
	        
			IF OBJECT_ID('tempdb..#t0') IS NOT NULL 
			BEGIN
				DROP TABLE #t0
			END 
			
			CREATE TABLE #t0
			(RecTime varchar(10),Monday decimal(18,2),Tuesday decimal(18,2),Wednesday decimal(18,2),
			Thursday decimal(18,2),Friday decimal(18,2),Saturday decimal(18,2),Sunday decimal(18,2),RowIDx int)
			
			set @HourNumber= 0
			set @SalesFound=0
			set @SalesVal=0
			set @DayNumber=1
			set @RowId=0
			
					
			while (@HourNumber<=23 ) 
			begin--@@
				set @Date=@FromDate
				
				while (@DayNumber<=7 ) 
				begin--/*/*

				if @LocationID=0
					begin
						set @SalesVal=(select case when SUM(nett) IS null then 0 else SUM(nett)end Nett
						from view_sales 
						where RecDate=@Date and datepart(hour,endtime)=@HourNumber and CompanyID=@CompanyID)
						
						if @SalesVal<>0
						begin
							insert into #t0 
							select cast(@HourNumber as varchar) + ':00' as recdate,
							case when @DayNumber =1 then case when SUM(nett) IS null then 0 else SUM(nett)end else 0 end Nett,
							case when @DayNumber =2 then case when SUM(nett) IS null then 0 else SUM(nett)end else 0 end Nett,
							case when @DayNumber =3 then case when SUM(nett) IS null then 0 else SUM(nett)end else 0 end Nett,
							case when @DayNumber =4 then case when SUM(nett) IS null then 0 else SUM(nett)end else 0 end Nett,
							case when @DayNumber =5 then case when SUM(nett) IS null then 0 else SUM(nett)end else 0 end Nett,
							case when @DayNumber =6 then case when SUM(nett) IS null then 0 else SUM(nett)end else 0 end Nett,
							case when @DayNumber =7 then case when SUM(nett) IS null then 0 else SUM(nett)end else 0 end Nett,
							@RowId as RowIDx
							from view_sales 
							where RecDate=@Date and datepart(hour,endtime)=@HourNumber
							
							set @SalesFound=1
						end				
					
					end
				else	
					begin
						set @SalesVal=(select case when SUM(nett) IS null then 0 else SUM(nett)end Nett
						from view_sales 
						where RecDate=@Date and datepart(hour,endtime)=@HourNumber and LocationID=@LocationID) 
						
						if @SalesVal<>0
						begin
							insert into #t0 
							select cast(@HourNumber as varchar) + ':00' as recdate,
							case when @DayNumber =1 then case when SUM(nett) IS null then 0 else SUM(nett)end else 0 end Nett,
							case when @DayNumber =2 then case when SUM(nett) IS null then 0 else SUM(nett)end else 0 end Nett,
							case when @DayNumber =3 then case when SUM(nett) IS null then 0 else SUM(nett)end else 0 end Nett,
							case when @DayNumber =4 then case when SUM(nett) IS null then 0 else SUM(nett)end else 0 end Nett,
							case when @DayNumber =5 then case when SUM(nett) IS null then 0 else SUM(nett)end else 0 end Nett,
							case when @DayNumber =6 then case when SUM(nett) IS null then 0 else SUM(nett)end else 0 end Nett,
							case when @DayNumber =7 then case when SUM(nett) IS null then 0 else SUM(nett)end else 0 end Nett,
							@RowId as RowIDx
							from view_sales 
							where RecDate=@Date and datepart(hour,endtime)=@HourNumber  and LocationID=@LocationID
							
							set @SalesFound=1
						end						
					end
					
					set @DayNumber=@DayNumber+1
					set @Date= dateadd(DAY,1,@Date)					
				--	set @SalesFound=0
					set @SalesVal=0
					
				end--@@
				
				set @HourNumber= @HourNumber+1
				set @DayNumber=1
				
				if @SalesFound=1
				begin
					set @RowId=@RowId+1
					set @SalesFound=0
				end
				
			end --/*/*
			set @Date1=DATEADD(DAY,1,@FromDate)
			
			SELECT RecTime,sum(Monday) as Monday ,
			sum(Tuesday) as Tuesday ,sum(Wednesday) as Wednesday ,
			sum(Thursday) as Thursday ,sum(Friday) as Friday,sum(Saturday) as Saturday,sum(Sunday) as Sunday  ,RowIDx
			FROM  #t0
			group by RecTime,RowIDx
		END TRY
	  
		BEGIN CATCH
			IF @@TRANCOUNT > 0 
				BEGIN
					--ROLLBACK TRANSACTION
					SELECT  ERROR_MESSAGE() AS Result

				END
			ELSE 
				BEGIN
					SELECT  ERROR_MESSAGE() AS Result
				END
		END CATCH ";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_DB_HourlySalesTable

            #region SP_DB_MostOrderedProductWiseSales
            spName = "SP_DB_MostOrderedProductWiseSales";
            query = @"CREATE PROCEDURE [dbo].[SP_DB_MostOrderedProductWiseSales]
	@FromDate date,
	@ToDate date,
    @LocationID INT ,
    @CustCatID int,--customer group
    @TopCount int,
	@CompanyID int
AS 
    SET NOCOUNT ON
    SET XACT_ABORT ON

    BEGIN TRY
			
		IF OBJECT_ID('tempdb..#t0') IS NOT NULL 
		BEGIN
			DROP TABLE #t0
		END 
		CREATE TABLE #t0
		(DeptID int,DeptName varchar(100),ProductCode varchar(20) ,ProductName varchar(100),Nett decimal,Cost decimal)
			
			if @CustCatID<>0
				begin
					if @LocationID=0
					begin
						insert into #t0 
						select top (@TopCount) rstdepartmentid as DeptID,departmentname as DeptName,
						  productcode as ProductCode,descrip as ProductName,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
						case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
						where recDATE between @FromDate and @ToDate and 
						customercategoryid=@CustCatID 
						group by rstdepartmentid,departmentname,productcode,descrip
					end
					else
					begin
						insert into #t0 
						select top (@TopCount) rstdepartmentid as DeptID,departmentname as DeptName,
						  productcode as ProductCode,descrip as ProductName,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
						case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
						where recDATE between @FromDate and @ToDate and 
						locationID=@LocationID and customercategoryid=@CustCatID
						group by rstdepartmentid,departmentname,productcode,descrip
					end
				end
			else
				begin
					if @LocationID=0
					begin
						insert into #t0 
						select top (@TopCount) rstdepartmentid as DeptID,departmentname as DeptName,
						  productcode as ProductCode,descrip as ProductName,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
						case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
						where recDATE between @FromDate and @ToDate  
						 group by rstdepartmentid,departmentname,productcode,descrip
					end
					else
					begin
						insert into #t0 
						select top (@TopCount) rstdepartmentid as DeptID,departmentname as DeptName,
						  productcode as ProductCode,descrip as ProductName,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
						case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
						where recDATE between @FromDate and @ToDate and 
						locationID=@LocationID  group by rstdepartmentid,departmentname,productcode,descrip
					end
				end

		SELECT DeptID,DeptName,ProductCode,ProductName,Nett,Cost  FROM  #t0
    END TRY
  
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            BEGIN
                --ROLLBACK TRANSACTION
                SELECT  ERROR_MESSAGE() AS Result

            END
        ELSE 
            BEGIN
                SELECT  ERROR_MESSAGE() AS Result
            END
    END CATCH ";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_DB_MostOrderedProductWiseSales

            #region SP_DB_NumberOfOrders
            spName = "SP_DB_NumberOfOrders";
            query = @"CREATE PROCEDURE [dbo].[SP_DB_NumberOfOrders]
	@FromDate date,
	@ToDate date,
    @LocationID INT ,
    @ChartType int, -- daily/weekly/monthly
    @CustCatID int, --customer group
    @CompanyID int
AS 
    SET NOCOUNT ON
    SET XACT_ABORT ON

    BEGIN TRY

        DECLARE @reccount int = 0 ,@date date,@dateyear int,@datemonth int,@dateweek int,
        @MonthsCount int,@MonthNumber int,
        @WeeksCount int, @WeekNumber int,
        @strMonth varchar(50),
        @KOT decimal,@BOT decimal,@NONE decimal,
        @KOTCount int,@BOTCount int , @NONECount int
			
		IF OBJECT_ID('tempdb..#t0') IS NOT NULL 
		BEGIN
			DROP TABLE #t0
		END 
		CREATE TABLE #t0
		(recdate varchar(50),KOT decimal,KOTCount int,BOT decimal,BOTCount int,NON decimal,NONCount int)
			
		if @ChartType=1 --daily
		begin
			set @date = @FromDate
		
			while (@date <= @ToDate) 
			begin
				if @CustCatID<>0
					begin
						if @LocationID=0
						begin
							set @KOT=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recdate=@date and customercategoryid=@CustCatID and printertypename='KOT' and companyID=@CompanyID)
							set @BOT=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recdate=@date and customercategoryid=@CustCatID and printertypename='BOT' and companyID=@CompanyID)
							set @NONE=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recdate=@date and customercategoryid=@CustCatID and printertypename='NONE' and companyID=@CompanyID)
								
							set @KOTCount=(select count(distinct receipt) Nett from view_sales
							where recdate=@date and customercategoryid=@CustCatID and printertypename='KOT' and companyID=@CompanyID)
							set @BOTCount=(select count(distinct receipt) Nett from view_sales
							where recdate=@date and customercategoryid=@CustCatID and printertypename='BOT' and companyID=@CompanyID)
							set @NONECount=(select count(distinct receipt) Nett from view_sales
							where recdate=@date and customercategoryid=@CustCatID and printertypename='NONE' and companyID=@CompanyID)
							
							insert into #t0 values (@date,@KOT,@KOTCount,@BOT,@BOTCount,@NONE,@NONECount) 

						end
						else
						begin
							set @KOT=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recdate=@date and locationID=@LocationID and customercategoryid=@CustCatID and printertypename='KOT')
							set @BOT=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recdate=@date and locationID=@LocationID and customercategoryid=@CustCatID and printertypename='BOT')
							set @NONE=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recdate=@date and locationID=@LocationID and customercategoryid=@CustCatID and printertypename='NONE')
				
							set @KOTCount=(select count(distinct receipt) Nett from view_sales
							where recdate=@date and locationID=@LocationID and customercategoryid=@CustCatID and printertypename='KOT')
							set @BOTCount=(select count(distinct receipt) Nett from view_sales
							where recdate=@date and locationID=@LocationID and customercategoryid=@CustCatID and printertypename='BOT')
							set @NONECount=(select count(distinct receipt) Nett from view_sales
							where recdate=@date and locationID=@LocationID and customercategoryid=@CustCatID and printertypename='NONE')
				
							insert into #t0 values (@date,@KOT,@KOTCount,@BOT,@BOTCount,@NONE,@NONECount) 
			 
						end
					end
				else
					begin
					if @LocationID=0
						begin
							set @KOT=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recdate=@date and printertypename='KOT' and companyID=@CompanyID)
							set @BOT=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recdate=@date and printertypename='BOT' and companyID=@CompanyID)
							set @NONE=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recdate=@date and printertypename='NONE' and companyID=@CompanyID)
				
							set @KOTCount=(select count(distinct receipt) Nett from view_sales
							where recdate=@date and printertypename='KOT' and companyID=@CompanyID)
							set @BOTCount=(select count(distinct receipt) Nett from view_sales
							where recdate=@date and printertypename='BOT' and companyID=@CompanyID)
							set @NONECount=(select count(distinct receipt) Nett from view_sales
							where recdate=@date and printertypename='NONE' and companyID=@CompanyID)
							
							insert into #t0 values (@date,@KOT,@KOTCount,@BOT,@BOTCount,@NONE,@NONECount) 

						end	
					else
						begin
							set @KOT=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recdate=@date and locationID=@LocationID and printertypename='KOT')
							set @BOT=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recdate=@date and locationID=@LocationID and printertypename='BOT')
							set @NONE=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recdate=@date and locationID=@LocationID and printertypename='NONE')
							
							set @KOTCount=(select count(distinct receipt) Nett from view_sales
							where recdate=@date and locationID=@LocationID and printertypename='KOT')
							set @BOTCount=(select count(distinct receipt) Nett from view_sales
							where recdate=@date and locationID=@LocationID and printertypename='BOT')
							set @NONECount=(select count(distinct receipt) Nett from view_sales
							where recdate=@date and locationID=@LocationID and printertypename='NONE')
							
							insert into #t0 values (@date,@KOT,@KOTCount,@BOT,@BOTCount,@NONE,@NONECount) 
						end						
					end
			
					
				set @date = dateadd(DAY, 1, @date)
			end 
			
		end
		if @ChartType=2  -- weekly
		begin
			set @date = @FromDate
			set @dateweek =(Select DatePart(week, @FromDate))
			set @dateyear = year(@FromDate)
			set @WeeksCount=(select ceiling(convert(float, abs(datediff(day, @FromDate,@ToDate))) / 7))+1
			set @WeekNumber=1
			
			while (@WeekNumber<=@WeeksCount ) 
			begin--@@

			set @dateweek=(Select DatePart(week, @date))
				if @CustCatID<>0
					begin
						if @LocationID=0
						begin
							set @KOT=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recyear=@dateyear and recweek=@dateweek and recdate between @FromDate and @ToDate and
							customercategoryid=@CustCatID and printertypename='KOT' and companyID=@CompanyID)
							set @BOT=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recyear=@dateyear and recweek=@dateweek and recdate between @FromDate and @ToDate and
							customercategoryid=@CustCatID and printertypename='BOT' and companyID=@CompanyID)
							set @NONE=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recyear=@dateyear and recweek=@dateweek and recdate between @FromDate and @ToDate and
							customercategoryid=@CustCatID and printertypename='NONE' and companyID=@CompanyID)
							
							set @KOTCount =(select count(distinct receipt) Nett from view_sales
							where recyear=@dateyear and recweek=@dateweek and recdate between @FromDate and @ToDate and
							customercategoryid=@CustCatID and printertypename='KOT' and companyID=@CompanyID)
							set @BOTCount =(select count(distinct receipt) Nett from view_sales
							where recyear=@dateyear and recweek=@dateweek and recdate between @FromDate and @ToDate and
							customercategoryid=@CustCatID and printertypename='BOT' and companyID=@CompanyID)
							set @NONECount =(select count(distinct receipt) Nett from view_sales
							where recyear=@dateyear and recweek=@dateweek and recdate between @FromDate and @ToDate and
							customercategoryid=@CustCatID and printertypename='NONE' and companyID=@CompanyID)
				
							insert into #t0 values (@WeekNumber,@KOT,@KOTCount,@BOT,@BOTCount,@NONE,@NONECount) 
							
						end
						else
						begin
							set @KOT=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recyear=@dateyear and recweek=@dateweek and recdate between @FromDate and @ToDate and
							locationID=@LocationID and customercategoryid=@CustCatID and printertypename='KOT')
							set @BOT=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recyear=@dateyear and recweek=@dateweek and recdate between @FromDate and @ToDate and
							locationID=@LocationID and customercategoryid=@CustCatID and printertypename='BOT')
							set @NONE=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recyear=@dateyear and recweek=@dateweek and recdate between @FromDate and @ToDate and
							locationID=@LocationID and customercategoryid=@CustCatID and printertypename='NONE')
				
							set @KOTCount =(select count(distinct receipt) Nett from view_sales
							where recyear=@dateyear and recweek=@dateweek and recdate between @FromDate and @ToDate and
							locationID=@LocationID and customercategoryid=@CustCatID and printertypename='KOT')
							set @BOTCount =(select count(distinct receipt) Nett from view_sales
							where recyear=@dateyear and recweek=@dateweek and recdate between @FromDate and @ToDate and
							locationID=@LocationID and customercategoryid=@CustCatID and printertypename='BOT')
							set @NONECount =(select count(distinct receipt) Nett from view_sales
							where recyear=@dateyear and recweek=@dateweek and recdate between @FromDate and @ToDate and
							locationID=@LocationID and customercategoryid=@CustCatID and printertypename='NONE')
							
							insert into #t0 values (@WeekNumber,@KOT,@KOTCount,@BOT,@BOTCount,@NONE,@NONECount) 

						end
					end
				else
					begin
						if @LocationID=0
						begin
							set @KOT=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recyear=@dateyear and recweek=@dateweek	and recdate between @FromDate and @ToDate and printertypename='KOT' and companyID=@CompanyID)
							set @BOT=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recyear=@dateyear and recweek=@dateweek	and recdate between @FromDate and @ToDate and printertypename='BOT' and companyID=@CompanyID)
							set @NONE=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recyear=@dateyear and recweek=@dateweek	and recdate between @FromDate and @ToDate and printertypename='NONE' and companyID=@CompanyID)
				
							set @KOTCount =(select count(distinct receipt) Nett from view_sales
							where recyear=@dateyear and recweek=@dateweek	and recdate between @FromDate and @ToDate and printertypename='KOT' and companyID=@CompanyID)
							set @BOTCount =(select count(distinct receipt) Nett from view_sales
							where recyear=@dateyear and recweek=@dateweek	and recdate between @FromDate and @ToDate and printertypename='BOT' and companyID=@CompanyID)
							set @NONECount =(select count(distinct receipt) Nett from view_sales
							where recyear=@dateyear and recweek=@dateweek	and recdate between @FromDate and @ToDate and printertypename='NONE' and companyID=@CompanyID)
				
							insert into #t0 values (@WeekNumber,@KOT,@KOTCount,@BOT,@BOTCount,@NONE,@NONECount) 
							
						end	
						else
						begin
							set @KOT=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recyear=@dateyear and recweek=@dateweek	and recdate between @FromDate and @ToDate and
							locationID=@LocationID and printertypename='KOT')
							set @BOT=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recyear=@dateyear and recweek=@dateweek	and recdate between @FromDate and @ToDate and
							locationID=@LocationID and printertypename='BOT')
							set @NONE=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recyear=@dateyear and recweek=@dateweek	and recdate between @FromDate and @ToDate and
							locationID=@LocationID and printertypename='NONE')
							
							set @KOTCount =(select count(distinct receipt) Nett from view_sales
							where recyear=@dateyear and recweek=@dateweek	and recdate between @FromDate and @ToDate and
							locationID=@LocationID and printertypename='KOT')
							set @BOTCount =(select count(distinct receipt) Nett from view_sales
							where recyear=@dateyear and recweek=@dateweek	and recdate between @FromDate and @ToDate and
							locationID=@LocationID and printertypename='BOT')
							set @NONECount =(select count(distinct receipt) Nett from view_sales
							where recyear=@dateyear and recweek=@dateweek	and recdate between @FromDate and @ToDate and
							locationID=@LocationID and printertypename='NONE')
				
							insert into #t0 values (@WeekNumber,@KOT,@KOTCount,@BOT,@BOTCount,@NONE,@NONECount) 

						end							
					end
			
			
			set @date = dateadd(DAY, 7, @date)
			set @WeekNumber=@WeekNumber+1
			
			set @dateyear = year(@date)

			end--@@
		end
		
		if @ChartType=3 --monthly
		begin
			set @date = @FromDate
			set @dateyear = year(@FromDate)
			set @datemonth = month(@FromDate)
			
			set @strMonth=DATENAME(month, @FromDate)
			set @MonthsCount=(SELECT DATEDIFF(mm, @FromDate, @ToDate) +1)
			set @MonthNumber=1
			
			while (@MonthNumber<=@MonthsCount ) 
			begin

			set @strMonth=DATENAME(month, @date)

				if @CustCatID<>0
					begin
						if @LocationID=0
						begin
							set @KOT=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recyear=@dateyear and recmonth=@datemonth and 
							customercategoryid=@CustCatID  and printertypename='KOT' and companyID=@CompanyID)
							set @BOT=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recyear=@dateyear and recmonth=@datemonth and 
							customercategoryid=@CustCatID  and printertypename='BOT' and companyID=@CompanyID)
							set @NONE=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recyear=@dateyear and recmonth=@datemonth and 
							customercategoryid=@CustCatID  and printertypename='NONE' and companyID=@CompanyID)
							
							set @KOTCount =(select count(distinct receipt) Nett from view_sales
							where recyear=@dateyear and recmonth=@datemonth and 
							customercategoryid=@CustCatID  and printertypename='KOT' and companyID=@CompanyID)
							set @BOTCount =(select count(distinct receipt) Nett from view_sales
							where recyear=@dateyear and recmonth=@datemonth and 
							customercategoryid=@CustCatID  and printertypename='BOT' and companyID=@CompanyID)
							set @NONECount =(select count(distinct receipt) Nett from view_sales
							where recyear=@dateyear and recmonth=@datemonth and 
							customercategoryid=@CustCatID  and printertypename='NONE' and companyID=@CompanyID)
				
							insert into #t0 values (@strMonth,@KOT,@KOTCount,@BOT,@BOTCount,@NONE,@NONECount) 

						end
						else
						begin
							set @KOT=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recyear=@dateyear and recmonth=@datemonth and 
							locationID=@LocationID and customercategoryid=@CustCatID   and printertypename='KOT')
							set @BOT=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recyear=@dateyear and recmonth=@datemonth and 
							locationID=@LocationID and customercategoryid=@CustCatID   and printertypename='BOT')
							set @NONE=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recyear=@dateyear and recmonth=@datemonth and 
							locationID=@LocationID and customercategoryid=@CustCatID   and printertypename='NONE')
							
							set @KOTCount =(select count(distinct receipt) Nett from view_sales
							where recyear=@dateyear and recmonth=@datemonth and 
							locationID=@LocationID and customercategoryid=@CustCatID   and printertypename='KOT')
							set @BOTCount =(select count(distinct receipt) Nett from view_sales
							where recyear=@dateyear and recmonth=@datemonth and 
							locationID=@LocationID and customercategoryid=@CustCatID   and printertypename='BOT')
							set @NONECount =(select count(distinct receipt) Nett from view_sales
							where recyear=@dateyear and recmonth=@datemonth and 
							locationID=@LocationID and customercategoryid=@CustCatID   and printertypename='NONE')
				
							insert into #t0 values (@strMonth,@KOT,@KOTCount,@BOT,@BOTCount,@NONE,@NONECount) 

						end
	
					end
				else
					begin
						if @LocationID=0
						begin
							set @KOT=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recyear=@dateyear and recmonth=@datemonth   and printertypename='KOT' and companyID=@CompanyID)
							set @BOT=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recyear=@dateyear and recmonth=@datemonth   and printertypename='BOT' and companyID=@CompanyID)
							set @NONE=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recyear=@dateyear and recmonth=@datemonth   and printertypename='NONE' and companyID=@CompanyID)
							
							set @KOTCount =(select count(distinct receipt) Nett from view_sales
							where recyear=@dateyear and recmonth=@datemonth   and printertypename='KOT' and companyID=@CompanyID)
							set @BOTCount =(select count(distinct receipt) Nett from view_sales
							where recyear=@dateyear and recmonth=@datemonth   and printertypename='BOT' and companyID=@CompanyID)
							set @NONECount =(select count(distinct receipt) Nett from view_sales
							where recyear=@dateyear and recmonth=@datemonth   and printertypename='NONE' and companyID=@CompanyID)
				
							insert into #t0 values (@strMonth,@KOT,@KOTCount,@BOT,@BOTCount,@NONE,@NONECount) 

						end
						else
						begin
							set @KOT=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recyear=@dateyear and recmonth=@datemonth	and 
							locationID=@LocationID   and printertypename='KOT')
							set @BOT=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recyear=@dateyear and recmonth=@datemonth	and 
							locationID=@LocationID   and printertypename='BOT')
							set @NONE=(select case when SUM(nett) IS null then 0 else SUM(nett) end Nett from view_sales
							where recyear=@dateyear and recmonth=@datemonth	and 
							locationID=@LocationID   and printertypename='NONE')
							
							set @KOTCount =(select count(distinct receipt) Nett from view_sales
							where recyear=@dateyear and recmonth=@datemonth	and 
							locationID=@LocationID   and printertypename='KOT')
							set @BOTCount =(select count(distinct receipt) Nett from view_sales
							where recyear=@dateyear and recmonth=@datemonth	and 
							locationID=@LocationID   and printertypename='BOT')
							set @NONECount =(select count(distinct receipt) Nett from view_sales
							where recyear=@dateyear and recmonth=@datemonth	and 
							locationID=@LocationID   and printertypename='NONE')
				
							insert into #t0 values (@strMonth,@KOT,@KOTCount,@BOT,@BOTCount,@NONE,@NONECount) 
	
						end						
					end
				
			
			set @date = dateadd(MONTH, 1, @date)
			set @MonthNumber=@MonthNumber+1
			
			set @dateyear = year(@date)
			set @datemonth = month(@date)
			end

		end
		
		SELECT recdate,KOT,KOTCount,BOT,BOTCount, NON,NONCount  FROM  #t0
    END TRY
  
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            BEGIN
                --ROLLBACK TRANSACTION
                SELECT  ERROR_MESSAGE() AS Result

            END
        ELSE 
            BEGIN
                SELECT  ERROR_MESSAGE() AS Result
            END
    END CATCH ";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_DB_NumberOfOrders

            #region SP_DB_OrderTypeWiseProductSales
            spName = "SP_DB_OrderTypeWiseProductSales";
            query = @"CREATE PROCEDURE [dbo].[SP_DB_OrderTypeWiseProductSales]
	@FromDate date,
	@ToDate date,
    @LocationID INT ,
    @ChartType int,
    @CustCatID int, --customer group
    @OrderType int -- 1-KOT / 2-BOT / 3-NONE
AS 
    SET NOCOUNT ON
    SET XACT_ABORT ON

    BEGIN TRY

        DECLARE @reccount int = 0 ,@date date,@dateyear int,@datemonth int,@dateweek int,
        @MonthsCount int,@MonthNumber int,
        @WeeksCount int, @WeekNumber int,
        @strMonth varchar(50)
			
		IF OBJECT_ID('tempdb..#t0') IS NOT NULL 
		BEGIN
			DROP TABLE #t0
		END 
		CREATE TABLE #t0
		(recdate varchar(50), ProductCode varchar(20),Product varchar(100),Nett decimal)
			
		if @ChartType=1 --daily
		begin
			set @date = @FromDate
		
			while (@date <= @ToDate) 
			begin
				if @CustCatID<>0
					begin
						if @LocationID=0
						begin
							insert into #t0
							select @date recdate,Productcode ProductCode,descrip Product,SUM(nett) Nett from view_sales 
							where printertypeid=@OrderType and recdate =@date and customercategoryid=@CustCatID
							group by productid ,Productcode,descrip					
						end
						else
						begin
							insert into #t0
							select @date recdate,Productcode ProductCode,descrip Product,SUM(nett) Nett from view_sales 
							where printertypeid=@OrderType and recdate =@date and customercategoryid=@CustCatID
							and locationID=@LocationID
							group by productid ,Productcode,descrip	
			 
						end
					end
				else
					begin
					if @LocationID=0
						begin						
							insert into #t0
							select @date recdate,Productcode ProductCode,descrip Product,SUM(nett) Nett from view_sales 
							where printertypeid=@OrderType and recdate =@date 
							group by productid ,Productcode,descrip	 
						end	
					else
						begin
							insert into #t0
							select @date recdate,Productcode ProductCode,descrip Product,SUM(nett) Nett from view_sales 
							where printertypeid=@OrderType and recdate =@date and locationID=@LocationID
							group by productid ,Productcode,descrip	 
						end						
					end
			
					
				set @date = dateadd(DAY, 1, @date)
			end 
			
		end
		if @ChartType=2  -- weekly
		begin
			set @date = @FromDate
			set @dateweek =(Select DatePart(week, @FromDate))
			set @dateyear = year(@FromDate)
			set @WeeksCount=(select ceiling(convert(float, abs(datediff(day, @FromDate,@ToDate))) / 7))+1
			set @WeekNumber=1
			
			while (@WeekNumber<=@WeeksCount ) 
			begin--@@

			set @dateweek=(Select DatePart(week, @date))
				if @CustCatID<>0
					begin
						if @LocationID=0
						begin
							insert into #t0
							select @WeekNumber recdate,Productcode ProductCode,descrip Product,SUM(nett) Nett from view_sales 
							where printertypeid=@OrderType and recyear=@dateyear and recweek=@dateweek and recdate between @FromDate and @ToDate and
							customercategoryid=@CustCatID
							group by productid ,Productcode,descrip	  						
						end
						else
						begin
							insert into #t0
							select @WeekNumber recdate,Productcode ProductCode,descrip Product,SUM(nett) Nett from view_sales 
							where printertypeid=@OrderType and recyear=@dateyear and recweek=@dateweek and recdate between @FromDate and @ToDate and
							locationID=@LocationID and customercategoryid=@CustCatID
							group by productid ,Productcode,descrip	
						end
					end
				else
					begin
						if @LocationID=0
						begin
							insert into #t0
							select @WeekNumber recdate,Productcode ProductCode,descrip Product,SUM(nett) Nett from view_sales 
							where printertypeid=@OrderType and recyear=@dateyear and recweek=@dateweek	and recdate between @FromDate and @ToDate
							group by productid ,Productcode,descrip							
						end	
						else
						begin
							insert into #t0
							select @WeekNumber recdate,Productcode ProductCode,descrip Product,SUM(nett) Nett from view_sales 
							where printertypeid=@OrderType and recyear=@dateyear and recweek=@dateweek	and recdate between @FromDate and @ToDate and
							locationID=@LocationID
							group by productid ,Productcode,descrip
						end							
					end
			
			
			set @date = dateadd(DAY, 7, @date)
			set @WeekNumber=@WeekNumber+1
			
			set @dateyear = year(@date)

			end--@@
		end
		
		if @ChartType=3 --monthly
		begin
			set @date = @FromDate
			set @dateyear = year(@FromDate)
			set @datemonth = month(@FromDate)
			
			set @strMonth=DATENAME(month, @FromDate)
			set @MonthsCount=(SELECT DATEDIFF(mm, @FromDate, @ToDate) +1)
			set @MonthNumber=1
			
			while (@MonthNumber<=@MonthsCount ) 
			begin

			set @strMonth=DATENAME(month, @date)

				if @CustCatID<>0
					begin
						if @LocationID=0
						begin
							insert into #t0
							select @strMonth recdate,Productcode ProductCode,descrip Product,SUM(nett) Nett from view_sales 
							where printertypeid=@OrderType and recyear=@dateyear and recmonth=@datemonth and 
							customercategoryid=@CustCatID
							group by productid ,Productcode,descrip
						end
						else
						begin
							insert into #t0
							select @strMonth recdate,Productcode ProductCode,descrip Product,SUM(nett) Nett from view_sales 
							where printertypeid=@OrderType and recyear=@dateyear and recmonth=@datemonth and 
							locationID=@LocationID and customercategoryid=@CustCatID
							group by productid ,Productcode,descrip
						end
	
					end
				else
					begin
						if @LocationID=0
						begin
							insert into #t0
							select @strMonth recdate,Productcode ProductCode,descrip Product,SUM(nett) Nett from view_sales 
							where printertypeid=@OrderType and recyear=@dateyear and recmonth=@datemonth
							group by productid ,Productcode,descrip
						end
						else
						begin
							insert into #t0
							select @strMonth recdate,Productcode ProductCode,descrip Product,SUM(nett) Nett from view_sales 
							where printertypeid=@OrderType and recyear=@dateyear and recmonth=@datemonth	and 
							locationID=@LocationID 
							group by productid ,Productcode,descrip
						end						
					end
				
			
			set @date = dateadd(MONTH, 1, @date)
			set @MonthNumber=@MonthNumber+1
			
			set @dateyear = year(@date)
			set @datemonth = month(@date)
			end

		end
		
		SELECT recdate,ProductCode,Product,Nett  FROM  #t0
    END TRY
  
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            BEGIN
                --ROLLBACK TRANSACTION
                SELECT  ERROR_MESSAGE() AS Result

            END
        ELSE 
            BEGIN
                SELECT  ERROR_MESSAGE() AS Result
            END
    END CATCH";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_DB_OrderTypeWiseProductSales

            #region SP_DB_OrderTypeWiseSales
            spName = "SP_DB_OrderTypeWiseSales";
            query = @"CREATE PROCEDURE [dbo].[SP_DB_OrderTypeWiseSales]
	@FromDate date,
	@ToDate date,
    @LocationID INT ,
    @DeptID int ,
    @CustCatID int, --customer group
    @CompanyID int
AS 
    SET NOCOUNT ON
    SET XACT_ABORT ON

    BEGIN TRY

        DECLARE @reccount int = 0 ,@date date,@dateyear int,@datemonth int,@dateweek int,
        @MonthsCount int,@MonthNumber int,
        @WeeksCount int, @WeekNumber int,
        @strMonth varchar(50)
			
		IF OBJECT_ID('tempdb..#t0') IS NOT NULL 
		BEGIN
			DROP TABLE #t0
		END 
		CREATE TABLE #t0
		(OrderTypeID int,OrderType varchar(50),Nett decimal,Cost decimal)
			
	
				if @DeptID <>0
					begin--//
						if @CustCatID<>0
							begin
								if @LocationID=0
								begin
									insert into #t0 
									select cateringmoodid as OrderTypeID,cateringmoodname as OrderType,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recDATE between @FromDate and @ToDate and 
									customercategoryid=@CustCatID and RstDepartmentid=@DeptID
									and CompanyID=@CompanyID
									group by cateringmoodname,cateringmoodid
								end
								else
								begin
									insert into #t0 
									select cateringmoodid as OrderTypeID,cateringmoodname as OrderType,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recDATE between @FromDate and @ToDate and 
									locationID=@LocationID and customercategoryid=@CustCatID and RstDepartmentid=@DeptID
									group by cateringmoodname,cateringmoodid
								end
							end
						else
							begin
								if @LocationID=0
								begin
									insert into #t0 
									select cateringmoodid as OrderTypeID,cateringmoodname as OrderType,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recDATE between @FromDate and @ToDate and 
									RstDepartmentid=@DeptID and CompanyID=@CompanyID group by cateringmoodname,cateringmoodid
								end
								else
								begin
									insert into #t0 
									select cateringmoodid as OrderTypeID,cateringmoodname as OrderType,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recDATE between @FromDate and @ToDate and 
									locationID=@LocationID and RstDepartmentid=@DeptID group by cateringmoodname,cateringmoodid
								end
							end
					end--//
				else
					begin
						if @CustCatID<>0
							begin
								if @LocationID=0
								begin
									insert into #t0 
									select cateringmoodid as OrderTypeID,cateringmoodname as OrderType,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recDATE between @FromDate and @ToDate and 
									customercategoryid=@CustCatID and CompanyID=@CompanyID group by cateringmoodname,cateringmoodid
								end
								else
								begin
									insert into #t0 
									select cateringmoodid as OrderTypeID,cateringmoodname as OrderType,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recDATE between @FromDate and @ToDate and 
									locationID=@LocationID and customercategoryid=@CustCatID group by cateringmoodname,cateringmoodid
								end
							end
						else
							begin
							if @LocationID=0
								begin
									insert into #t0 
									select cateringmoodid as OrderTypeID,cateringmoodname as OrderType,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recDATE between @FromDate and @ToDate and CompanyID=@CompanyID group by cateringmoodname,cateringmoodid
								end	
							else
								begin
									insert into #t0 
									select cateringmoodid as OrderTypeID,cateringmoodname as OrderType,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recDATE between @FromDate and @ToDate	and locationID=@LocationID group by cateringmoodname,cateringmoodid
								end						
							end
					end
					
	
		
		SELECT OrderTypeID,OrderType,Nett,Cost  FROM  #t0
    END TRY
  
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            BEGIN
                --ROLLBACK TRANSACTION
                SELECT  ERROR_MESSAGE() AS Result

            END
        ELSE 
            BEGIN
                SELECT  ERROR_MESSAGE() AS Result
            END
    END CATCH ";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_DB_OrderTypeWiseSales

            #region SP_DB_OrderWiseTimeConsumption
            spName = "SP_DB_OrderWiseTimeConsumption";
            query = @"CREATE PROCEDURE [dbo].[SP_DB_OrderWiseTimeConsumption]
		@LocationID INT ,
		@FromDate date,
		@FromTime time,
		@ToDate date,
		@ToTime time ,
		@CateModeID int,
		@CompanyID int

	AS 
		SET NOCOUNT ON
		SET XACT_ABORT ON

		BEGIN TRY
			     
			DECLARE @ReceiptNo VARCHAR(50),
              @ZNo       INT,
              @RecDate   DATE,
              @StartTime TIME,
              @EndTime   TIME,
              @TimeDiff  INT,
              @strTime   VARCHAR(10)

              IF Object_id('tempdb..#t0') IS NOT NULL
                BEGIN
                    DROP TABLE #t0
                END

              CREATE TABLE #t0
                (
                   receipt VARCHAR(50),
                   value   INT,
                   zno     INT,
                   timeval VARCHAR(10)
                )

              IF Object_id('tempdb..#SalesData') IS NOT NULL
                BEGIN
                    DROP TABLE #salesdata
                END

              CREATE TABLE #salesdata
                (
                   recdate   DATE,
                   receipt   VARCHAR(50),
                   zno       INT,
                   starttime TIME,
                   endtime   TIME
                )

                    INSERT INTO #salesdata
                    EXEC SP_GetSalesData
                      @FromDate,
                      @ToDate,
                      @CateModeID,
                      @CompanyID

                    DECLARE cursr1 CURSOR FOR
                      SELECT DISTINCT recdate,
                                      receipt,
                                      zno
                      FROM   #salesdata

                    OPEN cursr1

                    FETCH next FROM cursr1 INTO @RecDate, @ReceiptNo, @ZNo

                    WHILE @@fetch_status = 0
                      BEGIN
                          SET @StartTime =(SELECT Min(starttime)
                                           FROM   #salesdata
                                           WHERE  receipt = @ReceiptNo
                                                  AND zno = @ZNo)
                          SET @EndTime=(SELECT Max(endtime)
                                        FROM   #salesdata
                                        WHERE  receipt = @ReceiptNo
                                               AND zno = @ZNo)
                          SET @TimeDiff = Datediff(mi, @StartTime, @EndTime)

                          IF @TimeDiff < 0
                            BEGIN
                                SET @TimeDiff=1440 + @TimeDiff
                            END

                          IF @TimeDiff < 61
                            BEGIN
                                SET @strTime= Cast(@TimeDiff AS VARCHAR(10)) + ' m'
                            END
                          ELSE
                            BEGIN
                                SET @strTime= Cast(@TimeDiff/60 AS VARCHAR) + ' h '
                                              + Cast(@TimeDiff%60 AS VARCHAR) + ' m'
                            END

                          IF @RecDate = @FromDate
                            BEGIN
                                IF @StartTime >= @FromTime
                                  BEGIN
                                      INSERT INTO #t0
                                      VALUES      (@ReceiptNo,
                                                   @TimeDiff,
                                                   @ZNo,
                                                   @strTime)
                                  END
                            END
                          ELSE IF @RecDate = @ToDate
                            BEGIN
                                IF @EndTime <= @ToTime
                                  BEGIN
                                      INSERT INTO #t0
                                      VALUES      (@ReceiptNo,
                                                   @TimeDiff,
                                                   @ZNo,
                                                   @strTime)
                                  END
                            END
                          ELSE
                            BEGIN
                                INSERT INTO #t0
                                VALUES      (@ReceiptNo,
                                             @TimeDiff,
                                             @ZNo,
                                             @strTime)
                            END

                          FETCH next FROM cursr1 INTO @RecDate, @ReceiptNo, @ZNo
                      END

                    CLOSE cursr1

                    DEALLOCATE cursr1

      
              SELECT receipt,
                     value,
                     zno,
                     timeval
              FROM   #t0

		END TRY
	  
		BEGIN CATCH
			IF @@TRANCOUNT > 0 
				BEGIN
					--ROLLBACK TRANSACTION
					SELECT  ERROR_MESSAGE() AS Result


				END
			ELSE 
				BEGIN
					SELECT  ERROR_MESSAGE() AS Result

				END
		END CATCH";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_DB_OrderWiseTimeConsumption

            #region SP_DB_PRNProductDetailsEstimate
            spName = "SP_DB_PRNProductDetailsEstimate";
            query = @"CREATE PROCEDURE [dbo].[SP_DB_PRNProductDetailsEstimate]
	@FromDate date,
	@ToDate date,
	@CompanyID int ,
    @LocationID INT ,
    @IsDeptWise int,
    @SupplierID  int

AS 
    SET NOCOUNT ON
    SET XACT_ABORT ON

    BEGIN TRY
    declare @date date
    
    IF OBJECT_ID('tempdb..#t0') IS NOT NULL 
				BEGIN
					DROP TABLE #t0
				END 
				
				CREATE TABLE #t0
				(
				  PRNcode varchar(50),
				  Value [decimal](18, 2) NOT NULL 
				)
    
		if @SupplierID=0
			begin
				
				
				set @date = convert(date,@FromDate)
		
				while (convert(date,@date) <= convert(date,@ToDate)) 
				begin
					if @LocationID=0
					begin
						insert into #t0 
						select ph.DocumentNo as PRNcode,
						ph.TotCostPrice as Value 
						from PurchaseHeaders ph
						where convert(date,ph.DocumentDate) =CONVERT(date,@date) and ph.CompanyID=@CompanyID
						and ph.DocumentID = 6 --and ph.IsGRN = 1 --and ph.DocumentStatus = 3
						
					end
					else
					begin
						insert into #t0 
						select ph.DocumentNo as PRNcode,
						ph.TotCostPrice as Value 
						from PurchaseHeaders ph
						where convert(date,ph.DocumentDate) =CONVERT(date,@date) and ph.CompanyID=@CompanyID
						and ph.LocationId = @LocationID and ph.DocumentID = 6 --and ph.IsGRN = 1 --and ph.DocumentStatus = 3
						
					end
					set @date = dateadd(DAY, 1, @date)
        		end
			SELECT PRNcode,Value  FROM  #t0
			
		end
		else 
		begin
				
			
				set @date = convert(date,@FromDate)
		
				while (convert(date,@date) <= convert(date,@ToDate))
				begin
					if @LocationID=0
					begin
						insert into #t0 
						select ph.DocumentNo as PRNcode,
						ph.TotCostPrice as Value 
						from PurchaseHeaders ph
						where convert(date,ph.DocumentDate) =CONVERT(date,@date) and ph.CompanyID=@CompanyID and ph.SupplierID = @SupplierID
						and ph.DocumentID = 6 --and ph.IsGRN = 1 --and ph.DocumentStatus = 3
					end
					else
					begin
						insert into #t0 
						select ph.DocumentNo as PRNcode,
						ph.TotCostPrice as Value 
						from PurchaseHeaders ph
						where convert(date,ph.DocumentDate) =CONVERT(date,@date) and ph.CompanyID=@CompanyID and ph.SupplierID = @SupplierID
						and ph.LocationId = @LocationID and ph.DocumentID = 6 --and ph.IsGRN = 1 --and ph.DocumentStatus = 3
						
					end
					set @date = dateadd(DAY, 1, @date)
        		end
			SELECT PRNcode,Value  FROM  #t0
		end
    END TRY
  
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            BEGIN
                --ROLLBACK TRANSACTION
                SELECT  ERROR_MESSAGE() AS Result

            END
        ELSE 
            BEGIN
                SELECT  ERROR_MESSAGE() AS Result
            END
    END CATCH";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_DB_PRNProductDetailsEstimate

            #region SP_DB_ProductGroupWisePriceChangeDetails
            spName = "SP_DB_ProductGroupWisePriceChangeDetails";
            query = @"CREATE PROCEDURE [dbo].[SP_DB_ProductGroupWisePriceChangeDetails]
	@FromDate date,
	@ToDate date,
    @LocationID INT ,
    @ChartType int, -- daily/weekly/monthly
    @CompanyID int,
    @ProductGroupId int
AS 
    SET NOCOUNT ON
    SET XACT_ABORT ON

    BEGIN TRY

        DECLARE @reccount int = 0 ,@date date,@dateyear int,@datemonth int,@dateweek int,
        @MonthsCount int,@MonthNumber int,
        @WeeksCount int, @WeekNumber int,
        @strMonth varchar(50)
			
		IF OBJECT_ID('tempdb..#t0') IS NOT NULL 
		BEGIN
			DROP TABLE #t0
		END 
		CREATE TABLE #t0
		(recday varchar(50),LocationId int ,ItemId int,ItemCode varchar(50),ItemName varchar(50),Cost decimal)
			
		if @ChartType=1 --daily
		begin
			set @date = @FromDate
		
			while (@date <= @ToDate) 
			begin
				insert into #t0 
				select @date as recday,@LocationID as LocationId,
				lp.ProductId as ItemId,lp.ProductCode as ItemCode, lp.ProductName as ItemName,lp.CostPrice as Cost
				from ProductGroupHeaders pgh inner join ProductGroupDetails pgd on pgh.ProductGroupHeaderID = pgd.ProductGroupHeaderID
				inner join LOGProductStockMasters lp on pgd.ProductCode = lp.ProductCode
				where cast(lp.ModifiedDate as date)=@date and lp.locationID=@LocationID
				and lp.CompanyID=@CompanyID and pgh.ProductGroupHeaderID = @ProductGroupId
				and pgd.IsActive = 1 and pgd.IsDelete = 0 
				and pgh.IsActive = 1 and pgh.IsDelete = 0 
				set @date = dateadd(DAY, 1, @date)
			end 
			
		end
		if @ChartType=2  -- weekly
		begin
			set @date = @FromDate
			set @dateweek =(Select DatePart(week, @FromDate))
			set @dateyear = year(@FromDate)
			set @WeeksCount=(select ceiling(convert(float, abs(datediff(day, @FromDate,@ToDate))) / 7))+1
			set @WeekNumber=1
			
			while (@WeekNumber<=@WeeksCount ) 
			begin--@@

			set @dateweek=(Select DatePart(week, @date))
			
			insert into #t0 
			select @WeekNumber as recday,@LocationID as LocationId,
			lp.ProductId as ItemId,lp.ProductCode as ItemCode, lp.ProductName as ItemName,lp.CostPrice as Cost
			from ProductGroupHeaders pgh inner join ProductGroupDetails pgd on pgh.ProductGroupHeaderID = pgd.ProductGroupHeaderID
			inner join LOGProductStockMasters lp on pgd.ProductCode = lp.ProductCode
			where YEAR(cast(lp.ModifiedDate as date))=@dateyear 
			and DATEPART(week, cast(lp.ModifiedDate as date))=@dateweek 
			and cast(lp.ModifiedDate as date) between @FromDate and @ToDate
			and lp.locationID=@LocationID
			and lp.CompanyID=@CompanyID
			and pgh.ProductGroupHeaderID = @ProductGroupId
			and pgd.IsActive = 1 and pgd.IsDelete = 0 
			and pgh.IsActive = 1 and pgh.IsDelete = 0 
			set @date = dateadd(DAY, 7, @date)
			set @WeekNumber=@WeekNumber+1
			
			set @dateyear = year(@date)

			end--@@
		end
		
		if @ChartType=3 --monthly
		begin
			set @date = @FromDate
			set @dateyear = year(@FromDate)
			set @datemonth = month(@FromDate)
			
			set @strMonth=DATENAME(month, @FromDate)
			set @MonthsCount=(SELECT DATEDIFF(mm, @FromDate, @ToDate) +1)
			set @MonthNumber=1
			
			while (@MonthNumber<=@MonthsCount ) 
			begin

			set @strMonth=DATENAME(month, @date)
			
			insert into #t0 
			select @WeekNumber as recday,@LocationID as LocationId,
			lp.ProductId as ItemId,lp.ProductCode as ItemCode, lp.ProductName as ItemName,lp.CostPrice as Cost
			from ProductGroupHeaders pgh inner join ProductGroupDetails pgd on pgh.ProductGroupHeaderID = pgd.ProductGroupHeaderID
			inner join LOGProductStockMasters lp on pgd.ProductCode = lp.ProductCode
			WHERE YEAR(cast(lp.ModifiedDate as date))=@dateyear 
			and MONTH(cast(lp.ModifiedDate as date))=@datemonth 
			and lp.locationID=@LocationID
			and lp.CompanyID=@CompanyID
			and pgh.ProductGroupHeaderID = @ProductGroupId
			and pgd.IsActive = 1 and pgd.IsDelete = 0 
			and pgh.IsActive = 1 and pgh.IsDelete = 0 
			
			set @date = dateadd(MONTH, 1, @date)
			set @MonthNumber=@MonthNumber+1
			
			set @dateyear = year(@date)
			set @datemonth = month(@date)
			end

		end
		
		SELECT LocationId,ItemId,ItemCode,ItemName,Cost,recday  FROM  #t0
    END TRY
  
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            BEGIN
                --ROLLBACK TRANSACTION
                SELECT  ERROR_MESSAGE() AS Result

            END
        ELSE 
            BEGIN
                SELECT  ERROR_MESSAGE() AS Result
            END
    END CATCH";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_DB_ProductGroupWisePriceChangeDetails

            #region SP_DB_ProductWiseSalesByDept
            spName = "SP_DB_ProductWiseSalesByDept";
            query = @"CREATE PROCEDURE [dbo].[SP_DB_ProductWiseSalesByDept]
	@FromDate date,
	@ToDate date,
    @LocationID INT ,
    @DeptID int ,
    @CustCatID int --customer group

AS 
    SET NOCOUNT ON
    SET XACT_ABORT ON

    BEGIN TRY

        DECLARE @reccount int = 0 ,@date date,@dateyear int,@datemonth int,@dateweek int,
        @MonthsCount int,@MonthNumber int,
        @WeeksCount int, @WeekNumber int,
        @strMonth varchar(50)
			
		IF OBJECT_ID('tempdb..#t0') IS NOT NULL 
		BEGIN
			DROP TABLE #t0
		END 
		CREATE TABLE #t0
		(DeptID int,DeptName varchar(100),ProductCode varchar(20) ,ProductName varchar(100),Nett decimal,Cost decimal)
			
			if @CustCatID<>0
				begin
					if @LocationID=0
					begin
						insert into #t0 
						select rstdepartmentid as DeptID,departmentname as DeptName,
						  productcode as ProductCode,descrip as ProductName,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
						case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
						where recDATE between @FromDate and @ToDate and 
						customercategoryid=@CustCatID 
						--and RstDepartmentid=@DeptID
						group by rstdepartmentid,departmentname,productcode,descrip
					end
					else
					begin
						insert into #t0 
						select rstdepartmentid as DeptID,departmentname as DeptName,
						  productcode as ProductCode,descrip as ProductName,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
						case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
						where recDATE between @FromDate and @ToDate and 
						locationID=@LocationID and customercategoryid=@CustCatID 
						--and RstDepartmentid=@DeptID
						group by rstdepartmentid,departmentname,productcode,descrip
					end
				end
			else
				begin
					if @LocationID=0
					begin
						insert into #t0 
						select rstdepartmentid as DeptID,departmentname as DeptName,
						  productcode as ProductCode,descrip as ProductName,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
						case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
						where recDATE between @FromDate and @ToDate 
						--and 
						--RstDepartmentid=@DeptID 
						group by rstdepartmentid,departmentname,productcode,descrip
					end
					else
					begin
						insert into #t0 
						select rstdepartmentid as DeptID,departmentname as DeptName,
						  productcode as ProductCode,descrip as ProductName,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
						case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
						where recDATE between @FromDate and @ToDate and 
						locationID=@LocationID 
						--and RstDepartmentid=@DeptID 
						group by rstdepartmentid,departmentname,productcode,descrip
					end
				end

		SELECT DeptID,DeptName,ProductCode,ProductName,Nett,Cost  FROM  #t0
    END TRY
  
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            BEGIN
                --ROLLBACK TRANSACTION
                SELECT  ERROR_MESSAGE() AS Result

            END
        ELSE 
            BEGIN
                SELECT  ERROR_MESSAGE() AS Result
            END
    END CATCH ";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_DB_ProductWiseSalesByDept

            #region SP_DB_SalesNRevenue
            spName = "SP_DB_SalesNRevenue";
            query = @"CREATE PROCEDURE [dbo].[SP_DB_SalesNRevenue]
	@FromDate date,
	@ToDate date,
    @LocationID INT ,
    @ChartType int, -- daily/weekly/monthly
    @DeptID int,
    @CustCatID int, --customer group
    @CompanyID int
AS 
    SET NOCOUNT ON
    SET XACT_ABORT ON

    BEGIN TRY

        DECLARE @reccount int = 0 ,@date date,@dateyear int,@datemonth int,@dateweek int,
        @MonthsCount int,@MonthNumber int,
        @WeeksCount int, @WeekNumber int,
        @strMonth varchar(50)
			
		IF OBJECT_ID('tempdb..#t0') IS NOT NULL 
		BEGIN
			DROP TABLE #t0
		END 
		CREATE TABLE #t0
		(recdate varchar(50),Nett decimal,Cost decimal)
			
		if @ChartType=1 --daily
		begin
			set @date = @FromDate
		
			while (@date <= @ToDate) 
			begin
				if @DeptID <>0
					begin--//
						if @CustCatID<>0
							begin
								if @LocationID=0
								begin
									insert into #t0 
									select @date recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales where recDATE=@date and 
									customercategoryid=@CustCatID and RstDepartmentid=@DeptID and CompanyID=@CompanyID
								end
								else
								begin
									insert into #t0 
									select @date recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales where recDATE=@date and 
									locationID=@LocationID and customercategoryid=@CustCatID and RstDepartmentid=@DeptID
								end
							end
						else
							begin
								if @LocationID=0
								begin
									insert into #t0 
									select @date recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales where recDATE=@date and 
									RstDepartmentid=@DeptID and CompanyID=@CompanyID
								end
								else
								begin
									insert into #t0 
									select @date recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales where recDATE=@date and 
									locationID=@LocationID and RstDepartmentid=@DeptID
								end
							end
					end--//
				else
					begin
						if @CustCatID<>0
							begin
								if @LocationID=0
								begin
									insert into #t0 
									select @date recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales where recDATE=@date and 
									customercategoryid=@CustCatID  and CompanyID=@CompanyID
								end
								else
								begin
									insert into #t0 
									select @date recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales where recDATE=@date and 
									locationID=@LocationID and customercategoryid=@CustCatID 
								end
							end
						else
							begin
							if @LocationID=0
								begin
									insert into #t0 
									select @date recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recDATE=@date and CompanyID=@CompanyID
								end	
							else
								begin
									insert into #t0 
									select @date recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recDATE=@date	and locationID=@LocationID 
								end						
							end
					end
					
				set @date = dateadd(DAY, 1, @date)
			end 
			
		end
		if @ChartType=2  -- weekly
		begin
			set @date = @FromDate
			set @dateweek =(Select DatePart(week, @FromDate))
			set @dateyear = year(@FromDate)
			set @WeeksCount=(select ceiling(convert(float, abs(datediff(day, @FromDate,@ToDate))) / 7))+1
			set @WeekNumber=1
			
			while (@WeekNumber<=@WeeksCount ) 
			begin--@@

			set @dateweek=(Select DatePart(week, @date))
			if @DeptID <>0
					begin--//
						if @CustCatID<>0
							begin
								if @LocationID=0
								begin
									insert into #t0 
									select @WeekNumber recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recyear=@dateyear and recweek=@dateweek and recdate between @FromDate and @ToDate and
									customercategoryid=@CustCatID and RstDepartmentid=@DeptID and CompanyID=@CompanyID
								end
								else
									begin
									insert into #t0 
									select @WeekNumber recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recyear=@dateyear and recweek=@dateweek and recdate between @FromDate and @ToDate and
									locationID=@LocationID and customercategoryid=@CustCatID and RstDepartmentid=@DeptID
								end
							end
						else
							begin
								if @LocationID=0
								begin
									insert into #t0 
									select @WeekNumber recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recyear=@dateyear and recweek=@dateweek and recdate between @FromDate and @ToDate and
									RstDepartmentid=@DeptID and CompanyID=@CompanyID
								end
								else
								begin
									insert into #t0 
									select @WeekNumber recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recyear=@dateyear and recweek=@dateweek and recdate between @FromDate and @ToDate and
									locationID=@LocationID and RstDepartmentid=@DeptID
								end
							end
					end--//
				else
					begin--**
						if @CustCatID<>0
							begin
								if @LocationID=0
								begin
									insert into #t0 
									select @WeekNumber recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recyear=@dateyear and recweek=@dateweek and recdate between @FromDate and @ToDate and
									customercategoryid=@CustCatID and CompanyID=@CompanyID 
								end
								else
								begin
									insert into #t0 
									select @WeekNumber recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recyear=@dateyear and recweek=@dateweek and recdate between @FromDate and @ToDate and
									locationID=@LocationID and customercategoryid=@CustCatID 
								end
							end
						else
							begin
								if @LocationID=0
								begin
									insert into #t0 
									select @WeekNumber recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where 
									recyear=@dateyear and recweek=@dateweek	and recdate between @FromDate and @ToDate
									 and CompanyID=@CompanyID
								end	
								else
								begin
									insert into #t0 
									select @WeekNumber recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where 
									recyear=@dateyear and recweek=@dateweek	and recdate between @FromDate and @ToDate and
									locationID=@LocationID
								end							
							end
					end--**
			
			set @date = dateadd(DAY, 7, @date)
			set @WeekNumber=@WeekNumber+1
			
			set @dateyear = year(@date)

			end--@@
		end
		
		if @ChartType=3 --monthly
		begin
			set @date = @FromDate
			set @dateyear = year(@FromDate)
			set @datemonth = month(@FromDate)
			
			set @strMonth=DATENAME(month, @FromDate)
			set @MonthsCount=(SELECT DATEDIFF(mm, @FromDate, @ToDate) +1)
			set @MonthNumber=1
			
			while (@MonthNumber<=@MonthsCount ) 
			begin

			set @strMonth=DATENAME(month, @date)
			if @DeptID <>0
					begin--//
						if @CustCatID<>0
							begin
								if @LocationID=0
								begin
									insert into #t0 
									select @strMonth recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recyear=@dateyear and recmonth=@datemonth and 
									customercategoryid=@CustCatID and RstDepartmentid=@DeptID and CompanyID=@CompanyID
								end
								else
								begin
									insert into #t0 
									select @strMonth recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recyear=@dateyear and recmonth=@datemonth and 
									locationID=@LocationID and customercategoryid=@CustCatID and RstDepartmentid=@DeptID
								end

							end
						else
							begin
								if @LocationID=0
								begin
									insert into #t0 
									select @strMonth recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recyear=@dateyear and recmonth=@datemonth and 
									RstDepartmentid=@DeptID and CompanyID=@CompanyID
								end
								else
								begin
									insert into #t0 
									select @strMonth recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recyear=@dateyear and recmonth=@datemonth and 
									locationID=@LocationID and RstDepartmentid=@DeptID
								end
		
							end
					end--//
				else
					begin--**
						if @CustCatID<>0
							begin
								if @LocationID=0
								begin
									insert into #t0 
									select @strMonth recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recyear=@dateyear and recmonth=@datemonth and 
									customercategoryid=@CustCatID  and CompanyID=@CompanyID
								end
								else
								begin
									insert into #t0 
									select @strMonth recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recyear=@dateyear and recmonth=@datemonth and 
									locationID=@LocationID and customercategoryid=@CustCatID 
								end
			
							end
						else
							begin
								if @LocationID=0
								begin
									insert into #t0 
									select @strMonth recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recyear=@dateyear and recmonth=@datemonth and CompanyID=@CompanyID
								end
								else
								begin
									insert into #t0 
									select @strMonth recdate,case when SUM(nett) IS null then 0 else SUM(nett)end Nett,
									case when SUM(cost) IS null then 0 else SUM(cost) end Cost from view_sales 
									where recyear=@dateyear and recmonth=@datemonth	and 
									locationID=@LocationID 	
								end						
							end
					end--**
			
			set @date = dateadd(MONTH, 1, @date)
			set @MonthNumber=@MonthNumber+1
			
			set @dateyear = year(@date)
			set @datemonth = month(@date)
			end

		end
		
		SELECT recdate,Nett,Cost  FROM  #t0
    END TRY
  
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            BEGIN
                --ROLLBACK TRANSACTION
                SELECT  ERROR_MESSAGE() AS Result

            END
        ELSE 
            BEGIN
                SELECT  ERROR_MESSAGE() AS Result
            END
    END CATCH ";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_DB_SalesNRevenue

            #region SP_DB_StockReOrderLevelDetailsEstimate
            spName = "SP_DB_StockReOrderLevelDetailsEstimate";
            query = @"CREATE PROCEDURE [dbo].[SP_DB_StockReOrderLevelDetailsEstimate]
	@FromDate date,
	@ToDate date,
	@CompanyID int ,
    @LocationID INT ,
    @IsDeptWise int,
    @ProductId  int

AS 
    SET NOCOUNT ON
    SET XACT_ABORT ON

    BEGIN TRY
    declare @date date
    
    IF OBJECT_ID('tempdb..#t0') IS NOT NULL 
				BEGIN
					DROP TABLE #t0
				END 
				
				CREATE TABLE #t0
				(
				  ProductCode varchar(50),
				  ProductName varchar(50),
				  Stock [decimal](18, 2) NOT NULL,
				  ReOrderLevel [decimal](18, 2) NOT NULL
				)
    
		if @ProductId=0
			begin
				insert into #t0 
						select p.ProductCode as ProductCode,
						p.ProductName as ProductName,
						ps.Stock as Stock,
						ps.ReOrderLevel as ReOrderLevel
						from Products p inner join ProductStockMasters ps on p.ProductId = ps.ProductId
						where  p.CompanyID=@CompanyID
						and ps.LocationId = @LocationID AND P.FastMovingGoods = 1
						
			SELECT ProductCode,ProductName,Stock,ReOrderLevel  FROM  #t0
			
		end
		else 
		begin
				
			insert into #t0 
						select p.ProductCode as ProductCode,
						p.ProductName as ProductName,
						ps.Stock as Stock,
						ps.ReOrderLevel as ReOrderLevel
						from Products p inner join ProductStockMasters ps on p.ProductId = ps.ProductId
						where  p.CompanyID=@CompanyID 
						and p.ProductId = @ProductId and ps.LocationId = @LocationID
						AND P.FastMovingGoods = 1
						
			SELECT ProductCode,ProductName,Stock,ReOrderLevel  FROM  #t0
		end
    END TRY
  
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            BEGIN
                --ROLLBACK TRANSACTION
                SELECT  ERROR_MESSAGE() AS Result

            END
        ELSE 
            BEGIN
                SELECT  ERROR_MESSAGE() AS Result
            END
    END CATCH ";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_DB_StockReOrderLevelDetailsEstimate

            #region SP_DB_WastageDetail
            spName = "SP_DB_WastageDetail";
            query = @"CREATE PROCEDURE [dbo].[SP_DB_WastageDetail]
	@FromDate date,
	@ToDate date,
    @LocationID INT ,
    @ChartType int, -- daily/weekly/monthly
	@OrderType int, -- 0-All / 1-KOT / 2-BOT / 3-NONE
	@CompanyID int

AS 
    SET NOCOUNT ON
    SET XACT_ABORT ON

    BEGIN TRY

        DECLARE @reccount int = 0 ,@date date,@dateyear int,@datemonth int,@dateweek int,
        @MonthsCount int,@MonthNumber int,
        @WeeksCount int, @WeekNumber int,
        @strMonth varchar(50)
			
		IF OBJECT_ID('tempdb..#t0') IS NOT NULL 
		BEGIN
			DROP TABLE #t0
		END 
		CREATE TABLE #t0
		(recdate varchar(50),ProductCode varchar(20),Product varchar(100),Nett decimal)
			
		if @ChartType=1 --daily
		begin
			set @date = @FromDate
		
			while (@date <= @ToDate) 
			begin
				if @OrderType=0  --all order types
				begin
					if @LocationID=0
					begin
						insert into #t0 
						SELECT @date recdate, Products.ProductCode as ProductCode, Products.ProductName as Product
						,case when SUM(AdjustStock * StockAdjustmentDetails.CostPrice) IS null then 0 else SUM(AdjustStock * StockAdjustmentDetails.CostPrice)end Nett 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						WHERE cast(StockAdjustmentHeaders.CreatedDate as date)=@date 
						and (StockAdjustmentDetails.Reason = N'Wastage')
						and StockAdjustmentHeaders.CompanyID=@CompanyID
						GROUP BY Products.ProductCode, Products.ProductName
					end
					else
					begin
						insert into #t0 
						SELECT @date recdate, Products.ProductCode as ProductCode, Products.ProductName as Product,case when SUM(AdjustStock * StockAdjustmentDetails.CostPrice) IS null then 0 else SUM(AdjustStock * StockAdjustmentDetails.CostPrice)end Nett 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						WHERE cast(StockAdjustmentHeaders.CreatedDate as date)=@date 
						and StockLocationId=@LocationID and (StockAdjustmentDetails.Reason = N'Wastage')
						GROUP BY Products.ProductCode, Products.ProductName
					end
				end
				else
				begin
					if @LocationID=0
					begin
						insert into #t0 
						SELECT @date recdate, Products.ProductCode as ProductCode, Products.ProductName as Product,case when SUM(AdjustStock * StockAdjustmentDetails.CostPrice) IS null then 0 else SUM(AdjustStock * StockAdjustmentDetails.CostPrice)end Nett 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						WHERE cast(StockAdjustmentHeaders.CreatedDate as date)=@date 
						and PrinterTypeId=@OrderType and (StockAdjustmentDetails.Reason = N'Wastage')
						and StockAdjustmentHeaders.CompanyID=@CompanyID
						GROUP BY Products.ProductCode, Products.ProductName
					end	
					else
					begin
						insert into #t0 
						SELECT @date recdate, Products.ProductCode as ProductCode, Products.ProductName as Product,case when SUM(AdjustStock * StockAdjustmentDetails.CostPrice) IS null then 0 else SUM(AdjustStock * StockAdjustmentDetails.CostPrice)end Nett 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						WHERE cast(StockAdjustmentHeaders.CreatedDate as date)=@date 
						and PrinterTypeId=@OrderType and StockLocationId=@LocationID and (StockAdjustmentDetails.Reason = N'Wastage')
						GROUP BY Products.ProductCode, Products.ProductName
					end						
				end		
				set @date = dateadd(DAY, 1, @date)
			end 
			
		end
		if @ChartType=2  -- weekly
		begin
			set @date = @FromDate
			set @dateweek =(Select DatePart(week, @FromDate))
			set @dateyear = year(@FromDate)
			set @WeeksCount=(select ceiling(convert(float, abs(datediff(day, @FromDate,@ToDate))) / 7))+1
			set @WeekNumber=1
			
			while (@WeekNumber<=@WeeksCount ) 
			begin--@@

			set @dateweek=(Select DatePart(week, @date))
			if @OrderType=0  --all order types
				begin
					if @LocationID=0
					begin
						insert into #t0 
						SELECT @WeekNumber recdate, Products.ProductCode as ProductCode, Products.ProductName as Product,case when SUM(AdjustStock * StockAdjustmentDetails.CostPrice) IS null then 0 else SUM(AdjustStock * StockAdjustmentDetails.CostPrice)end Nett 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						WHERE 
						YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and DATEPART(week, cast(StockAdjustmentHeaders.CreatedDate as date))=@dateweek 
						and cast(StockAdjustmentHeaders.CreatedDate as date) between @FromDate and @ToDate
						and (StockAdjustmentDetails.Reason = N'Wastage')
						and StockAdjustmentHeaders.CompanyID=@CompanyID
						GROUP BY Products.ProductCode, Products.ProductName
					end
					else
					begin
						insert into #t0 
						SELECT @WeekNumber recdate, Products.ProductCode as ProductCode, Products.ProductName as Product,case when SUM(AdjustStock * StockAdjustmentDetails.CostPrice) IS null then 0 else SUM(AdjustStock * StockAdjustmentDetails.CostPrice)end Nett 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						WHERE YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and DATEPART(week, cast(StockAdjustmentHeaders.CreatedDate as date))=@dateweek 
						and cast(StockAdjustmentHeaders.CreatedDate as date) between @FromDate and @ToDate 
						and StockLocationId=@LocationID and (StockAdjustmentDetails.Reason = N'Wastage')
						GROUP BY Products.ProductCode, Products.ProductName
					end
				end
				else
				begin
					if @LocationID=0
					begin
						insert into #t0 
						SELECT @WeekNumber recdate, Products.ProductCode as ProductCode, Products.ProductName as Product,case when SUM(AdjustStock * StockAdjustmentDetails.CostPrice) IS null then 0 else SUM(AdjustStock * StockAdjustmentDetails.CostPrice)end Nett 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						WHERE YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and DATEPART(week, cast(StockAdjustmentHeaders.CreatedDate as date))=@dateweek 
						and cast(StockAdjustmentHeaders.CreatedDate as date) between @FromDate and @ToDate 
						and PrinterTypeId=@OrderType and (StockAdjustmentDetails.Reason = N'Wastage')
						and StockAdjustmentHeaders.CompanyID=@CompanyID
						GROUP BY Products.ProductCode, Products.ProductName
					end	
					else
					begin
						insert into #t0 
						SELECT @WeekNumber recdate, Products.ProductCode as ProductCode, Products.ProductName as Product,case when SUM(AdjustStock * StockAdjustmentDetails.CostPrice) IS null then 0 else SUM(AdjustStock * StockAdjustmentDetails.CostPrice)end Nett 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						WHERE YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and DATEPART(week, cast(StockAdjustmentHeaders.CreatedDate as date))=@dateweek 
						and cast(StockAdjustmentHeaders.CreatedDate as date) between @FromDate and @ToDate 
						and PrinterTypeId=@OrderType and StockLocationId=@LocationID and (StockAdjustmentDetails.Reason = N'Wastage')
						GROUP BY Products.ProductCode, Products.ProductName
					end						
				end	

			
			set @date = dateadd(DAY, 7, @date)
			set @WeekNumber=@WeekNumber+1
			
			set @dateyear = year(@date)

			end--@@
		end
		
		if @ChartType=3 --monthly
		begin
			set @date = @FromDate
			set @dateyear = year(@FromDate)
			set @datemonth = month(@FromDate)
			
			set @strMonth=DATENAME(month, @FromDate)
			set @MonthsCount=(SELECT DATEDIFF(mm, @FromDate, @ToDate) +1)
			set @MonthNumber=1
			
			while (@MonthNumber<=@MonthsCount ) 
			begin

			set @strMonth=DATENAME(month, @date)
				if @OrderType=0  --all order types
				begin
					if @LocationID=0
					begin
						insert into #t0 
						SELECT @strMonth recdate, Products.ProductCode as ProductCode, Products.ProductName as Product,case when SUM(AdjustStock * StockAdjustmentDetails.CostPrice) IS null then 0 else SUM(AdjustStock * StockAdjustmentDetails.CostPrice)end Nett 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						WHERE 
						YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and MONTH(cast(StockAdjustmentHeaders.CreatedDate as date))=@datemonth
						and (StockAdjustmentDetails.Reason = N'Wastage')
						and StockAdjustmentHeaders.CompanyID=@CompanyID
						GROUP BY Products.ProductCode, Products.ProductName
					end
					else
					begin
						insert into #t0 
						SELECT @strMonth recdate, Products.ProductCode as ProductCode, Products.ProductName as Product,case when SUM(AdjustStock * StockAdjustmentDetails.CostPrice) IS null then 0 else SUM(AdjustStock * StockAdjustmentDetails.CostPrice)end Nett 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						WHERE YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and MONTH(cast(StockAdjustmentHeaders.CreatedDate as date))=@datemonth 
						and StockLocationId=@LocationID and (StockAdjustmentDetails.Reason = N'Wastage')
						GROUP BY Products.ProductCode, Products.ProductName
					end
				end
				else
				begin
					if @LocationID=0
					begin
						insert into #t0 
						SELECT @strMonth recdate, Products.ProductCode as ProductCode, Products.ProductName as Product,case when SUM(AdjustStock * StockAdjustmentDetails.CostPrice) IS null then 0 else SUM(AdjustStock * StockAdjustmentDetails.CostPrice)end Nett 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						WHERE YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and MONTH(cast(StockAdjustmentHeaders.CreatedDate as date))=@datemonth 
						and PrinterTypeId=@OrderType and (StockAdjustmentDetails.Reason = N'Wastage')
						and StockAdjustmentHeaders.CompanyID=@CompanyID
						GROUP BY Products.ProductCode, Products.ProductName
					end	
					else
					begin
						insert into #t0 
						SELECT @strMonth recdate, Products.ProductCode as ProductCode, Products.ProductName as Product,case when SUM(AdjustStock * StockAdjustmentDetails.CostPrice) IS null then 0 else SUM(AdjustStock * StockAdjustmentDetails.CostPrice)end Nett 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						WHERE YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and MONTH(cast(StockAdjustmentHeaders.CreatedDate as date))=@datemonth 
						and PrinterTypeId=@OrderType and StockLocationId=@LocationID and (StockAdjustmentDetails.Reason = N'Wastage')
						GROUP BY Products.ProductCode, Products.ProductName
					end						
				end	
			
			set @date = dateadd(MONTH, 1, @date)
			set @MonthNumber=@MonthNumber+1
			
			set @dateyear = year(@date)
			set @datemonth = month(@date)
			end

		end
		
		SELECT recdate,ProductCode,Product,Nett  FROM  #t0
    END TRY
  
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            BEGIN
                --ROLLBACK TRANSACTION
                SELECT  ERROR_MESSAGE() AS Result

            END
        ELSE 
            BEGIN
                SELECT  ERROR_MESSAGE() AS Result
            END
    END CATCH";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_DB_WastageDetail

            #region SP_DB_WastageSummary
            spName = "SP_DB_WastageSummary";
            query = @"CREATE PROCEDURE [dbo].[SP_DB_WastageSummary]
	@FromDate date,
	@ToDate date,
    @LocationID INT ,
    @ChartType int, -- daily/weekly/monthly
	@OrderType int, -- 0-All / 1-KOT / 2-BOT / 3-NONE
	@CompanyID int

AS 
    SET NOCOUNT ON
    SET XACT_ABORT ON

    BEGIN TRY

        DECLARE @reccount int = 0 ,@date date,@dateyear int,@datemonth int,@dateweek int,
        @MonthsCount int,@MonthNumber int,
        @WeeksCount int, @WeekNumber int,
        @strMonth varchar(50)
			
		IF OBJECT_ID('tempdb..#t0') IS NOT NULL 
		BEGIN
			DROP TABLE #t0
		END 
		CREATE TABLE #t0
		(recdate varchar(50),Nett decimal)
			
		if @ChartType=1 --daily
		begin
			set @date = @FromDate
		
			while (@date <= @ToDate) 
			begin
				if @OrderType=0  --all order types
				begin
					if @LocationID=0
					begin
						insert into #t0 
						SELECT @date recdate,case when SUM(AdjustStock * StockAdjustmentDetails.CostPrice) IS null then 0 else SUM(AdjustStock * StockAdjustmentDetails.CostPrice)end Nett 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						WHERE cast(StockAdjustmentHeaders.CreatedDate as date)=@date 
						and (StockAdjustmentDetails.Reason = N'Wastage') and StockAdjustmentHeaders.CompanyID=@CompanyID
					end
					else
					begin
						insert into #t0 
						SELECT @date recdate,case when SUM(AdjustStock * StockAdjustmentDetails.CostPrice) IS null then 0 else SUM(AdjustStock * StockAdjustmentDetails.CostPrice)end Nett 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						WHERE cast(StockAdjustmentHeaders.CreatedDate as date)=@date 
						and StockLocationId=@LocationID and (StockAdjustmentDetails.Reason = N'Wastage')
					end
				end
				else
				begin
					if @LocationID=0
					begin
						insert into #t0 
						SELECT @date recdate,case when SUM(AdjustStock * StockAdjustmentDetails.CostPrice) IS null then 0 else SUM(AdjustStock * StockAdjustmentDetails.CostPrice)end Nett 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						WHERE cast(StockAdjustmentHeaders.CreatedDate as date)=@date 
						and PrinterTypeId=@OrderType and (StockAdjustmentDetails.Reason = N'Wastage')
						 and StockAdjustmentHeaders.CompanyID=@CompanyID
					end	
					else
					begin
						insert into #t0 
						SELECT @date recdate,case when SUM(AdjustStock * StockAdjustmentDetails.CostPrice) IS null then 0 else SUM(AdjustStock * StockAdjustmentDetails.CostPrice)end Nett 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						WHERE cast(StockAdjustmentHeaders.CreatedDate as date)=@date 
						and PrinterTypeId=@OrderType and StockLocationId=@LocationID and (StockAdjustmentDetails.Reason = N'Wastage')
					end						
				end		
				set @date = dateadd(DAY, 1, @date)
			end 
			
		end
		if @ChartType=2  -- weekly
		begin
			set @date = @FromDate
			set @dateweek =(Select DatePart(week, @FromDate))
			set @dateyear = year(@FromDate)
			set @WeeksCount=(select ceiling(convert(float, abs(datediff(day, @FromDate,@ToDate))) / 7))+1
			set @WeekNumber=1
			
			while (@WeekNumber<=@WeeksCount ) 
			begin--@@

			set @dateweek=(Select DatePart(week, @date))
			if @OrderType=0  --all order types
				begin
					if @LocationID=0
					begin
						insert into #t0 
						SELECT @WeekNumber recdate,case when SUM(AdjustStock * StockAdjustmentDetails.CostPrice) IS null then 0 else SUM(AdjustStock * StockAdjustmentDetails.CostPrice)end Nett 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						WHERE 
						YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and DATEPART(week, cast(StockAdjustmentHeaders.CreatedDate as date))=@dateweek 
						and cast(StockAdjustmentHeaders.CreatedDate as date) between @FromDate and @ToDate
						and (StockAdjustmentDetails.Reason = N'Wastage')
						 and StockAdjustmentHeaders.CompanyID=@CompanyID
					end
					else
					begin
						insert into #t0 
						SELECT @WeekNumber recdate,case when SUM(AdjustStock * StockAdjustmentDetails.CostPrice) IS null then 0 else SUM(AdjustStock * StockAdjustmentDetails.CostPrice)end Nett 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						WHERE YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and DATEPART(week, cast(StockAdjustmentHeaders.CreatedDate as date))=@dateweek 
						and cast(StockAdjustmentHeaders.CreatedDate as date) between @FromDate and @ToDate 
						and StockLocationId=@LocationID and (StockAdjustmentDetails.Reason = N'Wastage')
					end
				end
				else
				begin
					if @LocationID=0
					begin
						insert into #t0 
						SELECT @WeekNumber recdate,case when SUM(AdjustStock * StockAdjustmentDetails.CostPrice) IS null then 0 else SUM(AdjustStock * StockAdjustmentDetails.CostPrice)end Nett 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						WHERE YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and DATEPART(week, cast(StockAdjustmentHeaders.CreatedDate as date))=@dateweek 
						and cast(StockAdjustmentHeaders.CreatedDate as date) between @FromDate and @ToDate 
						and PrinterTypeId=@OrderType and (StockAdjustmentDetails.Reason = N'Wastage')
						 and StockAdjustmentHeaders.CompanyID=@CompanyID
					end	
					else
					begin
						insert into #t0 
						SELECT @WeekNumber recdate,case when SUM(AdjustStock * StockAdjustmentDetails.CostPrice) IS null then 0 else SUM(AdjustStock * StockAdjustmentDetails.CostPrice)end Nett 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						WHERE YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and DATEPART(week, cast(StockAdjustmentHeaders.CreatedDate as date))=@dateweek 
						and cast(StockAdjustmentHeaders.CreatedDate as date) between @FromDate and @ToDate 
						and PrinterTypeId=@OrderType and StockLocationId=@LocationID and (StockAdjustmentDetails.Reason = N'Wastage')
					end						
				end	

			
			set @date = dateadd(DAY, 7, @date)
			set @WeekNumber=@WeekNumber+1
			
			set @dateyear = year(@date)

			end--@@
		end
		
		if @ChartType=3 --monthly
		begin
			set @date = @FromDate
			set @dateyear = year(@FromDate)
			set @datemonth = month(@FromDate)
			
			set @strMonth=DATENAME(month, @FromDate)
			set @MonthsCount=(SELECT DATEDIFF(mm, @FromDate, @ToDate) +1)
			set @MonthNumber=1
			
			while (@MonthNumber<=@MonthsCount ) 
			begin

			set @strMonth=DATENAME(month, @date)
				if @OrderType=0  --all order types
				begin
					if @LocationID=0
					begin
						insert into #t0 
						SELECT @strMonth recdate,case when SUM(AdjustStock * StockAdjustmentDetails.CostPrice) IS null then 0 else SUM(AdjustStock * StockAdjustmentDetails.CostPrice)end Nett 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						WHERE 
						YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and MONTH(cast(StockAdjustmentHeaders.CreatedDate as date))=@datemonth
						and (StockAdjustmentDetails.Reason = N'Wastage')
						 and StockAdjustmentHeaders.CompanyID=@CompanyID
					end
					else
					begin
						insert into #t0 
						SELECT @strMonth recdate,case when SUM(AdjustStock * StockAdjustmentDetails.CostPrice) IS null then 0 else SUM(AdjustStock * StockAdjustmentDetails.CostPrice)end Nett 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						WHERE YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and MONTH(cast(StockAdjustmentHeaders.CreatedDate as date))=@datemonth 
						and StockLocationId=@LocationID and (StockAdjustmentDetails.Reason = N'Wastage')
					end
				end
				else
				begin
					if @LocationID=0
					begin
						insert into #t0 
						SELECT @strMonth recdate,case when SUM(AdjustStock * StockAdjustmentDetails.CostPrice) IS null then 0 else SUM(AdjustStock * StockAdjustmentDetails.CostPrice)end Nett 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						WHERE YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and MONTH(cast(StockAdjustmentHeaders.CreatedDate as date))=@datemonth 
						and PrinterTypeId=@OrderType and (StockAdjustmentDetails.Reason = N'Wastage')
						 and StockAdjustmentHeaders.CompanyID=@CompanyID
					end	
					else
					begin
						insert into #t0 
						SELECT @strMonth recdate,case when SUM(AdjustStock * StockAdjustmentDetails.CostPrice) IS null then 0 else SUM(AdjustStock * StockAdjustmentDetails.CostPrice)end Nett 
						FROM StockAdjustmentDetails INNER JOIN
						StockAdjustmentHeaders ON StockAdjustmentDetails.StockAdjustmentHeaderId = StockAdjustmentHeaders.StockAdjustmentHeaderId INNER JOIN
						Products ON StockAdjustmentDetails.ProductId = Products.ProductId
						WHERE YEAR(cast(StockAdjustmentHeaders.CreatedDate as date))=@dateyear 
						and MONTH(cast(StockAdjustmentHeaders.CreatedDate as date))=@datemonth 
						and PrinterTypeId=@OrderType and StockLocationId=@LocationID and (StockAdjustmentDetails.Reason = N'Wastage')
					end						
				end	
			
			set @date = dateadd(MONTH, 1, @date)
			set @MonthNumber=@MonthNumber+1
			
			set @dateyear = year(@date)
			set @datemonth = month(@date)
			end

		end
		
		SELECT recdate,Nett  FROM  #t0
    END TRY
  
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            BEGIN
                --ROLLBACK TRANSACTION
                SELECT  ERROR_MESSAGE() AS Result

            END
        ELSE 
            BEGIN
                SELECT  ERROR_MESSAGE() AS Result
            END
    END CATCH";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_DB_WastageSummary

            #region SP_GivenDateSales
            spName = "SP_GivenDateSales";
            query = @"CREATE PROCEDURE [dbo].[SP_GivenDateSales] --2023-10-10 --Uber/Pickme/Online
@Date Datetime='',
@DateTo  datetime='',
@Locations AS nvarchar(max)
AS
BEGIN

CREATE TABLE #TmpLocations
            (
              [item] [nvarchar](25) NULL 
            )

        	insert into #TmpLocations Select distinct CONVERT(Nvarchar(50),SysLocationID ) as item From
			dbo.SysLocations where ',' + @Locations + ',' like
			'%,' + Convert(Nvarchar(50),SysLocationID) + ',%'


SELECT distinct l.LocationName,t.RecDate,t.Receipt,t.LocationID,t.UnitNo,t.ZNo,

dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'FS',t.RecDate) as FoodSale,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'BS',t.RecDate) as BevSale,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'NS',t.RecDate) as NonSale,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'CH',t.RecDate) as Cash,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'CD',t.RecDate) as Card,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'OT',t.RecDate) as Others,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'Ol',t.RecDate) as Online,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'UB',t.RecDate) as UBER,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'PM',t.RecDate) as PICKME,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'SC',t.RecDate) as ServCharge,
0.0 as ChiliPaste,
dbo.GetDailySalesReportValues(RTRIM(t.Receipt),t.ZNo,t.UnitNo,t.LocationID,'VT',t.RecDate) as VAT,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'D',t.RecDate) as Discount,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'NB',t.RecDate) as NBT,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'TD',t.RecDate) as TDL,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'GR',t.RecDate) as Gross,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'NT',t.RecDate)+dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'SC',t.RecDate)+dbo.GetDailySalesReportValues(RTRIM(t.Receipt),t.ZNo,t.UnitNo,t.LocationID,'VT',t.RecDate)+dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'TD',t.RecDate)+dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'NB',t.RecDate)-dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'D',t.RecDate) as TNet,

dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'CR',t.RecDate) as Credit,
'NA' as HoldersName,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'DS',t.RecDate)  as DESSERTSales

FROM TransactionDets t join dbo.SysLocations  l on t.LocationID = l.SysLocationID
where
(cast(t.RecDate as date) between cast(@Date as date) and  cast(@DateTo as date) and 
t.Status=1 ) and t.LocationID in (select * from #TmpLocations)

drop table #TmpLocations

END";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_GivenDateSales

            #region SP_HMSLoyaltyPointsExpirationSchedular
            spName = "SP_HMSLoyaltyPointsExpirationSchedular";
            query = @"CREATE PROCEDURE [dbo].[SP_HMSLoyaltyPointsExpirationSchedular]
@CompId int=0
AS
BEGIN

declare  @CompanyId int =@CompId;
declare @date datetime=cast(getdate() as date);

select PointsExpirations.CardType,PointsExpirationSchedules.SQL,PointsExpirationSchedules.Idx into #ControlTable from 
		PointsExpirationSchedules INNER JOIN
		PointsExpirations ON PointsExpirationSchedules.PointsExpirationId = PointsExpirations.PointsExpirationId
		where cast(PointsExpirationSchedules.ScheduleDate as date)=@date and PointsExpirationSchedules.CompanyId=@CompanyId



declare @cardtype int
declare @idx int
declare @sql varchar(max)
while exists (select * from #ControlTable)
begin

    select top 1 @cardtype = CardType from #ControlTable
    select top 1 @idx = Idx from #ControlTable
    select top 1 @sql = SQL from #ControlTable
    
-- updating     RPoints
	update dbo.LoyaltyCustomers
	set RPoints=CPoints
	where dbo.LoyaltyCustomers.CardMasterId=@cardtype 
	
-- updating EndDate in PointsExpirationSchedules
	update PointsExpirationSchedules
	set PointsExpirationSchedules.EndDate=GETDATE()
	where PointsExpirationSchedules.Idx=@idx

-- updateing cPoint=0
	update dbo.LoyaltyCustomers
	set CPoints=0
	where dbo.LoyaltyCustomers.CardMasterId=@cardtype

-- inserting SMS DB
	exec(@sql)

    delete #ControlTable
    where CardType = @cardtype

end
drop table #ControlTable

END";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_HMSLoyaltyPointsExpirationSchedular

            #region SP_InitDeliveryApp
            spName = "SP_InitDeliveryApp";
            query = @"CREATE PROCEDURE [dbo].[SP_InitDeliveryApp]

AS
BEGIN

--TRUNCATE TABLE [HMSDeliveryApp].[dbo].[Product]

INSERT INTO [HMSDeliveryApp].[dbo].[Product]
([ProductCode]
           ,[ProductName]
           ,[ProductNameInSinhala]
           ,[IsActive]
           ,[ProductImage]
           ,[ProductImageName]
           ,[ProductImageType]
           ,[ProductImageUrl]
           ,[DepartmentId]
           ,[CategoryId]
           ,[SubCategoryId]
           ,[DepartmentName]
           ,[CategoryName]
           ,[SubCategoryName]
           ,[DeptImageUrl]
           ,[CatImageUrl]
           ,[SubCatImageUrl]
           ,[CostPrice]
           ,[SellingPrice]
           ,[Barcode]
           ,[CompanyId]
           ,[LocationId]
           ,[CreatedUser]
           ,[CreatedDate]
           ,[ModifiedUser]
           ,[ModifiedDate]
           ,[DataTransfer]
           ,[NameOnInvoice]
           ,[DiscountPrecentage]
           ,[MaximumDiscount]
           ,[FixedDiscountPercentage]
           ,[FixedDiscountAmount]
           ,[MaximumDiscountPercentage]
           ,[CurrencySymbol]
           ,[IsAddon]
           ,[ProductNote]
           ,[ServingUnit]
           ,[CateringMood]
           ,[PreprationTime]
           ,[PrinterTypeId]
           ,[RefCode])
SELECT   ProductCode, ProductName, ProductNameInSinhala, Products.IsActive, 
ProductImage, ProductImageName, ProductImageType,'ProductImageurl', DepartmentId, 

CategoryId,SubCategoryId, RstDepartments.DepartmentName, RstCategories.RstCategoryName, 
RstSubCategories.RstSubCategoryName,'DepartmentImageurl',
'CatImageurl','SubCatImageurl', ProductServingUnits.CostPrice, ProductServingUnits.SellingPrice, 
Barcode, Products.CompanyID, Products.LocationId, 
--Products.CreatedUser,
1,
Products.CreatedDate, Products.ModifiedUser, Products.ModifiedDate, 
Products.DataTransfer, NameOnInvoice, 
DiscountPrecentage, MaximumDiscount, FixedDiscountPercentage, FixedDiscountAmount, 
MaximumDiscountPercentage,'Rs', Products.IsAddon, 'ProductNote', ProductServingUnits.ServingUnit, '1,2,3,4', 
40,Products.PrinterTypeId,ProductCode
FROM         Products inner join
RstDepartments on Products.DepartmentId=RstDepartments.RstDepartmentID inner join
RstCategories on products.CategoryId=RstCategories.RstCategoryID inner join
RstSubCategories on Products.SubCategoryId=RstSubCategories.RstSubCategoryID inner join
ProductServingUnits on Products.ProductId=ProductServingUnits.ProductId inner join
PrinterTypes on Products.PrinterTypeId=PrinterTypes.PrinterTypeId
where Products.IsAddon=0 and Products.IsActive=1
order by Products.ProductId


END";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_InitDeliveryApp

            #region SP_RP_FoodCostEstimate
            spName = "SP_RP_FoodCostEstimate";
            query = @"CREATE PROCEDURE [dbo].[SP_RP_FoodCostEstimate]
	@FromDate date,
	@ToDate date,
	@CompanyID int ,
    @LocationID INT ,
    @DeptID int

AS 
    SET NOCOUNT ON
    SET XACT_ABORT ON

    BEGIN TRY

		IF OBJECT_ID('tempdb..#t0') IS NOT NULL 
		BEGIN
			DROP TABLE #t0
		END 
		
		CREATE TABLE #t0
		(
		  productcode varchar(20),
		  productdesc varchar(100),
		  salevalue [decimal](18, 2) NOT NULL ,
		  costvalue [decimal](18, 2) NOT NULL
		)
		
		if @LocationID=0
		begin
			if @DeptID=0
			begin
				insert into #t0 
				select  productcode,descrip,
				sum(nett) salevalue,sum(qty*cost) costvalue from view_sales where 
				recdate between @FromDate and @ToDate and CompanyID=@CompanyID
				group by productcode,descrip
			end
			else
			begin
				insert into #t0 
				select  productcode,descrip,
				sum(nett) salevalue,sum(qty*cost) costvalue from view_sales where 
				recdate between @FromDate and @ToDate and CompanyID=@CompanyID and RstDepartmentID=@DeptID
				group by productcode,descrip
			end
		end
		else
		begin
			if @DeptID=0
			begin
				insert into #t0 
				select  productcode,descrip,
				sum(nett) salevalue,sum(qty*cost) costvalue from view_sales where 
				recdate between @FromDate and @ToDate and CompanyID=@CompanyID and locationID=@LocationID
				group by productcode,descrip
			end
			else
			begin
				insert into #t0 
				select  productcode,descrip,
				sum(nett) salevalue,sum(qty*cost) costvalue from view_sales where 
				recdate between @FromDate and @ToDate and CompanyID=@CompanyID and locationID=@LocationID and RstDepartmentID=@DeptID
				group by productcode,descrip
			end
		end
        	
	SELECT productcode as ProductCode,productdesc as Description ,salevalue as SaleValue,costvalue as CostValue,salevalue-costvalue as GP,round(costvalue/salevalue*100,2) as FoodCost  FROM  #t0

    END TRY
  
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            BEGIN
                --ROLLBACK TRANSACTION
                SELECT  ERROR_MESSAGE() AS Result

            END
        ELSE 
            BEGIN
                SELECT  ERROR_MESSAGE() AS Result
            END
    END CATCH";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_RP_FoodCostEstimate

            #region SP_RP_MaterialConsumption
            spName = "SP_RP_MaterialConsumption";
            query = @"CREATE PROCEDURE [dbo].[SP_RP_MaterialConsumption]
	@FromDate date,
	@ToDate date,
	@CompanyID int ,
    @LocationID INT ,
    @DeptID int

AS 
    SET NOCOUNT ON
    SET XACT_ABORT ON

    BEGIN TRY
		declare @ProductID int,@ServingUnitID int, @Qty decimal(18,2),
		@Cnt int,
		@ProductCode varchar(20) , @ProductName varchar(100), @SubUnit varchar(50), 
		@Quantity decimal(18,2), @CostPrice decimal(18,2)

		IF OBJECT_ID('tempdb..#t0') IS NOT NULL 
		BEGIN
			DROP TABLE #t0
		END 
		
		CREATE TABLE #t0
		(
		  ProductID int,
		  ServingUnitID int,
		  qty [decimal](18, 2) NOT NULL 	  
		)
		
		if @LocationID=0
		begin
			if @DeptID=0
			begin
				insert into #t0 
				SELECT ProductID,ServingUnitID , SUM(Qty) qty
				FROM View_Sales where productid in(select productid from Receipes) and
				recdate between @FromDate and @ToDate and CompanyID=@CompanyID 
				GROUP BY ProductID,ServingUnitID
			end
			else
			begin
				insert into #t0 
				SELECT ProductID,ServingUnitID ,  SUM(Qty) qty
				FROM View_Sales where productid in(select productid from Receipes) and
				recdate between @FromDate and @ToDate and CompanyID=@CompanyID and RstDepartmentID=@DeptID
				GROUP BY ProductID,ServingUnitID
				
			end
		end
		else
		begin
			if @DeptID=0
			begin
				insert into #t0 
				SELECT ProductID,ServingUnitID,  SUM(Qty) qty
				FROM View_Sales where productid in(select productid from Receipes) and
				recdate between @FromDate and @ToDate and CompanyID=@CompanyID and locationID=@LocationID
				GROUP BY ProductID,ServingUnitID
				
			end
			else
			begin
				insert into #t0 
				SELECT ProductID,ServingUnitID,  SUM(Qty) qty
				FROM View_Sales where productid in(select productid from Receipes) and
				recdate between @FromDate and @ToDate and CompanyID=@CompanyID and locationID=@LocationID and RstDepartmentID=@DeptID
				GROUP BY ProductID,ServingUnitID

			end
		end
       
       	IF OBJECT_ID('tempdb..#t1') IS NOT NULL 
		BEGIN
			DROP TABLE #t1
		END 
		
		CREATE TABLE #t1
		(
		  productcode varchar(20),
		  productdesc varchar(100),
		  unit varchar(50),
		  qty [decimal](18, 2) NOT NULL,  
		  costvalue [decimal](18, 2) NOT NULL  
		)
		
		begin  --////
		DECLARE cur CURSOR
		FOR
			SELECT ProductID,ServingUnitID,Qty	FROM #t0
			
		OPEN cur
		FETCH NEXT FROM cur INTO @ProductID,@ServingUnitID,@Qty

		WHILE @@fetch_status = 0 
			BEGIN
				set @Cnt=(SELECT    count(*) FROM Receipes INNER JOIN
                Products ON Receipes.MaterialId = Products.ProductId LEFT OUTER JOIN
                UnitConversions ON Products.WeightPerUnit = UnitConversions.UnitConversionId
				WHERE     (Receipes.ProductId = @ProductID) AND (Receipes.ProductServingUnitId = @ServingUnitID) 
				AND (Receipes.ProductQty = @Qty))
				
				if @Cnt>0
				
					begin  --****
						DECLARE cur1 CURSOR
						FOR
							SELECT    Products.ProductCode, Products.ProductName, UnitConversions.SubUnit, Receipes.Quantity, Receipes.CostPrice
							FROM  Receipes INNER JOIN
							Products ON Receipes.MaterialId = Products.ProductId LEFT OUTER JOIN
							UnitConversions ON Products.WeightPerUnit = UnitConversions.UnitConversionId
							WHERE     (Receipes.ProductId = @ProductID) AND (Receipes.ProductServingUnitId = @ServingUnitID) 
							AND (Receipes.ProductQty = @Qty)

							
						OPEN cur1
						FETCH NEXT FROM cur1 INTO @ProductCode,@ProductName,@SubUnit,@Quantity,@CostPrice 

						WHILE @@fetch_status = 0 
							BEGIN
					
								insert into #t1 values (@ProductCode,@ProductName,@SubUnit,@Quantity,@CostPrice )

													
								FETCH NEXT FROM cur1 INTO @ProductCode,@ProductName,@SubUnit,@Quantity,@CostPrice 
				            
						 END
						close  cur1
						DEALLOCATE   cur1 
					end --****
				else
					begin --###
						DECLARE cur1 CURSOR
						FOR
							SELECT    Products.ProductCode, Products.ProductName, UnitConversions.SubUnit, Receipes.Quantity, Receipes.CostPrice
							FROM  Receipes INNER JOIN
							Products ON Receipes.MaterialId = Products.ProductId LEFT OUTER JOIN
							UnitConversions ON Products.WeightPerUnit = UnitConversions.UnitConversionId
							WHERE     (Receipes.ProductId = @ProductID) AND (Receipes.ProductServingUnitId = @ServingUnitID) 
							AND (Receipes.ProductQty = 1)
							
						OPEN cur1
						FETCH NEXT FROM cur1 INTO @ProductCode,@ProductName,@SubUnit,@Quantity,@CostPrice 

						WHILE @@fetch_status = 0 
							BEGIN
					
								insert into #t1 values (@ProductCode,@ProductName,@SubUnit,@Quantity*@Qty,@CostPrice*@Qty )

													
								FETCH NEXT FROM cur1 INTO @ProductCode,@ProductName,@SubUnit,@Quantity,@CostPrice 
				            
						 END
						close  cur1
						DEALLOCATE   cur1 
					end--###

									
				FETCH NEXT FROM cur INTO @ProductID,@ServingUnitID,@Qty
            
		 END
		close  cur
		DEALLOCATE   cur 
		end --////
		
select productcode , productdesc , unit , sum(qty) as Qty , sum(costvalue) as Value   FROM  #t1
		  group by productcode,productdesc,unit
		  order by productdesc

    END TRY
  
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            BEGIN
                --ROLLBACK TRANSACTION
                SELECT  ERROR_MESSAGE() AS Result

            END
        ELSE 
            BEGIN
                SELECT  ERROR_MESSAGE() AS Result
            END
    END CATCH";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_RP_MaterialConsumption


            #region sp_rpt-BinCard

            spName = "sp_rpt_BinCard";
            query = @"CREATE PROCEDURE[dbo].[sp_rpt_BinCard]
            @SelectedLocationID NVARCHAR(MAX) ,
    @FromDate DATETIME,
    @ToDate DATETIME ,
    @ProductID NVARCHAR(MAX),
    @DepartmentID NVARCHAR(mAX)
AS

    DECLARE @FromId BIGINT, @ToId BIGINT
    BEGIN

set dateformat dmy

          Print 'AA'

        CREATE TABLE #tempBinCard 
            (
              [LocationID][bigint] NOT NULL,
              [ToLocationName][nvarchar] (50) NULL ,
              [StockCode] [nvarchar](25) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL ,
              [BatchNo] [nvarchar](25) NULL ,
              [Qty] [decimal](18, 3) NOT NULL,
              [CostPrice] [decimal](18, 2) ,
              [SellingPrice] [decimal](18, 2) ,
              [TransactionType] [nvarchar](50) NULL ,
              [TransactionNo] [nvarchar](20) NULL ,
              [TransactionDate] [DateTime],
              [ZNo] [int] NULL ,
              [UnitNo] [int] NULL,
              [EventName] [nvarchar](max)NULL
            )

        CREATE TABLE #tempSummary
            (
              [StockCode][nvarchar] (25) NULL ,
              [Type] [nvarchar](50) NULL ,
              [Qty] [decimal](18, 3) NOT NULL,
              [BatchNo] [int] NOT NULL,
            )

CREATE TABLE #TmpProductStockDetails
(
     [TmpProductStockDetailsID][int] IDENTITY(1, 1) NOT NULL,
     [CompanyID] [int] NOT NULL,
     [LocationID] [int] NOT NULL,
     [ToLocationName] [nvarchar](max)NULL,
     [GivenDate] [datetime] NOT NULL,
     [ProductID] [int] NOT NULL,
     [ProductCode] [nvarchar](max)NULL,
     [ProductName] [nvarchar](max)NULL,
     [TransactionType] [Nvarchar](max)NOT NULL,
     [TransactionNo] [nvarchar](max)NULL,
     [BatchNo] [nvarchar](max)NULL,
     [TransactionDate] [date] NOT NULL,
     [CostPrice] [decimal](18, 2) default(0),
     [SellingPrice] [decimal](18, 2) default(0),
     [AverageCost] [decimal](18, 2) default(0),
     [Amount] [decimal](18, 2) default(0),
     [DepartmentID] [int] default(0) ,
     [CategoryID] [int] default(0),
     [SubCategoryID] [int] default(0),
     [SubCategory2ID] [int] default(0),
     [SupplierID] [int] default(0),
     [CustomerID] [int] default(0),
     [StockQty] [decimal](18, 3) default(0) ,
     [Qty1] [decimal](18, 2) default(0),
     [Qty2] [decimal](18, 2) default(0),
     [Qty3] [decimal](18, 2) default(0),
     [Qty4] [decimal](18, 2) default(0),
     [Qty5] [decimal](18, 2) default(0),
     [Qty6] [decimal](18, 2) default(0),
     [Qty7] [decimal](18, 2) default(0),
     [Qty8] [decimal](18, 2) default(0),
     [Qty9] [decimal](18, 2) default(0),
     [Qty10] [decimal](18, 2) default(0),
     [UserID] [int] default(0),
     [UniqueID] [int] default(0),
     [GrossProfit] [decimal](18, 2) default(0),
     [IsDelete] [int] default(0),
     [GroupOfCompanyID] [int] default(0),
     [CreatedUser] [nvarchar](max)NULL,
     [CreatedDate] [datetime]  NULL,
     [ModifiedUser] [nvarchar](max)NULL,
     [ModifiedDate] [datetime]  NULL,
     [DataTransfer] [int] default(0),
     [ZNo] [int] default(0),
     [UnitNo] [int] default(0),
     [SuppName] [nvarchar](max)NULL,
     [SerialNo] [int]  default(0),
     [OpeningBalance] decimal(18, 3) default(0),
     [EventName] [nvarchar](max)NULL
     )

        CREATE TABLE #tmpSelProducts
            (
              [item][nvarchar] (25) NULL
            )
            
      CREATE TABLE #TmpSelDepartments
            (
              [item][nvarchar] (25) NULL
            )

			 CREATE TABLE #tmpLocs
            (
              [SysLocationID][bigint] NULL
            )





        if @ProductID <> '0'
        begin
            insert into #tmpSelProducts Select distinct CONVERT(Nvarchar(50), ProductID) as item From
			Products where ',' + @ProductID + ',' like

            '%,' + Convert(Nvarchar(50), ProductID) + ',%'
        end
        else
                begin--all items

            insert into #tmpSelProducts Select distinct CONVERT(Nvarchar(50), ProductID) as item From
			Products
        end

        if @DepartmentID <> '0'
        begin
        insert into #TmpSelDepartments Select distinct CONVERT(Nvarchar(50), RstDepartmentID  ) as item From
        RstDepartments where ',' + @DepartmentID + ',' like
        '%,' + Convert(Nvarchar(50), RstDepartmentID) + ',%'
        end
        else
                begin--all items
             insert into #TmpSelDepartments Select distinct CONVERT(Nvarchar(50), RstDepartmentID  ) as item From
			RstDepartments
        end

         if @SelectedLocationID <> '0'begin

          insert into #tmpLocs    select SysLocationID  from SysLocations where ',' + @SelectedLocationID + ',' like
        '%,' + CONVERT(nvarchar(20), syslocationid) + ',%'
        --select SysLocationID into #tmpLocs from SysLocations where ',' + @SelectedLocationID + ',' like
								--    '%,' + CONVERT(nvarchar(20), syslocationid) + ',%'


        end

                        else begin
                     insert into #tmpLocs  select SysLocationID  from SysLocations end


          /*--GRN  */


                INSERT  INTO #tempBinCard
                        (LocationID,
                          StockCode,
                          BatchNo,
                          Qty,
                          TransactionType,
                          TransactionNo,
                          TransactionDate,
                          CostPrice,
                          SellingPrice,
                          ZNo,
                          UnitNo, ToLocationName, EventName
                        )

                        SELECT ph.GRNLocationId  ,
                                pd.StockCode ,
                                pd.BatchNo ,
                                (pd.GRNQuantity + pd.FreeQty) AS QTY,
                                'GRN' ,
                                ph.DocumentNo ,
                                --DATEADD(day, 0, DATEDIFF(day, 0, ph.DocumentDate)) + DATEADD(day, 0 - DATEDIFF(day, 0, ph.CreatedDate), ph.CreatedDate),
                                ph.GRNDate,
                                pd.CostPrice ,
                                pd.SellingPrice ,
                                0 ,
                                0,l.LocationName,Events.EventName
                        FROM    PurchaseDetails pd
                                INNER JOIN PurchaseHeaders ph ON pd.PurchaseHeaderID = ph.PurchaseHeaderId
                                AND pd.DocumentNo = ph.DocumentNo
                                inner JOIN SysLocations l ON
                                l.SysLocationID = ph.GRNLocationId
                                 LEFT OUTER JOIN
                      Events  ON ph.EventId = Events.EventId
                        WHERE ISNULL(ph.IsTempGRN,0) = 0
                                AND pd.DocumentID = 4
                                --AND ph.GRNLocationId = @SelectedLocationID
                                And ph.GRNLocationId in (select SysLocationID from #tmpLocs)
                                AND CAST(ph.GRNDate AS DATE) <= @ToDate
                                AND convert(nvarchar(25),pd.Productid) in (select item from #tmpSelProducts )
                                and Pd.ProductID in (select ProductID From Products where CONVERT(NVARCHAR(25), DepartmentId) in (select item from #TmpSelDepartments  ) )
                                and ph.DocumentStatus = 3
                        ORDER BY ph.GRNDate

                /*--Purchase Returns  */

                INSERT  INTO #tempBinCard
                        (LocationID,
                          StockCode,
                          BatchNo,
                          Qty,
                          TransactionType,
                          TransactionNo,
                          TransactionDate,
                          CostPrice,
                          SellingPrice,
                          ZNo,
                          UnitNo, ToLocationName, EventName
                        )

                        SELECT ph.GRNLocationId ,
                                pd.StockCode ,
                                pd.BatchNo ,
                                ((pd.GRNQuantity + pd.FreeQty) * -1) AS QTY,
                                'Purchase Returns' ,
                                ph.DocumentNo ,
                                --DATEADD(day, 0, DATEDIFF(day, 0, ph.DocumentDate)) + DATEADD(day, 0 - DATEDIFF(day, 0, ph.CreatedDate), ph.CreatedDate),
                                ph.GRNDate ,
                                pd.CostPrice ,
                                pd.SellingPrice ,
                                0 ,
                                0,l.LocationName,Events.EventName
                        FROM    PurchaseDetails pd
                                INNER JOIN PurchaseHeaders ph ON pd.PurchaseHeaderID = ph.PurchaseHeaderId
                                 AND pd.DocumentID = ph.DocumentID
                                   inner JOIN SysLocations l ON
                                l.SysLocationID = ph.GRNLocationId
                                LEFT OUTER JOIN
                      Events  ON ph.EventId = Events.EventId
                        WHERE CAST(ph.GRNDate AS DATE) <= @ToDate
                                AND pd.ProductID in (select item from #tmpSelProducts ) 
                                      AND pd.DocumentID = 6
                                      and isnull(Ph.IsTempPRN,0)= 0
                                      --and ph.GRNLocationId = @SelectedLocationID
                                      and ph.GRNLocationId in (select SysLocationID from #tmpLocs)
                                      and pd.ProductID in (select ProductID From Products where CONVERT(NVARCHAR(25), DepartmentId) in (select item from #TmpSelDepartments ))
                                      and ph.DocumentStatus = 3
                        ORDER BY ph.GRNDate

                /*--TOG INS */

                INSERT  INTO #tempBinCard
                        (LocationID,
                          StockCode,
                          BatchNo,
                          Qty,
                          TransactionType,
                          TransactionNo,
                          TransactionDate,
                          CostPrice,
                          SellingPrice,
                          ToLocationName,
                          ZNo,
                          UnitNo, EventName
                        )

                        SELECT th.ToLocationID   ,
                                td.StockCode ,
                                td.BatchNo ,
                                   td.OrderQty ,
                                   'TOG IN-' + l.LocationName,
                                th.DocumentNo ,
                                --DATEADD(day, 0, DATEDIFF(day, 0, th.DocumentDate)) + DATEADD(day, 0 - DATEDIFF(day, 0, th.CreatedDate), th.CreatedDate),
                                th.TOGDate ,
                                td.CostPrice ,
                                td.SellingPrice ,
                                ISNULL(l.LocationName, '') ,
                                0,
                                0,Events.EventName
                        FROM    TransferNoteDetails td
                                INNER JOIN TransferNoteHeaders th ON th.TransferNoteHeaderID = td.TransferNoteHeaderID
                                inner JOIN SysLocations l ON
                                -- l.SysLocationID = @SelectedLocationID
                                l.SysLocationID = th.ToLocationID
                                  LEFT OUTER JOIN
                      Events  ON th.EventId = Events.EventId
                        WHERE
                        th.ToLocationID in (select SysLocationID from #tmpLocs  ) and
                              ISNULL(th.IsTempTOG, 0) = 0
                              and CAST(th.TOGDate AS DATE)  <= @ToDate
                                AND convert(nvarchar(25),td.ProductID)  in (select item from #tmpSelProducts ) 
                                and td.ProductID in (select ProductID From Products where CONVERT(NVARCHAR(25), DepartmentId) in (select item from #TmpSelDepartments ))
                                and th.DocumentStatus = 3
                        ORDER BY th.TOGDate


                /*TOG OUT*/

                INSERT  INTO #tempBinCard
                        (LocationID,
                          StockCode,
                          BatchNo,
                          Qty,
                          TransactionType,
                          TransactionNo,
                          TransactionDate,
                          CostPrice,
                          SellingPrice,
                          ToLocationName,
                          ZNo,
                          UnitNo, EventName
                        )

                        SELECT th.FromLocationId    ,
                                td.StockCode ,
                                td.BatchNo ,
                                   td.OrderQty * -1 ,
                                   'TOG OUT-' + l.LocationName,
                                th.DocumentNo ,
                                --DATEADD(day, 0, DATEDIFF(day, 0, th.DocumentDate)) + DATEADD(day, 0 - DATEDIFF(day, 0, th.CreatedDate), th.CreatedDate),
                                th.TOGDate ,
                                td.CostPrice ,
                                td.SellingPrice ,
                                ISNULL(l.LocationName, '') ,
                                0,
                                0,Events.EventName
                        FROM    TransferNoteDetails td
                                INNER JOIN TransferNoteHeaders th ON th.TransferNoteHeaderID = td.TransferNoteHeaderID
                                inner JOIN SysLocations l ON
                                -- l.SysLocationID = @SelectedLocationID
                                l.SysLocationID = th.FromLocationId
                                 LEFT OUTER JOIN
                      Events  ON th.EventId = Events.EventId
                        WHERE
                        th.FromLocationId  in (select SysLocationID from #tmpLocs ) and
                              ISNULL(th.IsTempTOG, 0) = 0
                              and CAST(th.TOGDate AS DATE)  <= @ToDate
                                AND convert(nvarchar(25),td.ProductID)  in (select item from #tmpSelProducts ) 
                                and td.ProductID in (select ProductID From Products where CONVERT(NVARCHAR(25), DepartmentId) in (select item from #TmpSelDepartments ))
                                and th.DocumentStatus = 3
                        ORDER BY th.TOGDate

                /*--Stock Adjustment (ADD, reduce and override all types) */

                INSERT  INTO #tempBinCard
                        (LocationID,
                          StockCode,
                          BatchNo,
                          Qty,
                          TransactionType,
                          TransactionNo,
                          TransactionDate,
                          CostPrice,
                          SellingPrice,
                          ZNo,
                          UnitNo, ToLocationName
                        )
                        SELECT SH.StockLocationId ,
                                PSM.StockCode  ,
                                0 ,
                                Case when ad.BaseType = 'Add' then ad.AdjustStock
                                      when ad.BaseType = 'Reduce' then - 1 * ad.AdjustStock
                                     when ad.BaseType = 'Override' then(ad.AdjustStock)--(ad.AdjustStock - ad.currentstock)
                                     else ad.AdjustStock end,
                                case when Ad.BaseType = 'Add' then 'Stock Adjustment (ADD)'
                                     when Ad.BaseType = 'Reduce' then 'Stock Adjustment (Reduce)'
                                     When Ad.BaseType = 'Override' then 'Stock Adjustment (Override)'
                                     else 'NA' end  ,
                                sh.DocumentNo ,
                                      SH.CreatedDate ,
                                ad.CostPrice ,
                                ad.SellingPrice ,
                                0 ,
                                0,l.LocationName
                        FROM    StockAdjustmentDetails ad
                                INNER JOIN StockAdjustmentHeaders sh ON sh.StockAdjustmentHeaderID = ad.StockAdjustmentHeaderID
                                INNER JOIN ProductStockMasters psm ON AD.ProductId = PSM.ProductId AND PSM.LocationId = SH.LocationId
                                  inner JOIN SysLocations l ON
                                l.SysLocationID = SH.StockLocationId
                        WHERE
                                sh.StockLocationId in (select SysLocationID from #tmpLocs)
                                and CAST(sh.CreatedDate AS DATE)  <= @ToDate
                               AND convert(nvarchar(25),psm.ProductId)  in (select item from #tmpSelProducts )
                               and ad.ProductId in (select ProductID From Products where CONVERT(NVARCHAR(25), DepartmentId) in (select item from #TmpSelDepartments))
                        ORDER BY sh.CreatedDate

                             print 'Stock Adjustments'

          /*--Sales & Returns */

                INSERT INTO #tempBinCard
                        (LocationID ,
                          StockCode ,
                          BatchNo ,
                          Qty ,
                          TransactionType ,
                          TransactionNo ,
                          TransactionDate ,
                          CostPrice ,
                          SellingPrice ,
                          ZNo ,
                          UnitNo, ToLocationName
                        )
                        SELECT--td.LocationID ,
                                td.StockLocationID,
                                td.ProductCode ,
                                td.BatchNo ,
                                SUM(CASE DocumentID
                                      WHEN 1 THEN - (Qty)
                                      WHEN 3 THEN - (Qty)
                                      WHEN 2 THEN Qty
                                      WHEN 4 THEN Qty
                                      ELSE 0
                                    END) ,
                                'Sales & Returns',
                                td.Receipt ,
                                dateadd(second, datepart(second, td.endtime),
                                DATEADD(MINUTE, DATEPART(MINUTE, td.EndTime),
                                dateadd(hh, DATEPART(hh, td.EndTime), td.RecDate))), --add end time to recdate
                                td.Cost ,
                                td.Price ,
                                td.ZNo ,
                                td.UnitNo,l.LocationName
                        FROM    TransactionDets td
                         inner JOIN SysLocations l ON
                                l.SysLocationID = td.StockLocationID
                        WHERE CAST(td.RecDate AS DATE)  <= @ToDate
                                      /* --AND  td.ProductCode BETWEEN  CAST(@FromId AS NVARCHAR(25))  AND CAST(@ToId AS NVARCHAR(25))  */
                                AND convert(nvarchar(25),td.ProductID ) in (select item from #tmpSelProducts )     

                               --and td.LocationID = @SelectedLocationID
                              --and td.LocationID in (select SysLocationID from #tmpLocs)
                                and td.StockLocationID in (select sysLocationID from #tmpLocs)
                                
                                and ProductID in (select ProductID From Products where CONVERT(NVARCHAR(25), DepartmentId) in (select item from #TmpSelDepartments) )
                                and DocumentID in (1, 2, 3, 4)
                        GROUP BY --td.LocationID ,
                                  td.StockLocationID,
                                td.ProductCode ,
                                td.BatchNo ,
                                    dateadd(second, datepart(second, td.endtime),
                                DATEADD(MINUTE, DATEPART(MINUTE, td.EndTime),
                                dateadd(hh, DATEPART(hh, td.EndTime), td.RecDate))) ,
                                td.Cost ,
                                td.Price ,
                                td.Receipt ,
                                td.ZNo ,
                                td.UnitNo,l.LocationName
                        ORDER BY dateadd(second, datepart(second, td.endtime),
                                DATEADD(MINUTE, DATEPART(MINUTE, td.EndTime),
                                dateadd(hh, DATEPART(hh, td.EndTime), td.RecDate)))

/*--PRODUCTION NOTE ADD  */

                INSERT INTO #tempBinCard
                        (LocationID ,
                          StockCode ,
                          BatchNo ,
                          Qty ,
                          TransactionType ,
                          TransactionNo ,
                          TransactionDate ,
                          CostPrice ,
                          SellingPrice ,
                          ZNo ,
                          UnitNo, ToLocationName
                        )
                        SELECT distinct ph.LocationId   ,
                                p.ProductCode  ,
                                '',
                                pd.ProductQty AS QTY ,
                                'PRODUCTION-ADD' ,
                                ph.DocumentNo ,
                                --DATEADD(day, 0, DATEDIFF(day, 0, ph.DocumentDate)) + DATEADD(day, 0 - DATEDIFF(day, 0, ph.CreatedDate), ph.CreatedDate),
                                ph.CreatedDate ,
                                ph.ProductCostPrice  ,
                                pd.ProductSellingPrice  ,
                                0 ,
                                0,l.LocationName
                        FROM(select  PD.ProductionNoteHeaderId, ProductID, pd.ProductCostPrice,
                                 pd.ProductSellingPrice, sum(ProductQty) as ProductQty
                                 From ProductionNoteDetails PD
                                 where PD.productQty <> 0
                                 group by pd.ProductionNoteHeaderId, PD.productID,
                                 pd.ProductCostPrice, pd.ProductSellingPrice
                                 ) PD inner join
                                ProductionNoteHeaders ph
                                on pd.ProductionNoteHeaderId = ph.ProductionNoteHeaderId
                                inner join Products P on p.ProductId = pd.ProductId
                                 inner JOIN SysLocations l ON
                                l.SysLocationID = ph.LocationId
                        WHERE
                        -- ph.ProductionLocId = @SelectedLocationID
                                ph.LocationId in (select SysLocationID from #tmpLocs)
                                AND CAST(ph.CreatedDate AS DATE) <= @ToDate
                                AND convert(nvarchar(25),pd.ProductId) in (select item from #tmpSelProducts )
                                and Pd.ProductID in (select ProductId From Products where CONVERT(NVARCHAR(25), DepartmentId) in (select item from #TmpSelDepartments  ) )
                        ORDER BY ph.CreatedDate



                /*--PRODUCTION NOTE REDUCE  */

                INSERT  INTO #tempBinCard
                        (LocationID,
                          StockCode,
                          BatchNo,
                          Qty,
                          TransactionType,
                          TransactionNo,
                          TransactionDate,
                          CostPrice,
                          SellingPrice,
                          ZNo,
                          UnitNo, ToLocationName
                        )

                        SELECT distinct ph.LocationId   ,
                                p.ProductCode  ,
                                '',
                                pd.MaterialQty * -1  AS QTY,
                                'PRODUCTION-CONSUME' ,
                                ph.DocumentNo ,
                                --DATEADD(day, 0, DATEDIFF(day, 0, ph.DocumentDate)) + DATEADD(day, 0 - DATEDIFF(day, 0, ph.CreatedDate), ph.CreatedDate),

                                ph.CreatedDate ,
                                pd.CostPrice / pd.MaterialQty ,
                                pd.SellingPrice ,
                                0 ,
                                0,l.LocationName
                        FROM(select  PD.ProductionNoteHeaderId, MaterialId, pd.CostPrice,
                                 pd.SellingPrice, sum(MaterialQty) as MaterialQty
                                 From ProductionNoteDetails PD
                                 where PD.MaterialQty <> 0
                                 group by pd.ProductionNoteHeaderId, PD.MaterialId,
                                 pd.CostPrice, pd.SellingPrice
                                 ) PD inner join
                                ProductionNoteHeaders ph
                                on pd.ProductionNoteHeaderId = ph.ProductionNoteHeaderId
                                inner join Products P on p.ProductId = pd.MaterialId
                                 inner JOIN SysLocations l ON
                                l.SysLocationID = ph.LocationId
                        WHERE-- ph.ProductionLocId = @SelectedLocationID
                            ph.ProductionLocId in (select SysLocationID from #tmpLocs)
                                AND CAST(ph.CreatedDate AS DATE) <= @ToDate
                                AND convert(nvarchar(25),pd.MaterialId ) in (select item from #tmpSelProducts )
                                and Pd.MaterialId  in (select ProductId From Products where CONVERT(NVARCHAR(25), DepartmentId) in (select item from #TmpSelDepartments  ) )
                        ORDER BY ph.CreatedDate

          print 'SALES'

                    INSERT INTO #TmpProductStockDetails
                        (GroupOfCompanyID ,
                          CompanyID ,
                          LocationID ,
                          ProductCode ,
                          ProductName ,
                          TransactionDate ,
                          TransactionType ,
                          TransactionNo ,
                          ProductID ,
                          StockQty ,
                          UserID ,
                          IsDelete ,
                          GivenDate ,
                          CreatedUser ,
                          CreatedDate ,
                          ModifiedUser ,
                          ModifiedDate ,
                          DepartmentID ,
                          CategoryID ,
                          SubCategoryID ,
                          SubCategory2ID ,
                          SupplierID ,
                          DataTransfer ,
                          BatchNo ,
                          CostPrice ,
                          SellingPrice ,
                          toLocationName,
                          AverageCost ,
                               Amount,
                          ZNo ,
                          UnitNo,
                               SuppName,
                               SerialNo,
                               CustomerID,
                               [Qty1]
           , [Qty2]
           , [Qty3]
           , [Qty4]
           , [Qty5]
           , [Qty6]
           , [Qty7]
           , [Qty8]
           , [Qty9]
           , [Qty10]
             , [UniqueID]
             , GrossProfit, EventName
                        )
                        SELECT  1 ,
                                1 ,
                                tb.LocationID ,
                                pm.ProductCode ,
                                pm.ProductName ,
                                tb.TransactionDate ,
                                tb.TransactionType ,
                                tb.TransactionNo ,
                                pm.ProductID ,
                                tb.Qty ,
                                0 as userid , /*later send login userid*/
                                0 ,
                                GETDATE() ,
                                0 as CreatedUser , /*later send created userid*/
                                GETDATE() ,
                                0 as CreatedUser , /*later send created userid*/
                                GETDATE() ,
                                0 ,
                                0 ,
                                0 ,
                                0 ,
                                0 ,
                                0 ,
                                tb.BatchNo ,
                                tb.CostPrice ,
                                tb.SellingPrice ,
                                tb.ToLocationName,
                                0 ,
                                      0,
                                tb.ZNo ,
                                tb.UnitNo,
                                      '',
                                      0,
                                      0,
                                      0,
                                      0,0,0,0,0,0,0,0,0,0,0,tb.EventName
                        FROM    #tempBinCard tb
                                INNER JOIN Products pm ON tb.StockCode = pm.ProductCode
                                --AND tb.LocationID = pm.LocationID




               print 'final'

              Select* into #tmpOpBal
              From #TmpProductStockDetails --where TransactionDate <@FromDate
              delete from #TmpProductStockDetails where TransactionDate < @FromDate


insert into #TmpProductStockDetails
                   (GroupOfCompanyID,
                          CompanyID,
                          LocationID,
                          ProductCode,
                          ProductName,
                          TransactionDate,
                          TransactionType,
                          TransactionNo,
                          ProductID,
                          StockQty,
                          UserID,
                          IsDelete,
                          GivenDate,
                          CreatedUser,
                          CreatedDate,
                          ModifiedUser,
                          ModifiedDate,
                          DepartmentID,
                          CategoryID,
                          SubCategoryID,
                          SubCategory2ID,
                          SupplierID,
                          DataTransfer,
                          BatchNo,
                          CostPrice,
                          SellingPrice,
                          toLocationName,
                          AverageCost,
                               Amount,
                          ZNo,
                          UnitNo,
                               SuppName,
                               SerialNo,
                               CustomerID,
                               [Qty1]
           ,[Qty2]
           ,[Qty3]
           ,[Qty4]
           ,[Qty5]
           ,[Qty6]
           ,[Qty7]
           ,[Qty8]
           ,[Qty9]
           ,[Qty10]
             ,[UniqueID]
             , GrossProfit
                        )
                        SELECT  #T.GroupOfCompanyID ,
                          #T.CompanyID ,
                          0, --LocationID , 
                          ProductCode ,
                          ProductName ,
                          CONVERT(date, @FromDate)  ,
                          'Opening balance' ,
                          '' ,
                          ProductID ,
                          sum(case when transactionDate < @FromDate then StockQty
                                   else 0 end
                          ) ,
                          0 ,
                          0 ,
                          CONVERT(date, @FromDate)    ,
                          '' ,
                          CONVERT(date, @FromDate)    ,
                          '' ,
                          CONVERT(date, @FromDate)    ,
                          0 ,
                          0 ,
                          0 ,
                          0 ,
                          0 ,
                          0 ,
                          0 ,
                          0 ,
                          0 ,
                          l.LocationName,
                          0 ,
                               0,
                          0 ,
                          0,
                               '',
                               0,
                               0,
                               0
           ,0
           ,0
           ,0
           ,0
           ,0
           ,0
           ,0
           ,0
           ,0
             ,0
             ,0
From #tmpOpBal #T inner JOIN SysLocations l ON
                                l.SysLocationID = LocationID
group by #T.GroupOfCompanyID ,#T.CompanyID ,
     ProductCode , ProductName , ProductID ,l.LocationName

             select* From #TmpProductStockDetails tp inner join Products p
             on tp.ProductID = p.ProductId
             where p.IsActive = 1 and p.IsDelete = 0

     END";
            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);


            #endregion


            #region sp_rpt_BinCard 1
            spName = "sp_rpt_BinCard1";
            query = @"CREATE PROCEDURE [dbo].[sp_rpt_BinCard1]
    @SelectedLocationID  NVARCHAR(MAX) ,
    @FromDate DATETIME ,
    @ToDate DATETIME ,
    @ProductID NVARCHAR(MAX),
    @DepartmentID NVARCHAR(mAX)
AS

    DECLARE @FromId BIGINT ,@ToId BIGINT
    BEGIN

set dateformat dmy

          Print 'AA'

        CREATE TABLE #tempBinCard 
            (
              [LocationID] [bigint] NOT NULL ,
              [ToLocationName] [nvarchar](50) NULL ,
              [StockCode] [nvarchar](25) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL ,
              [BatchNo] [nvarchar](25) NULL ,
              [Qty] [decimal](18, 3) NOT NULL ,
              [CostPrice] [decimal](18, 2) ,
              [SellingPrice] [decimal](18, 2) ,
              [TransactionType] [nvarchar](50) NULL ,
              [TransactionNo] [nvarchar](20) NULL ,
              [TransactionDate] [DateTime],
              [ZNo] [int] NULL ,
              [UnitNo] [int] NULL,
              [EventName] [nvarchar](max) NULL 
            )

        CREATE TABLE #tempSummary
            (
              [StockCode] [nvarchar](25) NULL ,
              [Type] [nvarchar](50) NULL ,
              [Qty] [decimal](18, 3) NOT NULL,
              [BatchNo] [int] NOT NULL ,
            )

CREATE TABLE #TmpProductStockDetails
(
     [TmpProductStockDetailsID] [int] IDENTITY(1,1) NOT NULL,
     [CompanyID] [int] NOT NULL,
     [LocationID] [int] NOT NULL,
     [ToLocationName] [nvarchar](max) NULL,
     [GivenDate] [datetime] NOT NULL,
     [ProductID] [int] NOT NULL,
     [ProductCode] [nvarchar](max) NULL,
     [ProductName] [nvarchar](max) NULL,
     [TransactionType] [Nvarchar](max) NOT NULL,
     [TransactionNo] [nvarchar](max) NULL,
     [BatchNo] [nvarchar](max) NULL,
     [TransactionDate] [date] NOT NULL,
     [CostPrice] [decimal](18, 2) default(0),
     [SellingPrice] [decimal](18, 2) default(0),
     [AverageCost] [decimal](18, 2) default(0),
     [Amount] [decimal](18, 2) default(0),
     [DepartmentID] [int] default(0) ,
     [CategoryID] [int] default(0),
     [SubCategoryID] [int] default(0),
     [SubCategory2ID] [int] default(0),
     [SupplierID] [int] default(0),
     [CustomerID] [int] default(0),
     [StockQty] [decimal](18, 3) default(0) ,
     [Qty1] [decimal](18, 2) default(0),
     [Qty2] [decimal](18, 2) default(0),
     [Qty3] [decimal](18, 2) default(0),
     [Qty4] [decimal](18, 2) default(0),
     [Qty5] [decimal](18, 2) default(0),
     [Qty6] [decimal](18, 2) default(0),
     [Qty7] [decimal](18, 2) default(0),
     [Qty8] [decimal](18, 2) default(0),
     [Qty9] [decimal](18, 2) default(0),
     [Qty10] [decimal](18, 2) default(0),
     [UserID] [int] default(0),
     [UniqueID] [int] default(0),
     [GrossProfit] [decimal](18, 2) default(0),
     [IsDelete] [int] default(0),
     [GroupOfCompanyID] [int] default(0),
     [CreatedUser] [nvarchar](max) NULL,
     [CreatedDate] [datetime]  NULL,
     [ModifiedUser] [nvarchar](max) NULL,
     [ModifiedDate] [datetime]  NULL,
     [DataTransfer] [int] default(0),
     [ZNo] [int] default(0),
     [UnitNo] [int] default(0),
     [SuppName] [nvarchar](max) NULL,
     [SerialNo] [int]  default(0),
     [OpeningBalance] decimal(18,3) default(0),
     [EventName] [nvarchar](max) NULL
     )

        CREATE TABLE #tmpSelProducts
            (
              [item] [nvarchar](25) NULL 
            )
            
      CREATE TABLE #TmpSelDepartments
            (
              [item] [nvarchar](25) NULL 
            )
            
        if @ProductID <>'0'
        begin
        	insert into #tmpSelProducts Select distinct CONVERT(Nvarchar(50), ProductID) as item From
			Products where ',' + @ProductID + ',' like
			'%,' + Convert(Nvarchar(50),ProductID) + ',%'
        end
        else
        begin	--all items
			insert into #tmpSelProducts Select distinct CONVERT(Nvarchar(50), ProductID) as item From
			Products 
        end

        if @DepartmentID <>'0'
        begin
        insert into #TmpSelDepartments Select distinct CONVERT(Nvarchar(50), RstDepartmentID  ) as item From
        RstDepartments where ',' + @DepartmentID  + ',' like
        '%,' + Convert(Nvarchar(50),RstDepartmentID) + ',%'
        end
        else
        begin     --all items
             insert into #TmpSelDepartments Select distinct CONVERT(Nvarchar(50), RstDepartmentID  ) as item From
			RstDepartments
        end

        select SysLocationID into #tmpLocs from SysLocations where ',' + @SelectedLocationID + ',' like
        '%,' + CONVERT(nvarchar(20),syslocationid)  + ',%'

          /*--GRN  */

                INSERT  INTO #tempBinCard
                        ( LocationID ,
                          StockCode ,
                          BatchNo ,
                          Qty ,
                          TransactionType ,
                          TransactionNo ,
                          TransactionDate ,
                          CostPrice ,
                          SellingPrice ,
                          ZNo ,
                          UnitNo,ToLocationName,EventName
                        )

                        SELECT  ph.GRNLocationId  ,
                                pd.StockCode ,
                                pd.BatchNo ,
                                ( pd.GRNQuantity + pd.FreeQty ) AS QTY ,
                                'GRN' ,
                                ph.DocumentNo ,
                                --DATEADD(day, 0, DATEDIFF(day, 0, ph.DocumentDate)) + DATEADD(day, 0 - DATEDIFF(day, 0, ph.CreatedDate), ph.CreatedDate),
                                ph.GRNDate,
                                pd.CostPrice ,
                                pd.SellingPrice ,
                                0 ,
                                0,l.LocationName,Events.EventName
                        FROM    PurchaseDetails pd
                                INNER JOIN PurchaseHeaders ph ON pd.PurchaseHeaderID = ph.PurchaseHeaderId
                                AND pd.DocumentNo = ph.DocumentNo
                                inner JOIN SysLocations l ON
                                l.SysLocationID  =ph.GRNLocationId 
                                 LEFT OUTER JOIN
                      Events  ON ph.EventId = Events.EventId
                        WHERE   ISNULL(ph.IsTempGRN,0) = 0
                                AND pd.DocumentID = 4
                                --AND ph.GRNLocationId = @SelectedLocationID
                                And ph.GRNLocationId in (select SysLocationID from #tmpLocs)
                                AND CAST(ph.GRNDate AS DATE) <= @ToDate
                                AND convert(nvarchar(25),pd.Productid) in (select item from #tmpSelProducts )
                                and Pd.ProductID in (select ProductID From Products where CONVERT(NVARCHAR(25), DepartmentId) in (select item from #TmpSelDepartments  ) )
                                and ph.DocumentStatus=3
                        ORDER BY ph.GRNDate

          /*--Purchase Returns  */

                INSERT  INTO #tempBinCard
                        ( LocationID ,
                          StockCode ,
                          BatchNo ,
                          Qty ,
                          TransactionType ,
                          TransactionNo ,
                          TransactionDate ,
                          CostPrice ,
                          SellingPrice ,
                          ZNo ,
                          UnitNo,ToLocationName,EventName
                        )

                        SELECT  ph.GRNLocationId ,
                                pd.StockCode ,
                                pd.BatchNo ,
                                ( ( pd.GRNQuantity + pd.FreeQty ) * -1 ) AS QTY ,
                                'Purchase Returns' ,
                                ph.DocumentNo ,
                                --DATEADD(day, 0, DATEDIFF(day, 0, ph.DocumentDate)) + DATEADD(day, 0 - DATEDIFF(day, 0, ph.CreatedDate), ph.CreatedDate),
                                ph.GRNDate ,
                                pd.CostPrice ,
                                pd.SellingPrice ,
                                0 ,
                                0,l.LocationName,Events.EventName
                        FROM    PurchaseDetails pd
                                INNER JOIN PurchaseHeaders ph ON pd.PurchaseHeaderID = ph.PurchaseHeaderId
                                 AND pd.DocumentID = ph.DocumentID
                                   inner JOIN SysLocations l ON
                                l.SysLocationID  =ph.GRNLocationId 
                                LEFT OUTER JOIN
                      Events  ON ph.EventId = Events.EventId
                        WHERE    CAST(ph.GRNDate  AS DATE) <=@ToDate
                                AND pd.ProductID in (select item from #tmpSelProducts ) 
                                      AND pd.DocumentID = 6
                                      and isnull(Ph.IsTempPRN,0)=0
                                      --and ph.GRNLocationId =@SelectedLocationID
                                      and ph.GRNLocationId in (select SysLocationID from #tmpLocs)
                                      and pd.ProductID in (select ProductID From Products where CONVERT(NVARCHAR(25), DepartmentId) in (select item from #TmpSelDepartments ))
                                      and ph.DocumentStatus=3
                        ORDER BY ph.GRNDate
       
          /*--TOG INS */

                INSERT  INTO #tempBinCard
                        ( LocationID ,
                          StockCode ,
                          BatchNo ,
                          Qty ,
                          TransactionType ,
                          TransactionNo ,
                          TransactionDate ,
                          CostPrice ,
                          SellingPrice ,
                          ToLocationName ,
                          ZNo ,
                          UnitNo,EventName
                        )

                        SELECT  th.ToLocationID   ,
                                td.StockCode ,
                                td.BatchNo ,
                                   td.OrderQty ,
                                   'TOG IN-' + l.LocationName,
                                th.DocumentNo ,
                                --DATEADD(day, 0, DATEDIFF(day, 0, th.DocumentDate)) + DATEADD(day, 0 - DATEDIFF(day, 0, th.CreatedDate), th.CreatedDate),
                                th.TOGDate ,
                                td.CostPrice ,
                                td.SellingPrice ,
                                ISNULL(l.LocationName, '') ,
                                0,
                                0,Events.EventName
                        FROM    TransferNoteDetails td
                                INNER JOIN TransferNoteHeaders th ON th.TransferNoteHeaderID = td.TransferNoteHeaderID
                                inner JOIN SysLocations l ON
                                -- l.SysLocationID = @SelectedLocationID
                                l.SysLocationID  =th.ToLocationID 
                                  LEFT OUTER JOIN
                      Events  ON th.EventId = Events.EventId
                        WHERE  
                        th.ToLocationID in (select SysLocationID from #tmpLocs  ) and
                              ISNULL( th.IsTempTOG,0)=0
                              and  CAST(th.TOGDate AS DATE)  <=@ToDate
                                AND convert(nvarchar(25),td.ProductID)  in (select item from #tmpSelProducts ) 
                                and td.ProductID in (select ProductID From Products where CONVERT(NVARCHAR(25), DepartmentId) in (select item from #TmpSelDepartments ))
                                and th.DocumentStatus=3
                        ORDER BY th.TOGDate

                                 
/*TOG OUT*/

				INSERT  INTO #tempBinCard
                        ( LocationID ,
                          StockCode ,
                          BatchNo ,
                          Qty ,
                          TransactionType ,
                          TransactionNo ,
                          TransactionDate ,
                          CostPrice ,
                          SellingPrice ,
                          ToLocationName ,
                          ZNo ,
                          UnitNo,EventName
                        )

                        SELECT  th.FromLocationId    ,
                                td.StockCode ,
                                td.BatchNo ,
                                   td.OrderQty * -1 ,
                                   'TOG OUT-' + l.LocationName,
                                th.DocumentNo ,
                                --DATEADD(day, 0, DATEDIFF(day, 0, th.DocumentDate)) + DATEADD(day, 0 - DATEDIFF(day, 0, th.CreatedDate), th.CreatedDate),
                                th.TOGDate ,
                                td.CostPrice ,
                                td.SellingPrice ,
                                ISNULL(l.LocationName, '') ,
                                0,
                                0,Events.EventName
                        FROM    TransferNoteDetails td
                                INNER JOIN TransferNoteHeaders th ON th.TransferNoteHeaderID = td.TransferNoteHeaderID
                                inner JOIN SysLocations l ON
                                -- l.SysLocationID = @SelectedLocationID
                                l.SysLocationID  =th.ToLocationId  
                                 LEFT OUTER JOIN
                      Events  ON th.EventId = Events.EventId
                        WHERE  
                        th.FromLocationId  in (select SysLocationID from #tmpLocs ) and
                              ISNULL( th.IsTempTOG,0)=0
                              and  CAST(th.TOGDate AS DATE)  <=@ToDate
                                AND convert(nvarchar(25),td.ProductID)  in (select item from #tmpSelProducts ) 
                                and td.ProductID in (select ProductID From Products where CONVERT(NVARCHAR(25), DepartmentId) in (select item from #TmpSelDepartments ))
                                and th.DocumentStatus=3
                        ORDER BY th.TOGDate                                                                         

          /*--Stock Adjustment (ADD, reduce and override all types) */

                INSERT  INTO #tempBinCard
                        ( LocationID ,
                          StockCode ,
                          BatchNo ,
                          Qty ,
                          TransactionType ,
                          TransactionNo ,
                          TransactionDate ,
                          CostPrice ,
                          SellingPrice ,
                          ZNo ,
                          UnitNo,ToLocationName
                        )
                        SELECT  SH.StockLocationId ,
                                PSM.StockCode  ,
                                0 ,
                                Case when ad.BaseType ='Add' then ad.AdjustStock
                                     when ad.BaseType ='Reduce' then -1* ad.AdjustStock
                                     when ad.BaseType ='Override' then (ad.AdjustStock)--(ad.AdjustStock- ad.currentstock)
                                     else ad.AdjustStock  end,
                                case when Ad.BaseType ='Add' then 'Stock Adjustment (ADD)'
                                     when Ad.BaseType ='Reduce' then 'Stock Adjustment (Reduce)'
                                     When Ad.BaseType = 'Override' then 'Stock Adjustment (Override)'
                                     else 'NA' end  ,
                                sh.DocumentNo ,
                                      SH.CreatedDate ,
                                ad.CostPrice ,
                                ad.SellingPrice ,
                                0 ,
                                0,l.LocationName
                        FROM    StockAdjustmentDetails ad
                                INNER JOIN StockAdjustmentHeaders sh ON sh.StockAdjustmentHeaderID = ad.StockAdjustmentHeaderID
                                INNER JOIN ProductStockMasters psm ON AD.ProductId = PSM.ProductId AND PSM.LocationId = SH.LocationId
                                  inner JOIN SysLocations l ON
                                l.SysLocationID  =SH.StockLocationId 
                        WHERE  
                                sh.StockLocationId in (select SysLocationID from #tmpLocs)
                                and CAST(sh.CreatedDate  AS DATE)  <=@ToDate
                               AND convert(nvarchar(25),psm.ProductId)  in (select item from #tmpSelProducts )
                               and ad.ProductId in (select ProductID From Products where CONVERT(NVARCHAR(25), DepartmentId) in (select item from #TmpSelDepartments))
                        ORDER BY sh.CreatedDate

                             print 'Stock Adjustments'

          /*--Sales & Returns */ 

                INSERT  INTO #tempBinCard
                        ( LocationID ,
                          StockCode ,
                          BatchNo ,
                          Qty ,
                          TransactionType ,
                          TransactionNo ,
                          TransactionDate ,
                          CostPrice ,
                          SellingPrice ,
                          ZNo ,
                          UnitNo,ToLocationName
                        )
                        SELECT  --td.LocationID ,
                                td.StockLocationID,
                                td.ProductCode ,
                                td.BatchNo ,
                                SUM(CASE DocumentID
                                      WHEN 1 THEN -(Qty)
                                      WHEN 3 THEN -(Qty)
                                      WHEN 2 THEN Qty
                                      WHEN 4 THEN Qty
                                      ELSE 0
                                    END) ,
                                'Sales & Returns',
                                td.Receipt ,
                                dateadd(second,datepart(second,td.endtime),
                                DATEADD(MINUTE ,DATEPART(MINUTE ,td.EndTime),
                                dateadd(hh,DATEPART(hh,td.EndTime), td.RecDate))), --add end time to recdate
                                td.Cost ,
                                td.Price ,
                                td.ZNo ,
                                td.UnitNo,l.LocationName
                        FROM    TransactionDets td
                         inner JOIN SysLocations l ON
                                l.SysLocationID  =td.StockLocationID 
                        WHERE  CAST(td.RecDate AS DATE)  <=@ToDate
                                      /* --AND  td.ProductCode BETWEEN  CAST(@FromId AS NVARCHAR(25))  AND CAST(@ToId AS NVARCHAR(25))  */                                                              
                                AND  convert(nvarchar(25),td.ProductID ) in (select item from #tmpSelProducts )     

                               -- and td.LocationID = @SelectedLocationID
                               --and td.LocationID in (select SysLocationID from #tmpLocs)
                                and td.StockLocationID in (select sysLocationID from #tmpLocs)
                                
                                and ProductID in (select ProductID From Products where CONVERT(NVARCHAR(25), DepartmentId) in (select item from #TmpSelDepartments) )
                                and DocumentID in (1,2,3,4)
                        GROUP BY --td.LocationID ,
                                  td.StockLocationID,
                                td.ProductCode ,
                                td.BatchNo ,
                                    dateadd(second,datepart(second,td.endtime),
                                DATEADD(MINUTE ,DATEPART(MINUTE ,td.EndTime),
                                dateadd(hh,DATEPART(hh,td.EndTime), td.RecDate))) ,
                                td.Cost ,
                                td.Price ,
                                td.Receipt ,
                                td.ZNo ,
                                td.UnitNo,l.LocationName
                        ORDER BY dateadd(second,datepart(second,td.endtime),
                                DATEADD(MINUTE ,DATEPART(MINUTE ,td.EndTime),
                                dateadd(hh,DATEPART(hh,td.EndTime), td.RecDate)))

/*--PRODUCTION NOTE ADD  */

                INSERT  INTO #tempBinCard
                        ( LocationID ,
                          StockCode ,
                          BatchNo ,
                          Qty ,
                          TransactionType ,
                          TransactionNo ,
                          TransactionDate ,
                          CostPrice ,
                          SellingPrice ,
                          ZNo ,
                          UnitNo,ToLocationName
                        )
                        SELECT distinct ph.LocationId   ,
                                p.ProductCode  ,
                                '',
                                pd.ProductQty  AS QTY ,
                                'PRODUCTION-ADD' ,
                                ph.DocumentNo ,
                                --DATEADD(day, 0, DATEDIFF(day, 0, ph.DocumentDate)) + DATEADD(day, 0 - DATEDIFF(day, 0, ph.CreatedDate), ph.CreatedDate),
                                ph.CreatedDate ,
                                ph.ProductCostPrice  ,
                                pd.ProductSellingPrice  ,
                                0 ,
                                0,l.LocationName
                        FROM    (select  PD.ProductionNoteHeaderId, ProductID ,pd.ProductCostPrice  ,
                                 pd.ProductSellingPrice  , sum(ProductQty) as ProductQty
                                 From ProductionNoteDetails PD
                                 where PD.productQty<>0
                                 group by pd.ProductionNoteHeaderId, PD.productID,
                                 pd.ProductCostPrice  ,pd.ProductSellingPrice
                                 ) PD inner join
                                ProductionNoteHeaders ph
                                on pd.ProductionNoteHeaderId =ph.ProductionNoteHeaderId
                                inner join Products P on p.ProductId = pd.ProductId
                                 inner JOIN SysLocations l ON
                                l.SysLocationID  =ph.LocationId 
                        WHERE
                        -- ph.ProductionLocId   = @SelectedLocationID
                                ph.LocationId in (select SysLocationID from #tmpLocs)
                                AND CAST(ph.CreatedDate AS DATE) <= @ToDate
                                AND convert(nvarchar(25),pd.ProductId) in (select item from #tmpSelProducts )
                                and Pd.ProductID in (select ProductId From Products where CONVERT(NVARCHAR(25), DepartmentId) in (select item from #TmpSelDepartments  ) )
                        ORDER BY ph.CreatedDate                                     

    

     /*--PRODUCTION NOTE REDUCE  */

                INSERT  INTO #tempBinCard
                        ( LocationID ,
                          StockCode ,
                          BatchNo ,
                          Qty ,
                          TransactionType ,
                          TransactionNo ,
                          TransactionDate ,
                          CostPrice ,
                          SellingPrice ,
                          ZNo ,
                          UnitNo,ToLocationName
                        )

                        SELECT distinct ph.LocationId   ,
                                p.ProductCode  ,
                                '',
                                pd.MaterialQty * -1  AS QTY ,
                                'PRODUCTION-CONSUME' ,
                                ph.DocumentNo ,
                                --DATEADD(day, 0, DATEDIFF(day, 0, ph.DocumentDate)) + DATEADD(day, 0 - DATEDIFF(day, 0, ph.CreatedDate), ph.CreatedDate),

                                ph.CreatedDate ,
                                pd.CostPrice/pd.MaterialQty ,
                                pd.SellingPrice ,
                                0 ,
                                0,l.LocationName
                        FROM    (select  PD.ProductionNoteHeaderId, MaterialId  ,pd.CostPrice ,
                                 pd.SellingPrice , sum(MaterialQty) as MaterialQty
                                 From ProductionNoteDetails PD
                                 where PD.MaterialQty <>0
                                 group by pd.ProductionNoteHeaderId, PD.MaterialId ,
                                 pd.CostPrice  ,pd.SellingPrice
                                 ) PD inner join
                                ProductionNoteHeaders ph
                                on pd.ProductionNoteHeaderId =ph.ProductionNoteHeaderId
                                inner join Products P on p.ProductId = pd.MaterialId 
                                 inner JOIN SysLocations l ON
                                l.SysLocationID  =ph.LocationId 
                        WHERE -- ph.ProductionLocId   = @SelectedLocationID
                            ph.ProductionLocId in (select SysLocationID from #tmpLocs)
                                AND CAST(ph.CreatedDate AS DATE) <= @ToDate
                                AND convert(nvarchar(25),pd.MaterialId ) in (select item from #tmpSelProducts )
                                and Pd.MaterialId  in (select ProductId From Products where CONVERT(NVARCHAR(25), DepartmentId) in (select item from #TmpSelDepartments  ) )
                        ORDER BY ph.CreatedDate     

          print 'SALES'

                    INSERT  INTO #TmpProductStockDetails
                        ( GroupOfCompanyID ,
                          CompanyID ,
                          LocationID ,
                          ProductCode ,
                          ProductName ,
                          TransactionDate ,
                          TransactionType ,
                          TransactionNo ,
                          ProductID ,
                          StockQty ,
                          UserID ,
                          IsDelete ,
                          GivenDate ,
                          CreatedUser ,
                          CreatedDate ,
                          ModifiedUser ,
                          ModifiedDate ,
                          DepartmentID ,
                          CategoryID ,
                          SubCategoryID ,
                          SubCategory2ID ,
                          SupplierID ,
                          DataTransfer ,
                          BatchNo ,
                          CostPrice ,
                          SellingPrice ,
                          toLocationName,
                          AverageCost ,
                               Amount,
                          ZNo ,
                          UnitNo,
                               SuppName,
                               SerialNo,
                               CustomerID,
                               [Qty1]
           ,[Qty2]
           ,[Qty3]
           ,[Qty4]
           ,[Qty5]
           ,[Qty6]
           ,[Qty7]
           ,[Qty8]
           ,[Qty9]
           ,[Qty10]
             ,[UniqueID]
             ,GrossProfit,EventName
                        )
                        SELECT  1 ,
                                1 ,
                                tb.LocationID ,
                                pm.ProductCode ,
                                pm.ProductName ,
                                tb.TransactionDate ,
                                tb.TransactionType ,
                                tb.TransactionNo ,
                                pm.ProductID ,
                                tb.Qty ,
                                0 as userid , /*later send login userid*/
                                0 ,
                                GETDATE() ,
                                0 as CreatedUser , /*later send created userid*/
                                GETDATE() ,
                                0 as CreatedUser , /*later send created userid*/
                                GETDATE() ,
                                0 ,
                                0 ,
                                0 ,
                                0 ,
                                0 ,
                                0 ,
                                tb.BatchNo ,
                                tb.CostPrice ,
                                tb.SellingPrice ,
                                tb.ToLocationName,
                                0 ,
                                      0,
                                tb.ZNo ,
                                tb.UnitNo,
                                      '',
                                      0,
                                      0,
                                      0,
                                      0,0,0,0,0,0,0,0,0,0,0,tb.EventName
                        FROM    #tempBinCard tb
                                INNER JOIN Products pm ON tb.StockCode = pm.ProductCode
                                --AND tb.LocationID = pm.LocationID

               

               print 'final'

              Select *  into #tmpOpBal
              From #TmpProductStockDetails --where TransactionDate <@FromDate
              delete from #TmpProductStockDetails where TransactionDate < @FromDate


insert into #TmpProductStockDetails
                   ( GroupOfCompanyID ,
                          CompanyID ,
                          LocationID ,
                          ProductCode ,
                          ProductName ,
                          TransactionDate ,
                          TransactionType ,
                          TransactionNo ,
                          ProductID ,
                          StockQty ,
                          UserID ,
                          IsDelete ,
                          GivenDate ,
                          CreatedUser ,
                          CreatedDate ,
                          ModifiedUser ,
                          ModifiedDate ,
                          DepartmentID ,
                          CategoryID ,
                          SubCategoryID ,
                          SubCategory2ID ,
                          SupplierID ,
                          DataTransfer ,
                          BatchNo ,
                          CostPrice ,
                          SellingPrice ,
                          toLocationName,
                          AverageCost ,
                               Amount,
                          ZNo ,
                          UnitNo,
                               SuppName,
                               SerialNo,
                               CustomerID,
                               [Qty1]
           ,[Qty2]
           ,[Qty3]
           ,[Qty4]
           ,[Qty5]
           ,[Qty6]
           ,[Qty7]
           ,[Qty8]
           ,[Qty9]
           ,[Qty10]
             ,[UniqueID]
             ,GrossProfit
                        )
                        SELECT  #T.GroupOfCompanyID ,
                          #T.CompanyID ,
                          0, --LocationID , 
                          ProductCode ,
                          ProductName ,
                          CONVERT(date, @FromDate)  ,
                          'Opening balance' ,
                          '' ,
                          ProductID ,
                          sum(case when transactionDate<@FromDate then StockQty
                                   else 0 end
                          ) ,
                          0 ,
                          0 ,
                          CONVERT(date, @FromDate)    ,
                          '' ,
                          CONVERT(date, @FromDate)    ,
                          '' ,
                          CONVERT(date, @FromDate)    ,
                          0 ,
                          0 ,
                          0 ,
                          0 ,
                          0 ,
                          0 ,
                          0 ,
                          0 ,
                          0 ,
                          l.LocationName,
                          0 ,
                               0,
                          0 ,
                          0,
                               '',
                               0,
                               0,
                               0
           ,0
           ,0
           ,0
           ,0
           ,0
           ,0
           ,0
           ,0
           ,0
             ,0
             ,0
From #tmpOpBal #T inner JOIN SysLocations l ON
                                l.SysLocationID  =LocationID
group by #T.GroupOfCompanyID ,#T.CompanyID ,
     ProductCode , ProductName , ProductID ,l.LocationName

             select *  From #TmpProductStockDetails tp inner join Products p
             on tp.ProductID = p.ProductId 
             where p.IsActive = 1 and p.IsDelete = 0

     END";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion sp_rpt_BinCard

            #region sp_rpt_PaymentMethod
            spName = "sp_rpt_PaymentMethod";
            query = @"CREATE PROCEDURE [dbo].[sp_rpt_PaymentMethod]


As

Select * From RstDepartments";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion sp_rpt_PaymentMethod

            #region sp_rpt_SalesRegistry
            spName = "sp_rpt_SalesRegistry";
            query = @"CREATE PROCEDURE [dbo].[sp_rpt_SalesRegistry] --2023-10-11 Add Uber/Pickme/Online
@Type varchar(10)='sales',
@FromDate datetime = '',
@ToDate datetime = '',
@IsAsAtDate bit ='',
@FromTime datetime = '',
@ToTime datetime = '',
@LocationF int=0,
@LocationT int=0,
@Department int =0,
@Category int=0,
@SubCategory int=0,
@Customer int =0,
@CompanyId int =0
AS
BEGIN

		IF @IsAsAtDate ='false' AND @Type='sales' 
		
		BEGIN
		SELECT  TD.Receipt,TD.ZNo,TD.ProductCode,TD.Cost,TD.AvgCost,TD.Qty,TD.IDI1,TD.UnitOfMeasureName,TD.Amount,TD.TaxAmount,
		cast(TD.recdate as date) as recdate,(SELECT ProductName FROM Products WHERE ProductID=TD.ProductID) ProductName, 
		td.LocationID, p.DepartmentId,p.CategoryId,p.SubCategoryId, (td.Nett - td.Cost*td.Qty) as GP,
		 CASE WHEN td.Nett = 0 THEN 0 ELSE (td.Nett - td.Cost*td.Qty)/td.Nett*100 end as GPPres ,
         CONVERT(VARCHAR(20), RecDate, 101) AS [DATEPART],
         CONVERT(VARCHAR(20), StartTime, 108) AS TIMEPART,
        ISNULL( (Select ISNULL(Nett,0) FROM TransactionDets TDD WHere TDD.Receipt = TD.Receipt 
		AND TDD.UnitNo = TD.UnitNo 
		AND TDD.LocationID = TD.LocationID 
			AND TDD.ZNo = TD.ZNo 
				AND TDD.ZDate = TD.ZDate 
				AND TDD.DocumentID IN (100)
				),0) AS  ServCharge,
        ISNULL( (Select ISNULL(Nett,0) FROM TransactionDets TDD WHere TDD.Receipt = TD.Receipt 
		AND TDD.UnitNo = TD.UnitNo 
		AND TDD.LocationID = TD.LocationID 
			AND TDD.ZNo = TD.ZNo 
				AND TDD.ZDate = TD.ZDate 
				AND TDD.DocumentID IN (101)
				),0) AS  ACCharge,(SELECT MAX(Descrip) FROM PayTypes WHERE Type=pay.PayTypeID) PayType,
				CASE WHEN pay.PayTypeID = 58 THEN pay.Balance ELSE 0 end as Online ,
				CASE WHEN pay.PayTypeID = 59 THEN pay.Balance ELSE 0 end as Uber ,
				CASE WHEN pay.PayTypeID = 62 THEN pay.Balance ELSE 0 end as Pickme 
		FROM TransactionDets TD 
		inner join Products p on td.ProductID = p.ProductId 
		inner join PaymentDets pay on td.Receipt = pay.Receipt And td.ZNo = pay.ZNo And td.LocationID = pay.LocationID
		inner join RstDepartments d on p.DepartmentId = d.RstDepartmentID
		inner join RstCategories c on p.CategoryId = c.RstCategoryID
		inner join RstSubCategories s on p.SubCategoryId = s.RstSubCategoryID
		inner join SysLocations l on td.LocationID = l.SysLocationID
		inner join SysCompanies co on co.SysCompanyID = l.CompanyID
		WHERE (td.DocumentID = 1 OR
		td.DocumentID = 3) AND 
		(td.Status = 1) AND 
		(td.TransStatus = 1) AND 
		(td.SaleTypeID = 1) AND 
		(td.BillTypeID = 1) AND 
		
		CAST(td.RecDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND cast(@ToDate as DATE) 
		AND cast(td.EndTime as time)  BETWEEN cast(@FromTime as time) AND cast(@ToTime as time)
		--and (@LocationF != 0 and @LocationT!=0 AND td.LocationID BETWEEN @LocationF AND @LocationT)
		and td.LocationID  between CASE ISNULL(@LocationF, 0)  WHEN 0 THEN td.LocationID ELSE @LocationF END
		and CASE ISNULL(@LocationT, 0)  WHEN 0 THEN td.LocationID ELSE @LocationT END			
		and p.DepartmentId =CASE ISNULL(@Department, 0)  WHEN 0 THEN p.DepartmentId ELSE @Department END
		and p.CategoryId =CASE ISNULL(@Category, 0)  WHEN 0 THEN p.CategoryId ELSE @Category END
		and p.SubCategoryId =CASE ISNULL(@SubCategory, 0)  WHEN 0 THEN p.SubCategoryId ELSE @SubCategory END
		and td.CustomerID =CASE ISNULL(@Customer, 0)  WHEN 0 THEN td.CustomerID ELSE @Customer END
		and l.CompanyID =CASE ISNULL(@CompanyId, 0)  WHEN 0 THEN td.CustomerID ELSE @CompanyId END
 
		UNION ALL
		
		SELECT Pd.Receipt,Pd.ZNo,'',0.00,0.00,0.00,0.00,'',Pd.Amount,0,Cast(GETDATE() as Date),'Round Off',Pd.LocationID,'',
		                                        0,
		                                        0,0.00,0.00 
		                                        , CONVERT(VARCHAR(20), GETDATE(), 101) AS [DATEPART],
         CONVERT(VARCHAR(20), GETDATE(), 108) AS TIMEPART,
         ISNULL(0.00,0),ISNULL(0.00,0),'',0.00,0.00,0.00
		                                FROM    PaymentDets Pd ( NOLOCK ) 
		                                        INNER JOIN SysLocations l ( NOLOCK ) ON l.SysLocationID = Pd.LocationID
		                                        INNER JOIN SysCompanies com ( NOLOCK ) ON com.SysCompanyID = l.CompanyID
						
		                                WHERE   Pd.PayTypeID = 67
		                                        AND Pd.Status = 1 
		                                        AND Pd.SaleTypeID = 1
		                                        AND Pd.BillTypeID = 1
		                                        AND CAST(Pd.SDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND cast(@ToDate as DATE)
		                                        
		
		END

	 IF @IsAsAtDate ='true' AND @Type='sales' 

 BEGIN

		SELECT  TD.Receipt,TD.ZNo,TD.ProductCode,TD.Cost,TD.AvgCost,TD.Qty,TD.IDI1,TD.UnitOfMeasureName,TD.Amount,TD.TaxAmount,cast(TD.recdate as date) as recdate,(SELECT ProductName FROM Products WHERE ProductID=TD.ProductID) ProductName, 
		td.LocationID, p.DepartmentId,p.CategoryId,p.SubCategoryId, (td.Nett - td.Cost*td.Qty) as GP,
		 CASE WHEN td.Nett = 0 THEN 0 ELSE (td.Nett - td.Cost*td.Qty)/td.Nett*100 end as GPPres ,
         CONVERT(VARCHAR(20), RecDate, 101) AS [DATEPART],
         CONVERT(VARCHAR(20), StartTime, 108) AS TIMEPART,
        ISNULL( (Select ISNULL(Nett,0) FROM TransactionDets TDD WHere TDD.Receipt = TD.Receipt 
		AND TDD.UnitNo = TD.UnitNo 
		AND TDD.LocationID = TD.LocationID 
			AND TDD.ZNo = TD.ZNo 
				AND TDD.ZDate = TD.ZDate 
				AND TDD.DocumentID IN (100)
				),0) AS  ServCharge,
        ISNULL( (Select ISNULL(Nett,0) FROM TransactionDets TDD WHere TDD.Receipt = TD.Receipt 
		AND TDD.UnitNo = TD.UnitNo 
		AND TDD.LocationID = TD.LocationID 
			AND TDD.ZNo = TD.ZNo 
				AND TDD.ZDate = TD.ZDate 
				AND TDD.DocumentID IN (101)
				),0) AS  ACCharge,(SELECT MAX(Descrip) FROM PayTypes WHERE Type=pay.PayTypeID) PayType,
				CASE WHEN pay.PayTypeID = 58 THEN pay.Balance ELSE 0 end as Online ,
				CASE WHEN pay.PayTypeID = 59 THEN pay.Balance ELSE 0 end as Uber ,
				CASE WHEN pay.PayTypeID = 60 THEN pay.Balance ELSE 0 end as Pickme 
				--CASE WHEN pay.PayTypeID = 58 THEN 'Online' ELSE ' ' end as Online ,
				--CASE WHEN pay.PayTypeID = 59 THEN 'Uber' ELSE ' ' end as Uber ,
				--CASE WHEN pay.PayTypeID = 62 THEN 'Pickme' ELSE ' ' end as Pickme 
		FROM TransactionDets TD 
		inner join Products p on td.ProductID = p.ProductId 
		inner join PaymentDets pay on td.Receipt = pay.Receipt And td.ZNo = pay.ZNo And td.LocationID = pay.LocationID  
		inner join RstDepartments d on p.DepartmentId = d.RstDepartmentID
		inner join RstCategories c on p.CategoryId = c.RstCategoryID
		inner join RstSubCategories s on p.SubCategoryId = s.RstSubCategoryID
		inner join SysLocations l on td.LocationID = l.SysLocationID
		inner join SysCompanies co on co.SysCompanyID = l.CompanyID
		WHERE (td.DocumentID = 1 OR
		td.DocumentID = 3) 
		AND (td.Status = 1) 
		AND (td.TransStatus = 1) 
		AND (td.SaleTypeID = 1) 
		AND (td.BillTypeID = 1) AND 
	
		CAST(td.RecDate AS DATE) <= @ToDate 
		and td.LocationID  between CASE ISNULL(@LocationF, 0)  WHEN 0 THEN td.LocationID ELSE @LocationF END
		and CASE ISNULL(@LocationT, 0)  WHEN 0 THEN td.LocationID ELSE @LocationT END
		and p.DepartmentId =CASE ISNULL(@Department, 0)  WHEN 0 THEN p.DepartmentId ELSE @Department END
		and p.CategoryId =CASE ISNULL(@Category, 0)  WHEN 0 THEN p.CategoryId ELSE @Category END
		and p.SubCategoryId =CASE ISNULL(@SubCategory, 0)  WHEN 0 THEN p.SubCategoryId ELSE @SubCategory END
		and td.CustomerID =CASE ISNULL(@Customer, 0)  WHEN 0 THEN td.CustomerID ELSE @Customer END
		and l.CompanyID =CASE ISNULL(@CompanyId, 0)  WHEN 0 THEN td.CustomerID ELSE @CompanyId END
		
		UNION ALL
		
		SELECT Pd.Receipt,Pd.ZNo,'',0.00,0.00,0.00,0.00,'',Pd.Amount,0,Cast(GETDATE() as Date),'Round Off',Pd.LocationID,'',
		                                        0,
		                                        0,0.00,0.00 --,Cast(GETDATE() as Date), CONVERT(time,getdate() )   --'15:37:04'
		                                        , CONVERT(VARCHAR(20), GETDATE(), 101) AS [DATEPART],
         CONVERT(VARCHAR(20), GETDATE(), 108) AS TIMEPART,
         ISNULL(0.00,0),ISNULL(0.00,0),'',0.00,0.00,0.00
		                                FROM    PaymentDets Pd ( NOLOCK ) 
		                                        INNER JOIN SysLocations l ( NOLOCK ) ON l.SysLocationID = Pd.LocationID
		                                        INNER JOIN SysCompanies com ( NOLOCK ) ON com.SysCompanyID = l.CompanyID
						
		                                WHERE   Pd.PayTypeID = 67
		                                        AND Pd.Status = 1 
		                                        AND Pd.SaleTypeID = 1
		                                        AND Pd.BillTypeID = 1
		                                        AND CAST(Pd.SDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND cast(@ToDate as DATE)
		
		END
END";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion sp_rpt_SalesRegistry

            #region SP_StockCAL
            spName = "SP_StockCAL";
            query = @"CREATE PROCEDURE [dbo].[SP_StockCAL]
    @CompanyId INT ,
    @SelectedLocationID INT ,
    @ToDate dateTime,
    @SetToCurrentStock bit =0 

AS 
     
    BEGIN
      
set dateformat dmy
  
		Print 'AA'
        CREATE TABLE #TmpStockTrans  
            (
              [LocationID] [bigint] NOT NULL ,
              [ToLocationName] [nvarchar](50) NULL ,
              [StockCode] [nvarchar](25)  NULL ,
              [BatchNo] [nvarchar](25) NULL ,
              [Qty] [decimal](18, 2) NOT NULL ,
              [CostPrice] [decimal](18, 2) ,
              [SellingPrice] [decimal](18, 2) ,
              [TransactionType] [nvarchar](50) NULL ,
              [TransactionNo] [nvarchar](20) NULL ,
              [TransactionDate] [DateTime],
              [ZNo] [int] NULL ,
              [UnitNo] [int] NULL 
            )
	
      

		/*--GRN  */ 
                INSERT  INTO #TmpStockTrans 
                        ( LocationID ,
                          StockCode ,
                          BatchNo ,
                          Qty ,
                          TransactionType ,
                          TransactionNo ,
                          TransactionDate ,
                          CostPrice ,
                          SellingPrice ,
                          ZNo ,
                          UnitNo
                        )
                        SELECT  ph.GRNLocationId  ,
                                pd.StockCode ,
                                pd.BatchNo ,
                                ( pd.GRNQuantity + pd.FreeQty ) AS QTY ,
                                'GRN' ,
                                ph.DocumentNo ,
                                --DATEADD(day, 0, DATEDIFF(day, 0, ph.DocumentDate)) + DATEADD(day, 0 - DATEDIFF(day, 0, ph.CreatedDate), ph.CreatedDate),
                                ph.DocumentDate,
                                pd.CostPrice ,
                                pd.SellingPrice ,
                                0 ,
                                0
                        FROM    PurchaseDetails pd
                                INNER JOIN PurchaseHeaders ph ON pd.PurchaseHeaderID = ph.PurchaseHeaderId
                                                              AND pd.DocumentNo = ph.DocumentNo
                        WHERE   ph.DocumentID = 4
                                AND ph.GRNLocationId = @SelectedLocationID
                                AND CAST(ph.DocumentDate AS DATE) <= @ToDate and ph.DocumentStatus=3
                                
                        ORDER BY ph.DocumentDate
		Print 'A'
 
		/*--Purchase Returns  */
                INSERT  INTO #TmpStockTrans 
                        ( LocationID ,
                          StockCode ,
                          BatchNo ,
                          Qty ,
                          TransactionType ,
                          TransactionNo ,
                          TransactionDate ,
                          CostPrice ,
                          SellingPrice ,
                          ZNo ,
                          UnitNo
                        )
                        SELECT  ph.GRNLocationId ,
                                pd.StockCode ,
                                pd.BatchNo ,
                                ( ( pd.GRNQuantity + pd.FreeQty ) * -1 ) AS QTY ,
                                'Purchase Returns' ,
                                ph.DocumentNo ,
                                --DATEADD(day, 0, DATEDIFF(day, 0, ph.DocumentDate)) + DATEADD(day, 0 - DATEDIFF(day, 0, ph.CreatedDate), ph.CreatedDate),
                                ph.DocumentDate ,
                                pd.CostPrice ,
                                pd.SellingPrice ,
                                0 ,
                                0
                        FROM    PurchaseDetails pd
                                INNER JOIN PurchaseHeaders ph ON pd.PurchaseHeaderID = ph.PurchaseHeaderId
                                                              AND pd.DocumentID = ph.DocumentID
                        WHERE    CAST(ph.DocumentDate  AS DATE) <=@ToDate                              
								AND ph.DocumentID = 6
								and ph.GRNLocationId =@SelectedLocationID  and ph.DocumentStatus=3
								
                        ORDER BY ph.DocumentDate
                        
		/*--TOG IN  */
                INSERT  INTO #TmpStockTrans 
                        ( LocationID ,
                          StockCode ,
                          BatchNo ,
                          Qty ,
                          TransactionType ,
                          TransactionNo ,
                          TransactionDate ,
                          CostPrice ,
                          SellingPrice ,
                          ToLocationName ,
                          ZNo ,
                          UnitNo
                        )
                        SELECT  @SelectedLocationID  ,
                                td.StockCode ,
                                td.BatchNo ,
                               td.OrderQty   ,
                                'TOG IN'  ,
                                th.DocumentNo ,
                                --DATEADD(day, 0, DATEDIFF(day, 0, th.DocumentDate)) + DATEADD(day, 0 - DATEDIFF(day, 0, th.CreatedDate), th.CreatedDate),
                                th.DocumentDate ,
                                td.CostPrice ,
                                td.SellingPrice ,
                                ISNULL(l.LocationName, '') ,
                                0,
                                0
                        FROM   TransferNoteDetails AS td INNER JOIN
                         TransferNoteHeaders AS th ON th.TransferNoteHeaderID = td.TransferNoteHeaderID INNER JOIN
                         SysLocations l ON th.ToLocationID = l.SysLocationID
                        WHERE   th.DocumentStatus=3 and
				  ( th.ToLocationID  = @SelectedLocationID )
                              and  CAST(th.DocumentDate AS DATE)  <=@ToDate
                                
                        ORDER BY th.DocumentDate

							/*--TOG OUT  */
                INSERT  INTO #TmpStockTrans 
                        ( LocationID ,
                          StockCode ,
                          BatchNo ,
                          Qty ,
                          TransactionType ,
                          TransactionNo ,
                          TransactionDate ,
                          CostPrice ,
                          SellingPrice ,
                          ToLocationName ,
                          ZNo ,
                          UnitNo
                        )
                        SELECT  @SelectedLocationID  ,
                                td.StockCode ,
                                td.BatchNo ,
                               td.OrderQty*-1   ,
                                'TOG OUT'  ,
                                th.DocumentNo ,
                                --DATEADD(day, 0, DATEDIFF(day, 0, th.DocumentDate)) + DATEADD(day, 0 - DATEDIFF(day, 0, th.CreatedDate), th.CreatedDate),
                                th.DocumentDate ,
                                td.CostPrice ,
                                td.SellingPrice ,
                                ISNULL(l.LocationName, '') ,
                                0,
                                0
                        FROM   TransferNoteDetails AS td INNER JOIN
                         TransferNoteHeaders AS th ON th.TransferNoteHeaderID = td.TransferNoteHeaderID INNER JOIN
                         SysLocations l ON th.FromLocationId = l.SysLocationID
                        WHERE   th.DocumentStatus=3 and
				  ( th.FromLocationId  = @SelectedLocationID )
                              and  CAST(th.DocumentDate AS DATE)  <=@ToDate
                                
                        ORDER BY th.DocumentDate
							
		print 'TOG'
								
		/*--Stock Adjustment (ADD, reduce and override all types) */
                INSERT  INTO #TmpStockTrans 
                        ( LocationID ,
                          StockCode ,
                          BatchNo ,
                          Qty ,
                          TransactionType ,
                          TransactionNo ,
                          TransactionDate ,
                          CostPrice ,
                          SellingPrice ,
                          ZNo ,
                          UnitNo
                        )
                        SELECT  SH.StockLocationId ,
                                PSM.StockCode  ,
                                0 ,
                                Case when ad.BaseType ='Add' then ad.AdjustStock
                                     when ad.BaseType ='Reduce' then -1* ad.AdjustStock 
                                     when ad.BaseType ='Override' then (ad.AdjustStock- ad.currentstock)
                                     else ad.AdjustStock  end
                                     
                                 ,
                                case when Ad.BaseType ='Add' then 'Stock Adjustment (ADD)' 
                                     when Ad.BaseType ='Reduce' then 'Stock Adjustment (Reduce)'
                                     When Ad.BaseType = 'Override' then 'Stock Adjustment (Override)' 
                                     else 'NA' end                    
                                      ,
                                sh.DocumentNo ,
								SH.CreatedDate ,
                                ad.CostPrice ,
                                ad.SellingPrice ,
                                0 ,
                                0
                        FROM    StockAdjustmentDetails ad
                                INNER JOIN StockAdjustmentHeaders sh ON sh.StockAdjustmentHeaderID = ad.StockAdjustmentHeaderID
                                INNER JOIN ProductStockMasters psm ON AD.ProductId = PSM.ProductId AND PSM.LocationId = SH.LocationId 
                        WHERE   
                        --ad.BaseType ='Add' and
                             --sh.DocumentStatus=3 and
                                 sh.StockLocationId = @SelectedLocationID
                                and CAST(sh.CreatedDate  AS DATE)  <=@ToDate
                               
                        ORDER BY sh.CreatedDate 
						print 'Stock Adjustments'
						
							 
		 						
		/*--Sales & Returns */	
                INSERT  INTO #TmpStockTrans 
                        ( LocationID ,
                          StockCode ,
                          BatchNo ,
                          Qty ,
                          TransactionType ,
                          TransactionNo ,
                          TransactionDate ,
                          CostPrice ,
                          SellingPrice ,
                          ZNo ,
                          UnitNo
                        )
                        SELECT  td.StockLocationID,
                                td.ProductCode ,
                                td.BatchNo ,
                                SUM(CASE DocumentID
                                      WHEN 1 THEN -(Qty)
                                      WHEN 3 THEN -(Qty)
                                      WHEN 2 THEN Qty
                                      WHEN 4 THEN Qty
                                      ELSE 0
                                    END) ,
                                'Sales & Returns',
                                td.Receipt ,
                                 
                                dateadd(second,datepart(second,td.endtime),
                                DATEADD(MINUTE ,DATEPART(MINUTE ,td.EndTime), 
                                dateadd(hh,DATEPART(hh,td.EndTime), td.RecDate))), --add end time to recdate
                                
                                td.Cost ,
                                td.Price ,
                                td.ZNo ,
                                td.UnitNo
                        FROM    TransactionDets td
                        WHERE  CAST(td.RecDate AS DATE)  <=@ToDate
								     
                                and td.StockLocationID = @SelectedLocationID 
                               
                        GROUP BY td.StockLocationID ,
                                td.ProductCode ,
                                td.BatchNo ,
                                    dateadd(second,datepart(second,td.endtime),
                                DATEADD(MINUTE ,DATEPART(MINUTE ,td.EndTime), 
                                dateadd(hh,DATEPART(hh,td.EndTime), td.RecDate))) ,
                                td.Cost ,
                                td.Price ,
                                td.Receipt ,
                                td.ZNo ,
                                td.UnitNo
                        ORDER BY dateadd(second,datepart(second,td.endtime),
                                DATEADD(MINUTE ,DATEPART(MINUTE ,td.EndTime), 
                                dateadd(hh,DATEPART(hh,td.EndTime), td.RecDate)))	
                                
                                
                                
/*--PRODUCTION NOTE ADD  */ 
                INSERT  INTO #TmpStockTrans 
                        ( LocationID ,
                          StockCode ,
                          BatchNo ,
                          Qty ,
                          TransactionType ,
                          TransactionNo ,
                          TransactionDate ,
                          CostPrice ,
                          SellingPrice ,
                          ZNo ,
                          UnitNo
                        )
                        SELECT distinct ph.ProductionLocId    ,
                                p.ProductCode  ,
                                '',
                                pd.ProductQty  AS QTY ,
                                'PRODUCTION-ADD' ,
                                ph.DocumentNo ,
                                --DATEADD(day, 0, DATEDIFF(day, 0, ph.DocumentDate)) + DATEADD(day, 0 - DATEDIFF(day, 0, ph.CreatedDate), ph.CreatedDate),
                                ph.CreatedDate ,
                                pd.ProductCostPrice  ,
                                pd.ProductSellingPrice  ,
                                0 ,
                                0
                        FROM    (select  PD.ProductionNoteHeaderId, ProductID ,pd.ProductCostPrice  ,
                                 pd.ProductSellingPrice  , sum(ProductQty) as ProductQty 
                                 From ProductionNoteDetails PD
                                 where PD.productQty<>0
                                 group by pd.ProductionNoteHeaderId, PD.productID,
                                 pd.ProductCostPrice  ,pd.ProductSellingPrice 
                                 ) PD inner join 
                                ProductionNoteHeaders ph 
                                on pd.ProductionNoteHeaderId =ph.ProductionNoteHeaderId 
                                inner join Products P on p.ProductId = pd.ProductId 
                        WHERE ph.ProductionLocId   = @SelectedLocationID
                                AND CAST(ph.CreatedDate AS DATE) <= @ToDate
                    
                        ORDER BY ph.CreatedDate                                 	
	
	/*--PRODUCTION NOTE REDUCE  */ 
                INSERT  INTO #TmpStockTrans 
                        ( LocationID ,
                          StockCode ,
                          BatchNo ,
                          Qty ,
                          TransactionType ,
                          TransactionNo ,
                          TransactionDate ,
                          CostPrice ,
                          SellingPrice ,
                          ZNo ,
                          UnitNo
                        )
                        SELECT distinct ph.ProductionLocId    ,
                                p.ProductCode  ,
                                '',
                                pd.MaterialQty * -1  AS QTY ,
                                'PRODUCTION-CONSUME' ,
                                ph.DocumentNo ,
                                --DATEADD(day, 0, DATEDIFF(day, 0, ph.DocumentDate)) + DATEADD(day, 0 - DATEDIFF(day, 0, ph.CreatedDate), ph.CreatedDate),
                                ph.CreatedDate ,
                                pd.CostPrice ,
                                pd.SellingPrice ,
                                0 ,
                                0
                        FROM    (select  PD.ProductionNoteHeaderId, MaterialId  ,pd.CostPrice ,
                                 pd.SellingPrice , sum(MaterialQty) as MaterialQty 
                                 From ProductionNoteDetails PD
                                 where PD.MaterialQty <>0
                                 group by pd.ProductionNoteHeaderId, PD.MaterialId ,
                                 pd.CostPrice  ,pd.SellingPrice
                                 ) PD inner join 
                                ProductionNoteHeaders ph 
                                on pd.ProductionNoteHeaderId =ph.ProductionNoteHeaderId 
                                inner join Products P on p.ProductId = pd.MaterialId  
                        WHERE ph.ProductionLocId   = @SelectedLocationID
                                AND CAST(ph.CreatedDate AS DATE) <= @ToDate
 
                        ORDER BY ph.CreatedDate 	



               if @SetToCurrentStock=1
                  begin
                  update PSM set PSM.stock =isnull( Stock.StockQty ,0) from productStockMasters PSM Left join 
                    (select stockCode,sum(Qty) as StockQty From #TmpStockTrans
                    group by stockCode) STOCK 
                    ON Stock.StockCode  = PSM.stockCode and PSM.locationid=@SelectedLocationID
                    where   PSM.locationid=@SelectedLocationID
                    
                  end
               else
                  begin
                  SELECT p.ProductCode  ,@SelectedLocationID as LocationID  ,p.PRODUCTNAME,SUM(ISNULL(stock.Qty,0)) AS STOCK 
		          FROM PRODUCTS p LEFT JOIN #TmpStockTrans STOCK ON P.ProductCode  = STOCK.StockCode
		          GROUP BY  p.ProductCode ,p.PRODUCTNAME
                  end

				END";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_StockCAL

            #region SPIsValidMonthEnd
            spName = "SPIsValidMonthEnd";
            query = @"CREATE PROCEDURE [dbo].[SPIsValidMonthEnd]
    @LocationID INT ,
    @Year INT ,
    @Month INT 
AS 
    SET NOCOUNT ON
    SET XACT_ABORT ON

    BEGIN TRY

        DECLARE @reccount int = 0 ,@DocNumbers varchar(max),@DOCNO varchar(20)
			
		--8. Temporary saved GRN--------------------------------		
		truncate table tmpMonthEnds
			
        SELECT @reccount = ISNULL(COUNT(PurchaseHeaders.DocumentID), 0)
		FROM  PurchaseHeaders
		WHERE (GRNLocationID = @LocationID AND IsTempGRN=1 and DocumentStatus=1 and DocumentID=4
		and  year(GRNDate)=@Year and MONTH(grndate)=@Month)
		
		if @reccount<>0 
		begin
			set @DocNumbers=''
			DECLARE cur CURSOR
			FOR
				SELECT  DocumentNo	FROM PurchaseHeaders
				WHERE (GRNLocationID = @LocationID AND IsTempGRN=1 and DocumentStatus=1 and DocumentID=4
				and  year(GRNDate)=@Year and MONTH(grndate)=@Month)

			OPEN cur
			FETCH NEXT FROM cur INTO @DOCNO

			WHILE @@fetch_status = 0 
				BEGIN
					if @DocNumbers =''
					begin
						set @DocNumbers=@DOCNO
					end
					else
					begin
						set @DocNumbers= @DocNumbers + ' , ' + @DOCNO
					end
					
					FETCH NEXT FROM cur INTO @DOCNO
	            
			 END
            
			insert	INTO    tmpMonthEnds
			SELECT  SysLocationID,8 as DocumentType,@DocNumbers as Message,@reccount as DocumentCount
			FROM    SysLocations where SysLocationID=@LocationID
	 
			SELECT SysLocationID,DocumentType,Message,DocumentCount  FROM  tmpMonthEnds
			return
		end
		
		--9.approval pending GRN--------------------------------

        SELECT @reccount = ISNULL(COUNT(PurchaseHeaders.DocumentID), 0)
		FROM  PurchaseHeaders
		WHERE (GRNLocationID = @LocationID AND IsTempGRN=0 and DocumentStatus=2  and DocumentID=4
		and  year(GRNDate)=@Year and MONTH(grndate)=@Month)
		
		if @reccount<>0 
		begin
			set @DocNumbers=''
			DECLARE cur CURSOR
			FOR
				SELECT  DocumentNo	FROM PurchaseHeaders
				WHERE (GRNLocationID = @LocationID AND IsTempGRN=0 and DocumentStatus=2  and DocumentID=4
				and  year(GRNDate)=@Year and MONTH(grndate)=@Month)

			OPEN cur
			FETCH NEXT FROM cur INTO @DOCNO

			WHILE @@fetch_status = 0 
				BEGIN
					if @DocNumbers =''
					begin
						set @DocNumbers=@DOCNO
					end
					else
					begin
						set @DocNumbers= @DocNumbers + ' , ' + @DOCNO
					end
					
					FETCH NEXT FROM cur INTO @DOCNO
	            
			 END
			 
			insert	INTO    tmpMonthEnds
			SELECT  SysLocationID,9 as DocumentType,@DocNumbers as Message,@reccount as DocumentCount
			FROM    SysLocations where SysLocationID=@LocationID
	 
			SELECT SysLocationID,DocumentType,Message,DocumentCount  FROM  tmpMonthEnds
			return
		end
		
		--10. Reopened GRN--------------------------------					
        SELECT @reccount = ISNULL(COUNT(PurchaseHeaders.DocumentID), 0)
		FROM  PurchaseHeaders
		WHERE (GRNLocationID = @LocationID AND DocumentStatus=6 and DocumentID=4
		and  year(GRNDate)=@Year and MONTH(grndate)=@Month)
		
		if @reccount<>0 
		begin
			set @DocNumbers=''
			DECLARE cur CURSOR
			FOR
				SELECT  DocumentNo	FROM PurchaseHeaders
				WHERE (GRNLocationID = @LocationID AND DocumentStatus=6 and DocumentID=4
				and  year(GRNDate)=@Year and MONTH(grndate)=@Month)

			OPEN cur
			FETCH NEXT FROM cur INTO @DOCNO

			WHILE @@fetch_status = 0 
				BEGIN
					if @DocNumbers =''
					begin
						set @DocNumbers=@DOCNO
					end
					else
					begin
						set @DocNumbers= @DocNumbers + ' , ' + @DOCNO
					end
					
					FETCH NEXT FROM cur INTO @DOCNO
	            
			 END
            
			insert	INTO    tmpMonthEnds
			SELECT  SysLocationID,10 as DocumentType,@DocNumbers as Message,@reccount as DocumentCount
			FROM    SysLocations where SysLocationID=@LocationID
	 
			SELECT SysLocationID,DocumentType,Message,DocumentCount  FROM  tmpMonthEnds
			return
		end
		
		--11.temporary saved PRN--------------------------------

        SELECT @reccount = ISNULL(COUNT(PurchaseHeaders.DocumentID), 0)
		FROM  PurchaseHeaders
		WHERE (GRNLocationID = @LocationID AND IsTempPRN=1 and DocumentStatus=1 and DocumentID=6
		and  year(GRNDate)=@Year and MONTH(grndate)=@Month)
		
		if @reccount<>0 
		begin
			set @DocNumbers=''
			DECLARE cur CURSOR
			FOR
				SELECT  DocumentNo	FROM PurchaseHeaders
				WHERE (GRNLocationID = @LocationID AND IsTempPRN=1 and DocumentStatus=1 and DocumentID=6
			and  year(GRNDate)=@Year and MONTH(grndate)=@Month)

			OPEN cur
			FETCH NEXT FROM cur INTO @DOCNO

			WHILE @@fetch_status = 0 
				BEGIN
					if @DocNumbers =''
					begin
						set @DocNumbers=@DOCNO
					end
					else
					begin
						set @DocNumbers= @DocNumbers + ' , ' + @DOCNO
					end
					
					FETCH NEXT FROM cur INTO @DOCNO
	            
			 END
			 
			insert	INTO    tmpMonthEnds
			SELECT  SysLocationID,11 as DocumentType,@DocNumbers as Message,@reccount as DocumentCount
			FROM    SysLocations where SysLocationID=@LocationID
	 
			SELECT SysLocationID,DocumentType,Message,DocumentCount  FROM  tmpMonthEnds
			return
		end
		
		--12.Approval pending PRN--------------------------------

        SELECT @reccount = ISNULL(COUNT(PurchaseHeaders.DocumentID), 0)
		FROM  PurchaseHeaders
		WHERE 
		(GRNLocationID = @LocationID AND IsTempPRN=0 and DocumentStatus=2  and DocumentID=6
		and  year(GRNDate)=@Year and MONTH(grndate)=@Month)
		
		if @reccount<>0 
		begin
			set @DocNumbers=''
			DECLARE cur CURSOR
			FOR
			SELECT  DocumentNo	FROM PurchaseHeaders
			WHERE (GRNLocationID = @LocationID AND IsTempPRN=0 and DocumentStatus=2  and DocumentID=6
			and  year(GRNDate)=@Year and MONTH(grndate)=@Month)

			OPEN cur
			FETCH NEXT FROM cur INTO @DOCNO

			WHILE @@fetch_status = 0 
				BEGIN
					if @DocNumbers =''
					begin
						set @DocNumbers=@DOCNO
					end
					else
					begin
						set @DocNumbers= @DocNumbers + ' , ' + @DOCNO
					end
					
					FETCH NEXT FROM cur INTO @DOCNO
	            
			 END
			 
			insert	INTO    tmpMonthEnds
			SELECT  SysLocationID,12 as DocumentType,@DocNumbers as Message,@reccount as DocumentCount
			FROM    SysLocations where SysLocationID=@LocationID
	 
			SELECT SysLocationID,DocumentType,Message,DocumentCount  FROM  tmpMonthEnds
			return
		end
		
		--13.Reopened PRN --------------------------------			
		SELECT @reccount = ISNULL(COUNT(PurchaseHeaders.DocumentID), 0)
		FROM  PurchaseHeaders
		WHERE 
		(GRNLocationID = @LocationID AND DocumentStatus=6  and DocumentID=6
		and  year(GRNDate)=@Year and MONTH(grndate)=@Month)
		
		if @reccount<>0 
		begin
			set @DocNumbers=''
			DECLARE cur CURSOR
			FOR
			SELECT  DocumentNo	FROM PurchaseHeaders
			WHERE (GRNLocationID = @LocationID AND DocumentStatus=6  and DocumentID=6
			and  year(GRNDate)=@Year and MONTH(grndate)=@Month)

			OPEN cur
			FETCH NEXT FROM cur INTO @DOCNO

			WHILE @@fetch_status = 0 
				BEGIN
					if @DocNumbers =''
					begin
						set @DocNumbers=@DOCNO
					end
					else
					begin
						set @DocNumbers= @DocNumbers + ' , ' + @DOCNO
					end
					
					FETCH NEXT FROM cur INTO @DOCNO
	            
			 END
			 
			insert	INTO    tmpMonthEnds
			SELECT  SysLocationID,13 as DocumentType,@DocNumbers as Message,@reccount as DocumentCount
			FROM    SysLocations where SysLocationID=@LocationID
	 
			SELECT SysLocationID,DocumentType,Message,DocumentCount  FROM  tmpMonthEnds
			return
		end
		
		--14.temporary saved TOG--------------------------------
				
        SELECT @reccount = ISNULL(COUNT(TransferNoteHeaders.TransferNoteHeaderID), 0)
		FROM  TransferNoteHeaders
		WHERE (FromLocationID = @LocationID AND IsTempTOG=1 and DocumentStatus=1
		and  year(TOGDate)=@Year and MONTH(TOGDate)=@Month)
		
		if @reccount<>0 
		begin
			set @DocNumbers=''
			DECLARE cur CURSOR
			FOR
			SELECT  DocumentNo	FROM TransferNoteHeaders
			WHERE (FromLocationID = @LocationID AND IsTempTOG=1 and DocumentStatus=1
			and  year(TOGDate)=@Year and MONTH(TOGDate)=@Month)

			OPEN cur
			FETCH NEXT FROM cur INTO @DOCNO

			WHILE @@fetch_status = 0 
				BEGIN
					if @DocNumbers =''
					begin
						set @DocNumbers=@DOCNO
					end
					else
					begin
						set @DocNumbers= @DocNumbers + ' , ' + @DOCNO
					end
					
					FETCH NEXT FROM cur INTO @DOCNO
	            
			 END
			 
			insert	INTO    tmpMonthEnds
			SELECT  SysLocationID,14 as DocumentType,@DocNumbers as Message,@reccount as DocumentCount
			FROM    SysLocations where SysLocationID=@LocationID
	 
			SELECT SysLocationID,DocumentType,Message,DocumentCount  FROM  tmpMonthEnds
			return
		end
		
		--15.approval pending TOG--------------------------------
				
        SELECT @reccount = ISNULL(COUNT(TransferNoteHeaders.TransferNoteHeaderID), 0)
		FROM  TransferNoteHeaders
		WHERE (FromLocationID = @LocationID AND IsTempTOG=0 and DocumentStatus=2
		and  year(TOGDate)=@Year and MONTH(TOGDate)=@Month)
		
		if @reccount<>0 
		begin
			set @DocNumbers=''
			DECLARE cur CURSOR
			FOR
			SELECT  DocumentNo	FROM TransferNoteHeaders
			WHERE (FromLocationID = @LocationID AND IsTempTOG=0 and DocumentStatus=2
			and  year(TOGDate)=@Year and MONTH(TOGDate)=@Month)

			OPEN cur
			FETCH NEXT FROM cur INTO @DOCNO

			WHILE @@fetch_status = 0 
				BEGIN
					if @DocNumbers =''
					begin
						set @DocNumbers=@DOCNO
					end
					else
					begin
						set @DocNumbers= @DocNumbers + ' , ' + @DOCNO
					end
					
					FETCH NEXT FROM cur INTO @DOCNO
	            
			 END
			 
			insert	INTO    tmpMonthEnds
			SELECT  SysLocationID,15 as DocumentType,@DocNumbers as Message,@reccount as DocumentCount
			FROM    SysLocations where SysLocationID=@LocationID
	 
			SELECT SysLocationID,DocumentType,Message,DocumentCount  FROM  tmpMonthEnds
			return
		end
		
		--16.reopened TOG--------------------------------
				
        SELECT @reccount = ISNULL(COUNT(TransferNoteHeaders.TransferNoteHeaderID), 0)
		FROM  TransferNoteHeaders
		WHERE (FromLocationID = @LocationID AND DocumentStatus=6
		and  year(TOGDate)=@Year and MONTH(TOGDate)=@Month)
		
		if @reccount<>0 
		begin
			set @DocNumbers=''
			DECLARE cur CURSOR
			FOR
			SELECT  DocumentNo	FROM TransferNoteHeaders
			WHERE (FromLocationID = @LocationID AND DocumentStatus=6
			and  year(TOGDate)=@Year and MONTH(TOGDate)=@Month)

			OPEN cur
			FETCH NEXT FROM cur INTO @DOCNO

			WHILE @@fetch_status = 0 
				BEGIN
					if @DocNumbers =''
					begin
						set @DocNumbers=@DOCNO
					end
					else
					begin
						set @DocNumbers= @DocNumbers + ' , ' + @DOCNO
					end
					
					FETCH NEXT FROM cur INTO @DOCNO
	            
			 END
			 
			insert	INTO    tmpMonthEnds
			SELECT  SysLocationID,16 as DocumentType,@DocNumbers as Message,@reccount as DocumentCount
			FROM    SysLocations where SysLocationID=@LocationID
	 
			SELECT SysLocationID,DocumentType,Message,DocumentCount  FROM  tmpMonthEnds
			return
		end
		
		insert	INTO    tmpMonthEnds
		SELECT  SysLocationID as SysLocationID,7 as DocumentType,'NO PENDINGS' as Message,@reccount as DocumentCount
		FROM    SysLocations where SysLocationID=@LocationID
	 
		SELECT SysLocationID,DocumentType,Message,DocumentCount  FROM  tmpMonthEnds
			
    END TRY
  
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            BEGIN
                --ROLLBACK TRANSACTION
                SELECT  ERROR_MESSAGE() AS Result

            END
        ELSE 
            BEGIN
                SELECT  ERROR_MESSAGE() AS Result
            END
    END CATCH ";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SPIsValidMonthEnd

            #region spRegisterLoyaltyCustomer
            spName = "spRegisterLoyaltyCustomer";
            query = @"CREATE PROCEDURE [dbo].[spRegisterLoyaltyCustomer]
    @CardNo			NVARCHAR(MAX)  = '',
    @CardType		BIGINT  = 1,
    @CashierID		BIGINT  = 1,
    @LocationID		INT  = 1,    
    @Name			VARCHAR(60) = '',
    @Code			VARCHAR(20) = '',
    
    @NicNo			VARCHAR(20) = '',
    @DOB			DATETIME='9999-12-31',
    @Address		VARCHAR(50) = '',
    @Address2		VARCHAR(50) = '',    
    @Address3		VARCHAR(50) = '',
    @MobileNumber	VARCHAR(20) = '',
    @Email			VARCHAR(200) = '',    
    @Gender			INT = 1,
    @Organization	VARCHAR(50) = '',
    @Occupation		VARCHAR(50) = '',
    @LastName		VARCHAR(200) = '',
    @Country		VARCHAR(50) = ''  
           
AS 
    DECLARE @GroupOfCompanyID INT ,
        @Title INT ,
        @CardMasterID INT,@CompanyID int,
        @CustomerID int
	 
    SET NOCOUNT ON

    BEGIN TRY
        BEGIN TRANSACTION 

        IF NOT EXISTS ( SELECT  CardNo
                        FROM    LoyaltyCustomers
                        WHERE   CardNo = @CardNo
                                --OR NicNo = @NicNo 
                                ) 
            BEGIN


                IF LEFT(@CardNo, 1) = '9'
                    BEGIN

                        SET @CardType = 3
                        SELECT TOP 1
                                @CardMasterID = CardMasterID
                        FROM    CardMasters
                        WHERE   CardType = @CardType
                                AND Discount = 0
                        ORDER BY PointValue ASC

                    END
                ELSE 
                    BEGIN

                        SET @CardType = 2
                        SELECT TOP 1
                                @CardMasterID = CardMasterID
                        FROM    CardMasters
                        WHERE   CardType = @CardType
                                AND Discount = 0
                        ORDER BY PointValue ASC

                    END
	
                SELECT  @GroupOfCompanyID = SysGroupOfCompanyId
                FROM    SysGroupOfCompanies
                
                SELECT  @CompanyID = CompanyID
                FROM    SysLocations where SysLocationID=@LocationID
                
                SELECT  @CustomerID = CustomerID
                FROM    Customers where CustomerCode=@Code and CompanyID=@CompanyID
	

                SELECT TOP 1 @Title = LookupKey
                FROM    ReferenceTypes
                WHERE   LookupType = 1
                ORDER BY LookupValue ASC
	

                INSERT  INTO LoyaltyCustomers
                        ( [CardNo] ,[CustomerId] ,[CardMasterID] ,[CardIssued] ,[IssuedOn] ,[ExpiryDate] ,
                          [RenewedOn] ,[LedgerId] ,[LedgerId2] ,[CreditLimit] , [CreditPeriod] ,
                          [GroupOfCompanyID] , [CreatedDate] , [ModifiedDate] , [DataTransfer] ,
                          [CPoints] , [EPoints] ,  [RPoints] , [IsReDimm] , [AcitiveDate] ,
                          [CashierID] ,LocationID ,LoyaltyType ,
                          NameOnCard,IsCardIssued,ExpiryPoints,ExpiryPoints1,IsSold,Discount,
                          LastUpdatedLocId,Status,CompanyId )
                VALUES  ( @CardNo , @CustomerID , @CardMasterID , 1 ,GETDATE() , DATEADD(mm, 12, GETDATE()) ,
                          GETDATE() ,0,0,0,0,
                          @GroupOfCompanyID ,GETDATE() ,GETDATE() ,0 ,
                          0 ,0,0,1,GETDATE() ,
                          @CashierID ,@LocationID ,@CardType ,
                          @Name,1,0,0,1,0,
                          @LocationID,1,@CompanyID )
            END
        COMMIT TRANSACTION;
        SELECT  '0' AS Result
    END TRY
  
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            BEGIN
                ROLLBACK TRANSACTION
                SELECT  ERROR_MESSAGE() AS Result

            END
        ELSE 
            BEGIN
                SELECT  ERROR_MESSAGE() AS Result
            END
    END CATCH";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion spRegisterLoyaltyCustomer

            #region spUpdateAdvanceNoteDetails
            spName = "spUpdateAdvanceNoteDetails";
            query = @"CREATE PROCEDURE [dbo].[spUpdateAdvanceNoteDetails]
                        @AdNoteNo CHAR(12) ,
                        @Receipt CHAR(12) ,
                        @Amount DECIMAL(18, 4) ,
                        @Balance DECIMAL(18, 4) ,
                        @LocationID INT ,
                        @UnitNo INT ,
                        @CashierID BIGINT ,
                        @Zno BIGINT ,
                        @TransID INT,
                        @AdvDet AdvanceDet READONLY,
                        @DeliveryDate DATETIME = '1992-09-29',
                        @Remark VARCHAR(150)= '',
                        @AdvancePayTypeID BIGINT=0,
                        @AdvancePayRefNo VARCHAR(30) = '',
                        @BankID BIGINT = 0,
                        @ChequeDate DATE = NULL,
                        @AdvPaymentDet AdvancePaymentDet READONLY,
                        @PickupLocID int=0,
                        @RecallFromInvoice INT =0

                        AS  
                        SET NOCOUNT ON

                        BEGIN TRY
                            BEGIN TRANSACTION 

                            IF @TransID = 1 
	                        BEGIN        
                                  IF NOT EXISTS ( SELECT  AdNoteNo
                                                    FROM    InvAdvanceNoteHeds
                                                    WHERE   AdNoteNo = @AdNoteNo ) 
                                        BEGIN   
                                           INSERT  INTO InvAdvanceNoteHeds
                                                    ( [AdNoteNo] ,
                                                      [Receipt] ,
                                                      [Amount] ,
                                                      [Balance] ,
                                                      [LocationID] ,
                                                      [Date] ,
                                                      [UnitNo] ,
                                                      [CashierID] ,
                                                      [Time] ,
                                                      [Zno],
                                                      DeliveryDate,
                                                      Remark,RecallFromInvoice,ProcessLoc,PickupLoc
	                                                )
                                            VALUES  ( @AdNoteNo ,
                                                      @Receipt ,
                                                      @Amount ,
                                                      @Balance ,
                                                      @LocationID ,
                                                      GETDATE() ,
                                                      @UnitNo ,
                                                      @CashierID ,
                                                      GETDATE() ,
                                                      @Zno,
                                                      @DeliveryDate,
                                                      @Remark,
                                                      @RecallFromInvoice,@LocationID,@PickupLocID
	                                                )
		           
		                                   INSERT INTO [InvAdvanceNoteDets]
							                           ([ProductID]
							                           ,[ProductCode]
							                           ,[RefCode]
							                           ,[BarCodeFull]
							                           ,[Descrip]
							                           ,[BatchNo]
							                           ,[SerialNo]
							                           ,[ExpiryDate]
							                           ,[Cost]
							                           ,[AvgCost]
							                           ,[Price]
							                           ,[Qty]
							                           ,[Amount]
							                           ,[UnitOfMeasureID]
							                           ,[UnitOfMeasureName]
							                           ,[ConvertFactor]
							                           ,[IDI1]
							                           ,[IDis1]
							                           ,[IDiscount1]
							                           ,[IDI1CashierID]
							                           ,[IDI2]
							                           ,[IDis2]
							                           ,[IDiscount2]
							                           ,[IDI2CashierID]
							                           ,[IDI3]
							                           ,[IDis3]
							                           ,[IDiscount3]
							                           ,[IDI3CashierID]
							                           ,[IDI4]
							                           ,[IDis4]
							                           ,[IDiscount4]
							                           ,[IDI4CashierID]
							                           ,[IDI5]
							                           ,[IDis5]
							                           ,[IDiscount5]
							                           ,[IDI5CashierID]
							                           ,[Rate]
							                           ,[IsSDis]
							                           ,[SDNo]
							                           ,[SDID]
							                           ,[SDIs]
							                           ,[SDiscount]
							                           ,[DDisCashierID]
							                           ,[Nett]
							                           ,[LocationID]
							                           ,[DocumentID]
							                           ,[BillTypeID]
							                           ,[SaleTypeID]
							                           ,[Receipt]
							                           ,[SalesmanID]
							                           ,[Salesman]
							                           ,[CustomerID]
							                           ,[Customer]
							                           ,[CashierID]
							                           ,[Cashier]
							                           ,[StartTime]
							                           ,[EndTime]
							                           ,[RecDate]
							                           ,[BaseUnitID]
							                           ,[UnitNo]
							                           ,[RowNo]
							                           ,[IsRecall]
							                           ,[RecallNo]
							                           ,[RecallAdv]
							                           ,[TaxAmount]
							                           ,[IsTax]
							                           ,[TaxPercentage]
							                           ,[IsStock]
							                           ,[CreditNoteNo]
							                           ,[CreditNoteBy]
							                           ,[CustomerType]
							                           ,[TransStatus]
							                           ,[IsPromotionApplied]
							                           ,[PromotionID]
							                           ,[IsPromotion]
							                           ,[ItemSerial]
							                           ,[warranty]
							                           ,[RecallFromInvoiceNo]
							                           ,IsNewPrice
							                           ,IsApproved
							                           ,ApprovedBy
							                           ,ApprovedFor,ReferenceProductId,ReferenceProductRow,
                                                        PrinterType,IsAddonItem,TableNumber ,IsTaxEnable ,TaxCode ,SplitItemReceiptNo ,IsPritRpt ,
                                                        ProductRemark ,OrderStatus ,ServingUnit ,NoOfCustomers ,IsShowOnBill ,DeploCardNo ,ServingUnitId )
						                           SELECT 
								                        [ProductID]
							                           ,[ProductCode]
							                           ,[RefCode]
							                           ,[BarCodeFull]
							                           ,[Descrip]
							                           ,[BatchNo]
							                           ,[SerialNo]
							                           ,[ExpiaryDate]
							                           ,[Cost]
							                           ,[AvgCost]
							                           ,[Price]
							                           ,[Qty]
							                           ,[Amount]
							                           ,[UnitOfMeasureID]
							                           ,[UnitOfMeasureName]
							                           ,[ConvertFactor]
							                           ,[IDI1]
							                           ,[IDis1]
							                           ,[IDiscount1]
							                           ,[IDI1CashierID]
							                           ,[IDI2]
							                           ,[IDis2]
							                           ,[IDiscount2]
							                           ,[IDI2CashierID]
							                           ,[IDI3]
							                           ,[IDis3]
							                           ,[IDiscount3]
							                           ,[IDI3CashierID]
							                           ,[IDI4]
							                           ,[IDis4]
							                           ,[IDiscount4]
							                           ,[IDI4CashierID]
							                           ,[IDI5]
							                           ,[IDis5]
							                           ,[IDiscount5]
							                           ,[IDI5CashierID]
							                           ,[Rate]
							                           ,[IsSDis]
							                           ,[SDNo]
							                           ,[SDID]
							                           ,[SDIs]
							                           ,[SDiscount]
							                           ,[DDisCashierID]
							                           ,[Nett]
							                           ,[LocationID]
							                           ,[DocumentID]
							                           ,[BillTypeID]
							                           ,[SaleTypeID]
							                           ,[Receipt]
							                           ,[SalesmanID]
							                           ,[Salesman]
							                           ,[CustomerID]
							                           ,[Customer]
							                           ,[CashierID]
							                           ,[Cashier]
							                           ,[StartTime]
							                           ,[EndTime]
							                           ,[RecDate]
							                           ,[BaseUnitID]
							                           ,[UnitNo]
							                           ,[RowNo]
							                           ,[IsRecall]
							                           ,[RecallNo]
							                           ,[RecallAdv]
							                           ,[TaxAmount]
							                           ,[IsTax]
							                           ,[TaxPercentage]
							                           ,[IsStock]
							                           ,[CreditNoteNo]
							                           ,[CreditNoteBy]
							                           ,[CustomerType]
							                           ,[TransStatus]
							                           ,[IsPromotionApplied]
							                           ,[PromotionID]
							                           ,[IsPromotion]
							                           ,[ItemSerial]
							                           ,[warranty]
							                           ,RecallFromInvoiceNo
							                           ,IsNewPrice
							                           ,IsApproved
							                           ,ApprovedBy
							                           ,ApprovedFor ,ReferenceProductId,ReferenceProductRow,
                                                        PrinterType,IsAddonItem,TableNumber ,IsTaxEnable ,TaxCode ,SplitItemReceiptNo ,IsPritRpt ,ProductRemark ,OrderStatus ,ServingUnit ,NoOfCustomers ,IsShowOnBill ,DeploCardNo ,ServingUnitId 
							                        FROM @AdvDet  /*--WHERE CreditNoteNo = @AdNoteNo AND [LocationID] = @LocationID AND UnitNo = @UnitNo   */    
					
				                           INSERT  INTO InvAdvancePaymentDets           
					                        ( [Idx],
					                          [RowNo] ,
					                          [PayTypeID] ,
					                          [Amount] ,
					                          [Balance] ,
					                          [SDate] ,
					                          [Receipt] ,
					                          [LocationID] ,
					                          [CashierID] ,
					                          [UnitNo] ,
					                          [BillTypeID] ,
					                          [RefNo] ,
					                          [BankId] ,
					                          [ChequeDate] ,
					                          [IsRecallAdv] ,
					                          [RecallNo] ,
					                          [Descrip] ,
					                          [EnCodeName] ,
					                          SuspendNo ,
					                          SuspendBy,
					                          AdvanceNumber,
					                          IsDeleteOnRecall
				                           )
				                           SELECT
				                              [Idx],
				                              [RowNo] ,
					                          [PayTypeID] ,
					                          [Amount] ,
					                          [Balance] ,
					                          [SDate] ,
					                          [Receipt] ,
					                          [LocationID] ,
					                          [CashierID] ,
					                          [UnitNo] ,
					                          [BillTypeID] ,
					                          [RefNo] ,
					                          [BankId] ,
					                          [ChequeDate] ,
					                          [IsRecallAdv] ,
					                          [RecallNo] ,
					                          [Descrip] ,
					                          [EnCodeName] ,
					                          SuspendNo ,
					                          SuspendBy,
					                          AdvanceNumber,
					                          IsDeleteOnRecall
				                           FROM @AdvPaymentDet
				   					
				                           SET @Balance = @Balance - (SELECT ISNULL(SUM(Amount),0) FROM InvAdvanceNoteHeds WHERE AdNoteNo IN (SELECT LEFT(SerialNo, 12) FROM InvAdvanceNoteDets WHERE LEFT(SerialNo, 12) != @AdNoteNo AND RecallFromInvoiceNo IN (SELECT DISTINCT CASE RecallFromInvoiceNo WHEN '' THEN 'ALL' ELSE RecallFromInvoiceNo END FROM InvAdvanceNoteDets WHERE LEFT(CreditNoteNo, 12) = @AdNoteNo) OR CreditNoteNo IN (SELECT DISTINCT CASE RecallFromInvoiceNo WHEN '' THEN 'ALL' ELSE RecallFromInvoiceNo END FROM InvAdvanceNoteDets WHERE LEFT(CreditNoteNo, 12) = @AdNoteNo)))
                   
                                           UPDATE InvAdvanceNoteHeds SET Balance = @Balance WHERE AdNoteNo = @AdNoteNo                   
			                        END
                                END
                            ELSE 
                                IF @TransID = 2 
                                    BEGIN

                                        UPDATE  InvAdvanceNoteHeds
                                        SET     RecallFromInvoice = 1
                                        WHERE   AdNoteNo = @AdNoteNo
	
			                            SET @Balance = @Balance - (SELECT ISNULL(SUM(Amount),0) FROM InvAdvanceNoteHeds WHERE AdNoteNo IN (SELECT LEFT(SerialNo, 12) FROM InvAdvanceNoteDets WHERE LEFT(SerialNo, 12) != @AdNoteNo AND RecallFromInvoiceNo IN (SELECT DISTINCT CASE RecallFromInvoiceNo WHEN '' THEN 'ALL' ELSE RecallFromInvoiceNo END FROM InvAdvanceNoteDets WHERE LEFT(CreditNoteNo, 12) = @AdNoteNo) OR CreditNoteNo IN (SELECT DISTINCT CASE RecallFromInvoiceNo WHEN '' THEN 'ALL' ELSE RecallFromInvoiceNo END FROM InvAdvanceNoteDets WHERE LEFT(CreditNoteNo, 12) = @AdNoteNo)))
                   
                                        UPDATE InvAdvanceNoteHeds SET Balance = @Balance WHERE AdNoteNo = @AdNoteNo
                   
				                        UPDATE InvAdvanceNoteDets
				                        SET CustCollected = 1
				                        WHERE LEFT(CreditNoteNo, 12) = @AdNoteNo
                                    END

                            COMMIT TRANSACTION;
                            SELECT  '0' AS Result
                        END TRY

                        BEGIN CATCH
                            IF @@TRANCOUNT > 0 
                                BEGIN
                                    ROLLBACK TRANSACTION
                                    SELECT  ERROR_MESSAGE() AS Result

                                END
                            ELSE 
                                BEGIN
                                    SELECT  ERROR_MESSAGE() AS Result
                                END
                        END CATCH";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion spUpdateAdvanceNoteDetails

            #region UpdateSuspendInvoice
            spName = "UpdateSuspendInvoice";
            query = @"CREATE PROCEDURE [dbo].[UpdateSuspendInvoice]
	                        @PosSuspendDet PosSuspendDet2 READONLY,
	                        @PosSuspendHed PosSuspendHed1 READONLY,
	                        @LocationCode INT,
	                        @UnitNumber INT,
	                        @User varchar(50),
	                        @TokenNumber VARCHAR(50) = '',
	                        @NextBillDate INT = 0,
	                        @CustomerId int= 0,
	                        @CustomerName varchar(100) = ''
                        AS
                         SET NOCOUNT ON

                            BEGIN TRY
                                BEGIN TRANSACTION 
	
	                             INSERT INTO [SuspendDets]
                                   (ProductID,ProductCode,RefCode,BarCodeFull,Descrip,BatchNo,SerialNo,ExpiaryDate,Cost,AvgCost,Qty,Amount,UnitOfMeasureID,UnitOfMeasureName,ConvertFactor,IDI1,IDis1,IDiscount1,IDI1CashierID,IDI2,IDis2, 
                IDiscount2,IDI2CashierID,IDI3,IDis3,IDiscount3,IDI3CashierID,IDI4,IDis4,IDiscount4,IDI4CashierID,IDI5,IDis5,IDiscount5,IDI5CashierID,Rate,IsSDis,SDNo,SDID,SDIs,SDiscount,DDisCashierID,Nett,LocationID, 
                DocumentID,BillTypeID,SaleTypeID,Receipt,SalesmanID,Salesman,CustomerID,Customer,CashierID,Cashier,StartTime,EndTime,RecDate,BaseUnitID,UnitNo,RowNo,IsRecall,RecallNo,RecallAdv,TaxAmount,IsTax,TaxPercentage,IsStock, 
                SuspendNo,SuspendBy,CustomerType,TransStatus,IsPromotionApplied,PromotionID,IsPromotion,InvPriceLevelID,ItemSerial,warranty,TableNumber,PrinterType,IsPritRpt,ReferenceProductId,ReferenceProductRow,IsAddonItem,IsTaxEnable,TaxCode, 
                SplitItemReceiptNo,Price,ProductRemark,DeploCardNo,IsShowOnBill,ServingUnit,OrderStatus,NoOfCustomers,KitchenCode,ServingUnitId,OrigUnitNo)
                             SELECT 
                                    ProductID,ProductCode,RefCode,BarCodeFull,Descrip,BatchNo,SerialNo,ExpiaryDate,Cost,AvgCost,Qty,Amount,UnitOfMeasureID,UnitOfMeasureName,ConvertFactor,IDI1,IDis1,IDiscount1,IDI1CashierID,IDI2,IDis2, 
                IDiscount2,IDI2CashierID,IDI3,IDis3,IDiscount3,IDI3CashierID,IDI4,IDis4,IDiscount4,IDI4CashierID,IDI5,IDis5,IDiscount5,IDI5CashierID,Rate,IsSDis,SDNo,SDID,SDIs,SDiscount,DDisCashierID,Nett,LocationID, 
                DocumentID,BillTypeID,SaleTypeID,Receipt,SalesmanID,Salesman,CustomerID,Customer,CashierID,Cashier,StartTime,EndTime,RecDate,BaseUnitID,UnitNo,RowNo,IsRecall,RecallNo,RecallAdv,TaxAmount,IsTax,TaxPercentage,IsStock, 
                SuspendNo,SuspendBy,CustomerType,TransStatus,IsPromotionApplied,PromotionID,IsPromotion,InvPriceLevelID,ItemSerial,warranty,TableNumber,PrinterType,IsPritRpt,ReferenceProductId,ReferenceProductRow,IsAddonItem,IsTaxEnable,TaxCode, 
                SplitItemReceiptNo,Price,ProductRemark,DeploCardNo,IsShowOnBill,ServingUnit,OrderStatus,NoOfCustomers,KitchenCode,ServingUnitId,OrigUnitNo
                             FROM @PosSuspendDet
                      
                             INSERT INTO [SuspendHeds]
                                   ([SuspendNo]
                                   ,[Receipt]
                                   ,[LocationID]
                                   ,[UnitNo]
                                   ,[STime]
                                   ,[SDate]
                                   ,[Amount]
                                   ,[CashierID]
                                   ,[IsRecall]
                                   ,[RecallReceipt]
                                   ,[RecallCashierID]
                                   ,[RecallCashier]
                                   ,[RecallUnitNo]
                                   ,[RecallTime]
                                   ,[TransStatus]
                                   ,[NextBillDate]
                                   ,CustomerId
                                   ,TokenNumber,   TableNumber, OrderStatus, OrigSuspendNo)
                             SELECT 
                                    [SuspendNo]
                                   ,[Receipt]
                                   ,[LocationID]
                                   ,[UnitNo]
                                   ,[STime]
                                   ,[SDate]
                                   ,[Amount]
                                   ,[CashierID]
                                   ,[IsRecall]
                                   ,[RecallReceipt]
                                   ,[RecallCashierID]
                                   ,[RecallCashier]
                                   ,[RecallUnitNo]
                                   ,[RecallTime]
                                   ,[TransStatus]
                                   ,@NextBillDate
                                   ,@CustomerId
                                   ,TokenNumber,   TableNumber, OrderStatus, OrigSuspendNo
                             FROM @PosSuspendHed
           
           
        
                            COMMIT TRANSACTION;
                                SELECT  '0' AS Result
                            END TRY
  
                            BEGIN CATCH
                                IF @@TRANCOUNT > 0 
                                    BEGIN
                                        ROLLBACK TRANSACTION
                                        SELECT  ERROR_MESSAGE() AS Result

                                    END
                                ELSE 
                                    BEGIN
                                        SELECT  ERROR_MESSAGE() AS Result
                                    END
                            END CATCH";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion UpdateSuspendInvoice

            #region sp_rpt_SalesRegistry
            //            spName = "sp_rpt_SalesRegistry";
            //            query = @"CREATE PROCEDURE [dbo].[sp_rpt_SalesRegistry]
            //@Type varchar(10)='sales',
            //@FromDate datetime = '',
            //@ToDate datetime = '',
            //@IsAsAtDate bit ='',
            //@FromTime datetime = '',
            //@ToTime datetime = '',
            //@LocationF int=0,
            //@LocationT int=0,
            //@Department int =0,
            //@Category int=0,
            //@SubCategory int=0,
            //@Customer int =0,
            //@CompanyId int =0
            //AS
            //BEGIN

            //		IF @IsAsAtDate ='false' AND @Type='sales' 

            //		BEGIN
            //		SELECT  TD.Receipt,TD.ZNo,TD.ProductCode,TD.Cost,TD.AvgCost,TD.Qty,TD.IDI1,TD.UnitOfMeasureName,TD.Amount,TD.TaxAmount,
            //		cast(TD.recdate as date) as recdate,(SELECT ProductName FROM Products WHERE ProductID=TD.ProductID) ProductName, 
            //		td.LocationID, p.DepartmentId,p.CategoryId,p.SubCategoryId, (td.Nett - td.Cost*td.Qty) as GP,
            //		 CASE WHEN td.Nett = 0 THEN 0 ELSE (td.Nett - td.Cost*td.Qty)/td.Nett*100 end as GPPres ,
            //         CONVERT(VARCHAR(20), RecDate, 101) AS [DATEPART],
            //         CONVERT(VARCHAR(20), StartTime, 108) AS TIMEPART,
            //        ISNULL( (Select ISNULL(Nett,0) FROM TransactionDets TDD WHere TDD.Receipt = TD.Receipt 
            //		AND TDD.UnitNo = TD.UnitNo 
            //		AND TDD.LocationID = TD.LocationID 
            //			AND TDD.ZNo = TD.ZNo 
            //				AND TDD.ZDate = TD.ZDate 
            //				AND TDD.DocumentID IN (100)
            //				),0) AS  ServCharge,
            //        ISNULL( (Select ISNULL(Nett,0) FROM TransactionDets TDD WHere TDD.Receipt = TD.Receipt 
            //		AND TDD.UnitNo = TD.UnitNo 
            //		AND TDD.LocationID = TD.LocationID 
            //			AND TDD.ZNo = TD.ZNo 
            //				AND TDD.ZDate = TD.ZDate 
            //				AND TDD.DocumentID IN (101)
            //				),0) AS  ACCharge,(SELECT MAX(Descrip) FROM PayTypes WHERE Type=pay.PayTypeID) PayType,
            //				CASE WHEN pay.PayTypeID = 58 THEN 'Online' ELSE ' ' end as Online ,
            //				CASE WHEN pay.PayTypeID = 59 THEN 'Uber' ELSE ' ' end as Uber ,
            //				CASE WHEN pay.PayTypeID = 60 THEN 'Pickme' ELSE ' ' end as Pickme 
            //		FROM TransactionDets TD 
            //		inner join Products p on td.ProductID = p.ProductId 
            //		inner join PaymentDets pay on td.Receipt = pay.Receipt 
            //		inner join RstDepartments d on p.DepartmentId = d.RstDepartmentID
            //		inner join RstCategories c on p.CategoryId = c.RstCategoryID
            //		inner join RstSubCategories s on p.SubCategoryId = s.RstSubCategoryID
            //		inner join SysLocations l on td.LocationID = l.SysLocationID
            //		inner join SysCompanies co on co.SysCompanyID = l.CompanyID
            //		WHERE (td.DocumentID = 1 OR
            //		td.DocumentID = 3) AND 
            //		(td.Status = 1) AND 
            //		(td.TransStatus = 1) AND 
            //		(td.SaleTypeID = 1) AND 
            //		(td.BillTypeID = 1) AND 

            //		CAST(td.RecDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND cast(@ToDate as DATE) 
            //		AND cast(td.EndTime as time)  BETWEEN cast(@FromTime as time) AND cast(@ToTime as time)
            //		--and (@LocationF != 0 and @LocationT!=0 AND td.LocationID BETWEEN @LocationF AND @LocationT)
            //		and td.LocationID  between CASE ISNULL(@LocationF, 0)  WHEN 0 THEN td.LocationID ELSE @LocationF END
            //		and CASE ISNULL(@LocationT, 0)  WHEN 0 THEN td.LocationID ELSE @LocationT END			
            //		and p.DepartmentId =CASE ISNULL(@Department, 0)  WHEN 0 THEN p.DepartmentId ELSE @Department END
            //		and p.CategoryId =CASE ISNULL(@Category, 0)  WHEN 0 THEN p.CategoryId ELSE @Category END
            //		and p.SubCategoryId =CASE ISNULL(@SubCategory, 0)  WHEN 0 THEN p.SubCategoryId ELSE @SubCategory END
            //		and td.CustomerID =CASE ISNULL(@Customer, 0)  WHEN 0 THEN td.CustomerID ELSE @Customer END
            //		and l.CompanyID =CASE ISNULL(@CompanyId, 0)  WHEN 0 THEN td.CustomerID ELSE @CompanyId END

            //		UNION ALL

            //		SELECT Pd.Receipt,Pd.ZNo,'',0.00,0.00,0.00,0.00,'',Pd.Amount,0,Cast(GETDATE() as Date),'Round Off',Pd.LocationID,'',
            //		                                        0,
            //		                                        0,0.00,0.00 
            //		                                        , CONVERT(VARCHAR(20), GETDATE(), 101) AS [DATEPART],
            //         CONVERT(VARCHAR(20), GETDATE(), 108) AS TIMEPART,
            //         ISNULL(0.00,0),ISNULL(0.00,0),'','','',''
            //		                                FROM    PaymentDets Pd ( NOLOCK ) 
            //		                                        INNER JOIN SysLocations l ( NOLOCK ) ON l.SysLocationID = Pd.LocationID
            //		                                        INNER JOIN SysCompanies com ( NOLOCK ) ON com.SysCompanyID = l.CompanyID

            //		                                WHERE   Pd.PayTypeID = 67
            //		                                        AND Pd.Status = 1 
            //		                                        AND Pd.SaleTypeID = 1
            //		                                        AND Pd.BillTypeID = 1
            //		                                        AND CAST(Pd.SDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND cast(@ToDate as DATE)


            //		END

            //	 IF @IsAsAtDate ='true' AND @Type='sales' 

            // BEGIN

            //		SELECT  TD.Receipt,TD.ZNo,TD.ProductCode,TD.Cost,TD.AvgCost,TD.Qty,TD.IDI1,TD.UnitOfMeasureName,TD.Amount,TD.TaxAmount,cast(TD.recdate as date) as recdate,(SELECT ProductName FROM Products WHERE ProductID=TD.ProductID) ProductName, 
            //		td.LocationID, p.DepartmentId,p.CategoryId,p.SubCategoryId, (td.Nett - td.Cost*td.Qty) as GP,
            //		 CASE WHEN td.Nett = 0 THEN 0 ELSE (td.Nett - td.Cost*td.Qty)/td.Nett*100 end as GPPres ,
            //         CONVERT(VARCHAR(20), RecDate, 101) AS [DATEPART],
            //         CONVERT(VARCHAR(20), StartTime, 108) AS TIMEPART,
            //        ISNULL( (Select ISNULL(Nett,0) FROM TransactionDets TDD WHere TDD.Receipt = TD.Receipt 
            //		AND TDD.UnitNo = TD.UnitNo 
            //		AND TDD.LocationID = TD.LocationID 
            //			AND TDD.ZNo = TD.ZNo 
            //				AND TDD.ZDate = TD.ZDate 
            //				AND TDD.DocumentID IN (100)
            //				),0) AS  ServCharge,
            //        ISNULL( (Select ISNULL(Nett,0) FROM TransactionDets TDD WHere TDD.Receipt = TD.Receipt 
            //		AND TDD.UnitNo = TD.UnitNo 
            //		AND TDD.LocationID = TD.LocationID 
            //			AND TDD.ZNo = TD.ZNo 
            //				AND TDD.ZDate = TD.ZDate 
            //				AND TDD.DocumentID IN (101)
            //				),0) AS  ACCharge,(SELECT MAX(Descrip) FROM PayTypes WHERE Type=pay.PayTypeID) PayType,
            //				CASE WHEN pay.PayTypeID = 58 THEN 'Online' ELSE ' ' end as Online ,
            //				CASE WHEN pay.PayTypeID = 59 THEN 'Uber' ELSE ' ' end as Uber ,
            //				CASE WHEN pay.PayTypeID = 60 THEN 'Pickme' ELSE ' ' end as Pickme 
            //		FROM TransactionDets TD 
            //		inner join Products p on td.ProductID = p.ProductId 
            //		inner join PaymentDets pay on td.Receipt = pay.Receipt 
            //		inner join RstDepartments d on p.DepartmentId = d.RstDepartmentID
            //		inner join RstCategories c on p.CategoryId = c.RstCategoryID
            //		inner join RstSubCategories s on p.SubCategoryId = s.RstSubCategoryID
            //		inner join SysLocations l on td.LocationID = l.SysLocationID
            //		inner join SysCompanies co on co.SysCompanyID = l.CompanyID
            //		WHERE (td.DocumentID = 1 OR
            //		td.DocumentID = 3) 
            //		AND (td.Status = 1) 
            //		AND (td.TransStatus = 1) 
            //		AND (td.SaleTypeID = 1) 
            //		AND (td.BillTypeID = 1) AND 

            //		CAST(td.RecDate AS DATE) <= @ToDate 
            //		and td.LocationID  between CASE ISNULL(@LocationF, 0)  WHEN 0 THEN td.LocationID ELSE @LocationF END
            //		and CASE ISNULL(@LocationT, 0)  WHEN 0 THEN td.LocationID ELSE @LocationT END
            //		and p.DepartmentId =CASE ISNULL(@Department, 0)  WHEN 0 THEN p.DepartmentId ELSE @Department END
            //		and p.CategoryId =CASE ISNULL(@Category, 0)  WHEN 0 THEN p.CategoryId ELSE @Category END
            //		and p.SubCategoryId =CASE ISNULL(@SubCategory, 0)  WHEN 0 THEN p.SubCategoryId ELSE @SubCategory END
            //		and td.CustomerID =CASE ISNULL(@Customer, 0)  WHEN 0 THEN td.CustomerID ELSE @Customer END
            //		and l.CompanyID =CASE ISNULL(@CompanyId, 0)  WHEN 0 THEN td.CustomerID ELSE @CompanyId END

            //		UNION ALL

            //		SELECT Pd.Receipt,Pd.ZNo,'',0.00,0.00,0.00,0.00,'',Pd.Amount,0,Cast(GETDATE() as Date),'Round Off',Pd.LocationID,'',
            //		                                        0,
            //		                                        0,0.00,0.00 --,Cast(GETDATE() as Date), CONVERT(time,getdate() )   --'15:37:04'
            //		                                        , CONVERT(VARCHAR(20), GETDATE(), 101) AS [DATEPART],
            //         CONVERT(VARCHAR(20), GETDATE(), 108) AS TIMEPART,
            //         ISNULL(0.00,0),ISNULL(0.00,0),'','','',''
            //		                                FROM    PaymentDets Pd ( NOLOCK ) 
            //		                                        INNER JOIN SysLocations l ( NOLOCK ) ON l.SysLocationID = Pd.LocationID
            //		                                        INNER JOIN SysCompanies com ( NOLOCK ) ON com.SysCompanyID = l.CompanyID

            //		                                WHERE   Pd.PayTypeID = 67
            //		                                        AND Pd.Status = 1 
            //		                                        AND Pd.SaleTypeID = 1
            //		                                        AND Pd.BillTypeID = 1
            //		                                        AND CAST(Pd.SDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND cast(@ToDate as DATE)

            //		END
            //END";

            //            CheckSP(spName);
            //            ExecuteSPQuery(Stringsqlconnection);

            #endregion sp_rpt_SalesRegistry

            #region SP_DailySales
            spName = "SP_DailySales";
            query = @"CREATE PROCEDURE [dbo].[SP_DailySales] --2023-10-11 Add Uber/Pickme/Online/Visacard/Amexcard/Debitcard/Credit
@Date Datetime='',
@LocationId int=0
AS
BEGIN



SELECT distinct t.Receipt,t.LocationID,t.UnitNo,t.ZNo,
--t.SerialNo,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'FS',t.RecDate) as FoodSale,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'BS',t.RecDate) as BevSale,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'NS',t.RecDate) as NonSale,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'CH',t.RecDate) as Cash,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'CD',t.RecDate) as Card,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'VD',t.RecDate) as Visacard,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'AX',t.RecDate) as Amexcard,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'DD',t.RecDate) as Debitcard,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'CI',t.RecDate) as Credit,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'OT',t.RecDate) as Others,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'Ol',t.RecDate) as Online,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'UB',t.RecDate) as UBER,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'PM',t.RecDate) as PICKME,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'SC',t.RecDate) as ServCharge,
0.0 as ChiliPaste,
dbo.GetDailySalesReportValues(RTRIM(t.Receipt),t.ZNo,t.UnitNo,t.LocationID,'VT',t.RecDate) as VAT,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'D',t.RecDate) as Discount,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'NB',t.RecDate) as NBT,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'TD',t.RecDate) as TDL,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'GR',t.RecDate) as Gross,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'NT',t.RecDate)+dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'SC',t.RecDate)+dbo.GetDailySalesReportValues(RTRIM(t.Receipt),t.ZNo,t.UnitNo,t.LocationID,'VT',t.RecDate)+dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'TD',t.RecDate)+dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'NB',t.RecDate)-dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'D',t.RecDate) as TNet,
--dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'NT',t.RecDate) as TNet,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'CR',t.RecDate) as Credit,
'NA' as HoldersName,
dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'AC',t.RecDate) as ACCharge--,
--(SELECT MAX(Descrip) FROM PayTypes WHERE Type=P.PayTypeID) PayType,
--				CASE WHEN p.PayTypeID = 58 THEN 'Online' ELSE ' ' end as Online ,
--				CASE WHEN p.PayTypeID = 59 THEN 'Uber' ELSE ' ' end as Uber ,
--				CASE WHEN p.PayTypeID = 60 THEN 'Pickme' ELSE ' ' end as Pickme 
FROM TransactionDets t 
Inner Join PaymentDets P ON t.LocationID = P.LocationID And t.ZNo = P.ZNo And t.Receipt = P.Receipt
where
--T.Receipt=P.Receipt AND
cast(t.RecDate as date)=CAST(@Date as date)
and t.LocationID=@LocationId and t.Status=1

END";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_DailySales

            #region SP_GivenDateSales
            //            spName = "SP_GivenDateSales";
            //            query = @"CREATE PROCEDURE [dbo].[SP_GivenDateSales] --2023-10-10
            //@Date Datetime='',
            //@DateTo  datetime='',
            //@Locations AS nvarchar(max)
            //AS
            //BEGIN

            //CREATE TABLE #TmpLocations
            //            (
            //              [item] [nvarchar](25) NULL 
            //            )

            //--if @Locations <>'0'
            //       -- begin
            //        	insert into #TmpLocations Select distinct CONVERT(Nvarchar(50),SysLocationID ) as item From
            //			dbo.SysLocations where ',' + @Locations + ',' like
            //			'%,' + Convert(Nvarchar(50),SysLocationID) + ',%'
            //       -- end
            //--select * from #TmpLocations

            //SELECT distinct l.LocationName,t.RecDate,t.Receipt,t.LocationID,t.UnitNo,t.ZNo,
            //--t.SerialNo,
            //dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'FS',t.RecDate) as FoodSale,
            //dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'BS',t.RecDate) as BevSale,
            //dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'NS',t.RecDate) as NonSale,
            //dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'CH',t.RecDate) as Cash,
            //dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'CD',t.RecDate) as Card,
            //dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'OT',t.RecDate) as Others,
            //dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'SC',t.RecDate) as ServCharge,
            //0.0 as ChiliPaste,
            //dbo.GetDailySalesReportValues(RTRIM(t.Receipt),t.ZNo,t.UnitNo,t.LocationID,'VT',t.RecDate) as VAT,
            //dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'D',t.RecDate) as Discount,
            //dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'NB',t.RecDate) as NBT,
            //dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'TD',t.RecDate) as TDL,
            //dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'GR',t.RecDate) as Gross,
            //dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'NT',t.RecDate)+dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'SC',t.RecDate)+dbo.GetDailySalesReportValues(RTRIM(t.Receipt),t.ZNo,t.UnitNo,t.LocationID,'VT',t.RecDate)+dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'TD',t.RecDate)+dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'NB',t.RecDate)-dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'D',t.RecDate) as TNet,
            //--dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'NT',t.RecDate) as TNet,
            //dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'CR',t.RecDate) as Credit,
            //'NA' as HoldersName,
            //dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'DS',t.RecDate)  as DESSERTSales,

            //dbo.GetDailySalesReportValues(t.Receipt,t.ZNo,t.UnitNo,t.LocationID,'AC',t.RecDate) as ACCharge,
            //(SELECT MAX(Descrip) FROM PayTypes WHERE Type=P.PayTypeID) PayType,
            //				CASE WHEN p.PayTypeID = 58 THEN 'Online' ELSE ' ' end as Online ,
            //				CASE WHEN p.PayTypeID = 59 THEN 'Uber' ELSE ' ' end as Uber ,
            //				CASE WHEN p.PayTypeID = (select PaymentID from PayTypes where Descrip like '%PickMe%') THEN 'Pickme' ELSE ' ' end as Pickme
            //FROM TransactionDets t join dbo.SysLocations  l on t.LocationID = l.SysLocationID
            //join PaymentDets P on t.Receipt=p.Receipt
            //where
            //(cast(t.RecDate as date) between cast(@Date as date) and  cast(@DateTo as date) and 
            //t.Status=1 ) and t.LocationID in (select * from #TmpLocations)

            //drop table #TmpLocations

            //END";
            //            CheckSP(spName);
            //            ExecuteSPQuery(Stringsqlconnection);
            #endregion SP_GivenDateSales

            #region spImportJournalDetails
            spName = "spImportJournalDetails";
            query = @"CREATE PROCEDURE [dbo].[spImportJournalDetails]
	                @FromDate DATE,
	                @ToDate DATE
	
                AS
                BEGIN
                    DECLARE @PaymentID AS INT , 
                        @Descrip VARCHAR(20)= '0' ,
                        @EXBATCH VARCHAR(15)= '0' ,
                        @TRANTYPE NVARCHAR(2)= '0' ,
                        @DOCNO VARCHAR(15)= '0' ,
                        @DOCNO1 VARCHAR(15)= '0' ,
                        @DATE DATETIME = GETDATE() ,
                        @DUEDATE DATETIME = GETDATE(),
                        @SEQNO NUMERIC(5, 0)= '0' ,
                        @ACODE VARCHAR(15)= '0' ,
                        @CCODE VARCHAR(3)= '0' ,
                        @DRCR VARCHAR(1)= '0' ,
                        @DESCRIPTION VARCHAR(250)= '0' ,
                        @AMOUNT decimal(18, 2)= 0 ,
                        @CQNO VARCHAR(13)= '0' ,
                        @CQDATE DATETIME = GETDATE() ,
                        @BANK VARCHAR(4)= '0' ,
                        @BANKBRANCH VARCHAR(4)= '0' ,
                        @PROCESS BIT = 0,
                        @GLPOST BIT = 0 ,
                        @GLPOSTUSER VARCHAR(10) = '0' ,
                        @GLPOSTDATETIME DATETIME = GETDATE() ,
                        @GLPOSTCPNAME VARCHAR(50) = '0' ,
                        @CUSTOMER BIT = 0 ,
                        @SUPPLIER BIT = 0 ,
                        @ISTAX BIT = 0 ,
                        @ADDITION BIT = 0 ,
                        @DEDUCTION BIT = 0 ,
                        @ISPAIDIN BIT = 0 ,
                        @ISPAIDOUT BIT = 0 ,
                        @DocumentID INT = 0,
                        @DOCID INT = 0,
                        @LocaId int = 0 ,
                        @IsBatch int =0,
                        @GVAMOUNT decimal(18, 2)= 0 ,
                        @ISAVGCOST VARCHAR(50) = '0',
                        @BillTypeID int=0,
                        @Location INT=0
        
	
	                SET NOCOUNT ON;
	
                        BEGIN TRY
           
                 --==============================================
                    --DEALLOCATE db_cursorLocation;  
                    DECLARE db_cursorLocation CURSOR
                        FOR
                          SELECT SysLocationID FROM dbo.SysLocations WHERE IsActive=1  AND IsDelete=0 AND IsShowRoom=1
                        OPEN db_cursorLocation   
                        FETCH NEXT FROM db_cursorLocation INTO @Location
                        WHILE @@FETCH_STATUS = 0 
                            BEGIN 
    
                     BEGIN TRANSACTION;     
                 --=============================================
 
                   SELECT @DOCID = ISNULL(MAX(IJDL.DocumentNumber),1)FROM ImportJournalDetailsLogs AS IJDL
                   SET @DOCNO = RIGHT('0000000000' + RTRIM(@DOCID), 8)
                   set @ISAVGCOST='0'
                   --(select ConfigValue from AppConfig where ConfigName='IsAverageCost')
 

                   DECLARE db_cursor CURSOR
                        FOR
                          SELECT PaymentID,Descrip FROM dbo.PayTypes WHERE PaymentID NOT IN (12,9,13)
                        OPEN db_cursor   
                        FETCH NEXT FROM db_cursor INTO @PaymentID,@Descrip
                        WHILE @@FETCH_STATUS = 0 
                            BEGIN 
                            --cash payments
                            print @FromDate
                            print @PaymentID
                            print @Location
            
                            SET @AMOUNT = 0
			                IF @PaymentID = 1
				                BEGIN
					                SELECT @BillTypeID=BillTypeID,  @CCODE = LocationID, @AMOUNT = ISNULL(SUM(CASE WHEN PaymentDets.Amount > PaymentDets.Balance
					                THEN PaymentDets.Balance ELSE PaymentDets.Amount END), 0)
					                FROM    PaymentDets
					                WHERE   BillTypeID in (1)
					                AND Status = 1
					                AND PayTypeID = @PaymentID
					                AND Balance > 0
					                AND CAST(SDate AS DATE) = @FromDate
					                AND IsGLTransfer = 0
					                and LocationID=@Location
					                Group by BillTypeID,LocationID,PayTypeID
				                END
			                ELSE 
				                if @PaymentID <>  9
					                BEGIN
						                SELECT @BillTypeID=BillTypeID,  @CCODE = LocationID,@AMOUNT = ISNULL(SUM(ISNULL(Amount, 0)), 0)     
						                FROM    PaymentDets
						                WHERE   PayTypeID = @PaymentID
						                AND Status = 1
						                AND BillTypeID in (1)
						                AND CAST(SDate AS DATE) = @FromDate
						                AND IsGLTransfer = 0
						                and LocationID=@Location
						                Group by BillTypeID,LocationID,PayTypeID
					                END

				                 INSERT INTO ImportJournalDetails
						                ( EXBATCH ,TRANTYPE ,DOCNO ,DOCNO1 ,[DATE] ,DUEDATE ,SEQNO ,ACODE ,CCODE ,
						                DRCR ,[DESCRIPTION] ,AMOUNT ,CQNO ,CQDATE ,BANK ,BANKBRANCH ,PROCESS ,GLPOST ,GLPOSTUSER ,GLPOSTDATETIME ,
						                GLPOSTCPNAME ,CUSTOMER ,SUPPLIER ,ISTAX ,ADDITION ,DEDUCTION ,ISPAIDIN ,ISPAIDOUT ,
						                ISCREDITED)
				                 VALUES ( @EXBATCH ,'08' ,@DOCNO ,@DOCNO1 ,CAST(@FromDate AS DATE) ,@DUEDATE ,@SEQNO ,@PaymentID ,@CCODE ,
						                'D' ,@Descrip,@AMOUNT ,@CQNO ,@CQDATE ,@BANK ,@BANKBRANCH ,@PROCESS ,@GLPOST ,@GLPOSTUSER ,@GLPOSTDATETIME ,
						                @GLPOSTCPNAME ,@CUSTOMER ,@SUPPLIER ,@ISTAX ,@ADDITION ,@DEDUCTION ,@ISPAIDIN ,@ISPAIDOUT,
						                CASE WHEN @BillTypeID=10 THEN 1 ELSE 0 END)   
			            
                             --credit settlement -cash
                             SET @AMOUNT = 0
			                IF @PaymentID = 1
				                BEGIN
					                SELECT @BillTypeID=BillTypeID,  @CCODE = LocationID, @AMOUNT = ISNULL(SUM(CASE WHEN PaymentDets.Amount > PaymentDets.Balance
					                THEN PaymentDets.Balance ELSE PaymentDets.Amount END), 0)
					                FROM    PaymentDets
					                WHERE   BillTypeID in (10)
					                AND Status = 1
					                AND PayTypeID = @PaymentID
					                AND Balance > 0
					                AND CAST(SDate AS DATE) = @FromDate
					                AND IsGLTransfer = 0
					                and LocationID=@Location
					                Group by BillTypeID,LocationID,PayTypeID
				                END
			                ELSE 
				                if @PaymentID <>  9
					                BEGIN
						                SELECT @BillTypeID=BillTypeID,  @CCODE = LocationID,@AMOUNT = ISNULL(SUM(ISNULL(Amount, 0)), 0)     
						                FROM    PaymentDets
						                WHERE   PayTypeID = @PaymentID
						                AND Status = 1
						                AND BillTypeID in (10)
						                AND CAST(SDate AS DATE) = @FromDate
						                AND IsGLTransfer = 0
						                and LocationID=@Location
						                Group by BillTypeID,LocationID,PayTypeID
					                END

				                 INSERT INTO ImportJournalDetails
						                ( EXBATCH ,TRANTYPE ,DOCNO ,DOCNO1 ,[DATE] ,DUEDATE ,SEQNO ,ACODE ,CCODE ,
						                DRCR ,[DESCRIPTION] ,AMOUNT ,CQNO ,CQDATE ,BANK ,BANKBRANCH ,PROCESS ,GLPOST ,GLPOSTUSER ,GLPOSTDATETIME ,
						                GLPOSTCPNAME ,CUSTOMER ,SUPPLIER ,ISTAX ,ADDITION ,DEDUCTION ,ISPAIDIN ,ISPAIDOUT ,
						                ISCREDITED)
				                 VALUES ( @EXBATCH ,'08' ,@DOCNO ,@DOCNO1 ,CAST(@FromDate AS DATE) ,@DUEDATE ,@SEQNO ,@PaymentID ,@CCODE ,
						                'D' ,@Descrip,@AMOUNT ,@CQNO ,@CQDATE ,@BANK ,@BANKBRANCH ,@PROCESS ,@GLPOST ,@GLPOSTUSER ,@GLPOSTDATETIME ,
						                @GLPOSTCPNAME ,@CUSTOMER ,@SUPPLIER ,@ISTAX ,@ADDITION ,@DEDUCTION ,@ISPAIDIN ,@ISPAIDOUT,
						                CASE WHEN @BillTypeID=10 THEN 1 ELSE 0 END) 
						 
                             FETCH NEXT FROM db_cursor INTO @PaymentID,@Descrip   
                            END   

                        CLOSE db_cursor   
                        DEALLOCATE db_cursor 
        
                        --cheque sales---------
                          INSERT INTO ImportJournalDetails( EXBATCH ,TRANTYPE ,DOCNO ,DOCNO1 ,[DATE] ,DUEDATE ,SEQNO ,ACODE ,CCODE ,DRCR ,[DESCRIPTION] ,AMOUNT ,CQNO ,CQDATE ,BANK ,BANKBRANCH ,PROCESS ,GLPOST ,GLPOSTUSER ,GLPOSTDATETIME ,GLPOSTCPNAME ,CUSTOMER ,SUPPLIER ,ISTAX ,ADDITION ,DEDUCTION ,ISPAIDIN ,ISPAIDOUT,ISCREDITED)
                      SELECT  @EXBATCH ,'08' ,@DOCNO ,@DOCNO1 ,CAST(@FromDate AS DATE) ,@DUEDATE ,@SEQNO ,9 ,PDT.LocationID ,'D' ,'CHEQUE',ISNULL(SUM(ISNULL(PDT.Amount, 0)), 0) ,PDT.RefNo ,PDT.ChequeDate ,@BANK ,@BANKBRANCH ,@PROCESS ,@GLPOST ,@GLPOSTUSER ,@GLPOSTDATETIME ,@GLPOSTCPNAME ,@CUSTOMER ,@SUPPLIER ,@ISTAX ,@ADDITION ,@DEDUCTION ,@ISPAIDIN ,@ISPAIDOUT,
                      CASE WHEN PDT.BillTypeID=10 THEN 1 ELSE 0 END    
                      from 
                      PaymentDets as PDT	WHERE   PayTypeID = 9 AND Status = 1 AND BillTypeID in (1,10)	AND CAST(SDate AS DATE) = @FromDate	AND IsGLTransfer = 0 and LocationID=@Location Group by LocationID,PayTypeID,RefNo,PDT.ChequeDate,PDT.BillTypeID
    
   
                 ------------------------  Credit Insert -----------------------------
 
                  INSERT INTO ImportJournalDetails
                            ( EXBATCH ,
                              TRANTYPE ,
                              DOCNO ,
                              DOCNO1 ,
                              [DATE] ,
                              DUEDATE ,
                              SEQNO ,
                              ACODE ,
                              CCODE ,
                              DRCR ,
                              [DESCRIPTION] ,
                              AMOUNT ,
                              CQNO ,
                              CQDATE ,
                              BANK ,
                              BANKBRANCH ,
                              PROCESS ,
                              GLPOST ,
                              GLPOSTUSER ,
                              GLPOSTDATETIME ,
                              GLPOSTCPNAME ,
                              CUSTOMER ,
                              SUPPLIER ,
                              ISTAX ,
                              ADDITION ,
                              DEDUCTION ,
                              ISPAIDIN ,
                              ISPAIDOUT
                            )
                      SELECT  @EXBATCH ,
                              '08' ,
                              Receipt,
                              @DOCNO1 ,
                              CAST(@FromDate AS DATE),
                              @DUEDATE ,
                              @SEQNO ,
                              Cus.CustomerCode ,
                              PDT.LocationID ,
                              'D' ,
                              'Credit Sale',
                              ISNULL(SUM(CASE WHEN PDT.Amount > PDT.Balance
                                                                            THEN PDT.Balance
                                                                            ELSE PDT.Amount
                                                                       END), 0) ,
                              @CQNO ,
                              @CQDATE ,
                              @BANK ,
                              @BANKBRANCH ,
                              @PROCESS ,
                              @GLPOST ,
                              @GLPOSTUSER ,
                              @GLPOSTDATETIME ,
                              @GLPOSTCPNAME ,
                              1 ,
                              @SUPPLIER ,
                              @ISTAX ,
                              @ADDITION ,
                              @DEDUCTION ,
                              @ISPAIDIN ,
                              @ISPAIDOUT 
                             FROM    PaymentDets AS PDT INNER JOIN dbo.Customers AS Cus ON cus.CustomerID = PDT.CustomerId
                                        WHERE   BillTypeID = 1
                                                AND Status = 1
                                                AND PDT.PayTypeID = 12
                                              --  AND SaleTypeID = 1
                                                AND Balance > 0
                                              --  AND PDT.CustomerType != 2
                                                AND IsGLTransfer = 0
                                                 AND CAST(SDate AS DATE) = @FromDate
                                                 and PDT.LocationID=@Location
                                                GROUP BY 
                                                Cus.CustomerCode,
                                                PDT.Receipt,
                                                PDT.LocationID  
                                
                ------------------------  Credit Note -----------------------------29/01/2020
 
                  INSERT INTO ImportJournalDetails
                            ( EXBATCH ,
                              TRANTYPE ,
                              DOCNO ,
                              DOCNO1 ,
                              [DATE] ,
                              DUEDATE ,
                              SEQNO ,
                              ACODE ,
                              CCODE ,
                              DRCR ,
                              [DESCRIPTION] ,
                              AMOUNT ,
                              CQNO ,
                              CQDATE ,
                              BANK ,
                              BANKBRANCH ,
                              PROCESS ,
                              GLPOST ,
                              GLPOSTUSER ,
                              GLPOSTDATETIME ,
                              GLPOSTCPNAME ,
                              CUSTOMER ,
                              SUPPLIER ,
                              ISTAX ,
                              ADDITION ,
                              DEDUCTION ,
                              ISPAIDIN ,
                              ISPAIDOUT
                            )
                      SELECT  @EXBATCH ,
                              '08' ,
                              Receipt,
                              @DOCNO1 ,
                              CAST(@FromDate AS DATE),
                              @DUEDATE ,
                              @SEQNO ,
                              Cus.CustomerCode ,
                              PDT.LocationID ,
                              'C' ,
                              'Credit Note',
                              sum(PDT.Amount)  ,
                              @CQNO ,
                              @CQDATE ,
                              @BANK ,
                              @BANKBRANCH ,
                              @PROCESS ,
                              @GLPOST ,
                              @GLPOSTUSER ,
                              @GLPOSTDATETIME ,
                              @GLPOSTCPNAME ,
                              1 ,
                              @SUPPLIER ,
                              @ISTAX ,
                              @ADDITION ,
                              @DEDUCTION ,
                              @ISPAIDIN ,
                              @ISPAIDOUT 
                             FROM    PaymentDets AS PDT INNER JOIN dbo.Customers AS Cus ON cus.CustomerID = PDT.CustomerId
                               WHERE   BillTypeID = 1
                               AND Status = 1
                               AND PDT.PayTypeID = 13
                                AND Amount > 0
                               AND IsGLTransfer = 0
                               AND CAST(SDate AS DATE) = @FromDate
                               and PDT.LocationID=@Location
                               GROUP BY Cus.CustomerCode, PDT.Receipt,PDT.LocationID  
               
                -------credit sales settlement--------------------------------------
                  INSERT INTO ImportJournalDetails
                            ( EXBATCH ,
                              TRANTYPE ,
                              DOCNO ,
                              DOCNO1 ,
                              [DATE] ,
                              DUEDATE ,
                              SEQNO ,
                              ACODE ,
                              CCODE ,
                              DRCR ,
                              [DESCRIPTION] ,
                              AMOUNT ,
                              CQNO ,
                              CQDATE ,
                              BANK ,
                              BANKBRANCH ,
                              PROCESS ,
                              GLPOST ,
                              GLPOSTUSER ,
                              GLPOSTDATETIME ,
                              GLPOSTCPNAME ,
                              CUSTOMER ,
                              SUPPLIER ,
                              ISTAX ,
                              ADDITION ,
                              DEDUCTION ,
                              ISPAIDIN ,
                              ISPAIDOUT,ISCREDITED
                            )
                      SELECT  @EXBATCH ,
                              '08' ,
                              Receipt,
                              @DOCNO1 ,
                              CAST(@FromDate AS DATE),
                              @DUEDATE ,
                              @SEQNO ,
                              Cus.CustomerCode ,
                              PDT.LocationID ,
                              'C' ,
                              'Credit Sale Settlement',
                              ISNULL(SUM(CASE WHEN PDT.Amount > PDT.Balance
                              THEN PDT.Balance
                              ELSE PDT.Amount
                              END), 0) ,
                              @CQNO ,
                              @CQDATE ,
                              @BANK ,
                              @BANKBRANCH ,
                              @PROCESS ,
                              @GLPOST ,
                              @GLPOSTUSER ,
                              @GLPOSTDATETIME ,
                              @GLPOSTCPNAME ,
                              1 ,
                              @SUPPLIER ,
                              @ISTAX ,
                              @ADDITION ,
                              @DEDUCTION ,
                              @ISPAIDIN ,
                              @ISPAIDOUT ,1
                             FROM    PaymentDets AS PDT INNER JOIN dbo.Customers AS Cus ON cus.CustomerID = PDT.CustomerId
                                        WHERE   Status = 1
                                                AND PDT.BillTypeID = 10
                                                AND Balance > 0
                                                AND IsGLTransfer = 0
                                                 AND CAST(SDate AS DATE) = @FromDate
                                                 and PDT.LocationID=@Location
                                                GROUP BY 
                                                Cus.CustomerCode,
                                                PDT.Receipt,
                                                PDT.LocationID  
                ------------------------------------------------------------------------------------------------------------                                                                   
                     --cash refund--
                     INSERT INTO ImportJournalDetails
                            ( EXBATCH ,
                              TRANTYPE ,
                              DOCNO ,
                              DOCNO1 ,
                              [DATE] ,
                              DUEDATE ,
                              SEQNO ,
                              ACODE ,
                              CCODE ,
                              DRCR ,
                              [DESCRIPTION] ,
                              AMOUNT ,
                              CQNO ,
                              CQDATE ,
                              BANK ,
                              BANKBRANCH ,
                              PROCESS ,
                              GLPOST ,
                              GLPOSTUSER ,
                              GLPOSTDATETIME ,
                              GLPOSTCPNAME ,
                              CUSTOMER ,
                              SUPPLIER ,
                              ISTAX ,
                              ADDITION ,
                              DEDUCTION ,
                              ISPAIDIN ,
                              ISPAIDOUT
                            )
                      SELECT   @EXBATCH ,
                              '08' ,
                              @DOCNO ,
                              @DOCNO1 ,
                              CAST(@FromDate AS DATE) ,
                              @DUEDATE ,
                              @SEQNO ,
                              @DOCNO ,
                              LocationID ,
                              'D' ,
                              'Cash Refund',
                             ISNULL(SUM(Amount), 0),
                              @CQNO ,
                              @CQDATE ,
                              @BANK ,
                              @BANKBRANCH ,
                              @PROCESS ,
                              @GLPOST ,
                              @GLPOSTUSER ,
                              @GLPOSTDATETIME ,
                              @GLPOSTCPNAME ,
                              @CUSTOMER ,
                              @SUPPLIER ,
                              @ISTAX ,
                              @ADDITION ,
                              @DEDUCTION ,
                              @ISPAIDIN ,
                              @ISPAIDOUT  
                             FROM    PaymentDets
                              WHERE  BillTypeID = 1
                                        AND Status = 1
                                        AND PayTypeID = 1
                                        AND Amount < 0
                                     --   AND SaleTypeID = 1
                                        AND CAST(SDate AS DATE) = @FromDate
                                        and LocationID=@Location
                                        and IsGLTransfer=0
                                        group by LocationID
     
     
                --credit refund-- 1/8/2018
                   INSERT INTO ImportJournalDetails
                            ( EXBATCH ,
                              TRANTYPE ,
                              DOCNO ,
                              DOCNO1 ,
                              [DATE] ,
                              DUEDATE ,
                              SEQNO ,
                              ACODE ,
                              CCODE ,
                              DRCR ,
                              [DESCRIPTION] ,
                              AMOUNT ,
                              CQNO ,
                              CQDATE ,
                              BANK ,
                              BANKBRANCH ,
                              PROCESS ,
                              GLPOST ,
                              GLPOSTUSER ,
                              GLPOSTDATETIME ,
                              GLPOSTCPNAME ,
                              CUSTOMER ,
                              SUPPLIER ,
                              ISTAX ,
                              ADDITION ,
                              DEDUCTION ,
                              ISPAIDIN ,
                              ISPAIDOUT
                            )
                      SELECT   @EXBATCH ,
                              '08' ,
                              Receipt ,
                              @DOCNO1 ,
                              CAST(@FromDate AS DATE) ,
                              @DUEDATE ,
                              @SEQNO ,
                              CustomerCode ,
                              LocationID ,
                              'D' ,
                              'Credit Refund',
                             ISNULL(SUM(Amount), 0),
                              @CQNO ,
                              @CQDATE ,
                              @BANK ,
                              @BANKBRANCH ,
                              @PROCESS ,
                              @GLPOST ,
                              @GLPOSTUSER ,
                              @GLPOSTDATETIME ,
                              @GLPOSTCPNAME ,
                              1 ,
                              @SUPPLIER ,
                              @ISTAX ,
                              @ADDITION ,
                              @DEDUCTION ,
                              @ISPAIDIN ,
                              @ISPAIDOUT  
                             FROM    PaymentDets 
                              WHERE  BillTypeID = 1
                                        AND Status = 1
                                        AND PayTypeID = 12
                                        AND Amount < 0
                                    --    AND SaleTypeID = 1
                                        AND CAST(SDate AS DATE) = @FromDate
                                        and LocationID=@Location
                                        and IsGLTransfer=0
                                        group by LocationID  ,CustomerCode  ,Receipt                            
                             
                               
     
    
                     ------------------------   Comment now -------------------------
                         --SET @IsBatch = (SELECT IsBatch FROM SystemFeature );
 
                          INSERT INTO ImportJournalDetails
                            ( EXBATCH ,
                              TRANTYPE ,
                              DOCNO ,
                              DOCNO1 ,
                              [DATE] ,
                              DUEDATE ,
                              SEQNO ,
                              ACODE ,
                              CCODE ,
                              DRCR ,
                              [DESCRIPTION] ,
                              AMOUNT ,
                              CQNO ,
                              CQDATE ,
                              BANK ,
                              BANKBRANCH ,
                              PROCESS ,
                              GLPOST ,
                              GLPOSTUSER ,
                              GLPOSTDATETIME ,
                              GLPOSTCPNAME ,
                              CUSTOMER ,
                              SUPPLIER ,
                              ISTAX ,
                              ADDITION ,
                              DEDUCTION ,
                              ISPAIDIN ,
                              ISPAIDOUT
                            )
                            SELECT 
                             @EXBATCH ,
                             '09' ,
                              CAT.RstCategoryID,
                              cast(CAT.RstCategoryID as varchar(10)) ,
                              CAST(@FromDate AS DATE),
                              @DUEDATE ,
                              @SEQNO ,
                              CAT.RstCategoryID,
                              INS.LocationID ,
                              'C' ,
                              'CostOfSale',
                               ISNULL(SUM(case when @ISAVGCOST='1' then INS.AvgCost else INS.Cost end *   Case When DocumentID = 1 or DocumentID = 3 Then Qty When DocumentID = 2 or DocumentID = 4 Then -Qty End       ), 0),
                              @CQNO ,
                              @CQDATE ,
                              @BANK ,
                              @BANKBRANCH ,
                              @PROCESS ,
                              @GLPOST ,
                              @GLPOSTUSER ,
                              @GLPOSTDATETIME ,
                              @GLPOSTCPNAME ,
                              1 ,
                              @SUPPLIER ,
                              @ISTAX ,
                              @ADDITION ,
                              @DEDUCTION ,
                              @ISPAIDIN ,
                              @ISPAIDOUT 
                             FROM dbo.TransactionDets AS INS  Inner Join Products as PM on PM.ProductCode = INS.ProductCode
                             Inner Join RstCategories as CAT on CAT.RstCategoryID = PM.CategoryID
                             where 
                             --DayEndComplete = 1 and
                              CAST(RecDate as Date)= @FromDate 
                             AND Status = 1
                                    AND TransStatus = 1
                                    AND SaleTypeID = 1
                                    AND BillTypeID = 1
                                    and INS.LocationID=@Location
                                    and IsGLTransfer=0
                    
                             Group by
                                      INS.LocationID,
                                      CAT.RstCategoryID
                     
			
                     --Update IJD set IJD.DOCNO = CAT.CategoryCode , IJD.ACODE = CAT.CategoryCode  from ImportJournalDetails as IJD
                     --Inner join InvCategory as CAT on CAT.InvCategoryID = IJD.ACODE where IJD.[DATE] = @FromDate
	
	                set @GVAMOUNT = (select SUM(nett)-sum(sdiscount) from TransactionDets where IsGLTransfer = 0 and CAST(RecDate As Date) = @FromDate and LocationID=@Location
				                 and  (DocumentID = 1) AND (BillTypeID = 1) AND (SaleTypeID = 2) AND (Status = 1) group by LocationID)
	                set @GVAMOUNT= isnull (@GVAMOUNT ,0)

                    INSERT INTO [ImportJournalDetailsLogs]
                           ([DocumentNumber]
                           ,[FromDate]
                           ,[ToDate])
                     VALUES
                           (@DOCID
                           ,@FromDate
                           ,@FromDate)                              

                COMMIT TRANSACTION;
                            UPDATE  PaymentDets SET  AcountNumber = @DOCID WHERE CAST(SDate AS DATE)  = @FromDate and LocationID=@Location
                            UPDATE PaymentDets SET IsGLTransfer = 1 WHERE CAST(SDate AS DATE) = @FromDate and LocationID=@Location
                            UPDATE TransactionDets set IsGLTransfer = 1 Where CAST(RecDate As Date) = @FromDate and LocationID=@Location
                            UPDATE dbo.InvSales SET IsUpLoad = 1 WHERE CAST(DocumentDate AS DATE) = CAST(@FromDate AS DATE) and InvSales.LocationID=@Location
                            DELETE  FROM ImportJournalDetails WHERE AMOUNT = 0 AND [DATE] = @FromDate
                            Update tmp set tmp.CCODE = LC.LocationCode from ImportJournalDetails as tmp inner join SysLocations as LC on LC.SysLocationID = tmp.CCODE
     
                     ------------------   Insert credit Summary ----------------------------
                      SELECT @DOCID = ISNULL(MAX(IJDL.DocumentNumber),1)FROM ImportJournalDetailsLogs AS IJDL
		                SET @DOCNO = RIGHT('0000000000' + RTRIM(@DOCID), 8)
        
                        --Sales summary
                                 INSERT INTO ImportJournalDetails
                            ( EXBATCH ,
                              TRANTYPE ,
                              DOCNO ,
                              DOCNO1 ,
                              [DATE] ,
                              DUEDATE ,
                              SEQNO ,
                              ACODE ,
                              CCODE ,
                              DRCR ,
                              [DESCRIPTION] ,
                              AMOUNT ,
                              CQNO ,
                              CQDATE ,
                              BANK ,
                              BANKBRANCH ,
                              PROCESS ,
                              GLPOST ,
                              GLPOSTUSER ,
                              GLPOSTDATETIME ,
                              GLPOSTCPNAME ,
                              CUSTOMER ,
                              SUPPLIER ,
                              ISTAX ,
                              ADDITION ,
                              DEDUCTION ,
                              ISPAIDIN ,
                              ISPAIDOUT
                            )
                     Select   @EXBATCH ,
                              Trantype ,
                              @DOCNO ,
                              @DOCNO1 ,
                              CAST(@FromDate AS DATE) ,
                              CAST(@FromDate AS DATE) ,
                              0 ,
                             '25SALES' as ACODE , 
                              CCODE ,
                             'C' as DRCR,
                             'SALES' as [DESCRIPTION],
                              SUM(Amount)-@GVAMOUNT,
                              '' ,
                              CAST(@FromDate AS DATE) ,
                              '' ,
                              '' ,
                              0 ,
                              0 ,
                              '' ,
                              @GLPOSTDATETIME ,
                              @GLPOSTCPNAME ,
                              0 ,
                              @SUPPLIER ,
                              @ISTAX ,
                              @ADDITION ,
                              @DEDUCTION ,
                              @ISPAIDIN ,
                              @ISPAIDOUT 
                         From [ImportJournalDetails] where [Date] = @FromDate and GLPOST = 0 and CCODE=@Location
                         and ISCREDITED=0 and Trantype='08' and DESCRIPTION<>'Credit Note'
                 Group by Trantype,CCODE
 
                 -----------credit note sales debit entry---------29/01/2020
                                  INSERT INTO ImportJournalDetails
                            ( EXBATCH ,
                              TRANTYPE ,
                              DOCNO ,
                              DOCNO1 ,
                              [DATE] ,
                              DUEDATE ,
                              SEQNO ,
                              ACODE ,
                              CCODE ,
                              DRCR ,
                              [DESCRIPTION] ,
                              AMOUNT ,
                              CQNO ,
                              CQDATE ,
                              BANK ,
                              BANKBRANCH ,
                              PROCESS ,
                              GLPOST ,
                              GLPOSTUSER ,
                              GLPOSTDATETIME ,
                              GLPOSTCPNAME ,
                              CUSTOMER ,
                              SUPPLIER ,
                              ISTAX ,
                              ADDITION ,
                              DEDUCTION ,
                              ISPAIDIN ,
                              ISPAIDOUT
                            )
                     Select   @EXBATCH ,
                              '08' ,
                              @DOCNO ,
                              @DOCNO1 ,
                              CAST(@FromDate AS DATE) ,
                              CAST(@FromDate AS DATE) ,
                              0 ,
                             '25SALES' as ACODE , 
                              CCODE ,
                             'D' as DRCR,
                             'SALES' as [DESCRIPTION],
                              SUM(Amount),
                              '' ,
                              CAST(@FromDate AS DATE) ,
                              '' ,
                              '' ,
                              0 ,
                              0 ,
                              '' ,
                              @GLPOSTDATETIME ,
                              @GLPOSTCPNAME ,
                              0 ,
                              @SUPPLIER ,
                              @ISTAX ,
                              @ADDITION ,
                              @DEDUCTION ,
                              @ISPAIDIN ,
                              @ISPAIDOUT 
                         From [ImportJournalDetails] where [Date] = @FromDate and GLPOST = 0 and CCODE=@Location
                         and ISCREDITED=0 and Trantype='08' and DESCRIPTION='Credit Note'
                 Group by CCODE
                 -----------------------------------------------------------------------
 
		                --GV sales summary
		                if @GVAMOUNT>0
		                begin
		                     INSERT INTO ImportJournalDetails
                            ( EXBATCH ,
                              TRANTYPE ,
                              DOCNO ,
                              DOCNO1 ,
                              [DATE] ,
                              DUEDATE ,
                              SEQNO ,
                              ACODE ,
                              CCODE ,
                              DRCR ,
                              [DESCRIPTION] ,
                              AMOUNT ,
                              CQNO ,
                              CQDATE ,
                              BANK ,
                              BANKBRANCH ,
                              PROCESS ,
                              GLPOST ,
                              GLPOSTUSER ,
                              GLPOSTDATETIME ,
                              GLPOSTCPNAME ,
                              CUSTOMER ,
                              SUPPLIER ,
                              ISTAX ,
                              ADDITION ,
                              DEDUCTION ,
                              ISPAIDIN ,
                              ISPAIDOUT
                            )
                     Select   @EXBATCH ,
                              Trantype ,
                              @DOCNO ,
                              @DOCNO1 ,
                              CAST(@FromDate AS DATE) ,
                              CAST(@FromDate AS DATE) ,
                              0 ,
                             '25GVSALES' as ACODE , 
                              CCODE ,
                             'C' as DRCR,
                             'GVSALES' as [DESCRIPTION],
                              @GVAMOUNT,
                              '' ,
                              CAST(@FromDate AS DATE) ,
                              '' ,
                              '' ,
                              0 ,
                              0 ,
                              '' ,
                              @GLPOSTDATETIME ,
                              @GLPOSTCPNAME ,
                              0 ,
                              @SUPPLIER ,
                              @ISTAX ,
                              @ADDITION ,
                              @DEDUCTION ,
                              @ISPAIDIN ,
                              @ISPAIDOUT 
			                From [ImportJournalDetails] where [Date] = @FromDate and GLPOST = 0 and CCODE=@Location
			                and ISCREDITED=0 and Trantype='08'
			                Group by Trantype,CCODE 
		                end
		                --cost of sales summary
                         INSERT INTO ImportJournalDetails
                            ( EXBATCH ,
                              TRANTYPE ,
                              DOCNO ,
                              DOCNO1 ,
                              [DATE] ,
                              DUEDATE ,
                              SEQNO ,
                              ACODE ,
                              CCODE ,
                              DRCR ,
                              [DESCRIPTION] ,
                              AMOUNT ,
                              CQNO ,
                              CQDATE ,
                              BANK ,
                              BANKBRANCH ,
                              PROCESS ,
                              GLPOST ,
                              GLPOSTUSER ,
                              GLPOSTDATETIME ,
                              GLPOSTCPNAME ,
                              CUSTOMER ,
                              SUPPLIER ,
                              ISTAX ,
                              ADDITION ,
                              DEDUCTION ,
                              ISPAIDIN ,
                              ISPAIDOUT
                            )
                     Select   @EXBATCH ,
                              Trantype ,
                              @DOCNO ,
                              @DOCNO1 ,
                              CAST(@FromDate AS DATE) ,
                              CAST(@FromDate AS DATE) ,
                              0 ,
                             '25COSTOFSALES' as ACODE , 
                              CCODE ,
                             'D'  as DRCR,
                             'COST OF SALES' as [DESCRIPTION],
                              SUM(Amount),
                              '' ,
                              CAST(@FromDate AS DATE) ,
                              '' ,
                              '' ,
                              0 ,
                              0 ,
                              '' ,
                              @GLPOSTDATETIME ,
                              @GLPOSTCPNAME ,
                              0 ,
                              @SUPPLIER ,
                              @ISTAX ,
                              @ADDITION ,
                              @DEDUCTION ,
                              @ISPAIDIN ,
                              @ISPAIDOUT 
                         From [ImportJournalDetails] where [Date] = @FromDate and GLPOST = 0 and CCODE=@Location
                         and ISCREDITED=0 and Trantype='09'
                 Group by Trantype,CCODE, ACODE,[DESCRIPTION],DRCR    
        
                     update ImportJournalDetails set ISCREDITED=1
                   Update  ImportJournalDetails set  DRCR = 'C'  where  ACODE = '25SALES' and [DESCRIPTION] = 'SALES' and Amount < 0
                  -- Update  ImportJournalDetails set   Amount = (Amount*-1) where Amount < 0

     
                            FETCH NEXT FROM db_cursorLocation INTO  @Location   
                            END 
                  CLOSE db_cursorLocation   
                  DEALLOCATE db_cursorLocation 
 
             
                            SELECT  '0' AS Result
                        END TRY
  
                        BEGIN CATCH
                            IF @@TRANCOUNT > 0 
                                BEGIN
                                    ROLLBACK TRANSACTION
                                    SELECT  ERROR_MESSAGE() AS Result

                                END
                            ELSE 
                                BEGIN
                                    SELECT  ERROR_MESSAGE() AS Result
                                END
                        END CATCH
	
                    END     ";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion spImportJournalDetails

            #region SpTransferToGL
            spName = "SpTransferToGL";
            query = @"CREATE PROCEDURE [dbo].[SpTransferToGL]  --exec SpTransferToGL '2017-10-23','2017-10-23',1
	                    @DateFrom DATE ,
		                @DateTo DATE,
		                @GroupOfCompany bigint = 1
                AS
                 --if ((select CompanyName from Company)='CATHOLIC PRESS') print 'Exit' return;
                BEGIN
	
	                DECLARE @PROCESS BIT ,
			                @GLPOST BIT = 0,
			                @ServerName VARCHAR(100) = '',
			                @ServerUserName VARCHAR(5) = '',
			                @ServerPsw  VARCHAR(10) = '',
			                @Dbname VARCHAR(500) = '',
			                @CCODE VARCHAR(5) = ''

	                BEGIN TRY
						
		                BEGIN
		
		                      Select @ServerName = AccountServerName , 
		                             @ServerUserName = AccountServerUserName,
		                             @ServerPsw = AccountServerUserPassword,
		                             @Dbname = AccountServerDBName 
		                      from SysGroupOfCompanies where SysGroupOfCompanyId = @GroupOfCompany  
		      
		                       DECLARE @SQLString VARCHAR(MAX)
		                       DECLARE @StringDay VARCHAR(MAX)
			                   DECLARE @LocID VARCHAR(MAX)
		       
		                       SET @StringDay=''''+CONVERT(VARCHAR(20), @DateFrom)+''''
		                       --set @LocID=''''+CONVERT(VARCHAR(20), @LocationID)+''''
                --set @CCODE=(select locationcode from SysLocations where SysLocationID=@LocationID )

				                Declare @Year int=0,@Month int=0,@Day int=0
				                Set @Year=YEAR(@DateFrom)
				                Set @Month=Month(@DateFrom)
				                Set @Day=Day(@DateFrom)
				                Declare @SYear Varchar(10)='',@SMonth Varchar(10)='',@SDay Varchar(10)=''
				                set @SYear=''''+CONVERT(VARCHAR(20), @Year)+''''
				                set @SMonth=''''+CONVERT(VARCHAR(20), @Month)+''''
				                set @SDay=''''+CONVERT(VARCHAR(20), @Day)+''''
				
				
	                 --==============================================
                 --   DEALLOCATE db_cursorLocation;  
                    DECLARE db_cursorLocation CURSOR
                        FOR
                          SELECT SysLocationID,locationcode FROM dbo.SysLocations WHERE IsActive=1  AND IsDelete=0 AND IsShowRoom=1
                        OPEN db_cursorLocation   
                        FETCH NEXT FROM db_cursorLocation INTO @LocID,@CCODE
                        WHILE @@FETCH_STATUS = 0 
                            BEGIN 
            
            
                            PRINT @LocID
            
                                 SET @SQLString = N' 
							                Insert Into  Openrowset(''Sqloledb'','''
                                    + CONVERT(VARCHAR(50), @ServerName) + ''';'''
                                    + CONVERT(VARCHAR(50), @ServerUserName) + ''';'''
                                    + CONVERT(VARCHAR(50), @ServerPsw) + ''','
                                    + CONVERT(VARCHAR(50), @Dbname)
                                    + '.[dbo].[ImportJournalDetails])
					                ([EXBATCH],[TRANTYPE],[DOCNO],[DOCNO1],[DATE],[DUEDATE]
					                ,[SEQNO],[ACODE],[CCODE],[DRCR],[DESCRIPTION],[AMOUNT]
					                ,[CQNO],[CQDATE],[BANK],[BANKBRANCH],[PROCESS],[GLPOST]
					                ,[GLPOSTUSER],[GLPOSTDATETIME],[GLPOSTCPNAME],[CUSTOMER]
					                ,[SUPPLIER],[ISTAX],[ADDITION],[DEDUCTION],[ISPAIDIN],[ISPAIDOUT],[SALESMANID])
				                SELECT 
							                [EXBATCH],[TRANTYPE],[DOCNO],[DOCNO1],[DATE],[DUEDATE]
					                ,[SEQNO],[ACODE],[CCODE],[DRCR],[DESCRIPTION],[AMOUNT]
					                ,[CQNO],[CQDATE],[BANK],[BANKBRANCH],[PROCESS],[GLPOST]
					                ,[GLPOSTUSER],[GLPOSTDATETIME],[GLPOSTCPNAME],[CUSTOMER]
					                ,[SUPPLIER],[ISTAX],[ADDITION],[DEDUCTION],[ISPAIDIN],[ISPAIDOUT],[SALESMANID]
				                FROM [ImportJournalDetails],SysLocations WHERE locationcode=CCODE and
				                GLPOST = 0 AND Year([DATE]) = '+ @SYear + '  and Month([DATE]) = '+ @SMonth + '  and Day([DATE]) = '+ @SDay + '  and SysLocationID=' + @LocID + ' '  
				                ---- GLPOST = 0 AND [DATE] = '+ @StringDay + '  and locationID= '+  @LocID +'  '  
				 
				                print @SQLString
	                EXEC(@SQLString)	
				
				
		
				                set @PROCESS = 1
		
		
		
		                IF(@PROCESS = 1)
			                BEGIN
			                --UPDATE [ImportJournalDetails] SET GLPOST = 1 WHERE GLPOST = 0 AND [DATE] = @DateFrom 
				                UPDATE [ImportJournalDetails] SET GLPOST = 1 WHERE GLPOST = 0 AND Year([DATE]) = @Year  and Month([DATE]) =  @Month  and Day([DATE]) = @Day
                and CCODE=@CCODE
			                END
			
			
		                            FETCH NEXT FROM db_cursorLocation INTO  @LocID,@CCODE   
                            END 
	                  CLOSE db_cursorLocation   
	                  DEALLOCATE db_cursorLocation 
	  
	                  END
	                  SELECT  1 AS Result
	  
	                END TRY
		
	                BEGIN CATCH
                        IF @@TRANCOUNT > 0 
                            BEGIN
                                ROLLBACK TRANSACTION
                                SELECT  ERROR_MESSAGE() AS Result
                                SELECT  0 AS Result
                            END
                        ELSE 
                            BEGIN
                                SELECT  ERROR_MESSAGE() AS Result
                                SELECT  0 AS Result

                            END
                    END CATCH
                END  ";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SpTransferToGL

            #region SP_LoadAllproducts
            spName = "SP_LoadAllproducts";
            query = @"CREATE PROCEDURE [dbo].[SP_LoadAllproducts]
                            @CompanyID int=0,
                            @LocationId int=0,
                            @productCodefrom nvarchar(20)='',
                            @productCodeto nvarchar(20)='',
                            @productID bigint=0
                            AS
                            BEGIN
	                            IF @LocationId!=0  AND @productCodefrom !='0' AND @productCodeto!='0'
	                            --location has/product code has
	                            BEGIN
	                            --ProductId,ProductName,Stock,ProductCode,CostPrice,AvgCost
		                            SELECT * FROM  ProductStockMasters WHERE LocationId=@LocationId AND CompanyID=@CompanyID  AND 
		                            ProductCode>=@productCodefrom AND ProductCode<=@productCodeto
		                            ORDER BY ProductCode desc
	                            END 
	                            --location has/
	
	                            --location not selected/product code has
	                            ELSE IF @LocationId =0  AND @productCodefrom !='0' AND @productCodeto!='0'
	                            BEGIN
		                            SELECT * FROM  ProductStockMasters WHERE  CompanyID=@CompanyID AND 
		                            ProductCode>=@productCodefrom AND ProductCode<=@productCodeto
		                            ORDER BY ProductCode desc
	                            END 

	                            ELSE IF @LocationId =0  AND @productCodefrom ='0' AND @productCodeto='0'
	                            BEGIN
		                            SELECT * FROM  ProductStockMasters WHERE CompanyID=@CompanyID 
		                            ORDER BY ProductCode desc
	                            END 
	                            ELSE IF @LocationId !=0 AND @productCodefrom ='0' AND @productCodeto='0'
	                            BEGIN
		                            SELECT * FROM  ProductStockMasters WHERE LocationId=@LocationId AND CompanyID=@CompanyID 
		                            ORDER BY ProductCode desc
	                            END 
	                            ELSE IF @LocationId !=0 AND @productCodefrom !='0' AND @productCodeto='0'
	                            BEGIN
		                            SELECT * FROM  ProductStockMasters WHERE LocationId=@LocationId AND CompanyID=@CompanyID  AND ProductCode=@productCodefrom
		                            ORDER BY ProductCode desc
	                            END
	                            ELSE IF @LocationId !=0 AND @productCodefrom ='0' AND @productCodeto!='0'
	                            BEGIN
		                            SELECT * FROM  ProductStockMasters WHERE LocationId=@LocationId AND CompanyID=@CompanyID 
		                            ORDER BY ProductCode desc
	                            END
	                            ELSE IF @LocationId =0 AND @productCodefrom !='0' AND @productCodeto='0'
	                            BEGIN
		                            SELECT * FROM  ProductStockMasters WHERE  CompanyID=@CompanyID  AND  ProductCode=@productCodefrom
		                            ORDER BY ProductCode desc
	                            END 
                            END";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SpTransferToGL

            #region SP_GetSalesData
            spName = "SP_GetSalesData";
            query = @"CREATE PROCEDURE [dbo].[SP_GetSalesData]
    
                    @FromDate DATE,
                    @ToDate DATE,
                    @CateModeID INT,
                    @CompanyID INT
                AS
                BEGIN
                    SET NOCOUNT ON;

			                SELECT 
			                dbo.TransactionDets.RecDate, 
			                dbo.TransactionDets.Receipt, 
			                dbo.TransactionDets.ZNo,
			                dbo.TransactionDets.StartTime, 
			                dbo.TransactionDets.EndTime
			
		                FROM 
			                dbo.TransactionDets 
			                INNER JOIN dbo.Products ON dbo.TransactionDets.ProductID = dbo.Products.ProductId 
			                LEFT JOIN dbo.CateringMoods ON dbo.TransactionDets.OrderStatus = dbo.CateringMoods.CateringMoodID 
			                INNER JOIN dbo.PrinterTypes ON dbo.Products.PrinterTypeId = dbo.PrinterTypes.PrinterTypeId 
			                INNER JOIN dbo.RstDepartments ON dbo.Products.DepartmentId = dbo.RstDepartments.RstDepartmentID 
			                INNER JOIN dbo.SysLocations ON dbo.TransactionDets.LocationID = dbo.SysLocations.SysLocationID 
			                LEFT OUTER JOIN dbo.Customers ON dbo.TransactionDets.CustomerID = dbo.Customers.CustomerID
		                WHERE 
			                dbo.TransactionDets.DocumentID IN (1, 2, 3, 4) 
			                AND dbo.TransactionDets.Status = 1
			                AND dbo.TransactionDets.RecDate BETWEEN @FromDate AND @ToDate
			                AND dbo.SysLocations.CompanyID = @CompanyID
			                AND (@CateModeID = 0 OR dbo.CateringMoods.CateringMoodID = @CateModeID)
                END  ";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_GetSalesData

            #region SP_TransferCustomer
            spName = "SP_TransferCustomer";
            query = @"CREATE PROCEDURE [dbo].[SP_TransferCustomer]
                    @CustomerID INT,
                    @CustomerCode NVARCHAR(MAX),
                    @CustomerTitle NVARCHAR(MAX),
                    @CustomerName NVARCHAR(100),
                    @CustomerType NVARCHAR(MAX),
                    @CustomerCategoryId INT,
                    @BillingAddress1 NVARCHAR(100),
                    @BillingAddress2 NVARCHAR(100),
                    @BillingAddress3 NVARCHAR(MAX),
                    @DOB DATETIME,
                    @NIC NVARCHAR(12),
                    @Passport NVARCHAR(MAX),
                    @Telephone NVARCHAR(MAX),
                    @Mobile NVARCHAR(MAX),
                    @Fax NVARCHAR(MAX),
                    @Email NVARCHAR(MAX),
                    @VehicleNo NVARCHAR(MAX),
                    @Profession NVARCHAR(MAX),
                    @WeddingAnniversary DATETIME,
                    @IsActiveForLoyalty BIT,
                    @CustomerPictureName NVARCHAR(MAX),
                    @CustomerPictureType NVARCHAR(MAX),
                    @IsActive BIT,
                    @IsDelete BIT,
                    @CreditLimit DECIMAL(18,2),
                    @Outstanding DECIMAL(18,2),
                    @EPFNo VARCHAR(50),
                    @MembershipCardNo VARCHAR(50),
                    @Other VARCHAR(50),
                    @Remarks VARCHAR(200),
                    @CustomerStatus VARCHAR(20),
                    @GroupOfCompanyID INT,
                    @CompanyID INT,
                    @LocationId INT,
                    @CreatedUser NVARCHAR(50),
                    @CreatedDate DATETIME,
                    @ModifiedUser NVARCHAR(50),
                    @ModifiedDate DATETIME,
                    @DataTransfer INT=1,
                    @ReferenceNo1 VARCHAR(MAX),
                    @Gender INT,
                    @ReferenceNo2 NVARCHAR(50),
                    @Age INT,
                    @Religion INT,
                    @Race INT,
                    @LandMark NVARCHAR(50),
                    @District NVARCHAR(50),
                    @Organization NVARCHAR(50),
                    @WorkAddres1 NVARCHAR(50),
                    @WorkAddres2 NVARCHAR(50),
                    @WorkAddres3 NVARCHAR(50),
                    @WorkEmail NVARCHAR(50),
                    @WorkTelephone NVARCHAR(50),
                    @WorkMobile NVARCHAR(50),
                    @WorkFax NVARCHAR(50),
                    @SpouseName NVARCHAR(50),
                    @CivilStatus INT,
                    @SpouseDateOfBirth DATETIME,
                    @DeliverTo INT,
                    @DeliverToAddress NVARCHAR(50),
                    @Country NVARCHAR(50),
                    @CustomerSince DATETIME,
                    @SpecialDayType INT,
                    @SendUpdatesViaEmail BIT,
                    @SendUpdatesViaSms BIT,
                    @IsRegByPOS BIT,
                    @SenderPreference INT,
                    @FirstName VARCHAR(150),
                    @LastName VARCHAR(150)
                AS
                BEGIN
                    SET NOCOUNT ON;
	                INSERT INTO Customers (
                        CustomerCode, CustomerTitle, CustomerName, CustomerType,
                        CustomerCategoryId, BillingAddress1, BillingAddress2, BillingAddress3,
                        DOB, NIC, Passport, Telephone, Mobile, Fax, Email, VehicleNo,
                        Profession, WeddingAnniversary, IsActiveForLoyalty, 
                        CustomerPictureName, CustomerPictureType, IsActive, IsDelete,
                        CreditLimit, Outstanding, EPFNo, MembershipCardNo, Other, Remarks,
                        CustomerStatus, GroupOfCompanyID, CompanyID, LocationId, CreatedUser,
                        CreatedDate, ModifiedUser, ModifiedDate, DataTransfer, ReferenceNo1,
                        Gender, ReferenceNo2, Age, Religion, Race, LandMark, District,
                        Organization, WorkAddres1, WorkAddres2, WorkAddres3, WorkEmail,
                        WorkTelephone, WorkMobile, WorkFax, SpouseName, CivilStatus,
                        SpouseDateOfBirth, DeliverTo, DeliverToAddress, Country, CustomerSince,
                        SpecialDayType, SendUpdatesViaEmail, SendUpdatesViaSms, IsRegByPOS,
                        SenderPreference, FirstName, LastName
                    )
                    VALUES (
                         @CustomerCode, @CustomerTitle, @CustomerName, @CustomerType,
                        @CustomerCategoryId, @BillingAddress1, @BillingAddress2, @BillingAddress3,
                        @DOB, @NIC, @Passport, @Telephone, @Mobile, @Fax, @Email, @VehicleNo,
                        @Profession, @WeddingAnniversary, @IsActiveForLoyalty, 
                        @CustomerPictureName, @CustomerPictureType, @IsActive, @IsDelete,
                        @CreditLimit, @Outstanding, @EPFNo, @MembershipCardNo, @Other, @Remarks,
                        @CustomerStatus, @GroupOfCompanyID, @CompanyID, @LocationId, @CreatedUser,
                        @CreatedDate, @ModifiedUser, @ModifiedDate, @DataTransfer, @ReferenceNo1,
                        @Gender, @ReferenceNo2, @Age, @Religion, @Race, @LandMark, @District,
                        @Organization, @WorkAddres1, @WorkAddres2, @WorkAddres3, @WorkEmail,
                        @WorkTelephone, @WorkMobile, @WorkFax, @SpouseName, @CivilStatus,
                        @SpouseDateOfBirth, @DeliverTo, @DeliverToAddress, @Country, @CustomerSince,
                        @SpecialDayType, @SendUpdatesViaEmail, @SendUpdatesViaSms, @IsRegByPOS,
                        @SenderPreference, @FirstName, @LastName
                    );
                END";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_TransferCustomer

            #region SP_TransferAdvanceNoteHed
            spName = "SP_TransferAdvanceNoteHed";
            query = @"CREATE PROCEDURE [dbo].[SP_TransferAdvanceNoteHed]
    @InvAdvanceNoteHedID bigint,
    @AdNoteNo nvarchar(15) ,
    @Receipt nvarchar(15),
    @Amount decimal(18, 2),
    @Balance decimal(18, 2),
    @LocationID int,
    @Date datetime,
    @UnitNo int,
    @CashierID int,
    @Time datetime,
    @Zno bigint,
    @RecallFromInvoice int,
    @DeliveryDate datetime,
    @Remark nvarchar(max) = NULL,
    @IsProduction bit,
    @ProcessLoc int,
    @PickupLoc int,
    @Status bit,
    @CompanyId int
AS
BEGIN
    SET NOCOUNT ON;
	 INSERT INTO [dbo].[InvAdvanceNoteHeds] (
            [AdNoteNo],
            [Receipt],
            [Amount],
            [Balance],
            [LocationID],
            [Date],
            [UnitNo],
            [CashierID],
            [Time],
            [Zno],
            [RecallFromInvoice],
            [DeliveryDate],
            [Remark],
            [IsProduction],
            [ProcessLoc],
            [PickupLoc],
            [Status],
            [CompanyId]
        )
        VALUES (
            @AdNoteNo,
            @Receipt,
            @Amount,
            @Balance,
            @LocationID,
            @Date,
            @UnitNo,
            @CashierID,
            @Time,
            @Zno,
            @RecallFromInvoice,
            @DeliveryDate,
            @Remark,
            @IsProduction,
            @ProcessLoc,
            @PickupLoc,
            @Status,
            @CompanyId
        );
END";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_TransferAdvanceNoteHed

            #region SP_TransferInvAdvanceNoteDets
            spName = "SP_TransferInvAdvanceNoteDets";
            query = @"CREATE PROCEDURE [dbo].[SP_TransferInvAdvanceNoteDets]
    @InvAdvanceNoteDetID INT,
@Idx BIGINT,
@ProductID BIGINT,
@ProductCode NVARCHAR(25),
@RefCode NVARCHAR(25),
@BarCodeFull BIGINT,
@Descrip NVARCHAR(50),
@BatchNo NVARCHAR(50),
@SerialNo NVARCHAR(50),
@ExpiryDate DATETIME,
@Cost DECIMAL(18, 2),
@AvgCost DECIMAL(18, 2),
@Price DECIMAL(18, 2),
@Qty DECIMAL(18, 2),
@Amount DECIMAL(18, 2),
@UnitOfMeasureID BIGINT,
@UnitOfMeasureName NVARCHAR(10),
@ConvertFactor DECIMAL(18, 2),
@IDI1 INT,
@IDis1 DECIMAL(18, 2),
@IDiscount1 DECIMAL(18, 2),
@IDI1CashierID BIGINT,
@IDI2 INT,
@IDis2 DECIMAL(18, 2),
@IDiscount2 DECIMAL(18, 2),
@IDI2CashierID BIGINT,
@IDI3 INT,
@IDis3 DECIMAL(18, 2),
@IDiscount3 DECIMAL(18, 2),
@IDI3CashierID BIGINT,
@IDI4 DECIMAL(18, 2),
@IDis4 DECIMAL(18, 2),
@IDiscount4 DECIMAL(18, 2),
@IDI4CashierID BIGINT,
@IDI5 INT,
@IDis5 DECIMAL(18, 2),
@IDiscount5 DECIMAL(18, 2),
@IDI5CashierID BIGINT,
@Rate DECIMAL(18, 2),
@IsSDis BIT,
@SDNo INT,
@SDID INT,
@SDIs DECIMAL(18, 2),
@SDiscount DECIMAL(18, 2),
@DDisCashierID BIGINT,
@Nett DECIMAL(18, 2),
@LocationID INT,
@DocumentID INT,
@BillTypeID INT,
@SaleTypeID INT,
@Receipt NVARCHAR(10),
@SalesmanID BIGINT,
@Salesman NVARCHAR(15),
@CustomerID BIGINT,
@Customer NVARCHAR(15),
@CashierID BIGINT,
@Cashier NVARCHAR(15),
@StartTime DATETIME,
@EndTime DATETIME,
@RecDate DATETIME,
@BaseUnitID BIGINT,
@UnitNo INT,
@RowNo INT,
@IsRecall BIT,
@RecallNO NVARCHAR(10),
@RecallAdv BIT,
@TaxAmount DECIMAL(18, 2),
@IsTax BIT,
@TaxPercentage DECIMAL(18, 2),
@IsStock BIT,
@CreditNoteNo NVARCHAR(150),
@CreditNoteBy BIGINT,
@CustomerType INT,
@TransStatus INT,
@IsPromotionApplied BIT,
@PromotionID INT,
@IsPromotion BIT,
@ItemSerial NVARCHAR(50),
@warranty NVARCHAR(50),
@RecallFromInvoiceNo VARCHAR(50),
@WorkComplete BIT,
@WorkCompUser NVARCHAR(30),
@WorkCompDateTime DATETIME,
@CustCollected BIT,
@CustColDateTime DATETIME,
@IsNewPrice BIT,
@IsApproved BIT,
@ApprovedBy BIGINT,
@ApprovedFor NCHAR(10),
@ReferenceProductId INT,
@ReferenceProductRow INT,
@PrinterType INT,
@IsAddonItem BIT,
@TableNumber INT,
@IsTaxEnable BIT,
@TaxCode VARCHAR(50),
@SplitItemReceiptNo VARCHAR(50),
@IsPritRpt BIT,
@ProductRemark VARCHAR(200),
@OrderStatus INT,
@ServingUnit VARCHAR(50),
@NoOfCustomers INT,
@IsShowOnBill BIT,
@DeploCardNo VARCHAR(50),
@ServingUnitId INT,
@IsProduction BIT

AS
BEGIN
    SET NOCOUNT ON;
	INSERT INTO [InvAdvanceNoteDets] (
      [Idx], [ProductID], [EndTime], [RecDate], [ProductCode], [RefCode], 
      [BarCodeFull], [Descrip], [BatchNo], [SerialNo], [ExpiryDate], 
      [Cost], [AvgCost], [Price], [Qty], [Amount], [UnitOfMeasureID], 
      [UnitOfMeasureName], [ConvertFactor], [IDI1], [IDis1], [IDiscount1], 
      [IDI1CashierID], [IDI2], [IDis2], [IDiscount2], [IDI2CashierID], 
      [IDI3], [IDis3], [IDiscount3], [IDI3CashierID], [IDI4], [IDis4], 
      [IDiscount4], [IDI4CashierID], [IDI5], [IDis5], [IDiscount5], 
      [IDI5CashierID], [Rate], [IsSDis], [SDNo], [SDID], [SDIs], 
      [SDiscount], [DDisCashierID], [Nett], [LocationID], [DocumentID], 
      [BillTypeID], [SaleTypeID], [Receipt], [SalesmanID], [Salesman], 
      [CustomerID], [Customer], [CashierID], [Cashier], [StartTime], 
      [BaseUnitID], [UnitNo], [RowNo], [IsRecall], [RecallNO], 
      [RecallAdv], [TaxAmount], [IsTax], [TaxPercentage], [IsStock], 
      [CreditNoteNo], [CreditNoteBy], [CustomerType], [TransStatus], 
      [IsPromotionApplied], [PromotionID], [IsPromotion], [ItemSerial], 
      [warranty], [RecallFromInvoiceNo], [WorkComplete], [WorkCompUser], 
      [WorkCompDateTime], [CustCollected], [CustColDateTime], [IsNewPrice], 
      [IsApproved], [ApprovedBy], [ApprovedFor], [ReferenceProductId], 
      [ReferenceProductRow], [PrinterType], [IsAddonItem], [TableNumber], 
      [IsTaxEnable], [TaxCode], [SplitItemReceiptNo], [IsPritRpt], 
      [ProductRemark], [OrderStatus], [ServingUnit], [NoOfCustomers], 
      [IsShowOnBill], [DeploCardNo], [ServingUnitId], [IsProduction]
)
VALUES (
      @Idx, @ProductID, @EndTime, @RecDate, @ProductCode, @RefCode, 
      @BarCodeFull, @Descrip, @BatchNo, @SerialNo, @ExpiryDate, 
      @Cost, @AvgCost, @Price, @Qty, @Amount, @UnitOfMeasureID, 
      @UnitOfMeasureName, @ConvertFactor, @IDI1, @IDis1, @IDiscount1, 
      @IDI1CashierID, @IDI2, @IDis2, @IDiscount2, @IDI2CashierID, 
      @IDI3, @IDis3, @IDiscount3, @IDI3CashierID, @IDI4, @IDis4, 
      @IDiscount4, @IDI4CashierID, @IDI5, @IDis5, @IDiscount5, 
      @IDI5CashierID, @Rate, @IsSDis, @SDNo, @SDID, @SDIs, 
      @SDiscount, @DDisCashierID, @Nett, @LocationID, @DocumentID, 
      @BillTypeID, @SaleTypeID, @Receipt, @SalesmanID, @Salesman, 
      @CustomerID, @Customer, @CashierID, @Cashier, @StartTime, 
      @BaseUnitID, @UnitNo, @RowNo, @IsRecall, @RecallNO, 
      @RecallAdv, @TaxAmount, @IsTax, @TaxPercentage, @IsStock, 
      @CreditNoteNo, @CreditNoteBy, @CustomerType, @TransStatus, 
      @IsPromotionApplied, @PromotionID, @IsPromotion, @ItemSerial, 
      @warranty, @RecallFromInvoiceNo, @WorkComplete, @WorkCompUser, 
      @WorkCompDateTime, @CustCollected, @CustColDateTime, @IsNewPrice, 
      @IsApproved, @ApprovedBy, @ApprovedFor, @ReferenceProductId, 
      @ReferenceProductRow, @PrinterType, @IsAddonItem, @TableNumber, 
      @IsTaxEnable, @TaxCode, @SplitItemReceiptNo, @IsPritRpt, 
      @ProductRemark, @OrderStatus, @ServingUnit, @NoOfCustomers, 
      @IsShowOnBill, @DeploCardNo, @ServingUnitId, @IsProduction
);

END";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_TransferInvAdvanceNoteDets

            #region SP_TransferInvAdvancePaymentDets
            spName = "SP_TransferInvAdvancePaymentDets";
            query = @"CREATE PROCEDURE [dbo].[SP_TransferInvAdvancePaymentDets]
    @InvAdvancePaymentDetId BIGINT,
    @Idx BIGINT,
    @RowNo BIGINT,
    @PayTypeID INT,
    @Amount DECIMAL(18, 4),
    @Balance DECIMAL(18, 4),
    @SDate DATETIME,
    @Receipt CHAR(10),
    @LocationID INT,
    @CashierID BIGINT,
    @UnitNo INT,
    @BillTypeID INT,
    @RefNo VARCHAR(30),
    @BankId BIGINT,
    @ChequeDate DATE,
    @IsRecallAdv BIT,
    @RecallNo VARCHAR(10),
    @Descrip VARCHAR(20),
    @EnCodeName VARCHAR(50),
    @SuspendNo NCHAR(50),
    @SuspendBy BIT,
    @IsDeleteOnRecall BIT,
    @AdvanceNumber VARCHAR(20)
AS
BEGIN
    INSERT INTO dbo.InvAdvancePaymentDets
    (
        Idx,
        RowNo,
        PayTypeID,
        Amount,
        Balance,
        SDate,
        Receipt,
        LocationID,
        CashierID,
        UnitNo,
        BillTypeID,
        RefNo,
        BankId,
        ChequeDate,
        IsRecallAdv,
        RecallNo,
        Descrip,
        EnCodeName,
        SuspendNo,
        SuspendBy,
        IsDeleteOnRecall,
        AdvanceNumber
    )
    VALUES
    (
        @Idx,
        @RowNo,
        @PayTypeID,
        @Amount,
        @Balance,
        @SDate,
        @Receipt,
        @LocationID,
        @CashierID,
        @UnitNo,
        @BillTypeID,
        @RefNo,
        @BankId,
        @ChequeDate,
        @IsRecallAdv,
        @RecallNo,
        @Descrip,
        @EnCodeName,
        @SuspendNo,
        @SuspendBy,
        @IsDeleteOnRecall,
        @AdvanceNumber
    );
END";

            CheckSP(spName);
            ExecuteSPQuery(Stringsqlconnection);

            #endregion SP_TransferInvAdvancePaymentDets

            #region ALTER COLUMN SuspendDets in Iscallorder B.L.M.Thebuwana 2023-10-10 created
            tableName = "SuspendDets";
            ColumnName = "Iscallorder";
            query = @"ALTER TABLE SuspendDets ADD Iscallorder bit NOT NULL DEFAULT 0";
            ExecuteSPQuery(CheckAlterTableAddColumn(tableName, ColumnName, query));
            #endregion

            #endregion Stored Procedures
        }


        public void ExecuteSPQuery(string con)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(con))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
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

        public void ExecuteSPCheckQuery(string con)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(con))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(CheckspName, connection))
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
