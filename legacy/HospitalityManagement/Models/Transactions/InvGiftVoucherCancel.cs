using RIT.HMS.Domain.Transactions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models.Transactions
{
    public class InvGiftVoucherCancel : BaseEntity
    {
        public int GiftVoucherCancelID { get; set; }
        [MaxLength(20)]
        [Required(ErrorMessage = "The field is required")]
        public string VoucherNo { get; set; }
        [Required(ErrorMessage = "The field is required")]
        [MaxLength(15)]
        public string Remark { get; set; }
        [DefaultValue(0)]
        public bool IsCancel { get; set; }
        public string BookCode { get; set; }
        public string BookName { get; set; }
        public int ValidityPeriod { get; set; }
        public decimal GiftVoucherValue { get; set; }
        [DefaultValue(0)]
        public int InvGiftVoucherGroupID { get; set; }

        //public long InvGiftVoucherMasterID { get; set; }

        //public long InvGiftVoucherBookCodeID { get; set; }


        //public int InvGiftVoucherGroupID { get; set; }

        ////public int CompanyID { get; set; }
        ////public int LocationID { get; set; }

        //[MaxLength(15)]
        //public string VoucherNo { get; set; }
        //public int VoucherNoSerial { get; set; }
        //[MaxLength(4)]
        //public string VoucherPrefix { get; set; }
        //public int SerialLength { get; set; }

        //[DefaultValue(0)]
        //public decimal GiftVoucherValue { get; set; }
        //[DefaultValue(0)]
        //public decimal GiftVoucherPercentage { get; set; }
        //public int StartingNo { get; set; }
        //public int VoucherCount { get; set; }
        //public int PageCount { get; set; }
        //public string VoucherSerial { get; set; }

        //public int VoucherSerialNo { get; set; }
        //public int VoucherType { get; set; }
        //public int VoucherStatus { get; set; }
        //public int ToLocationID { get; set; }
        //public int SoldLocationID { get; set; }

        //public long SoldCashierID { get; set; }
        //public string SoldReceiptNo { get; set; }
        //public int SoldUnitID { get; set; }

        //public long SoldZNo { get; set; }

        //public DateTime? SoldDate { get; set; }
        //public int RedeemedLocationID { get; set; }
        //public long RedeemedCashierID { get; set; }

        //public string RedeemedReceiptNo { get; set; }

        //public int RedeemedUnitID { get; set; }
        //public long RedeemedZNo { get; set; }
        //public DateTime? RedeemedDate { get; set; }
        //public bool IsBarcodePrinted { get; set; }

        //public bool IsDelete { get; set; }
        ////public int GroupOfCompanyID { get; set; }

        //public DateTime? Expirydate { get; set; }
        //public bool IsTemporaryBlocked { get; set; }
        //public int BlockedLocationID { get; set; }
        //public bool BlockedCashierID { get; set; }
        //public int BlockedUnitID { get; set; }
        //public DateTime? BlockedDate { get; set; }

        //public string GiftVoucherGroupCode { get; set; }
        //public string BookCode { get; set; }

        ////public List<InvGiftVoucherMasterTemp> GVMasTempList = new List<InvGiftVoucherMasterTemp>();
        //public List<InvGiftVoucherGroup> GVGroups { get; set; }
    }
}