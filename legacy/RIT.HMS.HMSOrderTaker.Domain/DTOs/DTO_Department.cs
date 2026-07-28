using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.HMSOrderTaker.Domain.DTOs
{
    public class DTO_Department
    {
        public int RstDepartmentID { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public string Remark { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public int GroupOfCompanyID { get; set; }
        public int CompanyID { get; set; }
        public int LocationId { get; set; }
        public string CreatedUser { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public int DataTransfer { get; set; }
        public byte[] DeptImage { get; set; }
        public string DeptImageName { get; set; }
        public string DeptImageType { get; set; }
        public string DashBoardColor { get; set; }
        [NotMapped]
        public List<DTO_Department> DepartmentList { get; set; }
    }
}

