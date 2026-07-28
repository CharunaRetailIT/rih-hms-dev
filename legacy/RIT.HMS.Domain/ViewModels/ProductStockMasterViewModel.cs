using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.ViewModels
{
    public class ProductStockMasterViewModel
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public string UOM { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal CostPrice { get; set; }
        public string ProductCode { get; set; }
        public long UOMId { get; set; }
        public int ServingUnitId { get; set; }
        public string ServingUnit { get; set; }
        public decimal Quantity { get; set; }
        public int PromotionItemType { get; set; }

        [DefaultValue(0)]
        public decimal DiscountPrc { get; set; }

        [DefaultValue(0)]
        public decimal DiscountAmt { get; set; }

        [DefaultValue("")]
        public string DiscountType { get; set; }

        [DefaultValue(0)]
        public int PromotionMasterId { get; set; }

        [DefaultValue(0)]
        public int PromotionTypeId { get; set; }

        [DefaultValue(0)]
        public string PromotionName { get; set; }

        [DefaultValue(0)]
        public int ProductType { get; set; }


        /// from chamodi's 

        [DefaultValue(0)]
        public long InvPromotionMasterId { get; set; }

      
        [DefaultValue(0)]
        public decimal ValueFrom { get; set; }

        [DefaultValue(0)]
        public decimal ValueTo { get; set; }

        public int LocationId { get; set; }

        public bool IsRowMaterial { get; set; }

        public int GroupId { get; set; }

        public string LocationName { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime StartDate { get; set; }

    }
}