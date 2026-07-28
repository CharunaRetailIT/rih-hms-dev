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
    public class Mearge_PO_Header
    {
    
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Location !")]
        public long LocationId { get; set; }

        public string DocumentNo { get; set; }

        public bool IsActive { get; set; }

        public long SupplierProductId { get; set; }
        public int SupplierId { get; set; }
        public int ProductId { get; set; }

        [DefaultValue(0)]
        public bool IsPreferredSupplier { get; set; }
        [NotMapped]
        public string Supplier { get; set; }

        [DefaultValue(0)]
        public decimal LastCostPrice { get; set; }

        [NotMapped]
        public string SupplierCode { get; set; }
        [NotMapped]
        public string ProductCode { get; set; }

        [NotMapped]
        [DefaultValue(0)]
        public decimal GrossAmount { get; set; }

        [NotMapped]

        public string StockCode { get; set; }




        [DefaultValue(0)]
        public decimal CostPrice { get; set; }

        [DefaultValue(0)]
        public decimal SellingPrice { get; set; }

        [DefaultValue(0)]

        public decimal RequestQty { get; set; }
        [DefaultValue(0)]
        public long RequestNoteAccptanceHeaderId { get; set; }


        [DefaultValue(0)]
        public long RequestnoteHeaderId { get; set; }

    }
}
