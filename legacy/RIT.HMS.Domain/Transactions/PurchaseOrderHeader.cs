using RIT.HMS.Domain.Transactions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.Transactions
{
    public class PurchaseOrderHeader:BaseEntity
    {
        public PurchaseOrderHeader()
        {
            PODetail = new List<PurchaseOrderDetail>();
        }

        public long PurchaseOrderHeaderId { get; set; }

        [DefaultValue(0)]
        public long JobClassId { get; set; }

        [Required(ErrorMessage = "* ")]
        [DefaultValue("")]
        [MaxLength(20)]
        public string DocumentNo { get; set; }
      
        [DefaultValue("0")]       
        public int DocumentId { get; set; }


        [Required(ErrorMessage = "* ")]
        public DateTime DocumentDate { get; set; }


        [Range(1, int.MaxValue, ErrorMessage = "Please select a Supplier !")]
        [Required(ErrorMessage = "* ")]
        [DefaultValue(0)]
        public long SupplierId { get; set; }

        [Required(ErrorMessage = "* ")]
        public DateTime ExpectedDate { get; set; }
        [Required(ErrorMessage = "* ")]
        public DateTime ExpiryDate { get; set; }


        [Required(ErrorMessage = "* ")]
        public DateTime PaymentExpectedDate { get; set; }


        [Required(ErrorMessage = "* ")]
        [DefaultValue(0)]
        public int ValidityPeriod { get; set; }

        [Required(ErrorMessage = "* ")]
        [DefaultValue(0)]
        public decimal GrossAmount { get; set; }
        [Required(ErrorMessage = "* ")]
        [DefaultValue(0)]

        public decimal OtherCharges { get; set; }
        [Required(ErrorMessage = "* ")]
        [DefaultValue(0)]

        public decimal DiscountAmount { get; set; }
        [Required(ErrorMessage = "* ")]
        [DefaultValue(0)]
        public decimal DiscountPercentage { get; set; }


        [Required(ErrorMessage = "* ")]
        [DefaultValue(0)]
        public decimal TotalTaxAmount { get; set; }


        [DefaultValue(0)]
        public decimal Addition { get; set; }

        [DefaultValue(0)]
        public decimal Deduction { get; set; }

        [Required(ErrorMessage = "* ")]
        [DefaultValue(0)]

        public decimal NetAmount { get; set; }

        [DefaultValue("")]

        [MaxLength(50)]

        public string RequestedBy { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a Delivery Location !")]
        [Required(ErrorMessage = "* ")]
        [DefaultValue(0)]
        public int DeliveryLocationId { get; set; }

        [Required(ErrorMessage = "* ")]
        [DefaultValue(0)]
        public decimal LineDiscountTotal { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a Payment Term !")]
        [Required(ErrorMessage = "* ")]
        [DefaultValue(0)]
        public int PaymentTermId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a Payment Method !")]
        [Required(ErrorMessage = "* ")]
        [DefaultValue(0)]
        public int PaymentMethodId { get; set; }

        [Required(ErrorMessage = "* ")]
        [DefaultValue(0)]
        public int PaymentPeriod { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a Currency Type !")]
        [Required(ErrorMessage = "* ")]
        [DefaultValue(0)]
        public int CurrencyId { get; set; }


        [DefaultValue(0)]
        public decimal CurrencyRate { get; set; }

        [DefaultValue("")]

        [MaxLength(500)]

        public string DeliveryDetail { get; set; }

        [DefaultValue("")]
        [MaxLength(20)]
        public string ReferenceNo { get; set; }

        [DefaultValue("")]
        [MaxLength(150)]
        public string Remark { get; set; }
        [DefaultValue(0)]
        public int DocumentStatus { get; set; }


        [DefaultValue("")]
        [MaxLength(50)]

        public string LastAuthorizedBy { get; set; }

        public DateTime? LastAuthorizedDate { get; set; }

        public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetail { get; set; }

        [NotMapped]
        public List<PurchaseOrderDetail> PODetail { get; set; }

        //  [NotMapped]
        public decimal TotSellingPrice { get; set; }
        //  [NotMapped]
        public decimal TotCostPrice { get; set; }

        //  [NotMapped]
        public decimal TotDiscounts { get; set; }

        public string POType { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a PO Location !")]
        public long POLocationId { get; set; }

        public string TempDocNumber { get; set; }

        [DefaultValue(false)]
        public bool IsTempPO { get; set; }


        //[NotMapped]
        [DefaultValue(false)]
        public bool IsGRN { get; set; }

        [NotMapped]
        public string Company { get; set; }

        [NotMapped]
        public string Location { get; set; }

        [NotMapped]
        public virtual SysCompany SysCompany { get; set; }

        [NotMapped]
        public virtual Supplier Supplier { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }

        [DefaultValue("")]
        public string DeliveryAddress { get; set; } 
        public DateTime PODate { get; set; }

        [NotMapped]
        public string SupplierName { get; set; }

        [NotMapped]
        public int PrintStatus { get; set; }

        [NotMapped]

        public string DocState { get; set; }

    //    [NotMapped]
        public int? EventId { get; set; }

        [DefaultValue("")]
        [MaxLength(20)]
        public string NewDocNumber { get; set; }

       // [NotMapped]
        [DefaultValue("")]
        [MaxLength(200)]
        public string RejectReason { get; set; }

      //  [NotMapped]
        [DefaultValue("")]
        [MaxLength(200)]
        public string CancelReason { get; set; }

        [NotMapped]
        public DateTime PODateFrom { get; set; }
        [NotMapped]
        public DateTime PODateTo { get; set; }

        [NotMapped]
       // [Range(1, int.MaxValue, ErrorMessage = "Please select a Doc Status !")]
        public int DocStatusId { get; set; }

        [NotMapped]
        public string PaymentTerm { get; set; }

        [NotMapped]
        public string POLocationAddress { get; set; }

        [DefaultValue(0)]
        public int  RequestNoteId { get; set; }

        [NotMapped]
        public int ReferanceDocId { get; set; }

        [NotMapped]

        public string ReqDocNo { get; set; }

        [NotMapped]

        public int ReqFromLocation { get; set; }
    }
}