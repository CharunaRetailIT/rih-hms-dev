using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Common
{
    

    public class StructureChangesTriggers
    {
        string tableName = "";
        string query = "";
        string spName = "";
        string ViewName = "";
        string ColumnName = "";
        public bool status = true;
        string Stringsqlconnection = "";
        string TriggerName = "";
        string triggerquery = "";

        public void RunTrigger(string connectionString)
        {
            Stringsqlconnection = connectionString;

            #region Trg_GenProductionNotes
            TriggerName = "Trg_GenProductionNotes";
            triggerquery = @"create TRIGGER [dbo].[Trg_GenProductionNotes] on [dbo].[PaymentDets]  after INSERT
AS 
BEGIN
	 
	SET NOCOUNT ON;

    declare @ReceiptLocID int, @R_Zno int,@DocumentID int,
         @ReceiptNo nvarchar(40), @UnitNo int 
         declare NewRecs cursor for 
         select distinct td.Receipt,td.LocationID,td.zNo,td.UnitNo from transactiondets TD 
         inner join inserted on LTRIM(RTRIM(inserted.Receipt)) = LTRIM(RTRIM(TD.receipt)) and inserted.locationid = td.locationID
         and td.unitNo=inserted.UnitNo and td.zno =inserted.zNo and td.DocumentID in (1,3)
                                                                                   
         open NewRecs
         fetch next from NewRecs into @ReceiptNo,@ReceiptLocID,@R_Zno,@UnitNo
         while @@FETCH_STATUS =0
         begin
         PRINT @ReceiptNo
         exec genProductionNotes @receiptLocID,@R_Zno ,@ReceiptNo,@UnitNo 
         fetch next from NewRecs into @ReceiptNo,@ReceiptLocID,@R_Zno,@UnitNo
         end
         close NewRecs
         deallocate NewRecs
        
END";


            ExecuteTrigger(Stringsqlconnection);

            #endregion Trg_GenProductionNotes

            #region Trigger_UpdateStockInHeadOffice

            triggerquery = @"IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[Trigger_UpdateStockInHeadOffice]'))
            DROP TRIGGER [dbo].[Trigger_UpdateStockInHeadOffice] ";//Check the triger is already is exist in db, then drop and create
            ExecuteTrigger(Stringsqlconnection);

            TriggerName = "Trigger_UpdateStockInHeadOffice";
            triggerquery = @"CREATE TRIGGER [dbo].[Trigger_UpdateStockInHeadOffice]
   ON  [dbo].[TransactionDets]
   AFTER INSERT
AS 
BEGIN

SET NOCOUNT ON;

	DECLARE @StockCode VARCHAR(20), @LocationID INT = 0, @DocumentID INT = 0, @BillTypeID INT = 0, @SaleTypeID INT = 0, @TransStatus INT = 0, @Status INT = 0,
	@Qty DECIMAL(18,2), @ProductId INT = 0, @AvgCost DECIMAL(18, 2) = 0, @SerialNo VARCHAR(20)
	
	DECLARE Update_Cursor CURSOR FOR SELECT ProductCode, Qty, LocationId, DocumentID, BillTypeID, SaleTypeID, TransStatus, Status, ProductId, AvgCost, SerialNo FROM inserted WHERE UnitNo != 0

	OPEN Update_Cursor

	FETCH NEXT FROM Update_Cursor INTO @StockCode, @Qty, @LocationID, @DocumentID, @BillTypeID, @SaleTypeID, @TransStatus, @Status, @ProductId, @AvgCost, @SerialNo;

	WHILE @@FETCH_STATUS = 0
	BEGIN	
		IF EXISTS (SELECT * FROM ProductStockMasters WHERE  ProductStockMasterId = @ProductId)
		BEGIN
			IF @DocumentID IN (1, 3) AND @BillTypeID = 1 AND @SaleTypeID = 1 AND @TransStatus = 1 And @Status = 1
			BEGIN					
				UPDATE S SET S.Stock = S.Stock - @Qty FROM ProductStockMasters AS S WHERE S.LocationID = @LocationID AND S.StockCode = @StockCode	AND S.ProductId = @ProductId AND S.IsBundle = 0		
			END
			
			IF @DocumentID in (2, 4) AND @BillTypeID=1 AND @SaleTypeID = 1 AND @TransStatus = 1 AND @Status = 1
			BEGIN
				DECLARE @SpecialConfig BIT = 0
						
					UPDATE S SET S.Stock = S.Stock + @Qty FROM ProductStockMasters AS S WHERE S.LocationID = @LocationID AND S.StockCode = @StockCode	AND S.ProductId = @ProductId AND S.IsBundle = 0
				
			END
		END
	  
	FETCH NEXT FROM Update_Cursor INTO @StockCode, @Qty, @LocationID, @DocumentID, @BillTypeID, @SaleTypeID, @TransStatus, @Status, @ProductId, @AvgCost, @SerialNo;
	END
	CLOSE Update_Cursor 
	DEALLOCATE Update_Cursor
  
END";


            ExecuteTrigger(Stringsqlconnection);

            #endregion Trigger_UpdateStockInHeadOffice


        }
        public void ExecuteTrigger(string con)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(con))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(triggerquery, connection))
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
