using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models.DeliveryModule
{
    public class DeliveryModuleProductMap
    {
        [Key]
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
       
        public string ClientProductCode { get; set; }
        public virtual Product Product { get; set; }
    }
}