using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.Transactions
{
    public class TmpProductStockDetail
    {
    [Key]
    public int TmpProductStockDetailsID { get; set; }
	public int CompanyID { get; set; }
	public int LocationID { get; set; }
	public string ToLocationName { get; set; }
	public DateTime GivenDate { get; set; }
	public int ProductID { get; set; }
	public string ProductCode { get; set; }
	public string ProductName { get; set; }
	public int TransactionType { get; set; }
	public string TransactionNo { get; set; }
	public string BatchNo { get; set; }
	public DateTime TransactionDate { get; set; }
	[DefaultValue(0)]
    public decimal CostPrice { get; set; }
    [DefaultValue(0)]
	public decimal SellingPrice { get; set; }
    [DefaultValue(0)]
	public decimal AverageCost { get; set; }
    [DefaultValue(0)]
	public decimal Amount { get; set; }
    [DefaultValue(0)]
	public int DepartmentID { get; set; }
    [DefaultValue(0)]
	public int CategoryID { get; set; }
    [DefaultValue(0)]
	public int SubCategoryID { get; set; }
    [DefaultValue(0)]
	public int SubCategory2ID { get; set; }
    [DefaultValue(0)]
	public int SupplierID { get; set; }
    [DefaultValue(0)]
	public int CustomerID { get; set; }
    [DefaultValue(0)]
	public decimal StockQty { get; set; }
    [DefaultValue(0)]
	public decimal Qty1 { get; set; }
    [DefaultValue(0)]
	public decimal Qty2 { get; set; }
    [DefaultValue(0)]
	public decimal Qty3 { get; set; }
    [DefaultValue(0)]
	public decimal Qty4 { get; set; }
    [DefaultValue(0)]
	public decimal Qty5 { get; set; }
    [DefaultValue(0)]
	public decimal Qty6 { get; set; }
    [DefaultValue(0)]
	public decimal Qty7 { get; set; }
    [DefaultValue(0)]
	public decimal Qty8 { get; set; }
    [DefaultValue(0)]
	public decimal Qty9 { get; set; }
    [DefaultValue(0)]
	public decimal Qty10 { get; set; }
    [DefaultValue(0)]
	public int UserID { get; set; }
    [DefaultValue(0)]
	public int UniqueID { get; set; }
    [DefaultValue(0)]
	public decimal GrossProfit { get; set; }
    [DefaultValue(0)]
	public int IsDelete { get; set; }
    [DefaultValue(1)]
	public int GroupOfCompanyID { get; set; }
    [DefaultValue("")]
	public string CreatedUser { get; set; }
	public DateTime CreatedDate { get; set; }
    [DefaultValue("")]
	public string ModifiedUser { get; set; }
	public DateTime ModifiedDate { get; set; }
    [DefaultValue(0)]
	public int DataTransfer { get; set; }
    [DefaultValue(0)]
	public int ZNo { get; set; }
    [DefaultValue(0)]
	public int UnitNo { get; set; }

	public string SuppName { get; set; }
    [DefaultValue(0)]
	public int SerialNo { get; set; }




    }
}