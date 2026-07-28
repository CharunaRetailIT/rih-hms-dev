using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.Transactions
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

        [DefaultValue("")]
        [MaxLength(20)]
        public string NewDocNumber { get; set; }

        [NotMapped]
        [DefaultValue("")]
        [MaxLength(200)]
        public string RejectReason { get; set; }

        [NotMapped]
        [DefaultValue("")]
        [MaxLength(200)]
        public string CancelReason { get; set; }

        [NotMapped]
        public string Company { get; set; }

        [NotMapped]
        public string Location { get; set; }

        [NotMapped]
        public int PrintStatus { get; set; }

        [NotMapped]
        public string DocState { get; set; }

        [NotMapped]
        public virtual SysCompany SysCompany { get; set; }

        [DefaultValue(0)]
        public int DocumentStatus { get; set; }

        [NotMapped]
        public bool IsExists { get; set; }

        
      //  public DateTime CreatedDate { get; set; }

    }
}