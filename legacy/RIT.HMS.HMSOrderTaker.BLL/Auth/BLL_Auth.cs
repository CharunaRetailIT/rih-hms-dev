using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RIT.HMS.HMSOrderTaker.Domain;
using RIT.HMS.HMSOrderTaker.Data;

namespace RIT.HMS.HMSOrderTaker.BLL.Auth
{
    public class BLL_Auth
    {
        private UnitOfWork<SmartLinkEntities> unitOfWork;
        public BLL_Auth()
        {
            unitOfWork = new UnitOfWork<SmartLinkEntities>();

        }
       
        public IEnumerable<CashierPermission> GetUserDetailsByPassword(string password)
        {
            var existsuser = unitOfWork.Tbl_CashierPermission.Get(filter:cp=>cp.Password==password);
            return existsuser == null ? null : existsuser;
        }
       
    }
}
