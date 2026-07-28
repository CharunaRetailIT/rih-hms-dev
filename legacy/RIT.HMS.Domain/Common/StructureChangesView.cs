using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Common
{
    public class StructureChangesView
    {
        string tableName = "";
        string query = "";
        string spName = "";
        string ViewName = "";
        string ColumnName = "";
        public bool status = true;
        string Stringsqlconnection = "";
        string CheckspName = "";
        string Viewquery = "";
        string CheckViewQuery = "";

        private void CheckView(string stringViewname)
        {
            CheckViewQuery = "";
               CheckViewQuery = string.Format(@"IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].{0}'))
                        DROP VIEW [dbo].{1}", stringViewname, stringViewname);

            ExecuteViewCheckQuery(Stringsqlconnection);
            
        }

        public void RunView(string connectionString)
        {
            Stringsqlconnection = connectionString;

            #region View_Sales
            ViewName = "View_Sales";
            Viewquery = @"  

Create VIEW dbo.View_Sales
AS
SELECT 
    dbo.TransactionDets.LocationID, 
    YEAR(dbo.TransactionDets.RecDate) AS recyear, 
    dbo.TransactionDets.RecDate, 
    DATEPART(week, dbo.TransactionDets.RecDate) AS recweek, 
    MONTH(dbo.TransactionDets.RecDate) AS recmonth, 
    CASE 
        WHEN documentid IN (1, 3) THEN nett 
        ELSE -1 * nett 
    END AS nett, 
    CASE 
        WHEN documentid IN (1, 3) THEN cost * Qty 
        ELSE -1 * cost * qty 
    END AS cost, 
    dbo.TransactionDets.ProductID, 
    dbo.Products.ProductCode, 
    dbo.TransactionDets.Descrip,
    CASE 
        WHEN documentid IN (1, 3) THEN dbo.TransactionDets.Qty 
        ELSE dbo.TransactionDets.Qty * -1 
    END AS Qty,
    dbo.PrinterTypes.PrinterTypeId, 
    dbo.PrinterTypes.PrinterTypeName,
    ISNULL(dbo.CateringMoods.CateringMoodID, 0) AS CateringMoodID, 
    ISNULL(dbo.CateringMoods.CateringMoodName, '') AS CateringMoodName,
    dbo.RstDepartments.RstDepartmentID, 
    dbo.RstDepartments.DepartmentName, 
    dbo.Customers.CustomerCategoryId, 
    dbo.TransactionDets.Receipt, 
    dbo.TransactionDets.EndTime, 
    dbo.TransactionDets.StartTime, 
    dbo.TransactionDets.ZNo, 
    dbo.SysLocations.CompanyID, 
    dbo.TransactionDets.ServingUnitId
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
    AND dbo.TransactionDets.Status = 1;




";


            CheckView(ViewName);
            ExecuteView(Stringsqlconnection);










            #endregion View_Sales

        }
        public void ExecuteViewCheckQuery(string con)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(con))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(CheckViewQuery, connection))
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
        public void ExecuteView(string con)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(con))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(Viewquery, connection))
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
