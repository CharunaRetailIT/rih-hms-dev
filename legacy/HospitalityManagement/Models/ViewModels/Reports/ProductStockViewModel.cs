using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models.ViewModels.Reports
{
    public class ProductStockViewModel
    {
        public long LocationId { get; set; }
        public string Location { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductCostPrice { get; set; }
        public decimal ProductSellingPrice { get; set; }     
        public decimal ProductDbStock { get; set; }
        public string ProductCode { get; set; }       
        public string ProductUOMName { get; set; }
        public string Quantity { get; set; }


    }
}