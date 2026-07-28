using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.ViewModels.Reports
{
    public class StockBinCardViewModel
    {
        public StockBinCardViewModel()
        {
            Details = new List<Detail>();
        }
        public int CompanyId { get; set; }
        public int LocationId { get; set; }
        public int DepartmentId { get; set; }
        public string Department { get; set; } 
        public string StockCode { get; set; }
        public string Unit { get; set; }
        public int ProductId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public List<Detail> Details { get; set; }
        public bool WithZeroBalances { get; set; }
        public class Detail
        {
            public int ProductId { get; set; }
            public string ProductCode { get; set; }
            public string StockCode { get; set; }
            public string ProductName { get; set; }
            public DateTime TransactionDate { get; set; }       
            public string TransactionNo { get; set; }
            public string ToLocationName { get; set; }
            public string TransactionType { get; set; }
            public decimal StockQty { get; set; }
            public string  Location { get; set; }
            public string  Department { get; set; }
            public decimal Stock { get; set; }
            public string Unit { get; set; }

            public decimal CostPrice { get; set; }

            public decimal CostValue { get; set; }
        }
    }
}
