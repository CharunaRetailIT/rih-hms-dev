using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalityManagement.Models.ViewModels.Promotions
{
  public class VMPromotionSchedular
    {
       
        public int Day { get; set; }

        public string DayId { get; set; }
  
        public DateTime? FromTime { get; set; }
   
        public DateTime? ToTime { get; set; }
    }
}
