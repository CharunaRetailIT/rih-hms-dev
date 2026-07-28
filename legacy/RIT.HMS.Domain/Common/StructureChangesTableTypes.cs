using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Common
{
    public class StructureChangesTableTypes
    {
        string tableName = "";
        string query = "";
        string spName = "";
        string ViewName = "";
        string ColumnName = "";
        public bool status = true;
        string Stringsqlconnection = "";
        string UDTypeName = "";
        string UDQuery = "";

        public void ExecuteUDtype(string con)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(con))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(UDQuery, connection))
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

        public void RunUDtype(string connectionString)
        {
            Stringsqlconnection = connectionString;

            #region AdvanceDet
            UDTypeName = "AdvanceDet";
            UDQuery = @"CREATE TYPE [dbo].[AdvanceDet] AS TABLE(
	[ProductID] [bigint] NOT NULL,
	[ProductCode] [varchar](25) NOT NULL,
	[RefCode] [varchar](25) NOT NULL,
	[BarCodeFull] [bigint] NOT NULL,
	[Descrip] [varchar](50) NOT NULL,
	[BatchNo] [varchar](50) NOT NULL,
	[SerialNo] [varchar](50) NOT NULL,
	[ExpiaryDate] [date] NULL,
	[Cost] [decimal](18, 4) NOT NULL,
	[AvgCost] [decimal](18, 4) NOT NULL,
	[Price] [decimal](18, 4) NOT NULL,
	[Qty] [decimal](18, 4) NOT NULL,
	[Amount] [decimal](18, 4) NOT NULL,
	[UnitOfMeasureID] [bigint] NOT NULL,
	[UnitOfMeasureName] [varchar](10) NOT NULL,
	[ConvertFactor] [decimal](18, 4) NOT NULL,
	[IDI1] [int] NOT NULL,
	[IDis1] [decimal](18, 4) NOT NULL,
	[IDiscount1] [decimal](18, 4) NOT NULL,
	[IDI1CashierID] [bigint] NOT NULL,
	[IDI2] [int] NOT NULL,
	[IDis2] [decimal](18, 4) NOT NULL,
	[IDiscount2] [decimal](18, 4) NOT NULL,
	[IDI2CashierID] [bigint] NOT NULL,
	[IDI3] [int] NOT NULL,
	[IDis3] [decimal](18, 4) NOT NULL,
	[IDiscount3] [decimal](18, 4) NOT NULL,
	[IDI3CashierID] [bigint] NOT NULL,
	[IDI4] [int] NOT NULL,
	[IDis4] [decimal](18, 4) NOT NULL,
	[IDiscount4] [decimal](18, 4) NOT NULL,
	[IDI4CashierID] [bigint] NOT NULL,
	[IDI5] [int] NOT NULL,
	[IDis5] [decimal](18, 4) NOT NULL,
	[IDiscount5] [decimal](18, 4) NOT NULL,
	[IDI5CashierID] [bigint] NOT NULL,
	[Rate] [decimal](18, 4) NOT NULL,
	[IsSDis] [bit] NOT NULL,
	[SDNo] [int] NOT NULL,
	[SDID] [int] NOT NULL,
	[SDIs] [decimal](18, 4) NOT NULL,
	[SDiscount] [decimal](18, 4) NOT NULL,
	[DDisCashierID] [bigint] NOT NULL,
	[Nett] [decimal](18, 4) NOT NULL,
	[LocationID] [int] NOT NULL,
	[DocumentID] [int] NOT NULL,
	[BillTypeID] [int] NOT NULL,
	[SaleTypeID] [int] NOT NULL,
	[Receipt] [char](10) NOT NULL,
	[SalesmanID] [bigint] NOT NULL,
	[Salesman] [varchar](15) NOT NULL,
	[CustomerID] [bigint] NOT NULL,
	[Customer] [varchar](15) NOT NULL,
	[CashierID] [bigint] NOT NULL,
	[Cashier] [varchar](15) NOT NULL,
	[StartTime] [time](7) NOT NULL,
	[EndTime] [time](7) NOT NULL,
	[RecDate] [date] NOT NULL,
	[BaseUnitID] [bigint] NOT NULL,
	[UnitNo] [int] NOT NULL,
	[RowNo] [int] NOT NULL,
	[IsRecall] [bit] NOT NULL,
	[RecallNo] [char](10) NOT NULL,
	[RecallAdv] [bit] NOT NULL,
	[TaxAmount] [decimal](18, 4) NOT NULL,
	[IsTax] [bit] NOT NULL,
	[TaxPercentage] [decimal](18, 4) NOT NULL,
	[IsStock] [bit] NOT NULL,
	[CreditNoteNo] [varchar](120) NOT NULL,
	[CreditNoteBy] [bigint] NOT NULL,
	[CustomerType] [int] NOT NULL,
	[TransStatus] [int] NOT NULL,
	[IsPromotionApplied] [bit] NOT NULL,
	[PromotionID] [int] NOT NULL,
	[IsPromotion] [bit] NOT NULL,
	[ItemSerial] [varchar](150) NOT NULL,
	[warranty] [varchar](150) NOT NULL,
	[RecallFromInvoiceNo] [varchar](50) NULL,
	[IsNewPrice] [bit] NOT NULL DEFAULT ((0)),
	[IsApproved] [bit] NOT NULL DEFAULT ((0)),
	[ApprovedBy] [bigint] NOT NULL DEFAULT ((0)),
	[ApprovedFor] [nchar](10) NULL DEFAULT (''),
	[ReferenceProductId] [int] NOT NULL DEFAULT ((0)),
	[ReferenceProductRow] [int] NOT NULL DEFAULT ((0)),
	[PrinterType] [int] NULL,
	[IsAddonItem] [bit] NULL,
	[TableNumber] [int] NULL,
	[IsTaxEnable] [bit] NULL,
	[TaxCode] [varchar](50) NULL,
	[SplitItemReceiptNo] [varchar](50) NULL,
	[IsPritRpt] [bit] NULL,
	[ProductRemark] [varchar](200) NULL,
	[OrderStatus] [int] NULL,
	[ServingUnit] [varchar](50) NULL,
	[NoOfCustomers] [int] NULL,
	[IsShowOnBill] [bit] NULL,
	[DeploCardNo] [varchar](50) NULL,
	[ServingUnitId] [int] NULL
)";


            ExecuteUDtype(Stringsqlconnection);

            #endregion AdvanceDet

            #region PosSuspendDet2
            UDTypeName = "PosSuspendDet2";
            UDQuery = @"CREATE TYPE [dbo].[PosSuspendDet2] AS TABLE(
	[ProductID] [int] NOT NULL,
	[ProductCode] [nvarchar](25) NULL,
	[RefCode] [nvarchar](25) NULL,
	[BarCodeFull] [bigint] NOT NULL,
	[Descrip] [nvarchar](50) NULL,
	[BatchNo] [nvarchar](50) NULL,
	[SerialNo] [nvarchar](50) NULL,
	[ExpiaryDate] [datetime] NULL,
	[Cost] [decimal](18, 2) NOT NULL,
	[AvgCost] [decimal](18, 2) NOT NULL,
	[Qty] [decimal](18, 2) NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[UnitOfMeasureID] [int] NOT NULL,
	[UnitOfMeasureName] [nvarchar](10) NULL,
	[ConvertFactor] [decimal](18, 2) NOT NULL,
	[IDI1] [int] NOT NULL,
	[IDis1] [decimal](18, 2) NOT NULL,
	[IDiscount1] [decimal](18, 2) NOT NULL,
	[IDI1CashierID] [int] NOT NULL,
	[IDI2] [int] NOT NULL,
	[IDis2] [decimal](18, 2) NOT NULL,
	[IDiscount2] [decimal](18, 2) NOT NULL,
	[IDI2CashierID] [int] NOT NULL,
	[IDI3] [int] NOT NULL,
	[IDis3] [decimal](18, 2) NOT NULL,
	[IDiscount3] [decimal](18, 2) NOT NULL,
	[IDI3CashierID] [int] NOT NULL,
	[IDI4] [int] NOT NULL,
	[IDis4] [decimal](18, 2) NOT NULL,
	[IDiscount4] [decimal](18, 2) NOT NULL,
	[IDI4CashierID] [int] NOT NULL,
	[IDI5] [int] NOT NULL,
	[IDis5] [decimal](18, 2) NOT NULL,
	[IDiscount5] [decimal](18, 2) NOT NULL,
	[IDI5CashierID] [int] NOT NULL,
	[Rate] [decimal](18, 2) NOT NULL,
	[IsSDis] [bit] NOT NULL,
	[SDNo] [int] NOT NULL,
	[SDID] [int] NOT NULL,
	[SDIs] [decimal](18, 2) NOT NULL,
	[SDiscount] [decimal](18, 2) NOT NULL,
	[DDisCashierID] [int] NOT NULL,
	[Nett] [decimal](18, 2) NOT NULL,
	[LocationID] [int] NOT NULL,
	[DocumentID] [int] NOT NULL,
	[BillTypeID] [int] NOT NULL,
	[SaleTypeID] [int] NOT NULL,
	[Receipt] [nvarchar](10) NULL,
	[SalesmanID] [int] NOT NULL,
	[Salesman] [nvarchar](15) NULL,
	[CustomerID] [int] NOT NULL,
	[Customer] [nvarchar](15) NULL,
	[CashierID] [int] NOT NULL,
	[Cashier] [nvarchar](15) NULL,
	[StartTime] [datetime] NOT NULL,
	[EndTime] [datetime] NOT NULL,
	[RecDate] [datetime] NOT NULL,
	[BaseUnitID] [int] NOT NULL,
	[UnitNo] [int] NOT NULL,
	[RowNo] [int] NOT NULL,
	[IsRecall] [bit] NOT NULL,
	[RecallNo] [nvarchar](50) NULL,
	[RecallAdv] [bit] NOT NULL,
	[TaxAmount] [decimal](18, 2) NOT NULL,
	[IsTax] [bit] NOT NULL,
	[TaxPercentage] [decimal](18, 2) NOT NULL,
	[IsStock] [bit] NOT NULL,
	[SuspendNo] [nvarchar](50) NULL,
	[SuspendBy] [int] NOT NULL,
	[CustomerType] [int] NOT NULL,
	[TransStatus] [int] NOT NULL,
	[IsPromotionApplied] [bit] NOT NULL,
	[PromotionID] [int] NOT NULL,
	[IsPromotion] [bit] NOT NULL,
	[InvPriceLevelID] [int] NOT NULL,
	[ItemSerial] [nvarchar](50) NULL,
	[warranty] [nvarchar](50) NULL,
	[TableNumber] [int] NOT NULL,
	[PrinterType] [int] NOT NULL,
	[IsPritRpt] [bit] NOT NULL,
	[ReferenceProductId] [int] NULL,
	[ReferenceProductRow] [int] NULL,
	[IsAddonItem] [bit] NOT NULL,
	[IsTaxEnable] [bit] NOT NULL,
	[TaxCode] [nvarchar](50) NULL,
	[SplitItemReceiptNo] [nvarchar](50) NULL,
	[Price] [decimal](18, 2) NULL,
	[ProductRemark] [varchar](200) NULL,
	[DeploCardNo] [varchar](50) NOT NULL,
	[IsShowOnBill] [bit] NOT NULL,
	[ServingUnit] [varchar](50) NULL,
	[OrderStatus] [int] NULL,
	[NoOfCustomers] [int] NOT NULL,
	[KitchenCode] [varchar](50) NOT NULL DEFAULT (''),
	[ServingUnitId] [int] NOT NULL DEFAULT ((0)),
	[OrigUnitNo] [int] NOT NULL DEFAULT ((0))
)";


            ExecuteUDtype(Stringsqlconnection);

            #endregion PosSuspendDet2

            #region AdvancePaymentDet
            UDTypeName = "AdvancePaymentDet";
            UDQuery = @"CREATE TYPE [dbo].[AdvancePaymentDet] AS TABLE(
	[Idx] [bigint] NOT NULL,
	[RowNo] [bigint] NOT NULL,
	[PayTypeID] [int] NOT NULL,
	[Amount] [decimal](18, 4) NOT NULL,
	[Balance] [decimal](18, 4) NOT NULL,
	[SDate] [datetime] NOT NULL,
	[Receipt] [char](10) NOT NULL,
	[LocationID] [int] NOT NULL,
	[CashierID] [bigint] NOT NULL,
	[UnitNo] [int] NOT NULL,
	[BillTypeID] [int] NOT NULL,
	[RefNo] [varchar](30) NOT NULL,
	[BankId] [bigint] NOT NULL,
	[ChequeDate] [date] NULL,
	[IsRecallAdv] [bit] NOT NULL,
	[RecallNo] [varchar](10) NOT NULL,
	[Descrip] [varchar](20) NOT NULL,
	[EnCodeName] [varchar](50) NOT NULL,
	[SuspendNo] [nchar](10) NOT NULL,
	[SuspendBy] [bigint] NOT NULL,
	[IsDeleteOnRecall] [bit] NOT NULL,
	[AdvanceNumber] [varchar](20) NOT NULL
)";


            ExecuteUDtype(Stringsqlconnection);

            #endregion AdvancePaymentDet

            #region PosSuspendHed1
            UDTypeName = "PosSuspendHed1";
            UDQuery = @"CREATE TYPE [dbo].[PosSuspendHed1] AS TABLE(
	[SuspendNo] [varchar](50) NOT NULL,
	[Receipt] [varchar](50) NOT NULL,
	[LocationID] [int] NOT NULL,
	[UnitNo] [int] NOT NULL,
	[STime] [time](7) NOT NULL,
	[SDate] [date] NOT NULL,
	[Amount] [money] NOT NULL,
	[CashierID] [bigint] NOT NULL,
	[IsRecall] [bit] NOT NULL,
	[RecallReceipt] [varchar](50) NULL,
	[RecallCashierID] [bigint] NULL,
	[RecallCashier] [varchar](20) NULL,
	[RecallUnitNo] [int] NULL,
	[RecallTime] [time](7) NOT NULL,
	[TransStatus] [int] NOT NULL,
	[TokenNumber] [nvarchar](50) NULL,
	[NextBillDate] [int] NOT NULL,
	[CustomerId] [int] NOT NULL,
	[TableNumber] [int] NOT NULL,
	[OrderStatus] [int] NULL,
	[OrigSuspendNo] [varchar](50) NULL
)";


            ExecuteUDtype(Stringsqlconnection);

            #endregion AdvancePaymentDet

            #region BudgetOutlet
            UDTypeName = "BudgetOutlet";
            UDQuery = @"CREATE TABLE [dbo].[BudgetOutlet](
	[BudgetOutletID] [int] IDENTITY(1,1) NOT NULL,
	[locationID] [int] NULL,
	[BudgetType] [int] NULL,
	[isActive] [bit] NULL,
	[StartingDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[totalbudget] [decimal](18, 2) NULL,
	[CreatedUser] [varchar](50) NULL,
	[CreatedDate] [datetime] NULL,
	[ModifiedUser] [varchar](50) NULL,
	[ModifiedDate] [datetime] NULL,
	[NoofDMWY] [int] NULL,
 CONSTRAINT [PK_BudgetOutlet] PRIMARY KEY CLUSTERED 
(
	[BudgetOutletID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";


            ExecuteUDtype(Stringsqlconnection);

            #endregion BudgetOutlet
        }
    }
}
