using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class InvGiftVoucherBookCode
    {
        public int InvGiftVoucherBookCodeID { get; set; }
        public int InvGiftVoucherGroupID { get; set; }
        public string BookCode { get; set; }
        public string BookName { get; set; }
        public string BookPrefix { get; set; }
        public decimal GiftVoucherValue { get; set; }
        [DefaultValue(0)]
        public decimal GiftVoucherPercentage { get; set; }
        [DefaultValue(0)]
        public int ValidityPeriod { get; set; }
        public int VoucherType { get; set; }
        public int StartingNo { get; set; }
        public int CurrentSerialNo { get; set; }
        public int SerialLength { get; set; }
        public int PageCount { get; set; }
        public bool IsDelete { get; set; }
        public int BasedOn { get; set; }
        public int GroupOfCompanyID { get; set; }
        public string CreatedUser { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int DataTransfer { get; set; }
    }
}