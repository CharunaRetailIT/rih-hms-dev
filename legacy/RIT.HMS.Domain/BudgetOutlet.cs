using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class BudgetOutlet
    {

        [Key]
        [Required]
        [DefaultValue(0)]
        public int BudgetOutletID { get; set; }
        [Required]
        [DefaultValue(0)]
        public int locationID { get; set; }
        [Required]
        [DefaultValue(0)]
        public int BudgetType { get; set; }

        [Required]
        [DefaultValue(0)]
        public bool isActive { get; set; }
        [Required]
        [DefaultValue("")]
        public DateTime StartingDate { get; set; }
        [Required]
        [DefaultValue("")]
        public DateTime EndDate { get; set; }
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
        [Required]
        [DefaultValue(0)]
        public int NoofDMWY { get; set; }
    }
}