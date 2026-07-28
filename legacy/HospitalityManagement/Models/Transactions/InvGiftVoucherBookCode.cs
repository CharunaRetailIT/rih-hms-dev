using RIT.HMS.Domain.Transactions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models.Transactions
{
    public class InvGiftVoucherBookCodeS
    {
        public List<InvGiftVoucherBookCode> giftvoucherBook = new List<InvGiftVoucherBookCode>();
        [Required(ErrorMessage = "The field is required")]
        public int InvGiftVoucherBookCodeID { get; set; }
        [Required(ErrorMessage = "The field is required")]
        public int InvGiftVoucherGroupID { get; set; }
        [Required(ErrorMessage = "The field is required")]

        public string BookCode { get; set; }
        [Required(ErrorMessage = "The field is required")]

        public string BookName { get; set; }
        [Required(ErrorMessage = "The field is required")]

        public string BookPrefix { get; set; }
        [Required(ErrorMessage = "The field is required")]


        public decimal GiftVoucherValue { get; set; }
        [Required(ErrorMessage = "The field is required")]


        public decimal GiftVoucherPercentage { get; set; }
        [Required(ErrorMessage = "The field is required")]
        public int ValidityPeriod { get; set; }
        [Required(ErrorMessage = "The field is required")]
        public int VoucherType { get; set; }
        [Required(ErrorMessage = "The field is required")]

        public int StartingNo { get; set; }
        [Required(ErrorMessage = "The field is required")]

        public int CurrentSerialNo { get; set; }
        [Required(ErrorMessage = "The field is required")]

        public int SerialLength { get; set; }
        [Required(ErrorMessage = "The field is required")]

        public int PageCount { get; set; }
        [Required(ErrorMessage = "The field is required")]

        public bool IsDelete { get; set; }
        [Required(ErrorMessage = "The field is required")]

        public int BasedOn { get; set; }
        [Required(ErrorMessage = "The field is required")]
        public string SerialFormat { get; set; }

    }
}