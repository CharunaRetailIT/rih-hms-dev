using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class InterDepartment:BaseEntity
    {
        [Key]

        public long InterDepartmentId  { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        [DefaultValue(0)]
        public string InterDepartmentCode { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DefaultValue("")]
        public string InterDepartmentName { get; set; }
        public long InterDeptLocId { get; set; }

        [DefaultValue("")]
        public string Remark { get; set; }
        public bool IsActive { get; set; }

    }
}