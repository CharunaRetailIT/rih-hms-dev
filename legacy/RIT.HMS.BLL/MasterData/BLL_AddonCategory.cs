using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
  public  class BLL_AddonCategory
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_AddonCategory()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_AddonCategory(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }
        public IEnumerable<AddonCategoryMaster> GetAddonCategory(Int32 compid)
        {
            try
            {
                IEnumerable<AddonCategoryMaster> AddonCategory = _unitofwork.AddonCategoryMasterRepository.Get(p => p.IsDelete == false && p.CompanyID==compid);

                if (AddonCategory != null)
                {
                    return AddonCategory;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
