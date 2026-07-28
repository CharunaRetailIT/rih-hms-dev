using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace RIT.HMS.Domain.Promotions
{
    public class InvComboPackBundleItemPrice : BaseEntity
    {
        [Key]
        public int InvBundleItemPriceId { get; set; } 
        public int PromotionMasterId { get; set; }
        public int InvId { get; set; }
        [DefaultValue(false)]
        public bool IsAllowDiscountPresentage { get; set; }
        [DefaultValue(false)]
        public bool IsAllowDiscountAmount { get; set; }
        [DefaultValue(0)]
        public int ProductId { get; set; }

        [NotMapped]
        public string ProductCode { get; set; }
        [NotMapped]
        public string ProductName { get; set; }

        [DefaultValue(0)]
        public int ServingUnitId { get; set; }

        [NotMapped]
        public string ServingUnit { get; set; }

        [DefaultValue(0)]
        public decimal Quantity { get; set; }
        [DefaultValue(0)]
        public decimal DiscountValue { get; set; }
        [DefaultValue(0)]
        public int GroupId { get; set; }


        [DefaultValue("")]
        [Column(TypeName = "VARCHAR")]
        [StringLength(50)]
        public string BundleName { get; set; }

        [NotMapped]
        public decimal BundleSellingPrice { get; set; }

        [NotMapped]
        public bool IsExists { get; set; }
    }
}
