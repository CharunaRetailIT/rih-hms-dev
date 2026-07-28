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
    public  class LoyaltyCardGenerationDetail
    {
        public long LoyaltyCardGenerationDetailId { get; set; }
        public long CardGenerationDetailID { get; set; }

        [DefaultValue(0)]
        public long LoyaltyCardGenerationHeaderID { get; set; }

        [StringLength(10)]
        public string CardPrefix { get; set; }
        [DefaultValue(0)]
        public int CardLength { get; set; }

        [DefaultValue(0)]
        public int CardStartingNo { get; set; }

        [DefaultValue(0)]
        public int EncodeLength { get; set; }

        [DefaultValue(0)]
        public int EncodeStartingNo { get; set; }
        [DefaultValue("")]
        [StringLength(3)]
        public string EncodePrefix { get; set; }
        public DateTime GeneratedDate { get; set; }
        [DefaultValue("")]
        [StringLength(50)]
        public string CardNo { get; set; }

        [DefaultValue("")]
        [StringLength(50)]
        public string EncodeNo { get; set; }

        [DefaultValue(false)]      
        public bool IsIssued { get; set; }
        [DefaultValue(true)]
        public bool IsActive { get; set; }
        [DefaultValue(false)]
        public bool IsDelete { get; set; }
       
        [DefaultValue("")]
        [StringLength(50)]
        public string RefCardNo1 { get; set; }
        [DefaultValue("")]
        [StringLength(50)]
        public string RefCardNo2 { get; set; }
        [NotMapped]
        public string CardNoWithPrefix { get; set; }


    }
}
