using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.ViewModels
{

    public class VMInvRequestNotePOTransactionsHead
    {
        public string Company { get; set; }

        public string Location { get; set; }

        [NotMapped]
        public virtual SysCompany SysCompany { get; set; }

        [NotMapped]
        public virtual Supplier Supplier { get; set; }

        public int PrintStatus { get; set; }

        public string DocState { get; set; }
        public string PaymentTerm { get; set; }

        public string POLocationAddress { get; set; }
        public string DocumentNo { get; set; }
        public DateTime PODate { get; set; }
        public DateTime ExpectedDate { get; set; }
        public string CreatedUser { get; set; }
        public virtual ICollection<VMInvRequestNotePOTransactions> POTransactionsDetails { get; set; }

        public virtual ICollection<VMInvRequestNotePOTransactionsKitchen> POTransactionsDetailsKitchen { get; set; }

        public string Remark { get; set; }

        public string RequestNoteNos { get; set; }

    }
 public   class VMInvRequestNotePOTransactions
    {
		public DateTime RequestNoteDate { get; set; }

		public string RequestNoteNo { get; set; }

		public string ReqLocationCode { get; set; }
		public string ReqLocationName { get; set; }

		public string ProductCode { get; set; }

		public string ProductDesp { get; set; }

		public decimal RequestedQty { get; set; }

		public decimal IssueQtY { get; set; }
		public decimal BalanceQtY { get; set; }

       public string Remark { get; set; }




    }


    public class VMInvRequestNotePOTransactionsKitchen
    {
        public string ProductCode { get; set; }

        public string ProductDesp { get; set; }

        public decimal RequestedQty { get; set; }

    }
  
}
