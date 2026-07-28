using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.ViewModels.Reports
{
    public class StockMovementViewModel
    {
        public StockMovementViewModel()
        {
            Details = new List<Detail>();
        }
        public int CompanyId { get; set; }
        public int LocationId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public List<Detail> Details { get; set; }
        public class Detail
        {
            public int ProductId { get; set; }
            public string ProductCode { get; set; }
            public string ProductDesc { get; set; }

            [DefaultValue(0)]
            public decimal OpeningBalance { get; set; }

            [DefaultValue(0)]
            public decimal OpeningBalanceToDate { get; set; }

            [DefaultValue(0)]
            public decimal GRNQty { get; set; }
            [DefaultValue(0)]
            public decimal ReturnQty { get; set; }
            [DefaultValue(0)]
            public decimal StockAdjustment { get; set; }
            [DefaultValue(0)]
            public decimal TransferIn { get; set; }

            [DefaultValue(0)]
            public decimal TransferOut { get; set; }

            [DefaultValue(0)]
            public decimal TotalQty { get; set; }
            [DefaultValue(0)]
            public decimal SoldQty { get; set; }
            [DefaultValue(0)]
            public decimal Rate { get; set; }
            [DefaultValue(0)]
            public decimal TotalAmount { get; set; }
            public string  BaseType { get; set; }

            [DefaultValue(0)]
            public decimal SalesInQty { get; set; }

            [DefaultValue(0)]
            public decimal SalesOutQty { get; set; }
        }

      

    }
}
