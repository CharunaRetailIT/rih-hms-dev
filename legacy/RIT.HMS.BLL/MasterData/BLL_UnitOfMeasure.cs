using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_UnitOfMeasure
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_UnitOfMeasure()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_UnitOfMeasure(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }
        public IEnumerable<UnitOfMeasure> GetUnitOfMeasures(Int32 compid)
        {
            try
            {
                IEnumerable<UnitOfMeasure> unitofmeasure = _unitofwork.UnitOfMeasureRepository.Get(u=>u.CompanyID==compid).OrderBy(um => um.UnitOfMeasureCode);
                if (unitofmeasure != null)
                {
                    return unitofmeasure;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<UnitOfMeasure> GetActiveUnitOfMeasures(Int32 compid)
        {
            try
            {
                IEnumerable<UnitOfMeasure> unitofmeasure = _unitofwork.UnitOfMeasureRepository.Get(um => um.IsDelete == false && um.CompanyID == compid).OrderBy(um => um.UnitOfMeasureCode);
                if (unitofmeasure != null)
                {
                    return unitofmeasure;

                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public UnitOfMeasure GetUnitOfMeasureById(long id)
        {
            try
            {
                UnitOfMeasure unitofmeasure = _unitofwork.UnitOfMeasureRepository.Get(um => um.UnitOfMeasureId == id).FirstOrDefault();
                if (unitofmeasure != null)
                {
                    return unitofmeasure;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int SaveUnitOfMeasure(UnitOfMeasure um)
        {
            try
            {
                _unitofwork.UnitOfMeasureRepository.Insert(um);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateUnitOfMeasure(UnitOfMeasure um)
        {
            try
            {
                _unitofwork.UnitOfMeasureRepository.Update(um);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public UnitOfMeasure GetUnitOfMeasureByCode(string code, Int32 compid)
        {
            try
            {
                UnitOfMeasure unitofmeasure = _unitofwork.UnitOfMeasureRepository.Get(g => g.UnitOfMeasureCode == code && g.CompanyID == compid).FirstOrDefault();
                if (unitofmeasure != null)
                {
                    return unitofmeasure;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }


    }
}
