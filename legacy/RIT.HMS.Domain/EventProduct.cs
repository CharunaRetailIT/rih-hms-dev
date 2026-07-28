using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain
{
    public class EventProduct
    {
        public int EventProductId { get; set; }
        [DefaultValue(0)]
        public int EventId { get; set; }

        [DefaultValue("")]
        public string EventName { get; set; }
        [DefaultValue(0)]
        public int ProductId { get; set; }

        [StringLength(100)]
        [DefaultValue("")]
        public string ProductName { get; set; }

        [DefaultValue(false)]
        public bool IsActive { get; set; }

        [DefaultValue(0)]
        public int OrdSeq { get; set; }
        [StringLength(50)]
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        [StringLength(50)]
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
