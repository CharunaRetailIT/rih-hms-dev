using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class InterDepartmentService
    {


        ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<InterDepartment> GetInterDepartments()
        {
            try
            {
                IEnumerable<InterDepartment> interdepartment = context.InterDepartment.OrderBy(g => g.InterDepartmentCode);
                if (interdepartment != null)
                {
                    return interdepartment;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<InterDepartment> GetByCompanyId(long id)
        {
            try
            {
                IEnumerable<InterDepartment> interdepartment = context.InterDepartment.Where(g => g.CompanyID == id).OrderBy(g => g.InterDepartmentCode);
                if (interdepartment != null)
                {
                    return interdepartment;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<InterDepartment> GetActiveInterDepartments()
        {
            try
            {
                IEnumerable<InterDepartment> interdeparment = context.InterDepartment.Where(g => g.IsActive == true).OrderBy(g => g.InterDepartmentCode);
                if (interdeparment != null)
                {
                    return interdeparment;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public InterDepartment GetInterDepartmentById(long id)
        {
            try
            {
                InterDepartment interdepartment = context.InterDepartment.Where(g => g.InterDepartmentId == id).FirstOrDefault();
                if (interdepartment != null)
                {
                    return interdepartment;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int SaveInterDepartment(InterDepartment interdept)
        {
            try
            {
                context.InterDepartment.Add(interdept);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateInterDepatment(InterDepartment interdept)
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


        public InterDepartment GetInterDeptByCode(string code)
        {
            try
            {
                InterDepartment interdept = context.InterDepartment.Where(g => g.InterDepartmentCode == code).FirstOrDefault();
                if (interdept != null)
                {
                    return interdept;
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