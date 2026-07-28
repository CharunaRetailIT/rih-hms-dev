using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class BudgetItemWise
    {
        [Key]
        [Required]
        [DefaultValue(0)]
        public int BudgetItemWiseID { get; set; }
        [Required]
        [DefaultValue(0)]
        public int BudgetOutletID { get; set; }
        [Required]
        [DefaultValue(0)]
        public int AssetType { get; set; }
        [Required]
        [DefaultValue(0)]
        public bool isActive { get; set; }
        [Required]
        [DefaultValue(0)]
        public decimal totalbudget { get; set; }
        [Required]
        [DefaultValue("")]
        public string CreatedUser { get; set; }
        [Required]
        [DefaultValue("")]
        public DateTime CreatedDate { get; set; }
        [Required]
        [DefaultValue("")]
        public string ModifiedUser { get; set; }
        [Required]
        [DefaultValue("")]
        public DateTime ModifiedDate { get; set; }
    }
}