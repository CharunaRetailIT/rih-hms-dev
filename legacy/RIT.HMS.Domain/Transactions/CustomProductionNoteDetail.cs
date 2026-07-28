using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Transactions
{
    public class CustomProductionNoteDetail
    {
        public long CustomProductionNoteDetailId { get; set; }
        public long CustomProductionNoteHeaderId { get; set; }
        public virtual CustomProductionNoteHeader CustomProductionNoteHeader { get; set; }
        public long ProductId { get; set; }
       // public long ServingUnitId { get; set; }

       // [NotMapped]
      //  public string ServingUnitName { get; set; }
        public string ProductName { get; set; }
        public decimal ProductQty { get; set; }
        public decimal ProductCostPrice { get; set; }
        //public decimal ProductSellingPrice { get; set; }
        public long MaterialId { get; set; }
        public string MaterialName { get; set; }
        public decimal MaterialQty { get; set; }
        //public decimal MaterialSellingPrice { get; set; }
        public decimal MaterialCostPrice { get; set; }
        public decimal MaterialAvgCost { get; set; }

        [NotMapped]
        public string ProductCode { get; set; }
        [NotMapped]
        public string MaterialCode { get; set; }


    }
}
