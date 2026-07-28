using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.ViewModels.Reports
{
    public class ProductionReportViewModel
    {
        public ProductionReportViewModel()
        {
            ProductionDetailReport = new List<ProductionDetail>();
        }


        public long HeaderId { get; set; }
        public long LocationId { get; set; }        
        public long DocumentId { get; set; }
        public string Location { get; set; }
        public string DocNo { get; set; }
        public string Date { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string Remark { get; set; }

        public List<ProductionDetail> ProductionDetailReport { get; set; }
        public class ProductionDetail
        {

        public long HeaderId { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductCostPrice { get; set; }
        public decimal ProductSellingPrice { get; set; }
        public decimal ProductQuantity { get; set; }
        public decimal ProductDbStock { get; set; }
        public string ProductCode { get; set; }
        public long ProductUOMId { get; set; }
        public string ProductUOMName { get; set; }

        }

        //public long MaterialId { get; set; }
        //public string MaterialName { get; set; }
        //public decimal MaterialCostPrice { get; set; }
        //public decimal MaterialSellingPrice { get; set; }
        //public decimal MaterialQuantity { get; set; }
        //public decimal MaterialDbStock { get; set; }
        //public string MaterialCode { get; set; }
        //public long MaterialUOMId { get; set; }
        //public string MaterialUOMName { get; set; }
        //public decimal FinalProductionQuantity { get; set; }
    }
}