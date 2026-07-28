using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Transactions
{
    public class InvGiftVoucherBookCode : BaseEntity
    {
        [Key]
        public long InvGiftVoucherBookCodeID { get; set; }
        [DefaultValue(0)]
        public int InvGiftVoucherGroupID { get; set; }

        [MaxLength(20)]
        public string BookCode { get; set; }

        [MaxLength(50)]
        public string BookName { get; set; }

        [MaxLength(4)]
        public string BookPrefix { get; set; }

        [DefaultValue(0)]

        public decimal GiftVoucherValue { get; set; }

        [DefaultValue(0)]

        public decimal GiftVoucherPercentage { get; set; }
        [DefaultValue(0)]
        public int ValidityPeriod { get; set; }
        [DefaultValue(0)]
        public int VoucherType { get; set; }

        [DefaultValue(0)]
        public int StartingNo { get; set; }

        [DefaultValue(0)]
        public int CurrentSerialNo { get; set; }

        [DefaultValue(0)]
        public int SerialLength { get; set; }

        [DefaultValue(0)]
        public int PageCount { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }

        [DefaultValue(0)]
        public int BasedOn { get; set; }

        //public virtual InvGiftVoucherGroup InvGiftVoucherGroups { get; set; }
        //public virtual ICollection<InvGiftVoucherMaster> InvGiftVoucherMasters { get; set; }

        //[DefaultValue(0)]
        //public int GroupOfCompanyID { get; set; }

        //[MaxLength(50)]
        //public string CreatedUser { get; set; }

        //public DateTime? CreatedDate { get; set; }

        //[MaxLength(50)]
        //public string ModifiedUser { get; set; }

        //public DateTime? ModifiedDate { get; set; }
        //public int DataTransfer { get; set; }
    }
}
