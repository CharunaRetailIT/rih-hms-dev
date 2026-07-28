using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_InterDepartment
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_InterDepartment()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_InterDepartment(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }
        public IEnumerable<InterDepartment> GetInterDepartments(Int32 compid)
        {
            try
            {
                IEnumerable<InterDepartment> interdepartment = _unitofwork.InterDepartmentRepository.Get(g=> g.CompanyID == compid).OrderBy(g => g.InterDepartmentCode);
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
                IEnumerable<InterDepartment> interdepartment = _unitofwork.InterDepartmentRepository.Get(g => g.CompanyID == id).OrderBy(g => g.InterDepartmentCode);
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

        public IEnumerable<InterDepartment> GetActiveInterDepartments(Int32 compid)
        {
            try
            {
                IEnumerable<InterDepartment> interdeparment = _unitofwork.InterDepartmentRepository.Get(g => g.IsActive == true && g.CompanyID==compid).OrderBy(g => g.InterDepartmentCode);
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
                InterDepartment interdepartment = _unitofwork.InterDepartmentRepository.Get(g => g.InterDepartmentId == id).FirstOrDefault();
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


        public InterDepartment GetInterDeptByCode(string code,Int32 compid)
        {
            try
            {
                InterDepartment interdept = _unitofwork.InterDepartmentRepository.Get(g => g.InterDepartmentCode == code && g.CompanyID==compid).FirstOrDefault();
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


        public int SaveInterDepartment(InterDepartment d)
        {
            try
            {
                _unitofwork.InterDepartmentRepository.Insert(d);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateInterDepartment(InterDepartment d)
        {
            try
            {
                _unitofwork.InterDepartmentRepository.Update(d);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }


        public IEnumerable<InterDepartment> GetInterDepartmentsByLocationId(int locationid)
        {
            try
            {
                IEnumerable<InterDepartment> interdepartment = _unitofwork.InterDepartmentRepository.Get(i=>i.InterDeptLocId==locationid && i.IsActive==true).OrderBy(g => g.InterDepartmentCode);
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



    }
}
