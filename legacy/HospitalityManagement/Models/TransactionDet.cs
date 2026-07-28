using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class TransactionDet
    {
        public int   TransactionDetID  { get; set; }
        [DefaultValue(0)]
	    public int  ProductID  { get; set; }
        [DefaultValue(0)]
	    public string  ProductCode  { get; set; }
        [DefaultValue(0)]
	    public string  RefCode  { get; set; }
        [DefaultValue(0)]
	    public int  BarCodeFull  { get; set; }
        [DefaultValue(0)]
	    public string  Descrip  { get; set; }
        [DefaultValue(0)]
	    public string  BatchNo  { get; set; }
        [DefaultValue(0)]
	    public string  SerialNo  { get; set; }
	    public DateTime?  ExpiryDate  { get; set; }
        [DefaultValue(0)]
	    public decimal  Cost  { get; set; }
        [DefaultValue(0)]
	    public decimal  AvgCost  { get; set; }
        [DefaultValue(0)]
	    public decimal  Price  { get; set; }
        [DefaultValue(0)]
	    public decimal  Qty  { get; set; }
        [DefaultValue(0)]
	    public decimal  BalanceQty  { get; set; }
        [DefaultValue(0)]
	    public decimal  Amount  { get; set; }
        [DefaultValue(0)]
	    public int  UnitOfMeasureID  { get; set; }
        [DefaultValue(0)]
	    public string  UnitOfMeasureName  { get; set; }
        [DefaultValue(0)]
	    public decimal  ConvertFactor  { get; set; }
        [DefaultValue(0)]
	    public int  IDI1  { get; set; }
        [DefaultValue(0)]
	    public decimal  IDis1  { get; set; }
        [DefaultValue(0)]
	    public decimal  IDiscount1  { get; set; }
        [DefaultValue(0)]
	    public int  IDI1CashierID  { get; set; }
        [DefaultValue(0)]
	    public int  IDI2    { get; set; }
        
        [DefaultValue(0)]
	    public decimal  IDis2  { get; set; }
        
        [DefaultValue(0)]
	    public decimal  IDiscount2  { get; set; }
        
        [DefaultValue(0)]
	    public int  IDI2CashierID  { get; set; }
        
        [DefaultValue(0)]
	    public int  IDI3  { get; set; }
        
        [DefaultValue(0)]
	    public decimal  IDis3  { get; set; }
        
        [DefaultValue(0)]
	    public decimal  IDiscount3  { get; set; }
        
        [DefaultValue(0)]
	    public int  IDI3CashierID  { get; set; }
        
        [DefaultValue(0)]
	    public decimal  IDI4  { get; set; }
        
        [DefaultValue(0)]
	    public decimal IDis4  { get; set; }
        
        [DefaultValue(0)]
	    public decimal IDiscount4 { get; set; }
        
        [DefaultValue(0)]
	    public int IDI4CashierID  { get; set; }
        
        [DefaultValue(0)]
	    public int IDI5  { get; set; }
        
        [DefaultValue(0)]
	    public decimal IDis5  { get; set; }
        [DefaultValue(0)]
	    public decimal IDiscount5 { get; set; }
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
	    public decimal Nett  { get; set; }
        [DefaultValue(0)]
	    public int LocationID  { get; set; }
        [DefaultValue(0)]
	    public int DocumentID  { get; set; }
        [DefaultValue(0)]
	    public int BillTypeID  { get; set; }
        [DefaultValue(0)]
	    public int SaleTypeID  { get; set; }
        [DefaultValue(0)]
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
	    public DateTime StartTime  { get; set; }
	    public DateTime EndTime  { get; set; }
	    public DateTime RecDate  { get; set; }
        [DefaultValue(0)]
	    public int BaseUnitID  { get; set; }
        [DefaultValue(0)]
	    public int UnitNo  { get; set; }
        [DefaultValue(0)]
	    public int RowNo  { get; set; }
        [DefaultValue(0)]
	    public bool IsRecall  { get; set; }
        [DefaultValue(0)]
	    public string RecallNO  { get; set; }
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
	    public int UpdateBy  { get; set; }
        [DefaultValue(0)]
	    public int Status  { get; set; }
        [DefaultValue(0)]
	    public int ZNo  { get; set; }
        [DefaultValue(0)]
	    public int GroupOfCompanyID  { get; set; }
        [DefaultValue(0)]
	    public int  DataTransfer  { get; set; }
        [DefaultValue(0)]
	    public int CustomerType  { get; set; }
        [DefaultValue(0)]
	    public int TransStatus  { get; set; }
	    public DateTime ZDate  { get; set; }
        [DefaultValue(0)]
	    public int IsPromotionApplied  { get; set; }
        [DefaultValue(0)]
	    public int PromotionID  { get; set; }
        [DefaultValue(0)]
	    public int IsPromotion  { get; set; }
        [DefaultValue(0)]
	    public int LocationIDBilling  { get; set; }
        [DefaultValue(0)]
	    public int TableID  { get; set; }
        [DefaultValue(0)]
	    public int OrderTerminalID  { get; set; }
        [DefaultValue(0)]
	    public int TicketID  { get; set; }
        [DefaultValue(0)]
	    public int OrderNo  { get; set; }
        [DefaultValue(0)]
	    public int IsPrinted  { get; set; }
        [DefaultValue(0)]
	    public string ItemComment  { get; set; }
        [DefaultValue(0)]
	    public int Packs  { get; set; }
        [DefaultValue(0)]
	    public bool IsCancelKOT  { get; set; }
        [DefaultValue(0)]
	    public int StewardID  { get; set; }
        [DefaultValue(0)]
	    public string StewardName  { get; set; }
        [DefaultValue(0)]
	    public decimal ServiceCharge  { get; set; }
        [DefaultValue(0)]
	    public decimal ServiceChargeAmount  { get; set; }
        [DefaultValue(0)]
	    public int ShiftNo  { get; set; }
        [DefaultValue(0)]
	    public bool IsDayEnd  { get; set; }
        [DefaultValue(0)]
	    public int UpdateUnitNo  { get; set; }
        [DefaultValue(0)]
	    public int InvPriceLevelID  { get; set; }
        [DefaultValue(0)]
	    public int Online  { get; set; }
	    public DateTime Deliverdate  { get; set; }
        [DefaultValue(0)]
	    public decimal PackSize  { get; set; }
        [DefaultValue(0)]
	    public string TourAgentCode  { get; set; }
        [DefaultValue(0)]
	    public int TourAgentId  { get; set; }
        [DefaultValue(0)]
	    public decimal TourAmount  { get; set; }
        [DefaultValue(0)]
	    public decimal TourPrecent  { get; set; }
        [DefaultValue(0)]
	    public decimal TourCommition  { get; set; }
        [DefaultValue(0)]
	    public decimal TourCommitionPaidAmount  { get; set; }
        
	    public string TourAgentCompanyCode  { get; set; }
        [DefaultValue(0)]
	    public int TourAgentCompanyId  { get; set; }
        [DefaultValue(0)]
	    public decimal TourCompanyAmount  { get; set; }
        [DefaultValue(0)]
	    public decimal TourCompanyPrecent  { get; set; }
        [DefaultValue(0)]
	    public decimal TourCompanyCommition  { get; set; }
        [DefaultValue(0)]
	    public decimal TourCompanyCommitionPaidAmount  { get; set; }
        [DefaultValue(0)]
	    public decimal DelvryBalQty  { get; set; }
        [DefaultValue(0)]
	    public decimal warranty  { get; set; }
	    public string ItemSerial  { get; set; }
        [DefaultValue(0)]
	    public int CreditPeriod  { get; set; }
        [DefaultValue(0)]
	    public decimal CopperratePrice  { get; set; }
        [DefaultValue(0)]
	    public decimal SellingCopperratePrice  { get; set; }
        [DefaultValue(0)]
	    public decimal AmountCopperratePrice  { get; set; }
        [DefaultValue(0)]
	    public decimal NettCopperratePrice  { get; set; }
        [DefaultValue(0)]
	    public bool IsCopperratePriceEnable  { get; set; }
        [DefaultValue(0)]
	    public decimal RateCopperratePrice  { get; set; }
	    public bool IsBundleItem  { get; set; }
        [DefaultValue(0)]
	    public int NextBillDate  { get; set; }
        [DefaultValue(0)]
	    public decimal PackPrice  { get; set; }
        [DefaultValue(0)]
	    public bool IsPackSale  { get; set; }
        [DefaultValue(0)]
	    public decimal ExchageQty  { get; set; }
    }
}