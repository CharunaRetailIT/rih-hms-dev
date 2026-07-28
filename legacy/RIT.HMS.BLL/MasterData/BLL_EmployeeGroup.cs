using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_EmployeeGroup
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_EmployeeGroup()
        {
            _unitofwork = new UnitOfWork();
        }

        public BLL_EmployeeGroup(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }

        public IEnumerable<EmployeeGroup> GetEmployeeGroups(Int32 companyid)
        {
            try
            {
                //Below line commented and added new line by pavi on 2019-12-01
                //IEnumerable<EmployeeGroup> employeegroup = _unitofwork.EmployeeGroupRepository.Get().OrderBy(eg => eg.EmployeeGroupCode);
                IEnumerable<EmployeeGroup> employeegroup = _unitofwork.EmployeeGroupRepository.Get(c => c.IsDelete.Equals(false) && c.CompanyID== companyid).OrderBy(eg => eg.EmployeeGroupCode);
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
                IEnumerable<EmployeeGroup> employeegroup = _unitofwork.EmployeeGroupRepository.Get(eg => eg.IsDelete == false).OrderBy(eg => eg.EmployeeGroupCode);
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
                EmployeeGroup employeegroup = _unitofwork.EmployeeGroupRepository.Get(eg => eg.EmployeeGroupID == id).FirstOrDefault();
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

        public EmployeeGroup GetEmpGroupByCode(string code, Int32 companyid)
        {
            try
            {
                EmployeeGroup empgroup = _unitofwork.EmployeeGroupRepository.Get(g => g.EmployeeGroupCode == code && g.CompanyID== companyid).FirstOrDefault();
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

        public int SaveEmployeeGroup(EmployeeGroup em)
        {
            try
            {
                _unitofwork.EmployeeGroupRepository.Insert(em);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateEmployeeGroup(EmployeeGroup em)
        {
            try
            {
                _unitofwork.EmployeeGroupRepository.Update(em);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }





    }
}
