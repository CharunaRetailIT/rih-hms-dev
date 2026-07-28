using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Promotions
{
  public  class InvPromoBusinessType
    {
        public long InvPromoBusinessTypeID { get; set; }
        
        public long InvPromotionMasterID { get; set; }
        
        public long CateringMoodID { get; set; }

        [Required()]
        [MaxLength(20)]
        public string CateringMoodName { get; set; }

        [MaxLength(150)]
        [DefaultValue("")]
        public string Remark { get; set; }

        [DefaultValue(0)]
        public bool Status { get; set; }

        [MaxLength(50)]
        public string CreatedUser { get; set; }

        public DateTime CreatedDate { get; set; }

        [MaxLength(50)]
        public string ModifiedUser { get; set; }

        public DateTime ModifiedDate { get; set; }


    }
}
