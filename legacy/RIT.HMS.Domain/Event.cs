using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain
{
   public class Event : BaseEntity
    {
        public Event()
        {
            EventProducts = new List<EventProduct>();
        }

        [DefaultValue(0)]
        public int EventId { get; set; }
        [DefaultValue("")]
        [Required(ErrorMessage = "Event Code Required")]
        public string EventCode { get; set; }
        [DefaultValue("")]     
        [Required(ErrorMessage="Event Name Required")]
       
        public string EventName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public TimeSpan FromTime { get; set; }
        public TimeSpan ToTime { get; set; }
        [DefaultValue(false)]
        public bool IsPOS { get; set; }

        [NotMapped]
        public List<EventProduct> EventProducts { get; set; }


    }
}
