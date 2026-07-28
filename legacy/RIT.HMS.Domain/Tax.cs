using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class Tax : BaseEntity
    {
        public int TaxId { get; set; }

        [Required]
        [MaxLength(10)]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        public string TaxCode { get; set; }

        [Required]
        [MaxLength(50)]
        public string TaxName { get; set; }

        [DefaultValue(0)]
        public decimal TaxPercentage { get; set; }

     

        [DefaultValue(0)]
        public bool IsActive { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }

        [DefaultValue(0)]
        public bool IsTaxOnTax { get; set; }

        [DefaultValue(0)]
        public bool IsPurchasingTax { get; set; }
        [DefaultValue(0)]
        public bool IsSellingTax { get; set; }

        [DefaultValue(0)]
        public bool IsServiceCharge { get; set; }


        [DefaultValue(0)]
        public bool isExcludeTax { get; set; }

        //   isExcludeTax
    }
}