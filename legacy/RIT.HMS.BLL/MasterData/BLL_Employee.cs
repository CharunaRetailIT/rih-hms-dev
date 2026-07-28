using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_Employee
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_Employee()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_Employee(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }
        public IEnumerable<Employee> GetEmployees(Int32 compid)
        {
            try
            {
                IEnumerable<Employee> employee = _unitofwork.EmployeeRepository.Get(e => e.IsDelete == false && e.CompanyID==compid).OrderBy(e => e.EmployeeID);
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

        public IEnumerable<Employee> GetActiveEmployees(Int32 compid)
        {
            try
            {
                IEnumerable<Employee> employee = _unitofwork.EmployeeRepository.Get(e => e.IsDelete == false && e.IsActive == true && e.CompanyID==compid).OrderBy(e => e.EmployeeCode);
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
                Employee employee = _unitofwork.EmployeeRepository.Get(e => e.EmployeeID == id).FirstOrDefault();
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

        public Employee GetEmployeeByCode(string code,Int32 compid)
        {
            try
            {
                Employee employee = _unitofwork.EmployeeRepository.Get(g => g.EmployeeCode == code && g.CompanyID== compid).FirstOrDefault();
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

        public int SaveEmployee(Employee emp)
        {
            try
            {
                _unitofwork.EmployeeRepository.Insert(emp);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateEmployee(Employee emp)
        {
            try
            {
                _unitofwork.EmployeeRepository.Update(emp);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int CheckEpfNo(string Epf, string code)
        {
            try
            {
                Employee employee = _unitofwork.EmployeeRepository.Get(g => g.EmployeeCode != code && g.EpfNo == Epf).FirstOrDefault();
                if (employee != null)
                {
                    return 0;
                }
                else
                    return 1;
            }
            catch (Exception Ex)
            {
                throw;
            }
        }
    }
}
