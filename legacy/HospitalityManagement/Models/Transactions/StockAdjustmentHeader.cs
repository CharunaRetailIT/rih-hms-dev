using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models.Transactions
{
    public class StockAdjustmentHeader : BaseEntity
    {
        public StockAdjustmentHeader()
        {
            StockAdjDetail = new List<StockAdjustmentDetail>();
        }

        public long StockAdjustmentHeaderId { get; set; }
        public string DocumentNo { get; set; }
        public long StockLocationId { get; set; }
        public string Remark { get; set; }

        [NotMapped]

        public List<StockAdjustmentDetail>  StockAdjDetail { get; set; }

        [NotMapped]
        public string BaseType { get; set; }

   
        public int DocumentId { get; set; }
    }
}