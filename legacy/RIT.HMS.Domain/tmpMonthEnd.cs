using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain
{
    public class tmpMonthEnd
    {
        public long tmpMonthEndId { get; set; }
        public int SysLocationID { get; set; }
        public string DocumentType { get; set; }
        public string Message { get; set; }
        public int DocumentCount { get; set; }
    }
}
