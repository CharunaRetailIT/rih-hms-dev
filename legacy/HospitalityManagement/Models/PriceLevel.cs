using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class PriceLevel
    {
        public long PriceLevelId { get; set; }

        [DefaultValue(0)]

        public long ProductId { get; set; }

        [DefaultValue(0)]

        public decimal Qty { get; set; }

        [DefaultValue(0)]

        public decimal CostPrice { get; set; }

        [DefaultValue(0)]

        public decimal SellingPrice { get; set; }

        [MaxLength(50)]

        public string CreatedUser { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        public int DataTransfer { get; set; }

        [DefaultValue(0)]
        public int LocationId { get; set; }

        [DefaultValue(0)]
        public int DocumentId { get; set; }
    }
}