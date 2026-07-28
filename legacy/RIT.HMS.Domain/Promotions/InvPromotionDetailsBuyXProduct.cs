using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.Promotions
{
    public class InvPromotionDetailsBuyXProduct : BaseEntity
    {
        public long InvPromotionDetailsBuyXProductId { get; set; }

        [DefaultValue(0)]
        public long InvPromotionMasterId { get; set; }
     
        [DefaultValue(0)]
        public long ProductId { get; set; }

        [DefaultValue("")]
        [MaxLength(25)]
        [Display(Name = "Product Code")]
        [NotMapped]
        public string ProductCode { get; set; }

        [Required]
        [MaxLength(100)]
        [NotMapped]
        public string ProductName { get; set; }

        [DefaultValue(0)]
        public int ServingUnitId { get; set; }

        [DefaultValue("")]
        [MaxLength(25)]
        [NotMapped]
        public string UnitOfMeasure { get; set; }

        [DefaultValue(0)]
        public long BuyUnitOfMeasureId { get; set; }

        [DefaultValue(0)]
        public decimal Rate { get; set; }

        [DefaultValue(0)]
        public decimal Qty { get; set; }

        public long Points { get; set; }

        [DefaultValue(0)]
        public decimal DiscountPercentage { get; set; }

        [DefaultValue(0)]
        public decimal DiscountAmount { get; set; }

        [DefaultValue(0)]
        public int ProductType { get; set; }

        [DefaultValue(0)]
        public int GroupId { get; set; }

        public virtual InvPromotionMaster InvPromotionMaster { get; set; }
    }
}