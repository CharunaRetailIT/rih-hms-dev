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
    public class LoyaltyCardGenerationHeader : BaseEntity
    {

        public LoyaltyCardGenerationHeader()
        {
            LoyaltyCardGenerationDetail = new List<LoyaltyCardGenerationDetail>();
        }

        public virtual ICollection<LoyaltyCardGenerationDetail> LoyaltyCardGenerationDetail { get; set; }
        public long LoyaltyCardGenerationHeaderId { get; set; }
        public long CardGenerationHeaderID { get; set; }
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
        [DefaultValue(0)]
        public long CardMasterId { get; set; }
        [DefaultValue(false)]
        public bool IsDelete { get; set; }
        [NotMapped]
        public bool IsExists { get; set; }
        [NotMapped]
        [DefaultValue(0)]
        [Range(1, int.MaxValue, ErrorMessage = "Select a Location !")]
        public int GenLocationId { get; set; }
        [NotMapped]
        public string DocNumber { get; set; }
        [NotMapped]
        [DefaultValue(false)]
        public bool Update { get; set; }
        [NotMapped]
        public string CardNoFrom { get; set; }
        [NotMapped]
        public string CardNoTo { get; set; }

    }
}
