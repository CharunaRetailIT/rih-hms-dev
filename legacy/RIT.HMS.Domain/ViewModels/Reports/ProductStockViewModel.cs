using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.ViewModels.Reports
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
        public decimal AverageCostPrice { get; set; } //--- Added By Nipuna Francisku #2619
        public decimal AverageCostValue { get; set; } //--- Added By Nipuna Francisku #2619

        //public DateTime DateFrom { get; set; } //added by Aruna
        //public DateTime DateTo { get; set; } //added by Aruna
        public string StockCodeFrom { get; set; }
        
        public string StockCodeTO { get; set; }

        public string ProductNameFrom { get; set; }

        public string ProductNameTO { get; set; }


        public List<ProductStockViewModel> stockmodel { get; set; }
        public List<ProductStockViewModel> stockresultmodel { get; set; }

        public ProductStockViewModel()
        {
            stockmodel = new List<ProductStockViewModel>();
            stockresultmodel = new List<ProductStockViewModel>();
        }
    }
}