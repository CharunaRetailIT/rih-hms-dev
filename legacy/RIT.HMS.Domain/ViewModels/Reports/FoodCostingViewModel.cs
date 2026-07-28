using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.ViewModels.Reports
{
    public class FoodCostingViewModel
    {
        
        public FoodCostingViewModel()
        {
            FoodCostDetails = new List<FoodCostingDetail>();
        }
        public long CompanyId { get; set; }
        public string Company { get; set; }
        public long LocationId { get; set; }
        public string Location { get; set; }
        public long DepartmentId { get; set; }
        public string Department { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public List<FoodCostingDetail> FoodCostDetails { get; set; }

        public class FoodCostingDetail
        {
            public long ProductId { get; set; }
            public string ProductCode { get; set; }
            public string ProductName { get; set; }
            public decimal Sale { get; set; }
            public decimal Cost { get; set; }
            public decimal GP { get; set; }
            public decimal FoodCostPrc { get; set; }
            public string Description { get; set; }
            public decimal SaleValue { get; set; }
            public decimal CostValue { get; set; }
            public decimal FoodCost { get; set; }

            // for consumption report
            public string Unit { get; set; }
            public decimal Qty { get; set; }
            public decimal Value { get; set; }
            public string productdesc { get; set; }

            public decimal AverageCost { get; set; } //--- Added By Nipuna Francisku #2619
            public decimal AverageGP { get; set; } //--- Added By Nipuna Francisku #2619
            public decimal AverageFoodCost { get; set; } //--- Added By Nipuna Francisku #2619

        }
    }
}
