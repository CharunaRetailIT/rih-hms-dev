using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.ViewModels.Reports
{
    public class TOGDetailViewModel
    {
        public TOGDetailViewModel()
        {
            Detail = new List<ReportDetail>();
        }

        public long TransferNoteHeaderId { get; set; }
        public long DocumentId { get; set; }
        public long LocationId { get; set; }
        public long ToLocationId { get; set; }
        public string Remark { get; set; }
        public string Status { get; set; }
        public string DocumentNo { get; set; }
        public string Location { get; set; }
        public string ToLocation { get; set; }
        public string DocumentDate { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public List<ReportDetail> Detail { get; set; }

        public class ReportDetail
        {
        public long ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal OrderQty { get; set; }
        }

    }
}