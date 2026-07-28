using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public  class BLL_InvPriceLevel
    {
        private readonly UnitOfWork _unitofwork;
        public InvPriceLevel GetPriceLevel(long id)
        {
            try
            {
                InvPriceLevel invpricelevel = _unitofwork.InvPriceLevels.Get(g => g.InvPriceLevelID == id).FirstOrDefault();
                if (invpricelevel != null)
                {
                    return invpricelevel;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public BLL_InvPriceLevel(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }

        public int SavePriceLevel(InvPriceLevel invPriceLev)
        {
            try
            {
                _unitofwork.InvPriceLevels.Insert(invPriceLev);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {

                return 0;
            }


        }

        public InvPriceLevel FindByCode(string code)
        {
            var PriceLevel = _unitofwork.InvPriceLevels.Get(p => p.PriceLevelCode == code).FirstOrDefault();
            if (PriceLevel != null)
            {
                return PriceLevel;
            }
            else
            {
                return null;
            }

        }

        public InvPriceLevel GetPriceLevelById(long id)
        {
            try
            {
                //  SysLocation syslocation = _unitofwork.LocationRepository.Get(g => g.SysLocationID == id).FirstOrDefault();
                // Changed by hasanka 
                InvPriceLevel PriceLevel = _unitofwork.InvPriceLevels.GetById(id);
                if (PriceLevel != null)
                {
                    return PriceLevel;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public InvPriceLevel GetPriceLevelByCode(string code, int companyid)
        {
            try
            {
                InvPriceLevel PriceLevel = _unitofwork.InvPriceLevels.Get(g => g.PriceLevelCode == code && g.CompanyID == companyid).FirstOrDefault();
                if (PriceLevel != null)
                {
                    return PriceLevel;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public int UpdateInvPriceLevel(InvPriceLevel loc)
        {
            try
            {
                _unitofwork.InvPriceLevels.Update(loc);
                int res = _unitofwork.Save();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<InvPriceLevel> GetPriceLevel(Int32 compid)
        {
            try
            {
                IEnumerable<InvPriceLevel> sysPriceLevel = _unitofwork.InvPriceLevels.Get(g => g.CompanyID == compid).OrderBy(g => g.PriceLevelCode);
                if (sysPriceLevel != null)
                {
                    return sysPriceLevel;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public SysCompany GetCompanyById(long id)
        {
            try
            {
                SysCompany syscompany = _unitofwork.CompanyRepository.GetById(id);
                if (syscompany != null)
                {
                    return syscompany;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public SysLocation GetLocationById(long id)
        {
            try
            {
                //  SysLocation syslocation = _unitofwork.LocationRepository.Get(g => g.SysLocationID == id).FirstOrDefault();
                // Changed by hasanka 
                SysLocation syslocation = _unitofwork.LocationRepository.GetById(id);
                if (syslocation != null)
                {
                    return syslocation;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public InvPriceLevel FindByCode(string code, Int32 compid)
        {
            var product = _unitofwork.InvPriceLevels.Get(p => p.PriceLevelCode == code && p.CompanyID == compid).FirstOrDefault();
            if (product != null)
            {
                return product;
            }
            else
            {
                return null;
            }
        }
    }
}
