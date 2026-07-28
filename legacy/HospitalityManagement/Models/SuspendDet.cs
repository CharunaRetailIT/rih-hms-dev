using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class SuspendDet
    {
        [Key]
        public int Idx { get; set; }
        public int ProductID { get; set; }
        [DefaultValue("")]
        [MaxLength(25)]
        public string ProductCode { get; set; }
        [DefaultValue("")]
        [MaxLength(25)]
        public string RefCode { get; set; }
        public long BarCodeFull { get; set; }
        [DefaultValue("")]
        [MaxLength(50)]
        public string Descrip { get; set; }
        [DefaultValue("")]
        [MaxLength(50)]
        public string BatchNo { get; set; }

        [DefaultValue("")]
        [MaxLength(50)]
        public string SerialNo { get; set; }


        public DateTime ExpiaryDate { get; set; }

        [DefaultValue(0)]
        public decimal Cost { get; set; }
        [DefaultValue(0)]
        public decimal AvgCost { get; set; }
        [DefaultValue(0)]
        public decimal Qty { get; set; }
        [DefaultValue(0)]
        public decimal Amount { get; set; }
        public int UnitOfMeasureID { get; set; }
        [DefaultValue("")]
        [MaxLength(10)]
        public string UnitOfMeasureName { get; set; }
        [DefaultValue(0)]
        public decimal ConvertFactor { get; set; }
        public int IDI1 { get; set; }
        [DefaultValue(0)]
        public decimal IDis1 { get; set; }
        [DefaultValue(0)]
        public decimal IDiscount1 { get; set; }
        public int IDI1CashierID { get; set; }
        public int IDI2 { get; set; }
        public decimal IDis2 { get; set; }
        public decimal IDiscount2 { get; set; }
        public int IDI2CashierID { get; set; }
        public int IDI3 { get; set; }
        public decimal IDis3 { get; set; }
        public decimal IDiscount3 { get; set; }
        public int IDI3CashierID { get; set; }
        public int IDI4 { get; set; }
        public decimal IDis4 { get; set; }
        public decimal IDiscount4 { get; set; }
        public int IDI4CashierID { get; set; }
        public int IDI5 { get; set; }
        public decimal IDis5 { get; set; }
        public decimal IDiscount5 { get; set; }
        public int IDI5CashierID { get; set; }
        public decimal Rate { get; set; }
        public bool IsSDis { get; set; }
        public int SDNo { get; set; }
        public int SDID { get; set; }
        public decimal SDIs { get; set; }
        public decimal SDiscount { get; set; }
        public int DDisCashierID { get; set; }
        public decimal Nett { get; set; }
        public int LocationID { get; set; }
        public int DocumentID { get; set; }
        public int BillTypeID { get; set; }
        public int SaleTypeID { get; set; }
        [DefaultValue("")]
        [MaxLength(10)]
        public string Receipt { get; set; }
        public int SalesmanID { get; set; }
        [DefaultValue("")]
        [MaxLength(15)]
        public string Salesman { get; set; }
        public int CustomerID { get; set; }
        [DefaultValue("")]
        [MaxLength(15)]
        public string Customer { get; set; }
        public int CashierID { get; set; }
        [DefaultValue("")]
        [MaxLength(15)]
        public string Cashier { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime RecDate { get; set; }
        public int BaseUnitID { get; set; }
        public int UnitNo { get; set; }
        public int RowNo { get; set; }
        public bool IsRecall { get; set; }
        [DefaultValue("")]
        [MaxLength(10)]
        public string RecallNo { get; set; }
        public bool RecallAdv { get; set; }
        public decimal TaxAmount { get; set; }
        public bool IsTax { get; set; }
        public decimal TaxPercentage { get; set; }
        public bool IsStock { get; set; }
        [DefaultValue("")]
        [MaxLength(50)]
        public string SuspendNo { get; set; }
        public int SuspendBy { get; set; }
        public int CustomerType { get; set; }
        public int TransStatus { get; set; }
        public bool IsPromotionApplied { get; set; }
        public int PromotionID { get; set; }
        public bool IsPromotion { get; set; }
        public int InvPriceLevelID { get; set; }
        [DefaultValue("")]
        [MaxLength(50)]
        public string ItemSerial { get; set; }
        [DefaultValue("")]
        [MaxLength(50)]
        public string warranty { get; set; }
        public int TableNumber { get; set; }
        public int PrinterType { get; set; }
        public bool IsPritRpt { get; set; }
        public int ReferenceProductId { get; set; }
        public int ReferenceProductRow { get; set; }
        public bool IsAddonItem { get; set; }
        public bool IsTaxEnable { get; set; }
        [DefaultValue("")]
        [MaxLength(50)]
        public string TaxCode { get; set; }
        [DefaultValue("")]
        [MaxLength(50)]
        public string SplitItemReceiptNo { get; set; }

       
        [DefaultValue(0)]     
        public decimal Price { get; set; }

        public DateTime ModifiedDate { get; set; }

        //[DefaultValue("")]
        //public string ProductRemark { get; set; }

        //[DefaultValue(0)]
        //public int OrderStatus { get; set; }

    }
}