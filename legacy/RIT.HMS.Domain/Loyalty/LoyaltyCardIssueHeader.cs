using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Loyalty
{
    public class LoyaltyCardIssueHeader : BaseEntity
    {
        public LoyaltyCardIssueHeader()
        {
            LoyaltyCardIssueDetail = new List<LoyaltyCardIssueDetail>();
        }

        public virtual ICollection<LoyaltyCardIssueDetail> LoyaltyCardIssueDetail { get; set; }

        public int LoyaltyCardIssueHeaderId { get; set; }
        [DefaultValue(0)]
        public long CardIssueHeaderID { get; set; }
        public DateTime IssueDate { get; set; }
        [DefaultValue(0)]
        public int ToLocationID { get; set; }
        [StringLength(50)]
        public string DocumentNo { get; set; }
        [StringLength(50)]
        public string Remark { get; set; }
        public string ReferenceNo { get; set; }
        [DefaultValue(0)]
        public int EmployeeID { get; set; }
       
    }
}
