using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Transactions
{
    public class InvGiftVoucherTransferNoteHeader
    {
        [Key]
        public int InvGiftVoucherTransferNoteHeaderID { get; set; }
        public int GiftVoucherTransferNoteHeaderID { get; set; }
        public int CompanyID { get; set; }
        public int LocationID { get; set; }
        public int CostCentreID { get; set; }
        public int DocumentID { get; set; }
        public string DocumentNo { get; set; }
        public DateTime? DocumentDate { get; set; }
        public int TransferTypeID { get; set; }
        public decimal GiftVoucherAmount { get; set; }
        public decimal GiftVoucherPercentage { get; set; }
        public int GiftVoucherQty { get; set; }
        public string Remark { get; set; }
        public string ReferenceNo { get; set; }
        public int ReferenceDocumentDocumentID { get; set; }
        public int ReferenceDocumentID { get; set; }
        public string ReferenceDocumentNo { get; set; }
        public int ToLocationID { get; set; }
        public int VoucherType { get; set; }
        public int DocumentStatus { get; set; }
        public int GroupOfCompanyID { get; set; }
        public string CreatedUser { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int DataTransfer { get; set; }
    }
}
