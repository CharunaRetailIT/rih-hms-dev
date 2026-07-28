using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalityManagement.Models.Promotions
{
    public class InvPromoCustomerCategory
    {
        public long InvPromoCustomerCategoryID { get; set; }

        public long InvPromotionMasterID { get; set; }

        public int CustomerCategoryID { get; set; }

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
