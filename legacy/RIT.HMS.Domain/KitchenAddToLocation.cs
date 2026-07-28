using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain
{
    public class KitchenAddToLocation
    {
        public int GeneralLocationId { get; set; }
        public SysLocation GeneralLocation { get; set; }
        public List<SysLocation> GeneralLocationList { get; set; }
        public List<SysLocation> KitchenLocationList { get; set; }
        public List<SysLocationMapper> LocationMapper { get; set; }

        public int GroupOfCompanyID { get; set; }
        public int CompanyID { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int DataTransfer { get; set; }
        public bool IsActive { get; set; }
        public KitchenAddToLocation()
        {
            GeneralLocationList = new List<SysLocation>();
            KitchenLocationList = new List<SysLocation>();
            LocationMapper = new List<SysLocationMapper>();
            GeneralLocation = new SysLocation();
        }
    }
}
