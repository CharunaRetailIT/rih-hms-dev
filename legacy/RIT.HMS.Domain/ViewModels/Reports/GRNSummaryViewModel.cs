using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.ViewModels.Reports
{
    public class GRNSummaryViewModel
    {
        public int DocumentId { get; set; }
        public long LocationId { get; set; }
        public string Location { get; set; }
        public long PurchaseHeaderId { get; set; }
        public string DocumentNo { get; set; }
        public string Status { get; set; }
        public DateTime DocumentDate { get; set; }
        public DateTime GRNDate { get; set; }
        public decimal NetAmount { get; set; }
        public decimal GrossAmount { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int SupplierId { get; set; }
    }
}