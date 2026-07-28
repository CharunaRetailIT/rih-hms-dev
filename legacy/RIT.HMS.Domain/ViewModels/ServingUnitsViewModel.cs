using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.ViewModels
{
    public class ServingUnitsViewModel
    {
        public string ServingUnitId { get; set; }
        public string ServingUnitName { get; set; }
        public string ServingUnitSellingPrice { get; set; }
        public string ServingUnitCostPrice { get; set; }
    }
}