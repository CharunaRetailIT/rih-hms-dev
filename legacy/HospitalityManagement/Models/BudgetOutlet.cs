using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public enum BudegtTypeEnum
    {
        Daily = 1,
        Weekly = 2,
        Monthly = 3,
        Quarterly = 4,
        Yearly = 5
    }
    public class BudgetOutlet 
    {
        public int BudgetOutletID { get; set; }
        public int locationID { get; set; }
        public int BudgetType { get; set; }
        public bool isActive { get; set; }
        public DateTime StartingDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal totalbudget { get; set; }        
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int NoofDMWY { get; set; }
        
    }
}