using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class Supplier : BaseEntity
    {
        public long SupplierID { get; set; }

        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        [Required]
        [DefaultValue("")]
        [MaxLength(15)]
        public string SupplierCode { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DataType(DataType.Text)]
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [MinLength(2, ErrorMessage = "Title Required")]
        [DefaultValue("")]
        public string SupplierTitle { get; set; }



        [Required]
        [DefaultValue("")]
        [MaxLength(100)]
        public string SupplierName { get; set; }

        [Required]
        public string Gender { get; set; }


        [Range(1, int.MaxValue, ErrorMessage = "Please select a supplier type !")]        
        [DefaultValue(0)]
        public int SupplierTypeID { get; set; }



        [DefaultValue("")]
        [MaxLength(100)]
        public string ContactPersonName { get; set; }



        [DefaultValue("")]
        [MaxLength(250)]
        public string BillingAddress1 { get; set; }



        [DefaultValue("")]
        [MaxLength(100)]
        public string BillingAddress2 { get; set; }



        [DefaultValue("")]
        [MaxLength(100)]
        public string BillingAddress3 { get; set; }



        [Required]
        [DefaultValue("")]
        [MaxLength(50)]
        public string BillingTelephone { get; set; }



        [DefaultValue("")]
        [MaxLength(50)]
        public string BillingMobile { get; set; }



        [DefaultValue("")]
        [MaxLength(50)]
        public string BillingFax { get; set; }



        [DefaultValue("")]
        [MaxLength(100)]
        //[EmailAddress]
        public string Email { get; set; }



        [MaxLength(100)]
        [DefaultValue("")]
        public string RepresentativeName { get; set; }



        [MaxLength(50)]
        [DefaultValue("")]
        public string RepresentativeNICNo { get; set; }



        // [Required]
        [MaxLength(100)]
        [DefaultValue("")]
        public string PayeeName { get; set; }



        [DefaultValue("")]
        [MaxLength(50)]
        public string DeliveryAddress1 { get; set; }



        [DefaultValue("")]
        [MaxLength(50)]
        public string DeliveryAddress2 { get; set; }


        [DefaultValue("")]
        [MaxLength(50)]
        public string DeliveryAddress3 { get; set; }

        [DefaultValue("")]
        [MaxLength(50)]
        public string DeliveryTelephone { get; set; }


        [DefaultValue("")]
        [MaxLength(50)]
        public string DeliveryMobile { get; set; }

        [DefaultValue("")]
        [MaxLength(50)]
        public string DeliveryFax { get; set; }



        [DefaultValue("")]
        public byte[] SupplierPicture { get; set; }



        [DefaultValue("")]
        public String SupplierPictureName { get; set; }

        [DefaultValue("")]
        public String SupplierPictureType { get; set; }



        [DefaultValue("")]
        [MaxLength(20)]
        public string ReferenceNo { get; set; }



        [DefaultValue("")]
        [MaxLength(20)]
        public string ReferenceSerial { get; set; }



        [DefaultValue("")]
        [MaxLength(20)]
        public string PostalCode { get; set; }


        
        public int TaxID1 { get; set; }



        [DefaultValue("")]
        [MaxLength(25)]
        public string TaxNo1 { get; set; }


        
        public int TaxID2 { get; set; }



        [DefaultValue("")]
        [MaxLength(25)]
        public string TaxNo2 { get; set; }


        
        public int TaxID3 { get; set; }

        [DefaultValue("")]
        [MaxLength(25)]
        public string TaxNo3 { get; set; }

        
        public int TaxID4 { get; set; }

        [DefaultValue("")]
        [MaxLength(25)]
        public string TaxNo4 { get; set; }

       
        public int TaxID5 { get; set; }



        [DefaultValue("")]
        [MaxLength(25)]
        public string TaxNo5 { get; set; }


        [DefaultValue("")]
        [MaxLength(50)]

        public string TaxRegistrationNo { get; set; }


        [DefaultValue("")]
        [MaxLength(100)]

        public string TaxRegistrationName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a payment method!")]
        [DefaultValue(0)]
        public int PaymentMethod { get; set; }

        [DefaultValue(0)]
        public decimal CreditLimit { get; set; }

        [DefaultValue(0)]
        public decimal ChequeLimit { get; set; }

        [DefaultValue(0)]
        public int ChequePeriod { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a payment term!")]
        [DefaultValue(0)]
        public int PaymentTermID { get; set; }

        [DefaultValue(0)]
        public int CreditPeriod { get; set; }

        [DefaultValue("")]
        [MaxLength(200)]
        public string ProductBusinessType { get; set; }

        [DefaultValue("")]
        [MaxLength(200)]
        public string SuppliedProducts { get; set; }

        [DefaultValue(0)]
        public int OrderCircle { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a supplier group !")]
        [DefaultValue(0)]
        [MaxLength(30)]
        public string SupplierGroupID { get; set; }

        //  [DefaultValue(0)]
        public long LedgerID { get; set; }

        //   [DefaultValue(0)]

        public long OtherLedgerID { get; set; }

        [DefaultValue("")]
        [MaxLength(50)]
        public string TaxIdNo { get; set; }

        [DefaultValue(0)]
        public decimal DepositeAmount { get; set; }

        [DefaultValue("")]
        [MaxLength(100)]
        public string EmailBoday { get; set; }

        [DefaultValue("")]
        [MaxLength(100)]
        public string EmailSubject { get; set; }

        [DefaultValue("")]
        [MaxLength(100)]
        public string Remark { get; set; }

        [DefaultValue(0)]
        public bool IsUpload { get; set; }

        [DefaultValue(0)]
        public bool IsSuspended { get; set; } // not allow to do transactions, view only

        [DefaultValue(0)]
        public bool IsPOMail { get; set; }

        [DefaultValue(0)]
        public bool IsBlocked { get; set; } // not allow to do transactions, view only
        
        [DefaultValue(0)]
        public bool IsDelete { get; set; }

        [DefaultValue(null)]
        [NotMapped]
        public HttpPostedFileBase Photograph { get; set; }
    }
}