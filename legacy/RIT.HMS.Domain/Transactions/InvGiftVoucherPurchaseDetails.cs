using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Transactions
{
    public class InvGiftVoucherPurchaseDetails
    {
        [Key]
        public long InvGiftVoucherPurchaseDetailID { get; set; }
        public long GiftVoucherPurchaseDetailID { get; set; }
        public long InvGiftVoucherPurchaseHeaderID { get; set; }
        public int CompanyID { get; set; }
        public int LocationID { get; set; }
        public int DocumentID { get; set; }
        public DateTime DocumentDate { get; set; }
        public long LineNo { get; set; }
        public long InvGiftVoucherMasterID { get; set; }
        public decimal NumberOfCount { get; set; }
        public decimal VoucherAmount { get; set; }
        public int VoucherType { get; set; }
        public bool IsTransfer { get; set; }
        public int DocumentStatus { get; set; }
        public int GroupOfCompanyID { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int DataTransfer { get; set; }
    }
}
