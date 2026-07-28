using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.ViewModels
{
    public class CustomerPricesViewModel
    {
        public int CustomerDiscountId { get; set; }
        public int CustomerId { get; set; }      
        public string CustomerCode { get; set; }     
        public int ProductId { get; set; }
        public int ServingUnitId { get; set; }
        public string ProductCode { get; set; }    
        public decimal DiscountAmount { get; set; }     
        public decimal DiscountPercentage { get; set; }    
        public decimal CustomerSellPrice { get; set; }    
        public decimal CreditDiscountAmount { get; set; }      
        public decimal CreditDiscountPercentage { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }      
        public bool IsActive { get; set; }       
        public string CustomerName { get; set; }   
        public string ProductName { get; set; }      
        public string ServingUnit { get; set; }
    }
}
