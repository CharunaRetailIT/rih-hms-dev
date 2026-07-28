using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_LocationType
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_LocationType()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_LocationType(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }

        public List<SysLocationType> GetActiveAll()
        {
            try
            {
                List<SysLocationType> sm = _unitofwork.LocationTypeRepository.Get(g => g.IsActive == true).ToList();
                if (sm != null)
                {
                    return sm;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                return null;
            }
        }


    }
}
