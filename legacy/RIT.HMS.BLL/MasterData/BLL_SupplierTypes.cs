using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
  public  class BLL_SupplierTypes
    {

        private readonly UnitOfWork _unitofwork;
        public BLL_SupplierTypes()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_SupplierTypes(string connectionstring)
        {
            _unitofwork = new UnitOfWork(connectionstring);
        }
        public IEnumerable<SupplierType> GetSupplierTypes(Int32 compid)
        {
            try
            {
                IEnumerable<SupplierType> suppliertypes = _unitofwork.SuplierTypeRepository.Get(g => g.IsDelete == false && g.CompanyID == compid).OrderBy(sg => sg.SupplierTypeCode);
                if (suppliertypes != null)
                {
                    return suppliertypes;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<SupplierType> GetActiveSupplierTypes(Int32 compid)
        {
            try
            {
                IEnumerable<SupplierType> suppliertypes = _unitofwork.SuplierTypeRepository.Get(sg => sg.IsDelete == false && sg.CompanyID == compid).OrderBy(sg => sg.SupplierTypeCode);
                if (suppliertypes != null)
                {
                    return suppliertypes;

                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public SupplierType GetSupplierTypeById(long id)
        {
            try
            {
                SupplierType suppliertypes = _unitofwork.SuplierTypeRepository.Get(sg => sg.SupplierTypeID == id).FirstOrDefault();
                if (suppliertypes != null)
                {
                    return suppliertypes;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int SaveSupplierType(SupplierType st)
        {
            try
            {
                _unitofwork.SuplierTypeRepository.Insert(st);
                int res = _unitofwork.Save();
                return res;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public int UpdateSupplierType(SupplierType st)
        {
            try
            {

                _unitofwork.SuplierTypeRepository.Update(st);
                int res = _unitofwork.Save();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public SupplierType GetSupTypeByCode(string code, Int32 compid)
        {
            try
            {
                SupplierType suppliertypes = _unitofwork.SuplierTypeRepository.Get(g => g.SupplierTypeCode == code && g.CompanyID == compid).FirstOrDefault();
                if (suppliertypes != null)
                {
                    return suppliertypes;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }


        public bool SupplierTypeIsUsing(int suppliertypeid)
        {
            try
            {

                return _unitofwork.SuplierTypeRepository.Get().Any(s => s.SupplierTypeID == suppliertypeid);

            }
            catch (Exception ex)
            {

                throw;
            }
        }


    }
}
