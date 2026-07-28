using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain
{
    public class InvAdvanceNoteDet
    {
        public int InvAdvanceNoteDetID { get; set; }
        public long? Idx { get; set; }
        public long ProductID { get; set; }
        [StringLength(25)]
        public string ProductCode { get; set; }
        [StringLength(25)]
        public string RefCode { get; set; }
     
        public long BarCodeFull { get; set; }
        [StringLength(50)]
        public string Descrip { get; set; }
        [StringLength(50)]
        public string BatchNo { get; set; }
        [StringLength(50)]
        public string SerialNo { get; set; }
        public DateTime? ExpiryDate { get; set; }
        [DefaultValue(0)]
        public decimal Cost { get; set; }
        [DefaultValue(0)]
        public decimal AvgCost { get; set; }
        [DefaultValue(0)]
        public decimal Price { get; set; }
        [DefaultValue(0)]
        public decimal Qty { get; set; }
        [DefaultValue(0)]     
        public decimal Amount { get; set; }
        [DefaultValue(0)]
        public long UnitOfMeasureID { get; set; }
        [StringLength(10)]
        public string UnitOfMeasureName { get; set; }
        [DefaultValue(0)]
        public decimal ConvertFactor { get; set; }
//-------------------------------------------------------------------------
        public int IDI1 { get; set; }
        [DefaultValue(0)]
        public decimal IDis1 { get; set; }
        [DefaultValue(0)]
        public decimal IDiscount1 { get; set; }
        [DefaultValue(0)]
        public long IDI1CashierID { get; set; }
        [DefaultValue(0)]
        public int IDI2 { get; set; }

        [DefaultValue(0)]
        public decimal IDis2 { get; set; }

        [DefaultValue(0)]
        public decimal IDiscount2 { get; set; }

        [DefaultValue(0)]
        public long IDI2CashierID { get; set; }

        [DefaultValue(0)]
        public int IDI3 { get; set; }

        [DefaultValue(0)]
        public decimal IDis3 { get; set; }

        [DefaultValue(0)]
        public decimal IDiscount3 { get; set; }

        [DefaultValue(0)]
        public long IDI3CashierID { get; set; }

        [DefaultValue(0)]
        public decimal IDI4 { get; set; }

        [DefaultValue(0)]
        public decimal IDis4 { get; set; }

        [DefaultValue(0)]
        public decimal IDiscount4 { get; set; }

        [DefaultValue(0)]
        public long IDI4CashierID { get; set; }

        [DefaultValue(0)]
        public int IDI5 { get; set; }

        [DefaultValue(0)]
        public decimal IDis5 { get; set; }
        [DefaultValue(0)]
        public decimal IDiscount5 { get; set; }
        [DefaultValue(0)]
        public long IDI5CashierID { get; set; }
        [DefaultValue(0)]
        public decimal Rate { get; set; }

        //-----------------------------------------------------------------
        [DefaultValue(0)]
        public bool IsSDis { get; set; }
        [DefaultValue(0)]
        public int SDNo { get; set; }
        [DefaultValue(0)]
        public int SDID { get; set; }
        [DefaultValue(0)]
        public decimal SDIs { get; set; }
        [DefaultValue(0)]
        public decimal SDiscount { get; set; }
        [DefaultValue(0)]
        public long DDisCashierID { get; set; }
        [DefaultValue(0)]
        public decimal Nett { get; set; }
        [DefaultValue(0)]
        public int LocationID { get; set; }
        [DefaultValue(0)]
        public int DocumentID { get; set; }
        [DefaultValue(0)]
        public int BillTypeID { get; set; }
        [DefaultValue(0)]
        public int SaleTypeID { get; set; }
       
        [StringLength(10)]
        public string Receipt { get; set; }
        [DefaultValue(0)]
        public long SalesmanID { get; set; }
        [StringLength(15)]
        public string Salesman { get; set; }
        [DefaultValue(0)]
        public long CustomerID { get; set; }
        [StringLength(15)]
        public string Customer { get; set; }
        [DefaultValue(0)]
        public Int64 CashierID { get; set; }
        [StringLength(15)]
        public string Cashier { get; set; }

        //-------------------------------------------------------

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime RecDate { get; set; }
        [DefaultValue(0)]
        public long BaseUnitID { get; set; }
        [DefaultValue(0)]
        public int UnitNo { get; set; }
        [DefaultValue(0)]
        public int RowNo { get; set; }
        [DefaultValue(0)]
        public bool IsRecall { get; set; }
        [StringLength(10)]
        public string RecallNO { get; set; }
        [DefaultValue(0)]
        public bool RecallAdv { get; set; }
        [DefaultValue(0)]
        public decimal TaxAmount { get; set; }
        [DefaultValue(0)]
        public bool IsTax { get; set; }
        [DefaultValue(0)]
        public decimal TaxPercentage { get; set; }
        [DefaultValue(0)]
        public bool IsStock { get; set; }

        //--------------------------------------------------------------------------
        [StringLength(150)]
        public string CreditNoteNo { get; set; }

        [DefaultValue(0)]
        public long CreditNoteBy { get; set; }
        [DefaultValue(0)]
        public int CustomerType { get; set; }

        [DefaultValue(0)]
        public int TransStatus { get; set; }
        [DefaultValue(0)]
        public bool IsPromotionApplied { get; set; }

        [DefaultValue(0)]
        public int PromotionID { get; set; }

        [DefaultValue(0)]
        public bool IsPromotion { get; set; }
        [StringLength(50)]
        public string ItemSerial { get; set; }
        [StringLength(50)]
        public string warranty { get; set; }

        [Column(TypeName = "varchar")]
        [StringLength(50)]
        public string RecallFromInvoiceNo { get; set; }
        public bool? WorkComplete { get; set; }
        [StringLength(30)]
        public string WorkCompUser { get; set; }
        public DateTime? WorkCompDateTime { get; set; }
        public bool? CustCollected { get; set; }
        public DateTime? CustColDateTime { get; set; }
        public bool IsNewPrice { get; set; }
        public bool IsApproved { get; set; }
        public long ApprovedBy { get; set; }
        [Column(TypeName = "nchar")]
        [StringLength(10)]
        public string ApprovedFor { get; set; }
        [DefaultValue(0)]
        public int ReferenceProductId { get; set; }
        [DefaultValue(0)]
        public int ReferenceProductRow { get; set; }
        public int? PrinterType { get; set; }
        public bool? IsAddonItem { get; set; }
        public int? TableNumber { get; set; }
        public bool? IsTaxEnable { get; set; }
        [Column(TypeName = "varchar")]
        [StringLength(50)]   
        public string TaxCode { get; set; }
        [Column(TypeName = "varchar")]
        [StringLength(50)]
        public string SplitItemReceiptNo { get; set; }
        public bool? IsPritRpt { get; set; }
        [Column(TypeName = "varchar")]
        [StringLength(200)]
        public string ProductRemark { get; set; }
        public int? OrderStatus { get; set; }
        [Column(TypeName = "varchar")]
        [StringLength(50)]
        public string ServingUnit { get; set; }
        public int? NoOfCustomers { get; set; }
        public bool? IsShowOnBill { get; set; }
        [Column(TypeName = "varchar")]
        [StringLength(50)]
        public string DeploCardNo { get; set; }
        public int? ServingUnitId { get; set; }

        //--------------------------------------------------
        [DefaultValue(false)]
        public bool IsProduction { get; set; }

        [NotMapped]
        public string ProductName { get; set; }


    }
}
