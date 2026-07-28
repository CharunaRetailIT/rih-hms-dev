using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.Transactions
{
    public class ProductionNoteDetail
    {
        public long ProductionNoteDetailId { get; set; }
        public long ProductionNoteHeaderId { get; set; }
        public long ProductId { get; set; }
        public long ServingUnitId { get; set; }

        [NotMapped]
        public string ServingUnitName { get; set; }
        public string ProductName { get; set; }
        public decimal ProductQty { get; set; }
        public decimal ProductCostPrice { get; set; }
        public decimal ProductSellingPrice { get; set; }
        public long MaterialId { get; set; }
        public string MaterialName { get; set; }
        public decimal MaterialQty { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal CostPrice { get; set; }

        public decimal AvgCost { get; set; }
        [NotMapped]
        public string UOM { get; set; }
        [NotMapped]
        public string QtyUOM { get; set; }

        [DefaultValue(0)]
        public decimal ActualQty { get; set; }

    }
}