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
    public class InvBillValueDiscount:BaseEntity
    {
        public int InvBillValueDiscountId { get; set; }

        [DefaultValue(0)]
        public int PromotionMasterId { get; set; }
        public bool TotalBillValueDiscount { get; set; }
        public bool BillValueRangeDiscount { get; set; }

        [DefaultValue(0)]
        public decimal BillValueRangeFrom { get; set; }

        [DefaultValue(0)]
        public decimal BillValueRangeTo { get; set; }

        [DefaultValue("")]
        [Column(TypeName = "VARCHAR")]
        [StringLength(3)]
        public string DiscountType { get; set; }
        [DefaultValue(0)]
        public decimal DiscountAmount { get; set; }

        [NotMapped]
        public string PromotionName { get; set; }

        [NotMapped]
        public bool IsExists { get; set; }
    }
}
