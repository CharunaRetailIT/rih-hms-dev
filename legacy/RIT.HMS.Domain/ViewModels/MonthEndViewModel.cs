using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.ViewModels
{
    public class MonthEndViewModel
    {
        public int LocationID { get; set; }
        public string LocDesc { get; set; }
        public int LocYear { get; set; }
        public int LocMonth { get; set; }
        public string LocMonthDesc { get; set; }
        public bool LocStatus { get; set; }
        public string LocStatusDesc { get; set; }
    }
}
