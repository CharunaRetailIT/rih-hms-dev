using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.HMSOrderTaker.Domain.DTOs
{
    public class DTO_Category
    {
        public int RstCategoryID { get; set; }
        public int RstDepartmentID { get; set; }
        public string RstCategoryCode { get; set; }
        public string RstCategoryName { get; set; }
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
        public byte[] CatImage { get; set; }
        public string CatImageName { get; set; }
        public string CatImageType { get; set; }
        [NotMapped]
        public List<DTO_Category> CategoryList { get; set; }

       // Dictionary<int, string> numberNames = new Dictionary<int, string>();
    }


  


}
