using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace RIT.HMS.Domain
{
    public class InvSupplier : BaseEntity
    {
        public int InvSupplierID { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DataType(DataType.Text)]
        [DefaultValue(0)]
        public string SupplierCode { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [DefaultValue("")]
        public string SupplierName { get; set; }

        public string SupplierType { get; set; }


        [DefaultValue("")]
        public string Address1 { get; set; }

        [DefaultValue("")]
        public string Address2 { get; set; }

        [DefaultValue("")]
        public string Address3 { get; set; }

        [DefaultValue("")]
        public string Telephone { get; set; }

        [DefaultValue("")]
        public string Mobile { get; set; }

        [DefaultValue("")]
        public string Fax { get; set; }

        [DefaultValue("")]
        public string Email { get; set; }

        public string ContactPerson { get; set; }

        [DefaultValue("")]
        public int ConsignmentType { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DataType(DataType.Text)]
        [DefaultValue(0)]
        public decimal CreditLimit { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DataType(DataType.Text)]
        [DefaultValue(0)]
        public decimal CreditPeriod { get; set; }

        [DefaultValue("")]
        public decimal OpeningBalance { get; set; }

        [DefaultValue("")]
        public string CurrentMonthPurchase { get; set; }

        public string CurrentMonthReturns { get; set; }

        [DefaultValue("")]
        public string CurrentMonthPayments { get; set; }

        [DefaultValue("")]
        public decimal TotalOutstandings { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DefaultValue(0)]
        public int SupplierGroup { get; set; }

        [DefaultValue("")]
        public string SupplierOrderCycle { get; set; }

        [DefaultValue("")]
        public string SupplierVATRegNo { get; set; }

        [DefaultValue(0)]
        public bool IsActive { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }

    }
}