using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class DepartmentService
    {
        ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<RstDepartment> GetDepartments()
        {
            try
            {
                IEnumerable<RstDepartment> sysdepartment = context.RstDepartment.Where(d=>d.IsDelete==false).
                                                                                       OrderBy(g => g.DepartmentCode);
                if (sysdepartment != null)
                {
                    return sysdepartment;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        //public IEnumerable<RstDepartment> GetByCompanyId(long id)
        //{
        //    try
        //    {
        //        IEnumerable<RstDepartment> sysdepartment = context.RstDepartment.Where(g => g.IsDelete == false && g.CompanyID == id).OrderBy(g => g.DepartmentCode);
        //        if (sysdepartment != null)
        //        {
        //            return sysdepartment;
        //        }
        //        else
        //            return null;
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //}
        public  IEnumerable<RstDepartment> GetActiveDepartments()
        {
            try
            {
               // IEnumerable<RstDepartment> sysdeparment = context.RstDepartment.Where(g => g.IsDelete == false
                    //                                                                  && g.IsActive == true).OrderBy(g => g.DepartmentCode);

                // return sysdeparment;
                var dept = context.RstDepartment.Select(x => new
                { x.RstDepartmentID, x.DepartmentName, x.IsActive, x.IsDelete, x.DepartmentCode }
                                                   ).Where(g => g.IsDelete == false && g.IsActive == true).OrderBy(g => g.DepartmentCode).ToList();

                List<RstDepartment> deptlist = new List<RstDepartment>();
                foreach (var d in dept)
                {
                    RstDepartment dp = new RstDepartment();
                    dp.RstDepartmentID = d.RstDepartmentID;
                    dp.DepartmentName = d.DepartmentName;
                    dp.DepartmentCode = d.DepartmentCode;
                    dp.IsActive = d.IsActive;
                    dp.IsDelete = d.IsDelete;
                    deptlist.Add(dp);
                }


                if (deptlist != null)
                {
                    return deptlist;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public RstDepartment GetDepartmentById(long id)
        {
            try
            {
                RstDepartment syscompany = context.RstDepartment.Where(g => g.RstDepartmentID == id).FirstOrDefault();
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

        public InterDepartment GetInterDepartmentById(long id)
        {
            try
            {
                InterDepartment interdepartments = context.InterDepartment.Where(g => g.InterDepartmentId == id).FirstOrDefault();
                if (interdepartments != null)
                {
                    return interdepartments;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int SaveDepartment(RstDepartment dept)
        {
            try
            {
                context.RstDepartment.Add(dept);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateDepatment(RstDepartment dept)
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


        public RstDepartment GetDeptByCode(string code)
        {
            try
            {
                RstDepartment dept = context.RstDepartment.Where(g => g.DepartmentCode == code).FirstOrDefault();
                if (dept != null)
                {
                    return dept;
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