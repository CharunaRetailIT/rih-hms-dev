using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.Transactions
{
    public class RequestNoteDetail
    {
        public long RequestNoteDetailId { get; set; }
        [Required]
        [DefaultValue(0)]
        public long RequestnoteHeaderId { get; set; }
        [DefaultValue(0)]
        public long LineNo { get; set; }
        [DefaultValue(0)]
        public long ProductId { get; set; }
        
        [DefaultValue(0)]
        public decimal AvgCost { get; set; }
        [DefaultValue(0)]
        public decimal CostPrice { get; set; }
        [DefaultValue(0)]
        public decimal SellingPrice { get; set; }
        [DefaultValue(0)]
        public decimal RequestQty { get; set; }
        [DefaultValue(0)]
        public long UnitOfMeasureId { get; set; }
        [NotMapped]
        public string ProductName { get; set; }
        [NotMapped]
        public string UOM { get; set; }

        [DefaultValue("")]
        public string RequestedBy { get; set; }

        [DefaultValue(0)]
        public int ServingUnitId { get; set; }
        [DefaultValue("")]
        public string ServingUnit { get; set; }

        [NotMapped]

        public string StockCode { get; set; }

        [NotMapped]

        public decimal GrossAmount { get; set; }

        [NotMapped]
        public int LocationId { get; set; }

        [NotMapped]

        public int ToLocationID { get; set; }

        [DefaultValue(0)]

        public int SupplierId { get; set; }

        [DefaultValue(0)]
        public bool IsPoTransfer { get; set; }

        [NotMapped]
        [DefaultValue(0)]

        public decimal SIH { get; set; }

        [NotMapped]
        [DefaultValue(0)]

        public decimal CostValue { get; set; }


        [DefaultValue("")]
        [MaxLength(300)]
        public string Remark { get; set; }


        [NotMapped]

        public string ProductCode { get; set; }


    }
}