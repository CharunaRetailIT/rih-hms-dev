using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class PaymentDet
    {
        public int   PaymentDetID  { get; set; }
	    public int  RowNo  { get; set; }
	    public int  PayTypeID  { get; set; }
	    public decimal  Amount  { get; set; }
	    public decimal  Balance  { get; set; }
	    public DateTime  SDate  { get; set; }
	    public string  Receipt  { get; set; }
	    public int  LocationID  { get; set; }
	    public int  CashierID  { get; set; }
	    public int  UnitNo  { get; set; }
	    public int  BillTypeID  { get; set; }
	    public int  SaleTypeID  { get; set; }
	    public string  RefNo  { get; set; }
	    public int  BankId  { get; set; }
	    public DateTime?  ChequeDate  { get; set; }
	    public int  IsRecallAdv  { get; set; }
	    public string  RecallNo  { get; set; }
	    public string  Descrip  { get; set; }
	    public string  EnCodeName  { get; set; }
	    public int  UpdatedBy  { get; set; }
	    public int  Status  { get; set; }
	    public int  ZNo  { get; set; }
	    public int  CustomerId  { get; set; }
	    public int  CustomerType  { get; set; }
	    public string  CustomerCode  { get; set; }
	    public int  GroupOfCompanyID  { get; set; }
	    public int  Datatransfer  { get; set; }
	    public DateTime  ZDate  { get; set; }
        [DefaultValue(0)]
	    public int  TerminalID  { get; set; }
	    public int  LoyaltyType  { get; set; }
        [DefaultValue(0)]
	    public int  IsUploadToGL  { get; set; }
        [DefaultValue(0)]
	    public int  LocationIDBilling  { get; set; }
        [DefaultValue(0)]
	    public int  TableID  { get; set; }
        [DefaultValue(0)]
	    public int  TicketID  { get; set; }
        [DefaultValue(0)]
	    public int  OrderNo  { get; set; }
        [DefaultValue(0)]
	    public int  ShiftNo  { get; set; }
        [DefaultValue(0)]
	    public bool  IsDayEnd  { get; set; }
        [DefaultValue(0)]
	    public int  UpdateUnitNo  { get; set; }
        [DefaultValue(0)]
	    public int  Online  { get; set; }
	    public string  SerialNo  { get; set; }
	    public string  CurrencyCode  { get; set; }
        [DefaultValue(0)]
	    public decimal  CurrencyRate  { get; set; }
	    public string  AcountNumber  { get; set; }
        [DefaultValue(0)]
	    public decimal  CopperratePrice  { get; set; }
        [DefaultValue(0)]
	    public bool  IsCopperratePriceEnable  { get; set; }
        [DefaultValue(0)]
	    public decimal  AmountCopperratePrice  { get; set; }
        [DefaultValue(0)]
	    public decimal  BalanceCopperratePrice  { get; set; }
    }
}