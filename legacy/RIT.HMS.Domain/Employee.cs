using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class Employee : BaseEntity
    {

        [NotMapped]
        public bool EnableEmpoyeeMandoryField { get; set; }

        public long EmployeeID { get; set; }

        [Required]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        [MaxLength(15)]
        [DefaultValue("")]
        public string EmployeeCode { get; set; }


        [ConditionalRequired("EnableEmpoyeeMandoryField", true, ErrorMessage = "The field is required")]
       
        [MaxLength(50)]
        [DefaultValue("")]
        public string EpfNo { get; set; }

      
        //[DataType(DataType.Text)]
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        //[MinLength(2, ErrorMessage = "Title Required")]
        //[DefaultValue("")]
        public string EmployeeTitle { get; set; }
        [DefaultValue("")]
        [Required]
        [MaxLength(100)]
        public string EmployeeName { get; set; }


        [ConditionalRequired("EnableEmpoyeeMandoryField", true, ErrorMessage = "The field is required")]
        [MaxLength(100)]
        [DefaultValue("")]
        public string Address1 { get; set; }


        [ConditionalRequired("EnableEmpoyeeMandoryField", true, ErrorMessage = "The field is required")]
       
        [MaxLength(100)]
        [DefaultValue("")]
        public string Address2 { get; set; }

        [DefaultValue("")]
        [MaxLength(100)]
        public string Address3 { get; set; }

        [ConditionalRequired("EnableEmpoyeeMandoryField", true, ErrorMessage = "The field is required")]
        public DateTime DOB { get; set; } = DateTime.Parse("1999-09-29 00:00:00.000");

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



        [ConditionalRequired("EnableEmpoyeeMandoryField", true, ErrorMessage = "The NIC field is required")]
        [MaxLength(12, ErrorMessage = "NIC should be less than 12 characters")]
        //[MinLength(10, ErrorMessage = "Invalid NIC")]
        [DefaultValue("")]
        public string NIC { get; set; }

        public string Passport { get; set; }

        [DefaultValue("")]
        [RegularExpression(@"^[0]{1}[0-9]{9}$",
                   ErrorMessage = "Entered Telephone Number format is not valid.")]
        public string Telephone { get; set; }


        [ConditionalRequired("EnableEmpoyeeMandoryField", true, ErrorMessage = "The field is required")]
        [RegularExpression(@"^[0]{1}[0-9]{9}$",
                   ErrorMessage = "Entered Mobile Number format is not valid.")]
        public string Mobile { get; set; }

        [DefaultValue("")]
        public string Email { get; set; }
        
        [DefaultValue(0)]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Department !")]
        public int DepartmentID { get; set; }

        [DefaultValue(0)]       
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Employee Group !")]
        public int EmployeeGroupID { get; set; }

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