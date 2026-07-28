using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.ViewModels.Reports
{
    public class TOGSummaryViewModel
    {
        public long LocationId { get; set; }
        public string Location { get; set; }
        public string ToLocation { get; set; }
        public long TransferNoteHeaderId { get; set; }
        public long DocumentId { get; set; }
        public string DocumentNo { get; set; }
        public DateTime DocumentDate { get; set; }
        public decimal NetAmount { get; set; }
        public string Remark { get; set; }
        public string Status { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }
}