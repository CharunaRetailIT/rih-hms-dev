using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Loyalty
{
    public class LoyaltyCustomer
    {
        public int LoyaltyCustomerId { get; set; }
        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        [DefaultValue("")]
        public string CardNo { get; set; }
        [DefaultValue(0)]
        public Int64 CustomerId { get; set; }
        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        public string NameOnCard { get; set; }
        [DefaultValue(0)]
        public Int64 CardMasterId { get; set; }
        [DefaultValue(0)]
        public bool CardIssued { get; set; }
        public DateTime IssuedOn { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime RenewedOn { get; set; }
        [DefaultValue(0)]
        public Int64 LedgerId { get; set; }
        [DefaultValue(0)]
        public Int64 LedgerId2 { get; set; }
        [DefaultValue(0)]
        public decimal CreditLimit { get; set; }
        [DefaultValue(0)]
        public int CreditPeriod { get; set; }
        [DefaultValue(0)]
        public decimal CPoints { get; set; }
        [DefaultValue(0)]
        public decimal EPoints { get; set; }
        [DefaultValue(0)]
        public decimal RPoints { get; set; }
        [DefaultValue(0)]
        public bool IsReDimm { get; set; }
        public DateTime AcitiveDate { get; set; }
        [DefaultValue(0)]
        public int LocationID { get; set; }
        [DefaultValue(0)]
        public int CashierID { get; set; }
        [DefaultValue(0)]
        public int LoyaltyType { get; set; }
        [Column(TypeName = "nvarchar")]
        [StringLength(200)]
        public string Remark { get; set; }
        [Column(TypeName = "nvarchar")]
        [StringLength(15)]
        public string SystemGeneratedCode { get; set; }
        [DefaultValue(0)]
        public decimal ExpiryPoints { get; set; }
        [DefaultValue(false)]
        public bool IsSold { get; set; }
        [DefaultValue(0)]
        public int GroupOfCompanyID { get; set; }
        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int DataTransfer { get; set; }
        [DefaultValue(0)]
        public decimal ExpiryPoints1 { get; set; }
        [DefaultValue(0)]
        public decimal Discount { get; set; }
        [Column(TypeName = "nvarchar")]
        [StringLength(10)]
        public string SalesPersonCode { get; set; }
        [DefaultValue(0)]
        public int LastUpdatedLocId { get; set; }
        [DefaultValue(0)]
        public int Status { get; set; }
        [DefaultValue(false)]
        public int IsCardIssued { get; set; }
        [DefaultValue(0)]
        public int CompanyId { get; set; }

    }
}
