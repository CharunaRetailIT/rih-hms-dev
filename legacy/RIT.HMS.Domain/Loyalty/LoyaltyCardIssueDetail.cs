using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Loyalty
{
   public class LoyaltyCardIssueDetail : BaseEntity
    {
        public long LoyaltyCardIssueDetailId { get; set; }
        [DefaultValue(0)]
        public int LoyaltyCardIssueHeaderId { get; set; }
        [DefaultValue(0)]
        public long CardIssueDetailID { get; set; }
        [DefaultValue(0)]
        public int ToLocationID { get; set; }
        public DateTime IssueDate { get; set; }
        [DefaultValue("")]
        public string CardNo { get; set; }
        public string EncodeNo { get; set; }
        [DefaultValue(false)]
        public bool IsIssued { get; set; }

        [DefaultValue(false)]
        public bool IsActive { get; set; }
        [DefaultValue(false)]
        public bool IsDelete { get; set; }
        [StringLength(50)]
        public string FefCardNo1 { get; set; }
        [StringLength(50)]
        public string FefCardNo2 { get; set; }
       
    }
}
