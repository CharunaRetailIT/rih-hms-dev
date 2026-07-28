using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class DeliveryPerson : BaseEntity
    {

        public long DeliveryPersonId { get; set; }
        [Required]
        [MaxLength(15)]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        public string EmployeeId { get; set; }
        [Required(ErrorMessage = "The field is required")]
        [DataType(DataType.Text)]  
        [MinLength(2, ErrorMessage = "Title Required")]
        [DefaultValue("")]
        public string Title { get; set; }
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }
        [Required]
        [MaxLength(200)]
        public string Address { get; set; }
        public DateTime DOB { get; set; }
       
        [MaxLength(100)]
        public string Designation { get; set; }
        [DefaultValue("")]
        public byte[] Picture { get; set; }
        [DefaultValue("")]
        public String PictureName { get; set; }
        [DefaultValue("")]
        public String PictureType { get; set; }
        [Required(ErrorMessage = "The field is required")]
        [MaxLength(12, ErrorMessage = "NIC should less than 12 charactors"), MinLength(10, ErrorMessage = "Invalid NIC")]
        public string NIC { get; set; }
        public string DrivingLicence { get; set; }
        [DefaultValue("")]
        public string Telephone { get; set; }
        [Required]
        public string Mobile { get; set; }
        [DefaultValue("")]
        public string Email { get; set; }

        [Required]
        [MaxLength(200)]
        public string InCaseOfEmergency { get; set; }


        [DefaultValue(true)]
        public bool IsActive { get; set; }
        [DefaultValue(0)]
        public bool IsDelete { get; set; }
        [DefaultValue(null)]
        [NotMapped]
        public HttpPostedFileBase Photograph { get; set; }

    }
}