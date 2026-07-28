using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.Transactions
{
    public class RequestNoteAcceptanceDetail
    {
        public long RequestNoteAcceptanceDetailId { get; set; }
        [Required]
        [DefaultValue(0)]
        public long RequestNoteAccptanceHeaderId { get; set; }
        [DefaultValue(0)]
        public long LineNo { get; set; }
        [DefaultValue(0)]
        public long ProductId { get; set; }
        [DefaultValue(0)]
        public long MaterialId { get; set; }
        [DefaultValue(0)]
        public decimal CostPrice { get; set; }
        [DefaultValue(0)]
        public decimal SellingPrice { get; set; }
        [DefaultValue(0)]
        public decimal MaterialQty { get; set; }
        [DefaultValue(0)]
        public decimal IssueQty { get; set; }
        [DefaultValue(0)]
        public long UnitOfMeasureId { get; set; }
        [NotMapped]
        public string ProductName { get; set; }
        [NotMapped]
        public string UOM { get; set; }
        [DefaultValue(false)]
        public bool IsTOG { get; set; }
        [DefaultValue("")]
        public string RequestedBy { get; set; }

        [DefaultValue("")]
        public int ServingUnitId { get; set; }
        [DefaultValue("")]
        public string ServingUnit { get; set; }


        [DefaultValue(0)]
        public long PurchaseOrderHeaderId { get; set; }



        [DefaultValue(0)]
        public long RequestnoteHeaderId { get; set; }
        


            

    }
}