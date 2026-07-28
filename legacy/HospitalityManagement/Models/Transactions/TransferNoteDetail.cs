using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models.Transactions
{
    public class TransferNoteDetail
    {
        
            public long TransferNoteDetailId { get; set; }
        
            [Required]
            [DefaultValue(0)]
            public long TransferNoteHeaderId { get; set; }
        
            [DefaultValue(0)]
            public long LineNo { get; set; }

            [DefaultValue(0)]
            public long ProductId { get; set; }

            [DefaultValue(0)]
            public bool IsBatch { get; set; }

            [DefaultValue("")]
            [MaxLength(50)]
            public string BatchNo { get; set; }

            [DefaultValue("")]
            [MaxLength(25)]
            public string StockCode { get; set; }

            public DateTime BatchExpiryDate { get; set; }

            [DefaultValue(0)]
            public decimal AvgCost { get; set; }

            [DefaultValue(0)]
            public decimal CostPrice { get; set; }

            [DefaultValue(0)]
            public decimal SellingPrice { get; set; }

            [DefaultValue(0)]
            public int PackId { get; set; }

            [DefaultValue(0)]
            public decimal OrderQty { get; set; }

            [DefaultValue(0)]
            public long UnitOfMeasureId { get; set; }

            [DefaultValue(0)]
            public string SerialNo { get; set; }

            [NotMapped]
            public string ProductName { get; set; }

            [NotMapped]
            public string UOM { get; set; }

            [NotMapped]
            public string ProductCode { get; set; }
         
            [NotMapped]
            public decimal TOGQty { get; set; }



    }
}