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
    public class PurchaseHeader : BaseEntity
    {
        public PurchaseHeader()
        {

            GRNDetail = new List<PurchaseDetail>();

        }

        public long PurchaseHeaderId { get; set; }
        [DefaultValue(0)]
        public int CostCentreID { get; set; }
        [DefaultValue(0)]
        public int DocumentID { get; set; }
        [Required(ErrorMessage = "* ")]
        [DefaultValue("")]
        [MaxLength(20)]
        public string DocumentNo { get; set; }
        [Required(ErrorMessage = "* ")]
        public DateTime DocumentDate { get; set; }
        [Required(ErrorMessage = "Supplier is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Supplier !")]
        [DataType(DataType.Text)]
        [DefaultValue(0)]
        public long SupplierID { get; set; }
        [Required(ErrorMessage = "* ")]
        [DefaultValue(0)]
        public decimal GrossAmount { get; set; }
        [Required(ErrorMessage = "* ")]
        [DefaultValue(0)]
        public decimal DiscountAmount { get; set; }
        [DefaultValue(0)]
        public decimal OtherChargers { get; set; }
        [DefaultValue(0)]
        public decimal DiscountPercentage { get; set; }
        [Required(ErrorMessage = "* ")]
        [DefaultValue(0)]
        public decimal TotalTax { get; set; }
        [DefaultValue(0)]
        public decimal NetAmount { get; set; }
        [MaxLength(50)]
        public string BatchNo { get; set; }

        [DefaultValue("")]
        //[MaxLength(150)]
        //[Required(ErrorMessage ="Return Reason is required")]
        public string Remark { get; set; }
        [DefaultValue(0)]
        public decimal LineDiscountTotal { get; set; }       
        [DefaultValue(0)]
        public int PaymentTermID { get; set; }
        [Required(ErrorMessage = "Payment Method is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Payment Method !")]
        [DefaultValue(0)]
        public int PaymentMethodId { get; set; }
        [DefaultValue(0)]
        public int PaymentPeriod { get; set; }
        [Required(ErrorMessage = "Currency Type is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Currency Type !")]
        [DefaultValue(0)]
        public int CurrencyID { get; set; }
        [Required(ErrorMessage = "* ")]
        [DefaultValue(0)]
        public decimal CurrencyRate { get; set; }
        [DefaultValue(0)]
        public int ReferenceDocumentDocumentID { get; set; }
        [DefaultValue(0)]
        public long ReferenceDocumentID { get; set; }
        [DefaultValue("")]
        [MaxLength(20)]
        public string ReferenceNo { get; set; }
        [DefaultValue("")]
        [MaxLength(20)]
        public string SupplierInvoiceNo { get; set; }
        [DefaultValue(0)]
        public int DocumentStatus { get; set; }
        [DefaultValue(0)]
        public bool IsUpLoad { get; set; }
        [DefaultValue(0)]
        public int ReturnTypeID { get; set; }
        [DefaultValue(0)]
        public decimal OtherDeduction { get; set; }
        public virtual ICollection<PurchaseDetail> PurchaseDetail { get; set; }
        [NotMapped]
        public List<PurchaseDetail> GRNDetail { get; set; }
        public decimal TotSellingPrice { get; set; }
        public decimal TotCostPrice { get; set; }
        public decimal TotDiscounts { get; set; }
        [Required(ErrorMessage = "Location is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Location !")]
        public long GRNLocationId { get; set; }
        [DefaultValue(false)]
        public bool IsTempGRN { get; set; }
        [MaxLength(20)]
        public string OtherDocNo { get; set; }
        [DefaultValue(false)]
        public bool IsGRN { get; set; }
        [DefaultValue(false)]
        public bool IsTempPRN { get; set; }
        [DefaultValue(0)]
        public long POID { get; set; }
        [DefaultValue(0)]
        public long GRNId { get; set; }
        public string PRNType { get; set; }
        public string GRNType { get; set; }
        [NotMapped]
        public string Company { get; set; }
        [NotMapped]
        public string Location { get; set; }
               
        public DateTime GRNDate { get; set; }
        [NotMapped]
        public virtual Supplier Supplier { get; set; }
        [NotMapped]
        public virtual SysCompany SysCompany { get; set; }       
        public decimal  Deductions { get; set; }

        [NotMapped]
        public int PrintStatus { get; set; }

        [NotMapped]
        public string DocState { get; set; }

     //   [NotMapped]
        public int? EventId { get; set; }

        [DefaultValue("")]
        [MaxLength(20)]
        public string NewDocNumber { get; set; }

        
        [DefaultValue(false)]
        public bool IsPRN { get; set; }

        [NotMapped]
        public string PONumber { get; set; }

       // [NotMapped]
        [DefaultValue("")]
        [MaxLength(200)]
        public string RejectReason { get; set; }

       // [NotMapped]
        [DefaultValue("")]
        [MaxLength(200)]
        public string CancelReason { get; set; }

        [NotMapped]
        public DateTime GRNDateFrom { get; set; }
        [NotMapped]
        public DateTime GRNDateTo { get; set; }

        [NotMapped]
       // [Range(1, int.MaxValue, ErrorMessage = "Please select a Doc Status !")]
        public int DocStatusId { get; set; }
        [NotMapped]
        public string BaseDocNo { get; set; }


        [DefaultValue(false)]
        public bool IsTOGTransfer { get; set; }
    }
}