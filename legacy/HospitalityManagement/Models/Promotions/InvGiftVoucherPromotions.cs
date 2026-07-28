using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models.Promotions
{
    public class InvGiftVoucherPromotions : BaseEntity
    {

        public int InvGiftVoucherPromotionsId { get; set; }
        public int PromotionMasterId { get; set; }
        [DefaultValue(0)]
        public decimal GiftVoucherAmount { get; set; }
        [DefaultValue(0)]
        public decimal BillValue { get; set; }
        [DefaultValue(0)]    
        public int NoOfOccurrences { get; set; }
        [DefaultValue("")]
        public string Remarks { get; set; }





    }
}