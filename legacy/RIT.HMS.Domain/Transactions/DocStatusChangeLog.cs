using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Transactions
{
   public class DocStatusChangeLog
    {
        public int DocStatusChangeLogId { get; set; }
        public string Module { get; set; }
        public int Status { get; set; }

        [StringLength(20)]
        [DefaultValue("")]
        public string StatusAppliedBy { get; set; }
        public DateTime StatusAppliedOn { get; set; }
    }
}
