using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain
{
    public class CateringMood
    {
        public long CateringMoodID { get; set; }
        
        [Required()]
        [MaxLength(20)]
        public string CateringMoodName { get; set; }

        [Required()]
        [MaxLength(50)]
        public string OrderSequence { get; set; }

        [DefaultValue(0)]
        public bool IsActive { get; set; }

        [DefaultValue(false)]
        public bool IsServiceCharge { get; set; }
 
        public DateTime ModifiedDate { get; set; }

        [DefaultValue(0)]
        public int CompanyId { get; set; }
    }
}
