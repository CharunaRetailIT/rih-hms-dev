using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.ViewModels.Journal
{
    public class JournalViewModel
    {

        public JournalViewModel()
        {
            JournalReport = new List<JournalReport>();
        }

        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int[] Locations { get; set; }
        public int CompanyId { get; set; }
        public int LocationId { get; set; }

        public List<JournalReport> JournalReport { get; set; }
      
        public virtual SysCompany SysCompany { get; set; }
    }

    public class JournalReport
    {
        public string TRANTYPE { get; set; }
        public string DOCNO { get; set; }
        public DateTime DATE { get; set; }
        public string ACODE { get; set; }
        public string CCODE { get; set; }
        public string DESCRIPTION { get; set; }
        public string DRCR { get; set; }
        public decimal AMOUNT { get; set; }
        public string CUSTOMER { get; set; }
        public string LocationName { get; set; }

        public List<string> LocationCodes { get; set; }
        public int CompanyId { get; set; }

    }
}
