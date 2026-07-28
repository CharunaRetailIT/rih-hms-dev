using RIT.HMS.Domain.Transactions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.Transactions
{
    public class InvRequestNotePOTransaction
    {
        [Key]
        public long POReqNoteNo { get; set; }

        [DefaultValue(0)]
        public long RequestNoteHeaderID { get; set; }

        [DefaultValue(0)]
        public long PurchaseOrderDetailID { get; set; }

        [MaxLength(200)]
        public string PurchaseOrderDocumentNo { get; set; }

        [MaxLength(200)]
        public string RequestNoteDocumentNo { get; set; }

        [DefaultValue(0)]
        public long LocationID { get; set; }

        [DefaultValue(0)]
        public long ProductID { get; set; }

        [DefaultValue(0)]
        public decimal QTY { get; set; }
        public DateTime ReqNoteCreatedDate { get; set; }
        public DateTime ReqNoteAcceptedDate { get; set; }
        public DateTime POCreateDate { get; set; }
        public decimal IssueQtY { get; set; }
        public decimal BalanceQtY { get; set; }


        [DefaultValue(0)]
        public long PurchaseOrderHeaderID { get; set; }

        [DefaultValue(0)]
        public long FromLocationID { get; set; }
    }
}
