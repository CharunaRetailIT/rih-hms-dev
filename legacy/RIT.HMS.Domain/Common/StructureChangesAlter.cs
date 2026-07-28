using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace RIT.HMS.Domain.Common
{
   public  class StructureChangesAlter
    {
        string tableName = "";
        string query = "";
        string spName = "";
        string ViewName = "";
        string ColumnName = "";
        public bool status = true;
        string Stringsqlconnection = "";
        string AlterName = "";
        string Alterquery = "";

        public void RunAlter(string connectionString)
        {
            Stringsqlconnection = connectionString;
            #region AlterTransactionDets
            tableName = "TransactionDets";
            Alterquery = @"ALTER TABLE dbo.TransactionDets ADD Iscallorder bit NOT NULL DEFAULT 0;";
            ExecuteAlter(Stringsqlconnection);
            #endregion AlterTransactionDets
           
            #region Products
            tableName = "Products";
            Alterquery = @"ALTER TABLE Products ADD ImagePath  varchar(Max) default '' NULL;";

            ExecuteAlter(Stringsqlconnection);

            #endregion Products

            #region KitchenPrinterTypes
            tableName = "KitchenPrinterTypes";
            Alterquery = @"ALTER TABLE KitchenPrinterTypes ADD PrinterName  nvarchar(50) default '' NULL;";

            ExecuteAlter(Stringsqlconnection);
            #endregion KitchenPrinterTypes

            #region InvPriceLevelLists
            tableName = "InvPriceLevelLists";
            Alterquery = @"ALTER TABLE InvPriceLevelLists ADD IsDelete  bit default 0;";
            ExecuteAlter(Stringsqlconnection);
            #endregion InvPriceLevelLists

            #region Products
            tableName = "Products";
            Alterquery = @"ALTER TABLE Products ADD KitchenCode  varchar(10) default '';";
            ExecuteAlter(Stringsqlconnection);
            #endregion Products

            #region TransactionDets
            tableName = "TransactionDets";
            Alterquery = @"ALTER TABLE TransactionDets ADD KitchenCode  varchar(10) default '';";
            ExecuteAlter(Stringsqlconnection);
            #endregion TransactionDets


            #region SuspendDets
            tableName = "SuspendDets";
            Alterquery = @"ALTER TABLE SuspendDets ADD KitchenCode  varchar(10) default '';";
            ExecuteAlter(Stringsqlconnection);
            #endregion SuspendDets	

            #region TransactionDets ADD ServingUnitId
            tableName = "TransactionDets";
            Alterquery = @"ALTER TABLE TransactionDets ADD ServingUnitId  int NULL DEFAULT 0;";
            ExecuteAlter(Stringsqlconnection);
            #endregion TransactionDets ADD ServingUnitId	 			


            #region TransactionDets ADD Iscallorder
            tableName = "TransactionDets";
            Alterquery = @"ALTER TABLE TransactionDets ADD Iscallorder  bit NOT NULL DEFAULT 0;";
            ExecuteAlter(Stringsqlconnection);
            #endregion TransactionDets ADD Iscallorder

            #region TransactionDets ADD OrigUnitNo
            tableName = "TransactionDets";
            Alterquery = @"ALTER TABLE TransactionDets ADD OrigUnitNo  int NULL DEFAULT 0;";
            ExecuteAlter(Stringsqlconnection);
            #endregion TransactionDets ADD OrigUnitNo	 	
            
            #region SuspendDets ADD Iscallorder
            tableName = "SuspendDets";
            Alterquery = @"ALTER TABLE SuspendDets ADD Iscallorder  bit NOT NULL DEFAULT 0;";
            ExecuteAlter(Stringsqlconnection);
            #endregion SuspendDets ADD Iscallorder				
            
            #region SuspendDets ADD ServingUnitId
            tableName = "SuspendDets";
            Alterquery = @"ALTER TABLE SuspendDets ADD ServingUnitId  int NULL DEFAULT 0;";
            ExecuteAlter(Stringsqlconnection);
            #endregion SuspendDets ADD ServingUnitId

            #region KitchenMaster ADD KitchenCode
            tableName = "KitchenMaster";
            Alterquery = @"ALTER TABLE KitchenMaster ADD KitchenCode  varchar(10) default '';";
            ExecuteAlter(Stringsqlconnection);
            #endregion KitchenMaster ADD KitchenCode		

            #region TransactionDets ADD WebOrderNumber
            tableName = "TransactionDets";
            Alterquery = @"ALTER TABLE TransactionDets ADD WebOrderNumber  nvarchar(MAX)NOT NULL DEFAULT '';";
            ExecuteAlter(Stringsqlconnection);
            #endregion TransactionDets ADD WebOrderNumber


            #region SuspendDets ADD WebOrderNumber
            tableName = "SuspendDets";
            Alterquery = @"ALTER TABLE SuspendDets ADD WebOrderNumber  nvarchar(MAX)NOT NULL DEFAULT '';";
            ExecuteAlter(Stringsqlconnection);
            #endregion SuspendDets ADD WebOrderNumber

            #region Customers ADD SenderPreference
            tableName = "Customers";
            Alterquery = @"ALTER TABLE Customers ADD SenderPreference  int NULL DEFAULT 0;";
            ExecuteAlter(Stringsqlconnection);
            #endregion Customers ADD SenderPreference


            #region LOGCustomers ADD SenderPreference
            tableName = "LOGCustomers";
            Alterquery = @"ALTER TABLE LOGCustomers ADD SenderPreference  int NULL DEFAULT 0;";
            ExecuteAlter(Stringsqlconnection);
            #endregion LOGCustomers ADD SenderPreference	

            #region TransactionDets ADD WebOrderItemID
            tableName = "TransactionDets";
            Alterquery = @"ALTER TABLE TransactionDets ADD WebOrderItemID  bigint NOT NULL DEFAULT 0;";
            ExecuteAlter(Stringsqlconnection);
            #endregion TransactionDets ADD WebOrderItemID
            
            #region SuspendDets ADD WebOrderItemID
            tableName = "SuspendDets";
            Alterquery = @"ALTER TABLE SuspendDets ADD WebOrderItemID  bigint NOT NULL DEFAULT 0;";
            ExecuteAlter(Stringsqlconnection);
            #endregion SuspendDets ADD WebOrderItemID

            #region TransactionDets Add ReprintCount
            tableName = "TransactionDets";
            Alterquery = @"ALTER TABLE TransactionDets ADD ReprintCount  int NOT NULL DEFAULT 0;";
            ExecuteAlter(Stringsqlconnection);
            #endregion TransactionDets Add ReprintCount


            #region Request Note Accptance Headers
            tableName = "RequestNoteAccptanceHeaders";
            Alterquery = @"ALTER TABLE RequestNoteAccptanceHeaders ADD RequestnoteHeaderId  bigint NOT NULL DEFAULT 0;";
            ExecuteAlter(Stringsqlconnection);
            #endregion TransactionDets Add ReprintCount

            #region TransactionDets ADD IsCancelInvoice
            tableName = "TransactionDets";
            Alterquery = @"ALTER TABLE TransactionDets ADD IsCancelInvoice BIT NOT NULL DEFAULT 0;";
            ExecuteAlter(Stringsqlconnection);
            #endregion TransactionDets ADD IsCancelInvoice

            #region TransactionDets ADD IsWarrantyClaim
            tableName = "TransactionDets";
            Alterquery = @"ALTER TABLE TransactionDets ADD IsWarrantyClaim BIT NOT NULL DEFAULT 0;";
            ExecuteAlter(Stringsqlconnection);
            #endregion TransactionDets ADD IsWarrantyClaim

            #region TransactionDets ADD IsRefundEntireInvoice
            tableName = "TransactionDets";
            Alterquery = @"ALTER TABLE TransactionDets ADD IsRefundEntireInvoice BIT NOT NULL DEFAULT 0;";
            ExecuteAlter(Stringsqlconnection);
            #endregion TransactionDets ADD IsRefundEntireInvoice

            #region TransactionDets ADD Remark
            tableName = "TransactionDets";
            Alterquery = @"ALTER TABLE TransactionDets ADD Remark varchar(250) ;";
            ExecuteAlter(Stringsqlconnection);
            #endregion TransactionDets ADD Remark
        }
        public void ExecuteAlter(string con)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(con))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(Alterquery, connection))
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
