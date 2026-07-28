using RIT.HMS.HMSOrderTaker.Data;
using RIT.HMS.HMSOrderTaker.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.HMSOrderTaker.BLL.Masters
{
    
    public class BLL_Locations
    {

        private UnitOfWork<SmartLinkEntities> unitOfWork;
        public BLL_Locations()
        {
            unitOfWork = new UnitOfWork<SmartLinkEntities>();

        }
        public List<SysLocation> GetActiveLocationsByCompanyId(int companyid)
        {
            return unitOfWork.Tbl_SysLocation.Get(filter: l => l.CompanyID == companyid
                               && l.IsActive == true && l.IsDelete == false
                                ).OrderBy(l => l.LocationName).ToList();
                                          
        }
    }
}
