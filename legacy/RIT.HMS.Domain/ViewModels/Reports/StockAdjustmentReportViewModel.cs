using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.ViewModels.Reports
{
    public class StockAdjustmentReportViewModel
    {
        public StockAdjustmentReportViewModel()
        {
            Detail = new List<ReportDetail>();
        }

        public long StockAdjustmentHeaderId { get; set; }
        public long DocumentId { get; set; }
        public long LocationId { get; set; }
        public string Remark { get; set; }

        public string DocumentNo { get; set; }
        public string Location { get; set; }
        public string DocumentDate { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public List<ReportDetail> Detail { get; set; }

        public class ReportDetail
        {
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal AdjustStock { get; set; }
        public decimal NewStock { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal AvgCost { get; set; }
        public string BaseType { get; set; }
        public string Reason { get; set; }

        public decimal costvalue { get; set; }
        public decimal TotalCostValue { get; set; }
        }

    }
}