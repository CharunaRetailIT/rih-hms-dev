using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class EmployeeGroup : BaseEntity
    {
        public int EmployeeGroupID { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        [DataType(DataType.Text)]
        [DefaultValue(0)]
        [MaxLength(15)]
        public string EmployeeGroupCode { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DataType(DataType.Text)]
        [DefaultValue(0)]      
        [MaxLength(50)]
        public string EmployeeGroupName { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }

    }
}