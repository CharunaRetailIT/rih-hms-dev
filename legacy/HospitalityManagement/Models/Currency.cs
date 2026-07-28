using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class Currency :BaseEntity
    {
        public int CurrencyId { get; set; }
        [MaxLength(5)]
        [DefaultValue("")]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        [Required]
        public string CurrencyCode { get; set; }

        [MaxLength(50)]
        [DefaultValue("")]
        [Required]
        public string CurrencyDescription { get; set; }

        [MaxLength(15)]
        [DefaultValue("")]
        [Required]
        public string CurrencyFormat { get; set; }

        [MaxLength(5)]
        [DefaultValue("")]
        [Required]
        public string CurrencySymbol { get; set; }

        [DefaultValue(0)]

        public decimal BuyingRate { get; set; }

        [DefaultValue(0)]
        public decimal SellingRate { get; set; }
        public DateTime AsofDate { get; set; }

        [DefaultValue(0)]
        public bool IsActive { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }



        //CurrencyCode CurrencyDescription  CurrencyFormat  CurrencySymbol

        //LKR         Sri Lanka Rupee     Rupees          Rs.

        //USD         US Doller           US Doller       $
    }
}