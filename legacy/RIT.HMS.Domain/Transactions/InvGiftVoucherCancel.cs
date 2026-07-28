using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Transactions
{
    public class InvGiftVoucherCancel : BaseEntity
    {
        [Key]
        public int GiftVoucherCancelID { get; set; }
        [MaxLength(20)]
        [Required(ErrorMessage = "The field is required")]
        public string VoucherNo { get; set; }
        [Required(ErrorMessage = "The field is required")]
        [MaxLength(15)]
        public string Remark { get; set; }
        [DefaultValue(0)]
        public bool IsCancel { get; set; }
        public string BookCode { get; set; }
        public string BookName { get; set; }
        public int ValidityPeriod { get; set; }
        public decimal GiftVoucherValue { get; set; }
        [DefaultValue(0)]
        public int InvGiftVoucherGroupID { get; set; }
    }
}
