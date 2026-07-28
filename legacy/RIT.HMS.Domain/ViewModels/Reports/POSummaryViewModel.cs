using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.ViewModels.Reports
{
    public class POSummaryViewModel
    {
        public long LocationId { get; set; }
        public string Location { get; set; }
        public long PurchaseOrderHeaderId { get; set; }
        public string DocumentNo { get; set; }
        public DateTime DocumentDate { get; set; }
        public DateTime PODate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public decimal NetAmount { get; set; }
        [DefaultValue(null)]
        public DateTime DateFrom { get; set; }
        [DefaultValue(null)]
        public DateTime DateTo { get; set; }
        public long DocumentId { get; set; }
        public string Status { get; set; }
    }
}