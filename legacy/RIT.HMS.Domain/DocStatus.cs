using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain
{
   public class DocStatus
    {
        public int DocStatusId { get; set; }
        public string DocType { get; set; }
        public int StatusId { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
    }
}
