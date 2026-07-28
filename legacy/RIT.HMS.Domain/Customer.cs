using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class Customer : BaseEntity
    {
        [Key]
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        [DataType(DataType.Text)]
        [DefaultValue(0)]
        public string CustomerCode { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public string CustomerTitle { get; set; }

        //[Required(ErrorMessage = "This field is required")]
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        //[DefaultValue("")]
        public string CustomerName { get; set; }

        [DefaultValue("")]
        public string CustomerType { get; set; }

        [DefaultValue(0)]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Customer Group !")]
        public int CustomerCategoryId { get; set; }


        [Required(ErrorMessage = "This field is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DefaultValue("")]
        public string BillingAddress1 { get; set; }


        [Required(ErrorMessage = "This field is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DefaultValue("")]
        public string BillingAddress2 { get; set; }

        [DefaultValue("")]
        public string BillingAddress3 { get; set; }

     //   [Required]
        public DateTime? DOB { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [MaxLength(12, ErrorMessage = "NIC should less than 12 characters"), MinLength(10, ErrorMessage = "Invalid NIC")]
        public string NIC { get; set; }

        public string Passport { get; set; }

        [DefaultValue("")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Only Numbers allowed")]
        public string Telephone { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Only Numbers allowed")]
        public string Mobile { get; set; }

        public string Fax { get; set; }

        public string Email { get; set; }

        public string VehicleNo { get; set; }

        public string Profession { get; set; }

        public DateTime? WeddingAnniversary { get; set; }

        [DefaultValue(false)]
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

        [Column(TypeName = "VARCHAR")]
        [StringLength(50)]
        [DefaultValue("")]
        public string EPFNo { get; set; }


        [Column(TypeName = "VARCHAR")]
        [StringLength(50)]
        [DefaultValue("")]
        public string MembershipCardNo { get; set; }


        [Column(TypeName = "VARCHAR")]
        [StringLength(50)]
        [DefaultValue("")]
        public string Other { get; set; }


        [Column(TypeName = "VARCHAR")]
        [StringLength(200)]
        [DefaultValue("")]
        public string Remarks { get; set; }

    
       // [Range(1, int.MaxValue, ErrorMessage = "Please select a Customer Status !")]
        [Column(TypeName = "VARCHAR")]
        [StringLength(20)]
        [DefaultValue("")]
        public string CustomerStatus { get; set; }


       // - Taken From Loyalty Customer (ERP)--------------------------------------------

        [DefaultValue(0)]
        public int Gender { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(50)] 
        public string ReferenceNo1 { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(50)]    
        public string ReferenceNo2 { get; set; }

        [DefaultValue(0)]
        public int Age { get; set; }

        [DefaultValue(0)]
        public int? Religion { get; set; }

        [DefaultValue(0)]
        public int? Race { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        public string LandMark { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        public string District { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        public string Organization { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        public string WorkAddres1 { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        public string WorkAddres2 { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        public string WorkAddres3 { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        public string WorkEmail { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        public string WorkTelephone { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        public string WorkMobile { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        public string WorkFax { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        public string SpouseName { get; set; }

        [DefaultValue(0)]
        public int CivilStatus { get; set; }

        public DateTime? SpouseDateOfBirth { get; set; }

        [DefaultValue(0)]
        public int DeliverTo { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        public string DeliverToAddress { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        public string Country { get; set; }

        public DateTime? CustomerSince { get; set; }
        [DefaultValue(0)]
        public int SpecialDayType { get; set; }
        [DefaultValue(false)]
        public bool SendUpdatesViaEmail { get; set; }
        [DefaultValue(false)]
        public bool SendUpdatesViaSms { get; set; }

        [DefaultValue(false)]
        public bool IsRegByPOS { get; set; }

        [NotMapped]
        public string CardNumber { get; set; }

        [NotMapped]
        [DefaultValue(false)]
        public bool? IsCardValid { get; set; }

        [NotMapped]
        public string NameOnCard { get; set; }

        [NotMapped]
        public DateTime ExpiryDate { get; set; }

        [DefaultValue(0)]
        [Required(ErrorMessage = "This field is required")]
        public int SenderPreference { get; set; }
        //[DefaultValue(0)]
        //public bool IsAttachInvoiceForSMS { get; set; }


        [Required(ErrorMessage = "This field is required")]
        [StringLength(150, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DefaultValue("")]
      
       
       
        public string FirstName { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [StringLength(150, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DefaultValue("")]
        public string LastName { get; set; }



    }
}