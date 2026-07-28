using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.ViewModels
{
   public class ServingUnitPricesViewModel : BaseEntity
    {
        public ServingUnitPricesViewModel()
        {
            ServingUnitsDetail = new List<ServingUnits>();
        }

        public int ProductId { get; set; }
        public long ProductServingUnitId { get; set; }
        public string Location { get; set; }
        public string ServingUnit { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public string ProductName { get; set; }
        public List<ServingUnits> ServingUnitsDetail { get; set; }
        public class ServingUnits : BaseEntity
        {
            public string Location { get; set; }
            public string ServingUnit { get; set; }
            public decimal CostPrice { get; set; }
            public decimal SellingPrice { get; set; }
        }

        //public List<SysLocation> Locations { get; set; }
        //public List<Product> Products { get; set; }
        //public List<ProductServingUnit> ProductServingUnits { get; set; }
        //public List<String> ServingUnitNames { get; set; }
    }
}
