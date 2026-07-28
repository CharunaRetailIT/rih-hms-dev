using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class BudgetItemWise
    {
        public int BudgetItemWiseID { get; set; }
        public int BudgetOutletID { get; set; }        
        public int AssetType { get; set; }
        public bool isActive { get; set; }
        public decimal totalbudget { get; set; }        
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}