using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class Receipe : BaseEntity
    {

        public Receipe()
        {
            Receipes = new List<ViewModels.ReceipeViewModel>();
            ServingUnits = new List<ProductServingUnit>();
            ReceipeReport = new List<Receipe>();
        }

        public long ReceipeId  { get; set; }

        [Required]
        [DefaultValue(0)]
        public long ProductId { get; set; }

        [Required]
        [DefaultValue(0)]
        public long MaterialId { get; set; } 

        [Required]
        [DefaultValue(0)]

        //[Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; }

        [NotMapped]
        public string ProductName { get; set; }

        [NotMapped]
        public string UOM { get; set; }

        [NotMapped]
        public string ServingUnitName { get; set; }

        [NotMapped]
        public decimal ServingUnitCP { get; set; }

        [NotMapped]
        public string ServingUnitSP { get; set; }

      //[NotMapped]
        [Required]
        [DefaultValue(0)]
        public long ProductServingUnitId { get; set; }

      

        //[NotMapped]
        //public long ProductServingUnitId { get; set; }


        [NotMapped]
        public List<ViewModels.ReceipeViewModel> Receipes { get; set; }

        [NotMapped]
        public List<Receipe> ReceipeReport { get; set; }

        [NotMapped]
        public List<ProductServingUnit> ServingUnits { get; set; }

        [NotMapped]
        public decimal TotCostPrice { get; set; }
         
        [NotMapped]
        public decimal TotSellingPrice { get; set; }
      
        public decimal CostPrice { get; set; }
     
        public decimal SellingPrice { get; set; }

        [NotMapped]
        public string ProductCode { get; set; }

        [NotMapped]
        public string MatCode { get; set; }

        [NotMapped]
        public string MatName { get; set; }

        [Required]
        [DefaultValue(0)]
       // [Column(TypeName = "decimal(18,4)")]
        public decimal ProductQty { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }

        [NotMapped]
        public bool ApplyForAllLocation { get; set; }


        [NotMapped]
        [DefaultValue(0)]
        public decimal ActualTotalCost { get; set; }

     
    }
}