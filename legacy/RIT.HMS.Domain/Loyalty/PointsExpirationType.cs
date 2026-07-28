using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Loyalty
{
   public class PointsExpirationType:BaseEntity
    {
        public int PointsExpirationTypeId { get; set; }
        public string Desc { get; set; }
        public bool IsActive { get; set; }
    }
}
