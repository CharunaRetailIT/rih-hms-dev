using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Transactions
{
    public class InvGiftVoucherPurchaseOrderDetail : BaseEntity
    {
        public long InvGiftVoucherPurchaseOrderDetailID { get; set; }

        [DefaultValue(0)]
        public long GiftVoucherPurchaseOrderDetailID { get; set; }

        [Required]
        public long InvGiftVoucherPurchaseOrderHeaderID { get; set; }

        //[DefaultValue(0)]
        //public int CompanyID { get; set; }

        //[DefaultValue(0)]
        //public int LocationID { get; set; }

        [DefaultValue(0)]
        public int DocumentID { get; set; } //????????????

        [DefaultValue("")]
        [MaxLength(20)]
        [NotMapped]
        public string DocumentNo { get; set; } //????????????

        public DateTime DocumentDate { get; set; }

        [DefaultValue(0)]
        public long LineNo { get; set; }

        [DefaultValue(0)]
        public long InvGiftVoucherMasterID { get; set; }

        [NotMapped]
        public int InvGiftVoucherBookCodeID { get; set; }

        [NotMapped]
        public int InvGiftVoucherGroupID { get; set; }

        [NotMapped]
        public string VoucherNo { get; set; }

        [NotMapped]
        public string VoucherSerial { get; set; }

        [DefaultValue(0)]
        public decimal NumberOfCount { get; set; }

        [DefaultValue(0)]
        public decimal VoucherAmount { get; set; }

        [NotMapped]
        public decimal GiftVoucherPercentage { get; set; }

        [NotMapped]
        public decimal VoucherValue { get; set; }

        [NotMapped]
        public decimal GrossAmount { get; set; }

        [NotMapped]
        public decimal DiscountPercentage { get; set; }

        [NotMapped]
        public decimal DiscountAmount { get; set; }

        [NotMapped]
        public decimal SubTotalDiscount { get; set; }

        [NotMapped]
        public decimal NetAmount { get; set; }

        [DefaultValue(0)]
        [Description("1 - Voucher, 2 - Coupon")]
        public int VoucherType { get; set; }

        [DefaultValue(0)]
        public bool IsPurchase { get; set; }

        [DefaultValue(0)]
        public int DocumentStatus { get; set; }
    }
}
