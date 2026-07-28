using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models.ViewModels
{
    public class ReceipeViewModel
    {
        public long ReceipeId { get; set; }
      
        public long ProductId { get; set; }
  
        public long MaterialId { get; set; }
    
        public decimal Quantity { get; set; }
   
        public string ProductName { get; set; }

        public string UOM { get; set; }

        public string ServingUnitName { get; set; }
       
        public decimal ServingUnitCP { get; set; }
       
        public string ServingUnitSP { get; set; }
        
        public string ProductServingUnitId { get; set; }
   
        public decimal CostPrice { get; set; }
      
        public decimal SellingPrice { get; set; }
    }
}