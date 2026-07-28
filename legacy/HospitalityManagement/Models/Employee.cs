using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class Employee : BaseEntity
    {

        public long EmployeeID { get; set; }

        [Required]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        [MaxLength(15)]
        public string EmployeeCode { get; set; }

        [Required(ErrorMessage = "The field is required")]
        //[DataType(DataType.Text)]
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        //[MinLength(2, ErrorMessage = "Title Required")]
        //[DefaultValue("")]
        public string EmployeeTitle { get; set; }

        [Required]
        [MaxLength(100)]
        public string EmployeeName { get; set; }

        [Required]
        [MaxLength(100)]
        public string Address1 { get; set; }

        [Required]
        [MaxLength(100)]
        public string Address2 { get; set; }

        [DefaultValue("")]
        [MaxLength(100)]
        public string Address3 { get; set; }

        public DateTime DOB { get; set; }

        [Required]
        public string Gender { get; set; }

        [MaxLength(100)]
        public string Designation { get; set; }

        [DefaultValue("")]
        public byte[] EmployeePicture { get; set; }

        [DefaultValue("")]
        public String EmployeePictureName { get; set; }
        [DefaultValue("")]
        public String EmployeePictureType { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [MaxLength(12, ErrorMessage = "NIC should less than 12 charactors"), MinLength(10, ErrorMessage = "Invalid NIC")]
        public string NIC { get; set; }

        public string Passport { get; set; }

        [DefaultValue("")]
        public string Telephone { get; set; }

        [Required]
        public string Mobile { get; set; }

        [DefaultValue("")]
        public string Email { get; set; }

        [DefaultValue("")]
        [MaxLength(30)]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Department !")]
        public string DepartmentID { get; set; }

        [DefaultValue(0)]
        public bool IsActive { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }

        [DefaultValue(null)]
        [NotMapped]
        public HttpPostedFileBase Photograph { get; set; }

        [NotMapped]
        public string LocationName { get; set; }

    }
}