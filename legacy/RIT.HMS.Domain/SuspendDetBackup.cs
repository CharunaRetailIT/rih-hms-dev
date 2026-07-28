using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class SuspendDetBackup
    {
        [Key]
        public int Idx  { get; set; }
	    public int ProductID  { get; set; }
        [DefaultValue(0)]
	    public string ProductCode  { get; set; }
        [DefaultValue(0)]
	    public string RefCode  { get; set; }
	    public int BarCodeFull  { get; set; }
        [DefaultValue(0)]
	    public string Descrip  { get; set; }
        [DefaultValue(0)]
	    public string BatchNo  { get; set; }
        [DefaultValue(0)]
	    public string SerialNo  { get; set; }
	    public DateTime ExpiaryDate  { get; set; }
	    public decimal Cost { get; set; }
        [DefaultValue(0)]
	    public decimal AvgCost  { get; set; }
	    public decimal Price { get; set; }
	    public decimal Qty { get; set; }
	    public decimal Amount { get; set; }
        [DefaultValue(0)]
	    public int UnitOfMeasureID  { get; set; }
        [DefaultValue(0)]
	    public string UnitOfMeasureName  { get; set; }
        [DefaultValue(0)]
	    public decimal ConvertFactor  { get; set; }
        [DefaultValue(0)]
	    public int IDI1  { get; set; }
        [DefaultValue(0)]
	    public decimal IDis1  { get; set; }
        [DefaultValue(0)]
	    public decimal IDiscount1  { get; set; }
        [DefaultValue(0)]
	    public int IDI1CashierID { get; set; }
        [DefaultValue(0)]
	    public int IDI2  { get; set; }
        [DefaultValue(0)]
	    public decimal IDis2 { get; set; }
        [DefaultValue(0)]
	    public decimal IDiscount2  { get; set; }
        [DefaultValue(0)]
	    public int IDI2CashierID  { get; set; }
        [DefaultValue(0)]
	    public int IDI3  { get; set; }
        [DefaultValue(0)]
	    public decimal IDis3 { get; set; }
        [DefaultValue(0)]
	    public decimal IDiscount3  { get; set; }
        [DefaultValue(0)]
	    public int IDI3CashierID  { get; set; }
        [DefaultValue(0)]
	    public int IDI4  { get; set; }
        [DefaultValue(0)]
	    public decimal IDis4  { get; set; }
        [DefaultValue(0)]
	    public decimal IDiscount4  { get; set; }
        [DefaultValue(0)]
	    public int IDI4CashierID { get; set; }
        [DefaultValue(0)]
	    public int IDI5  { get; set; }
        [DefaultValue(0)]
	    public decimal IDis5  { get; set; }
        [DefaultValue(0)]
	    public decimal IDiscount5  { get; set; }
        [DefaultValue(0)]
	    public int IDI5CashierID  { get; set; }
        [DefaultValue(0)]
	    public decimal Rate  { get; set; }
        [DefaultValue(0)]
	    public bool IsSDis  { get; set; }
        [DefaultValue(0)]
	    public int SDNo  { get; set; }
        [DefaultValue(0)]
	    public int SDID  { get; set; }
        [DefaultValue(0)]
	    public decimal SDIs  { get; set; }
        [DefaultValue(0)]
	    public decimal SDiscount  { get; set; }
        [DefaultValue(0)]
	    public int DDisCashierID  { get; set; }
        [DefaultValue(0)]
	    public decimal Nett { get; set; }
	    public int LocationID  { get; set; }
	    public int DocumentID  { get; set; }
	    public int BillTypeID  { get; set; }
	    public int SaleTypeID  { get; set; }
	    public string Receipt  { get; set; }
        [DefaultValue(0)]
	    public int SalesmanID  { get; set; }
        [DefaultValue(0)]
	    public string Salesman  { get; set; }
        [DefaultValue(0)]
	    public int CustomerID  { get; set; }
        [DefaultValue(0)]
	    public string Customer  { get; set; }
        [DefaultValue(0)]
	    public int CashierID  { get; set; }
        [DefaultValue(0)]
	    public string Cashier  { get; set; }
	    public DateTime StartTime { get; set; }
	    public DateTime EndTime  { get; set; }
	    public DateTime RecDate  { get; set; }
        [DefaultValue(0)]
	    public int BaseUnitID  { get; set; }
	    public int UnitNo  { get; set; }
	    public int RowNo  { get; set; }
        [DefaultValue(0)]
	    public bool IsRecall  { get; set; }
        [DefaultValue(0)]
	    public string RecallNo  { get; set; }
        [DefaultValue(0)]
	    public bool RecallAdv  { get; set; }
        [DefaultValue(0)]
	    public decimal TaxAmount  { get; set; }
        [DefaultValue(0)]
	    public bool IsTax  { get; set; }
        [DefaultValue(0)]
	    public decimal TaxPercentage  { get; set; }
        [DefaultValue(0)]
	    public bool IsStock  { get; set; }
        [DefaultValue(0)]
	    public string SuspendNo  { get; set; }
	    public int SuspendBy  { get; set; }
	    public int CustomerType  { get; set; }
	    public int TransStatus  { get; set; }
    }
}