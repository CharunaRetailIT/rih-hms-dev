using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.ViewModels.Promotions
{
  public class VMPromotionSchedular
    {

        public string Day { get; set; }

        public string DayId { get; set; }

        public DateTime? FromTime { get; set; }

        public DateTime? ToTime { get; set; }
    }
}
