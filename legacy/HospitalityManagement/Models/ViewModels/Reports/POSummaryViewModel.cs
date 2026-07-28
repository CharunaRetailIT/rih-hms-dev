using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models.ViewModels.Reports
{
    public class POSummaryViewModel
    {
        public long LocationId { get; set; }
        public string Location { get; set; }
        public long PurchaseOrderHeaderId { get; set; }
        public string DocumentNo { get; set; }
        public DateTime DocumentDate { get; set; }
        public decimal NetAmount { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public long DocumentId { get; set; }
    }
}