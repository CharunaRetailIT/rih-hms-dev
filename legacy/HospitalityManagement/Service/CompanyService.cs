using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class CompanyService
    {
        //
        ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<SysCompany> GetCompanies()
        {   
            try
            {
                IEnumerable<SysCompany> syscompany = context.SysCompanys.Where(g=>g.IsDelete == false && g.IsActive == true).OrderBy(g => g.CompanyCode);
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

        public IEnumerable<SysCompany> ShowCompanies()
        {
            try
            {
                IEnumerable<SysCompany> syscompany = context.SysCompanys.Where(g => g.IsDelete == false).OrderBy(g => g.CompanyCode);
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
        public IEnumerable<SysCompany> GetByGOCId(long id)
        {
            try
            {
                IEnumerable<SysCompany> syscompany = context.SysCompanys.Where(g => g.IsDelete == false && g.SysGroupOfCompanyId==id).OrderBy(g => g.CompanyCode);
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
                IEnumerable<SysCompany> syscompany = context.SysCompanys.Where(g => g.IsDelete == false).OrderBy(g => g.CompanyCode);
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
                SysCompany syscompany = context.SysCompanys.Where(g => g.SysCompanyID == id).FirstOrDefault();
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

        public int SaveCompany(SysCompany com)
        {
            try
            {
                context.SysCompanys.Add(com);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateCompany(SysCompany com)
        {
            try
            {

                //  ..context.SysGroupOfCompanys.Add(goc);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public SysCompany GetCompanyByCode(string code)
        {
            try
            {
                SysCompany company = context.SysCompanys.Where(g => g.CompanyCode == code).FirstOrDefault();
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