using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Loyalty
{
   public class PointsExpirationSchedule :BaseEntity
    {
        [Key]
        public int Idx { get; set; }

        [DefaultValue(0)]
        public int Type { get; set; }
        public string SQL { get; set; }
        [DefaultValue("")]
        [StringLength(15)]
        public string user { get; set; }
        public DateTime Date { get; set; }
        public DateTime ScheduleDate { get; set; }

        [DefaultValue(0)]
        public int Status { get; set; }
        public DateTime? EndDate { get; set; }

        public int PointsExpirationId { get; set; }

    }
}
