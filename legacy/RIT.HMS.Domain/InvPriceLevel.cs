using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class InvPriceLevel : BaseEntity 
    {

        [Key]
        public long InvPriceLevelID { get; set; }
        [MaxLength(15)]
        public string PriceLevelCode { get; set; }
        [MaxLength(100)]
        public string PriceLevelName { get; set; }

        [DefaultValue(0)]
        public int ServingUnitID { get; set; }

        [MaxLength(15)]
        public string ServingUnit { get; set; }

        [DefaultValue(0)]
        public decimal CostPrice { get; set; }

        [DefaultValue(0)]
        public decimal SellingPrice { get; set; }

        [DefaultValue(0)]
        public decimal Qty { get; set; }
        [MaxLength(150)]
        public string Remark { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }
    }
}
