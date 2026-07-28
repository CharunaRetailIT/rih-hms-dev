using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class Tax : BaseEntity
    {
        public int TaxId { get; set; }

        [Required]
        [MaxLength(10)]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        public string TaxCode { get; set; }

        [Required]
        [MaxLength(50)]
        public string TaxName { get; set; }

        [DefaultValue(0)]
        public decimal TaxPercentage { get; set; }

        //[DefaultValue(0)]
        //public decimal EffectivePercentage { get; set; }

        //public DateTime EffectiveDate { get; set; }

        //[DefaultValue(0)]
        //public bool Tax1 { get; set; }

        //[DefaultValue(0)]
        //public bool Tax2 { get; set; }

        //[DefaultValue(0)]
        //public bool Tax3 { get; set; }

        //[DefaultValue(0)]
        //public bool Tax4 { get; set; }

        //[DefaultValue(0)]
        //public bool Tax5 { get; set; }

        //public int PrintOrder { get; set; }

        //[DefaultValue(0)]
        //public long LedgerID { get; set; } // Collected Ledger ID

        //[DefaultValue(0)]
        //public long PaidLedgerID { get; set; } // Paid Ledger ID

        //[MaxLength(150)]
        //[DefaultValue("")]
        //public string Remark { get; set; }

        [DefaultValue(0)]
        public bool IsActive { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }

        [DefaultValue(0)]
        public bool IsTaxOnTax { get; set; }

        [DefaultValue(0)]
        public bool IsPurchasingTax { get; set; }
        [DefaultValue(0)]
        public bool IsSellingTax { get; set; }

        [DefaultValue(0)]
        public bool IsServiceCharge { get; set; }
    }
}