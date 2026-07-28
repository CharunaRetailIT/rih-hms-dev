using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
   public class BLL_GroupOfCompany
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_GroupOfCompany()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_GroupOfCompany(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }

        public IEnumerable<SysGroupOfCompany> GetGroupOfCompanies(int companyid)
        {
            try
            {
                int gcid = _unitofwork.CompanyRepository.Get(c => c.SysCompanyID == companyid && c.IsActive == true && c.IsDelete == false).FirstOrDefault().SysGroupOfCompanyId;

                IEnumerable<SysGroupOfCompany> sysgroupofcompany = _unitofwork.GroupOfCompanyRepository.Get(g => g.IsDelete == false && g.IsActive == true && g.SysGroupOfCompanyId==gcid ).OrderBy(g => g.GroupOfCompanyCode);
                if (sysgroupofcompany != null)
                {
                    return sysgroupofcompany;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public IEnumerable<SysGroupOfCompany> GetActiveGroupOfCompanies()
        {
            try
            {
                IEnumerable<SysGroupOfCompany> sysgroupofcompany = _unitofwork.GroupOfCompanyRepository.Get(g => g.IsDelete == false && g.IsActive == true).OrderBy(g => g.GroupOfCompanyCode);
                if (sysgroupofcompany != null)
                {
                    return sysgroupofcompany;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public SysGroupOfCompany GetGroupOfCompanyById(long id)
        {
            try
            {
                SysGroupOfCompany sysgroupofcompany = _unitofwork.GroupOfCompanyRepository.Get(g => g.SysGroupOfCompanyId == id).FirstOrDefault();
                if (sysgroupofcompany != null)
                {
                    return sysgroupofcompany;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public SysGroupOfCompany GetGOCByCode(string code)
        {
            try
            {
                SysGroupOfCompany goc = _unitofwork.GroupOfCompanyRepository.Get(g => g.GroupOfCompanyCode == code).FirstOrDefault();
                if (goc != null)
                {
                    return goc;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public int SaveGroupOfCompany(SysGroupOfCompany com)
        {
            try
            {
                _unitofwork.GroupOfCompanyRepository.Insert(com);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateGroupOfCompany(SysGroupOfCompany com)
        {
            try
            {
                _unitofwork.GroupOfCompanyRepository.Update(com);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }



    }
}
