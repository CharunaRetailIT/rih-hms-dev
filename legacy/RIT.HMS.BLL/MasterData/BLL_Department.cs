using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_Department
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_Department()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_Department(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }

        public IEnumerable<RstDepartment> GetDepartments(Int32 compid)
        {
            try
            {
                IEnumerable<RstDepartment> sysdepartment = _unitofwork.DepartmentRepository.Get(d => d.IsDelete == false && d.CompanyID== compid).
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
      
        public IEnumerable<RstDepartment> GetActiveDepartments(Int32 compid)
        {
            try
            {
                var dept = _unitofwork.DepartmentRepository.Get(g => g.IsDelete == false && g.IsActive == true &&g.CompanyID==compid).OrderBy(g => g.DepartmentCode)
                    .Select(x => new { x.RstDepartmentID, x.DepartmentName, x.IsActive, x.IsDelete, x.DepartmentCode }).ToList();

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

        public IEnumerable<RstDepartment> GetActiveDepartmentsByLocationId(Int32 compid, Int32 locationid)
        {
            try
            {
                var dept = _unitofwork.DepartmentRepository.Get(g => g.IsDelete == false && g.IsActive == true && g.CompanyID == compid && g.LocationId==locationid).OrderBy(g => g.DepartmentCode)
                    .Select(x => new { x.RstDepartmentID, x.DepartmentName, x.IsActive, x.IsDelete, x.DepartmentCode }).ToList();

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
                RstDepartment syscompany = _unitofwork.DepartmentRepository.Get(g => g.RstDepartmentID == id).FirstOrDefault();
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

        public RstDepartment GetDeptByCode(string code,Int32 compid)
        {
            try
            {
                RstDepartment dept = _unitofwork.DepartmentRepository.Get(g => g.DepartmentCode == code && g.CompanyID==compid).FirstOrDefault();
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


        public InterDepartment GetInterDepartmentById(long id)
        {
            try
            {
                InterDepartment interdepartments = _unitofwork.InterDepartmentRepository.Get(g => g.InterDepartmentId == id).FirstOrDefault();
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
                _unitofwork.DepartmentRepository.Insert(dept);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateDepatment(RstDepartment dept)
        {
            try
            {
                _unitofwork.DepartmentRepository.Update(dept);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }


        //Added by pavithra on 2019-11-30
        public RstDepartment FindByCode(string code,Int32 compid)
        {
            var department = _unitofwork.DepartmentRepository.Get(c => c.DepartmentCode == code && c.CompanyID==compid).FirstOrDefault();
            if (department != null)
            {
                return department;
            }
            else
            {
                return null;
            }

        }

    }
}
