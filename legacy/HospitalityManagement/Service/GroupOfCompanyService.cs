using HospitalityManagement.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class GroupOfCompanyService
    {

        ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<SysGroupOfCompany> GetGroupOfCompanies()
        {
            try
            {
                IEnumerable<SysGroupOfCompany> sysgroupofcompany = context.SysGroupOfCompanys.Where(g => g.IsDelete == false && g.IsActive == true).OrderBy(g => g.GroupOfCompanyCode);
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
                IEnumerable<SysGroupOfCompany> sysgroupofcompany = context.SysGroupOfCompanys.Where(g => g.IsDelete == false && g.IsActive == true).OrderBy(g => g.GroupOfCompanyCode);
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
                SysGroupOfCompany sysgroupofcompany = context.SysGroupOfCompanys.Where(g => g.SysGroupOfCompanyId == id).FirstOrDefault();
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

        public int SaveGroupOfCompany(SysGroupOfCompany goc)
        {
            try
            {
                context.SysGroupOfCompanys.Add(goc);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateGroupOfCompany(SysGroupOfCompany goc)
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


        public SysGroupOfCompany GetGOCByCode(string code)
        {
            try
            {
                SysGroupOfCompany goc = context.SysGroupOfCompanys.Where(g => g.GroupOfCompanyCode == code).FirstOrDefault();
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


    }
}