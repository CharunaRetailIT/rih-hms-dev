using RIT.HMS.Domain.Transactions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models.Transactions
{
    public class InvGiftVoucherBookCodeA
    {
        public List<RIT.HMS.Domain.Transactions.InvGiftVoucherBookCode> giftvoucherBook = new List<RIT.HMS.Domain.Transactions.InvGiftVoucherBookCode>();
        /* public long InvGiftVoucherBookCodeID { get; set; }

         public int InvGiftVoucherGroupID { get; set; }


         public string BookCode { get; set; }


         public string BookName { get; set; }


         public string BookPrefix { get; set; }



         public decimal GiftVoucherValue { get; set; }



         public decimal GiftVoucherPercentage { get; set; }

         public int ValidityPeriod { get; set; }

         public int VoucherType { get; set; }


         public int StartingNo { get; set; }


         public int CurrentSerialNo { get; set; }


         public int SerialLength { get; set; }


         public int PageCount { get; set; }


         public bool IsDelete { get; set; }


         public int BasedOn { get; set; }*/
        public string GiftVoucherGroupCode { get; set; }
        public int LocationId { get; set; }
        public string SerialFormat { get; set; }
    }
}