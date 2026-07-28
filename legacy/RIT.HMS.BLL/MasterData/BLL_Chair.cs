using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_Chair
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_Chair()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_Chair(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }

        public IEnumerable<ChairMaster> GetChairs()
        {
            try
            {
                IEnumerable<ChairMaster> chairmaster = _unitofwork.ChairRepository.Get().OrderBy(cm => cm.ChairCode);
                if (chairmaster != null)
                {
                    return chairmaster;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<ChairMaster> GetActiveChairs()
        {
            try
            {
                IEnumerable<ChairMaster> chairmaster = _unitofwork.ChairRepository.Get(cm => cm.IsDelete == false).OrderBy(cm => cm.ChairCode);
                if (chairmaster != null)
                {
                    return chairmaster;

                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public ChairMaster GetChairById(long id)
        {
            try
            {
                ChairMaster chairmaster = _unitofwork.ChairRepository.Get(cm => cm.ChairMasterID == id).FirstOrDefault();
                if (chairmaster != null)
                {
                    return chairmaster;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public ChairMaster GetChairByCode(string code)
        {
            try
            {
                ChairMaster chair = _unitofwork.ChairRepository.Get(g => g.ChairCode == code).FirstOrDefault();
                if (chair != null)
                {
                    return chair;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public int SaveChair(ChairMaster cm)
        {
            try
            {
                _unitofwork.ChairRepository.Insert(cm);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateChair(ChairMaster cm)
        {
            try
            {
                _unitofwork.ChairRepository.Update(cm);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }




    }
}
