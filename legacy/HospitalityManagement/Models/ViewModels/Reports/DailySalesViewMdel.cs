using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models.ViewModels.Reports
{
    public class DailySalesViewMdel
    {

        public DailySalesViewMdel()
        {
            SalesDataList = new List<SalesData>();
        }

        public int LocationId { get; set; }
        public DateTime Date { get; set; }

        public List<SalesData> SalesDataList { get; set; }

        public class SalesData
        {

            public string Receipt { get; set; }
            public decimal FoodSale { get; set; }     
            public decimal BevSale { get; set; }    
            public decimal ServCharge { get; set; }
            public decimal ChiliPaste { get; set; }
            public decimal Cash { get; set; }
            public decimal Card { get; set; }
            public decimal Others { get; set; }
            public decimal VAT { get; set; }
            public decimal Discount { get; set; }
            public string HoldersName { get; set; }
            public int ZNo { get; set; }
            public decimal Online { get; set; }
            public decimal Uber { get; set; }
            public decimal Pickme { get; set; }
        }

    }
}