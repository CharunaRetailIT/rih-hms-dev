using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.ViewModels
{
    public class PORepoViewModel
    {
   
        public decimal SellingPrice { get; set; }
        public decimal CostPrice { get; set; }
        public decimal CostValue { get; set; }
        public decimal Discounts { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal RecQty { get; set; }
        public long ItemId { get; set; }
        public string ItemDesc { get; set; }
        public string UOM { get; set; }
        public decimal OrderQuantity { get; set; }
        public decimal TOGQuantity { get; set; }
        public bool IsExpiry { get; set; }

        public decimal DiscountType { get; set; }

        public decimal FixedDiscountAmount { get; set; }

        public decimal FixedDiscountPercentage { get; set; }

        public decimal POQuantity { get; set; }
    }
}