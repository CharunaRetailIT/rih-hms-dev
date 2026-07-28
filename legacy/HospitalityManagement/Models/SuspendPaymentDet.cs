using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class SuspendPaymentDet
    {
        [Key]
        public int Idx  { get; set; }
	    public int RowNo  { get; set; }
	    public int PayTypeID  { get; set; }
	    public decimal Amount  { get; set; }
	    public decimal Balance  { get; set; }
	    public DateTime SDate  { get; set; }
	    public string Receipt  { get; set; }
	    public int LocationID  { get; set; }
	    public int CashierID  { get; set; }
	    public int UnitNo  { get; set; }
	    public int BillTypeID  { get; set; }
	    public string RefNo  { get; set; }
	    public int BankId  { get; set; }
	    public DateTime ChequeDate  { get; set; }
        [DefaultValue(0)]
	    public bool IsRecallAdv  { get; set; }
        [DefaultValue("")]
	    public string RecallNo  { get; set; }
        [DefaultValue("")]
	    public string Descrip { get; set; }
        [DefaultValue("")]
	    public string EnCodeName { get; set; }
	    public string SuspendNo { get; set; }
	    public int SuspendBy { get; set; }
        [DefaultValue(0)]
	    public bool IsDeleteOnRecall { get; set; }
    }
}