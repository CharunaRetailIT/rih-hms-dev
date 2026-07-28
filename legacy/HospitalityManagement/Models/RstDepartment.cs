using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class RstDepartment : BaseEntity
    {

        public int RstDepartmentID { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        [DefaultValue(0)]
        public string DepartmentCode { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DefaultValue("")]
        public string DepartmentName { get; set; }

        [DefaultValue("")]
        public string Remark { get; set; }

        [DefaultValue(0)]
        public bool IsActive { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }


        [DefaultValue(null)]
        [NotMapped]
        public HttpPostedFileBase Photograph { get; set; }

       
        [DefaultValue("")]
        public byte[] DeptImage { get; set; }

    
        [DefaultValue("")]
        public string DeptImageName { get; set; }

      
        [DefaultValue("")]
        public string DeptImageType { get; set; }


    }
}