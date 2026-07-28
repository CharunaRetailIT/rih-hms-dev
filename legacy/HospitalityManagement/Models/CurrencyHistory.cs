using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class CurrencyHistory : BaseEntity
    {
        public int CurrencyHistoryId { get; set; }
        public int CurrencyId { get; set; }
        [DefaultValue(0)]
        public decimal BuyingRate { get; set; }
        [DefaultValue(0)]
        public decimal SellingRate { get; set; }
        public DateTime AsofDate { get; set; }

        [NotMapped]
        public string CurrencyCode { get; set; }
        [NotMapped]
        public string CurrencyDescription { get; set; }
    
      
    }
}