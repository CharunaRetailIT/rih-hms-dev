using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class SupplierProduct : BaseEntity
    {
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

        [NotMapped]
        public int ToLocationID { get; set; }
       


        [DefaultValue(0)]
        public decimal CostPrice { get; set; }
       
        [DefaultValue(0)]
        public decimal SellingPrice { get; set; }

        public bool IsDelete { get; set; }
    }
}