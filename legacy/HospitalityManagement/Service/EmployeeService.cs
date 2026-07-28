using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace HospitalityManagement.Service
{
    public class EmployeeService
    {
        ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<Employee> GetEmployees()
        {
            try
            {
                IEnumerable<Employee> employee = context.Employees.Where(e=>e.IsDelete==false).OrderBy(e => e.EmployeeID);
                if (employee != null)
                {
                    return employee;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<Employee> GetActiveEmployees()
        {
            try
            {
                IEnumerable<Employee> employee = context.Employees.Where(e => e.IsDelete == false && e.IsActive == true).OrderBy(e => e.EmployeeCode);
                if (employee != null)
                {
                    return employee;

                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public Employee GetEmployeeById(long id)
        {
            try
            {
                Employee employee = context.Employees.Where(e => e.EmployeeID == id).FirstOrDefault();
                if (employee != null)
                {
                    return employee;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int SaveEmployee(Employee e)
        {
            try
            {
                context.Employees.Add(e);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateEmployee(Employee e)
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


        public Employee GetEmployeeByCode(string code)
        {
            try
            {
                Employee employee = context.Employees.Where(g => g.EmployeeCode == code).FirstOrDefault();
                if (employee != null)
                {
                    return employee;
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