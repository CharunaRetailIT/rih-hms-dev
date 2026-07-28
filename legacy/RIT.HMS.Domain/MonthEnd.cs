using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain
{
    public class MonthEnd
    {
        [Key]
        public long MonthEndId { get; set; }

        public int LocationId { get; set; }

        public int LocYear { get; set; }
        public int LocMonth { get; set; }

        [MaxLength(50)]
        public string LocMonthDesc { get; set; }
        public bool LocStatus { get; set; }
        public bool LocIsClose { get; set; }

        [MaxLength(50)]
        public string CreatedUser { get; set; }

        public DateTime? CreatedDate { get; set; }

        [MaxLength(50)]
        public string ModifiedUser { get; set; }

        public DateTime? ModifiedDate { get; set; }

        [DefaultValue(0)]
        public int DataTransfer { get; set; }

        [DefaultValue(0)]
        public int CompanyId { get; set; }
    }
}
