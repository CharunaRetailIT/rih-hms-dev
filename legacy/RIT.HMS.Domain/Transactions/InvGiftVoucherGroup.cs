using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Transactions
{
    public class InvGiftVoucherGroup:BaseEntity
    {
        [Key]
        public int InvGiftVoucherGroupID { get; set; }
        [MaxLength(20)]
        [Required(ErrorMessage = "The field is required")]
        public string GiftVoucherGroupCode { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [MaxLength(50)]
        public string GiftVoucherGroupName { get; set; }
        [MaxLength(150)]
        public string Remark { get; set; }
        [DefaultValue(0)]
        public bool IsDelete { get; set; }
        //public int GroupOfCompanyID { get; set; }

        //[MaxLength(50)]
        //public string CreatedUser { get; set; }

        //public DateTime? CreatedDate { get; set; }
        //[MaxLength(50)]
        //public string ModifiedUser { get; set; }

        //public DateTime? ModifiedDate { get; set; }
        //[DefaultValue(0)]
        //public int DataTransfer { get; set; }
    }
}
