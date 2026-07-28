using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class InvPriceLevelList : BaseEntity
    {
        [Key]
        public long InvPriceLevelListID { get; set; }


        [DefaultValue(0)]
        public int PriceLevelID { get; set; }


        [DefaultValue(0)]
        public int ServingUnitID { get; set; }


        [DefaultValue(0)]
        public int ProductID { get; set; }


        [DefaultValue(0)]
        public decimal CostPrice { get; set; }


        [DefaultValue(0)]
        public decimal SellingPrice { get; set; }


        [DefaultValue(0)]
        public decimal Qty { get; set; }


        [DefaultValue(0)]
        public bool IsDelete { get; set; }
        //public string Remark { get; set; } 
    }
}
