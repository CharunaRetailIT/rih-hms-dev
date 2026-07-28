using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.ViewModels.Reports
{
    public class PRNSummaryViewModel
    {
        public long LocationId { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public int DocumentId { get; set; }
        public long PurchaseHeaderId { get; set; }
        public string DocumentNo { get; set; }
        public DateTime DocumentDate { get; set; }  
        public decimal NetAmount { get; set; }
        public decimal GrossAmount { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }
}