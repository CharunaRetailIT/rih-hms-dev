using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
   public class BLL_SysYears
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_SysYears()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_SysYears(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }
        public IEnumerable<SysYears> GetYears()
        {
            try
            {
                IEnumerable<SysYears> sysyears = _unitofwork.SysYearsRepository.Get().OrderBy(g => g.SysYear);
                if (sysyears != null)
                {
                    return sysyears;
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
