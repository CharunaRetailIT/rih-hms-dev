using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Transactions
{
    public class InvGiftVoucherPurchaseOrderHeader : BaseEntity
    {
        public long InvGiftVoucherPurchaseOrderHeaderID { get; set; }

        [DefaultValue(0)]
        public long GiftVoucherPurchaseOrderHeaderID { get; set; }

        //[DefaultValue(0)]
        //public int CompanyID { get; set; }

        //[DefaultValue(0)]
        //public int LocationID { get; set; }

        [DefaultValue(0)]
        public int DocumentID { get; set; }

        [DefaultValue("")]
        [MaxLength(20)]
        public string DocumentNo { get; set; }

        public DateTime DocumentDate { get; set; }
        
        [Required(ErrorMessage = "The field is required")]
        public long SupplierID { get; set; }

        public DateTime ExpectedDate { get; set; }

        public DateTime ExpiryDate { get; set; }
        
        [Required(ErrorMessage = "The field is required")]
        public int PaymentTermID { get; set; }

        [DefaultValue(0)]
        public int PaymentPeriod { get; set; }

        [DefaultValue(0)]
        public decimal GiftVoucherAmount { get; set; }

        [DefaultValue(0)]
        public decimal GiftVoucherPercentage { get; set; }

        [DefaultValue(0)]
        public decimal GrossAmount { get; set; }

        [DefaultValue(0)]
        public decimal DiscountAmount { get; set; }

        [DefaultValue(0)]
        public decimal DiscountPercentage { get; set; }

        [DefaultValue(0)]
        public decimal OtherCharges { get; set; }

        [DefaultValue(0)]
        public decimal TaxAmount1 { get; set; }

        [DefaultValue(0)]
        public decimal TaxAmount2 { get; set; }

        [DefaultValue(0)]
        public decimal TaxAmount3 { get; set; }

        [DefaultValue(0)]
        public decimal TaxAmount4 { get; set; }

        [DefaultValue(0)]
        public decimal TaxAmount5 { get; set; }

        [DefaultValue(0)]
        public decimal TaxAmount { get; set; }

        [DefaultValue(0)]
        public decimal NetAmount { get; set; }

        [DefaultValue(0)]
        public decimal CreditLimit { get; set; }

        [DefaultValue(0)]
        public int CreditPeriod { get; set; }

        [DefaultValue(0)]
        public decimal ChequeLimit { get; set; }

        [DefaultValue(0)]
        public int ChequePeriod { get; set; }

        [DefaultValue(0)]
        public int GiftVoucherQty { get; set; }

        //[DefaultValue("")]
        [MaxLength(150)]
        public string Remark { get; set; }

        //[DefaultValue("")]
        [MaxLength(20)]
        public string ReferenceNo { get; set; }

        [DefaultValue(0)]
        [Description("1 - Voucher, 2 - Coupon")]
        public int VoucherType { get; set; }

        [DefaultValue(0)]
        public int DocumentStatus { get; set; }
    }
}
