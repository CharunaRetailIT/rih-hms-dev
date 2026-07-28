using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.ViewModels
{
    public class DashboardViewModel
    {
        public int POCount { get; set; }
        public int ProductionCount { get; set; }
        public int InvoiceCount { get; set; }
        public int TOGCount { get; set; }
        public int POCountThisWeek { get; set; }
        public int ProductionCountThisWeek { get; set; }
        public int TOGCountThisWeek { get; set; }
        public List<Top10Productions> Top10Products { get; set; }
        public class Top10Productions {
            public string ProductName { get; set; }
            public decimal ProductCount { get; set; }
            public string ProductValues { get; set; }
        }

        // Dashboard version 2.0.0

        public class DashboardParms
        {
            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }
            public int LocationId { get; set; }
            public int CustomerGroupId { get; set; }
            public int TypeId { get; set; }
            public int DepartmentId { get; set; }
            public int OrderTypeId { get; set; }
            public int CateringModeId { get; set; }
            public int RecordCount { get; set; }
            public TimeSpan TimeFrom { get; set; }
            public TimeSpan TimeTo { get; set; }
            public int CompanyId { get; set; }
        }

        public class RevenueVsCost
        {
            public string recdate { get; set; }
            public decimal Nett { get; set; }
            public decimal Cost { get; set; }
            public string day { get; set; }
        }

        public class Products
        {
            public string recdate { get; set; }
            public string ProductCode { get; set; }
            public string Product { get; set; }
            public string ProductName { get; set; }
            public decimal Nett { get; set; }
            public string Value { get; set; }
            public int DeptID { get; set; }  // oly for coding purposes

        }

        public class OrderTypesVariation
        {
            public string recdate { get; set; }
            public int KOTCount { get; set; }
            public int BOTCount { get; set; }
            public int NoneCount { get; set; }
            public List<Products> Products { get; set; }

        }
        public class OrderTypeBreakdown
        {
            public int OrderTypeId { get; set; }
            public string OrderType { get; set; }
            public decimal Nett { get; set; }
            public decimal Cost { get; set; }
           // public List<Products> Products { get; set; }

        }
        public class DeptOrderTypeWiseSales
        {
            public int DeptId { get; set; }
            public string DeptName { get; set; }
            public int OrderTypeId { get; set; }           
            public string OrderType { get; set; }
            public decimal Nett { get; set; }
            public decimal Cost { get; set; }
            public List<Products> Products { get; set; }
        }

        public class Waistage
        {
            public string recdate { get; set; }
            public decimal Nett { get; set; }
            public decimal Cost { get; set; }
            public List<Products> Products { get; set; }
        }

        public class HourlySales
        {
            public int RecTime { get; set; }
            public decimal Nett { get; set; }
         
        }

        public class OrderTimeConsumption
        {
            public string Receipt { get; set; }
            public int value { get; set; }

        }

        public class FoodCost
        {
            public int DeptId { get; set; }
            public string DeptName { get; set; }
            public string RecDate { get; set; }
            public decimal Value { get; set; }

        }

        public class HourlySalesTabular
        {
            public string RecTime { get; set; }
            public decimal Monday { get; set; }
            public decimal Tuesday { get; set; }
            public decimal Wednesday { get; set; }
            public decimal Thursday { get; set; }
            public decimal Friday { get; set; }
            public decimal Saturday { get; set; }
            public decimal Sunday { get; set; }
            public string ActualDate { get; set; }
            public string ActualDay { get; set; }
            public int RowIDx { get; set; }

        }

    }
}