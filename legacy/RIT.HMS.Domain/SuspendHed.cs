using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class SuspendHed
    {
        [Key]
        public int  Idx  { get; set; }
        [DefaultValue("")]
        [MaxLength(50)]
	    public string  SuspendNo  { get; set; }

        [DefaultValue("")]
        [MaxLength(50)]
	    public string  Receipt  { get; set; }
	    public int  LocationID  { get; set; }
	    public int  UnitNo  { get; set; }
	    public DateTime  STime  { get; set; }
	    public DateTime  SDate  { get; set; }
	    public decimal  Amount  { get; set; }
	    public int  CashierID  { get; set; }
	    public bool  IsRecall  { get; set; }
        [DefaultValue("")]
        [MaxLength(50)]
	    public string  RecallReceipt  { get; set; }
	    public int  RecallCashierID  { get; set; }
        [DefaultValue("")]
        [MaxLength(20)]
	    public string  RecallCashier  { get; set; }
	    public int  RecallUnitNo  { get; set; }
	    public DateTime  RecallTime  { get; set; }
	    public int  TransStatus  { get; set; }
        [DefaultValue("")]
        [MaxLength(50)]
	    public string  TokenNumber  { get; set; }
	    public int  NextBillDate  { get; set; }
	    public int  CustomerId  { get; set; }
	    public int  TableNumber  { get; set; }
        public DateTime  ModifiedDate { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(50)]
       
        public string OrigSuspendNo { get; set; }
        //[DefaultValue(0)]
        //public int OrderStatus { get; set; }


    }
}