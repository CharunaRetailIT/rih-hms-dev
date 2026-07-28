using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class EmployeeGroupService
    {


        ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<EmployeeGroup> GetEmployeeGroups()
        {
            try
            {
                IEnumerable<EmployeeGroup> employeegroup = context.EmployeeGroup.OrderBy(eg => eg.EmployeeGroupCode);
                if (employeegroup != null)
                {
                    return employeegroup;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<EmployeeGroup> GetActiveEmployeeGroups()
        {
            try
            {
                IEnumerable<EmployeeGroup> employeegroup = context.EmployeeGroup.Where(eg => eg.IsDelete == false).OrderBy(eg => eg.EmployeeGroupCode);
                if (employeegroup != null)
                {
                    return employeegroup;

                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public EmployeeGroup GetEmployeeGroupById(long id)
        {
            try
            {
                EmployeeGroup employeegroup = context.EmployeeGroup.Where(eg => eg.EmployeeGroupID == id).FirstOrDefault();
                if (employeegroup != null)
                {
                    return employeegroup;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int SaveEmployeeGroup(EmployeeGroup eg)
        {
            try
            {
                context.EmployeeGroup.Add(eg);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateEmployeeGroup(EmployeeGroup eg)
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

        public EmployeeGroup GetEmpGroupByCode(string code)
        {
            try
            {
                EmployeeGroup empgroup = context.EmployeeGroup.Where(g => g.EmployeeGroupCode == code).FirstOrDefault();
                if (empgroup != null)
                {
                    return empgroup;
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