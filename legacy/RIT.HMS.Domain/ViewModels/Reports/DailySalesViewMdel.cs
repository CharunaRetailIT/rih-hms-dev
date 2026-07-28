using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.ViewModels.Reports
{
    public class DailySalesViewMdel
    {
        public DailySalesViewMdel()
        {
            SalesDataList = new List<SalesData>();
            ValidMonthEndDataList = new List<ValidMonthEndData>();
        }

        public int LocationId { get; set; }
        public DateTime Date { get; set; }
        public DateTime DateTo { get; set; }
        public int[] Locations { get; set; }

        public List<SalesData> SalesDataList { get; set; }

        public List<ValidMonthEndData> ValidMonthEndDataList { get; set; }

        public class ValidMonthEndData
        {

            public int SysLocationID { get; set; }

            public string DocumentType { get; set; }
            public string Message { get; set; }
            public int DocumentCount { get; set; }

        }
        public class SalesData
        {
            public string LocationName { get; set; }
            public DateTime RecDate { get; set; }
            public string Receipt { get; set; }
            public decimal FoodSale { get; set; }
            public decimal BevSale { get; set; }
            public decimal NonSale { get; set; }
            public decimal ServCharge { get; set; }
            public decimal ChiliPaste { get; set; }
            public decimal Cash { get; set; }
            
            public decimal Card { get; set; } //Comment Card uncomment
            public decimal MASTERCARD { get; set; }
            public decimal VISACARD { get; set; }
            public decimal AMEXCARD { get; set; }
            public decimal DEBITCARD { get; set; }            
            public string PayType { get; set; }
            public decimal Others { get; set; }
            public decimal VAT { get; set; }
            public decimal Discount { get; set; }
            public string HoldersName { get; set; }
            public int ZNo { get; set; }
            public decimal NBT { get; set; }
            public decimal TDL { get; set; }
            public decimal Gross { get; set; }
            public decimal TNet { get; set; }
            public decimal Credit { get; set; }
            public string DiscountRemark { get; set; }
            public string DiscRem { get; set; }
            public string Location { get; set; }

            public decimal ACCharge { get; set; }
            public decimal Online { get; set; }
            public decimal Uber { get; set; }
            public decimal Pickme { get; set; }
            //public string Online { get; set; }
            //public string Uber { get; set; }
            //public string Pickme { get; set; }
        }
    }
}