using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    
    public class BLL_StewardsMaster
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_StewardsMaster()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_StewardsMaster(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }

        public IEnumerable<StewardsMaster> GetStewards(Int32 compid)
        {
            try
            {
                IEnumerable<StewardsMaster> stewardsMaster = _unitofwork.StewardsMasterRepository.Get(e => e.IsDelete == false && e.CompanyID == compid).OrderBy(e => e.StewardsMasterID);
                if (stewardsMaster != null)
                {
                    return stewardsMaster;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<StewardsMaster> GetActiveStewards(Int32 compid)
        {
            try
            {
                IEnumerable<StewardsMaster> stewardsMaster = _unitofwork.StewardsMasterRepository.Get(e => e.IsDelete == false && e.IsActive == true && e.CompanyID == compid).OrderBy(e => e.StewardCode);
                if (stewardsMaster != null)
                {
                    return stewardsMaster;

                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public StewardsMaster GetStewardsById(long id)
        {
            try
            {
                StewardsMaster stewardsMaster = _unitofwork.StewardsMasterRepository.Get(e => e.StewardsMasterID == id).FirstOrDefault();
                if (stewardsMaster != null)
                {
                    return stewardsMaster;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public StewardsMaster GetStewardsByCode(string code, Int32 compid)
        {
            try
            {
                StewardsMaster stewardsMaster = _unitofwork.StewardsMasterRepository.Get(g => g.StewardCode == code && g.CompanyID == compid).FirstOrDefault();
                if (stewardsMaster != null)
                {
                    return stewardsMaster;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public int SaveStewards(StewardsMaster Steward)
        {
            try
            {
                _unitofwork.StewardsMasterRepository.Insert(Steward);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateStewards(StewardsMaster Steward)
        {
            try
            {
                _unitofwork.StewardsMasterRepository.Update(Steward);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
}
