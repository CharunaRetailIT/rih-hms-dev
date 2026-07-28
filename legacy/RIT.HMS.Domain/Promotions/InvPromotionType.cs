using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.Promotions
{
    public class InvPromotionType :BaseEntity
    {
        public long InvPromotionTypeID { get; set; }

        [Required]
        [MaxLength(15)]
        public string PromotionTypeCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string PromotionTypeName { get; set; }

        [MaxLength(150)]
        [DefaultValue("")]
        public string Remark { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }
    }
}