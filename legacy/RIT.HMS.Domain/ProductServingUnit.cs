using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class ProductServingUnit:BaseEntity
    {
        public long ProductServingUnitId { get; set; }
        [Required]
        [DefaultValue(0)]
        public long ProductId { get; set; }

        [Required]
        [DefaultValue(0)]
        public string ServingUnit { get; set; }
        [Required]
        [DefaultValue(0)]
        public decimal CostPrice { get; set; }
        [Required]
        [DefaultValue(0)]
        public decimal SellingPrice { get; set; }

       // [NotMapped]
        public bool DeductStockOnRecipe { get; set; }

    }
}