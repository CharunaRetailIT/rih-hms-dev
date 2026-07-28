using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models.DeliveryModule
{
    public class DeliveryModuleCustomer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        [DataType(DataType.Text)]
        [DefaultValue(0)]
        public string CustomerCode { get; set; }


        [Required(ErrorMessage = "The field is required")]

        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        //[MinLength(2, ErrorMessage = "Title Required")]
        //[DefaultValue("")]
        public string CustomerTitle { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DefaultValue("")]
        public string CustomerName { get; set; }

        [DefaultValue("")]
        public string CustomerType { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DefaultValue("")]
        public string BillingAddress1 { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DefaultValue("")]
        public string BillingAddress2 { get; set; }

        [DefaultValue("")]
        public string BillingAddress3 { get; set; }

        [Required]
        public DateTime DOB { get; set; }
        [Required(ErrorMessage = "The field is required")]
        [MaxLength(12, ErrorMessage = "NIC should less than 12 charactors"), MinLength(10, ErrorMessage = "Invalid NIC")]

        public string NIC { get; set; }
        public string Passport { get; set; }
        public string Telephone { get; set; }
        public string Mobile { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string VehicleNo { get; set; }
        public string Profession { get; set; }
        public DateTime? WeddingAnniversary { get; set; }

        [DefaultValue(0)]
        public bool IsActiveForLoyalty { get; set; }

        [DefaultValue("")]
        public byte[] CustomerPicture { get; set; }

        [DefaultValue("")]
        public String CustomerPictureName { get; set; }

        [DefaultValue("")]
        public String CustomerPictureType { get; set; }

        [DefaultValue(0)]
        public bool IsActive { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }

        [NotMapped]
        [DefaultValue(null)]
        public HttpPostedFileBase Photograph { get; set; }

        [DefaultValue(0)]
        public decimal CreditLimit { get; set; }
        [DefaultValue(0)]
        public decimal Outstanding { get; set; }
    }
}