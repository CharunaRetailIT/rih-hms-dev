using RIT.HMS.HMSOrderTaker.Data;
using RIT.HMS.HMSOrderTaker.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.HMSOrderTaker.BLL.Masters
{
    
    public class BLL_Tables
    {
        private UnitOfWork<SmartLinkEntities> unitOfWork;
        public BLL_Tables()
        {
            unitOfWork = new UnitOfWork<SmartLinkEntities>();
        }
        public List<TableMaster> GetActiveTablesByCompanyIdAndLocationId(int locationid)
        {
            return unitOfWork.Tbl_TblMasters.Get(filter: l => l.LocationId == locationid
                               &&  l.IsDelete == false).ToList();

        }
    }
}
