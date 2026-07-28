using RIT.HMS.Data;
using RIT.HMS.Domain;
using RIT.HMS.Domain.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
  public  class BLL_Company
    {
       private readonly UnitOfWork _unitofwork;
       public BLL_Company()
        {
             _unitofwork = new UnitOfWork();
        }
        public BLL_Company(string actualdb)
        {
            _unitofwork = new UnitOfWork(actualdb);
        }
        public IEnumerable<SysCompany> GetCompanies()
        {
            try
            {
                              
                IEnumerable<SysCompany> syscompany = _unitofwork.CompanyRepository.Get(g => g.IsDelete == false && g.IsActive == true).OrderBy(g => g.CompanyCode);
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

        public IEnumerable<SysCompany> ShowCompanies(int companyid)
        {
            try
            {
                IEnumerable<SysCompany> syscompany = _unitofwork.CompanyRepository.Get(g => g.IsDelete == false && g.SysCompanyID==companyid).OrderBy(g => g.CompanyCode);
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
        public IEnumerable<SysCompany> GetByGOCId(long id,Int32 compid)
        {
            try
            {
                IEnumerable<SysCompany> syscompany = _unitofwork.CompanyRepository.Get(g => g.IsDelete == false && g.SysGroupOfCompanyId == id && g.SysCompanyID==compid).OrderBy(g => g.CompanyCode);
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
        public IEnumerable<SysCompany> GetActiveCompanies()
        {
            try
            {
                IEnumerable<SysCompany> syscompany = _unitofwork.CompanyRepository.Get().Where(g => g.IsDelete == false).OrderBy(g => g.CompanyCode);
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

        public int SaveCompany(SysCompany com,int createuserid)
        {
            try
            {
                _unitofwork.CompanyRepository.Insert(com);
                if (_unitofwork.Save() == 1)
                {
                    var configs = _unitofwork.ConfigurationRepository.Get().Select(c => new { c.ConfigurationKey, c.ConfigurationDescription }).Distinct();
                    List<Configuration> Configurations = new List<Configuration>();
                    foreach (var c in configs)
                    {
                        Configuration Configuration = new Configuration();
                        Configuration.ConfigurationKey = c.ConfigurationKey;
                        Configuration.ConfigurationDescription = c.ConfigurationDescription;
                        Configuration.EffectLocationId = 1;
                        Configuration.ConfigurationActive = false;
                        Configuration.ConfigurationDelete = false;
                        Configuration.ConfigurationOn = false;
                        Configuration.CreateDate = DateTime.Now;
                        Configuration.CompanyId = com.SysCompanyID;
                        Configuration.CreateUserId = createuserid;

                        Configurations.Add(Configuration);
                    }
                    _unitofwork.ConfigurationRepository.BulkInsert(Configurations);
                    _unitofwork.Save();
                   
                }

                return 1;         
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateCompany(SysCompany com)
        {
            try
            {
                _unitofwork.CompanyRepository.Update(com);
               return  _unitofwork.Save();
                
            }
            catch (Exception ex)
            {
                return 0;
            }
        }


        public SysCompany GetCompanyByCode(string code)
        {
            try
            {
                SysCompany company = _unitofwork.CompanyRepository.Get(g => g.CompanyCode == code).FirstOrDefault();
                if (company != null)
                {
                    return company;
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
