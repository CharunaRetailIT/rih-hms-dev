using RIT.HMS.HMSOrderTaker.Data;
using RIT.HMS.HMSOrderTaker.Domain;
using RIT.HMS.HMSOrderTaker.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.HMSOrderTaker.BLL.Masters
{
    public class BLL_Department
    {
        private UnitOfWork<SmartLinkEntities> unitOfWork;
        public BLL_Department()
        {
            unitOfWork = new UnitOfWork<SmartLinkEntities>();

        }
        public IEnumerable<DTO_Department> GetActiveDepartmentsByLocationId( int locationid)
        {
            var departments = unitOfWork.Tbl_RstDepartment.Get(filter:l=>
                                 l.IsActive == true && l.IsDelete == false && l.LocationId==locationid
                                ).OrderBy(l => l.DepartmentName);

            List<DTO_Department> objdepts = new List<DTO_Department>();
            foreach (var dept in departments)
            {
                DTO_Department objdept = new DTO_Department()
                {
                    RstDepartmentID = dept.RstDepartmentID,
                    DepartmentCode = dept.DepartmentCode,
                    DepartmentName=dept.DepartmentName,
                    DeptImage=dept.DeptImage,
                    DeptImageName=dept.DeptImageName,
                    DeptImageType=dept.DeptImageType,
                                        
                };
                objdepts.Add(objdept);

            }

            return objdepts;

        }
    }
}
