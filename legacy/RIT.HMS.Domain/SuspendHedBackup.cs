using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class SuspendHedBackup
    {
        [Key]
        public int Idx { get; set; }
	    public string SuspendNo  { get; set; }
	    public string Receipt  { get; set; }
	    public int LocationID  { get; set; }
	    public int UnitNo  { get; set; }
	    public DateTime STime  { get; set; }
	    public DateTime SDate  { get; set; }
	    public decimal Amount  { get; set; }
	    public int CashierID  { get; set; }
        [DefaultValue(0)]
	    public bool IsRecall  { get; set; }
	    public string RecallReceipt  { get; set; }
	    public int RecallCashierID  { get; set; }
	    public string RecallCashier  { get; set; }
	    public int RecallUnitNo  { get; set; }
	    public int TransStatus  { get; set; }
    }
}