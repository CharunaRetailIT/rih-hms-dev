using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_CateringMood
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_CateringMood()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_CateringMood(string connectionstring)
        {
            _unitofwork = new UnitOfWork(connectionstring);
        }
        public IEnumerable<CateringMood> GetByCaterMoodId(long id)
        {
            try
            {
                IEnumerable<CateringMood> catermood = _unitofwork.CateringMoodRepository.Get(g => g.IsActive == true).OrderBy(g => g.CateringMoodID);
                if (catermood != null)
                {
                    return catermood;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<CateringMood> GetByCateringMoods(int companyid)
        {
            try
            {
                IEnumerable<CateringMood> catermood = _unitofwork.CateringMoodRepository.Get(g => g.IsActive == true && g.CompanyId==companyid);
                if (catermood != null)
                {
                    return catermood;
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
