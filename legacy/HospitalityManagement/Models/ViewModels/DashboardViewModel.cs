using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models.ViewModels
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

    }
}