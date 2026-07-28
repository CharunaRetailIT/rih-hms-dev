using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain
{
    public class CustomerDiscount : BaseEntity
    {
        public int CustomerDiscountId { get; set; }
        public int CustomerId { get; set; }

        [MaxLength(20)]
        public string CustomerCode { get; set; }

        [DefaultValue(0)]
        public int ProductId { get; set; }

        [DefaultValue(0)]
        public int ServingUnitId { get; set; }

        [MaxLength(20)]
        public string ProductCode { get; set; }

        [DefaultValue(0)]
        public decimal DiscountAmount { get; set; }

        [DefaultValue(0)]
        public decimal DiscountPercentage { get; set; }

        [DefaultValue(0)]
        public decimal CustomerSellPrice { get; set; }

        [DefaultValue(0)]
        public decimal CreditDiscountAmount { get; set; }

        [DefaultValue(0)]
        public decimal CreditDiscountPercentage { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }

     

    }
}
