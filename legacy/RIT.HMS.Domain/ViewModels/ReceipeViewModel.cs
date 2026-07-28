using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.ViewModels
{
    public class ReceipeViewModel
    {
        public long ReceipeId { get; set; }   
        public long ProductId { get; set; }
        public long MaterialId { get; set; }  
        public decimal Quantity { get; set; }
        public string ProductCode { get; set; }
        public string MaterialCode { get; set; }
        public string ProductName { get; set; }
        public string UOM { get; set; }
        public string ServingUnitName { get; set; }     
        public decimal ServingUnitCP { get; set; }     
        public string ServingUnitSP { get; set; }       
        public string ProductServingUnitId { get; set; }
        public decimal CostPrice { get; set; }    
        public decimal SellingPrice { get; set; }
        public int LocationId { get; set; }
        public string LocationCode { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreateDate { get; set; }
        public int CompanyId { get; set; }
        public string UnitConvertion { get; set; }
        public decimal RecipeQuantity { get; set; }
        public bool IsActive { get; set; }
    }
}