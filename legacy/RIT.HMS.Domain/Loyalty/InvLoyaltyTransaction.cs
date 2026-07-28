using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Loyalty
{
    public class InvLoyaltyTransaction
    {
        public long InvLoyaltyTransactionID { get; set; }
        public long CustomerID { get; set; }
        public Int16 CustomerType { get; set; }

        [StringLength(15)]
        [DefaultValue("")]
        public string Receipt { get; set; }
        [DefaultValue(0)]
        public decimal Amount { get; set; }

        [DefaultValue(0)]
        public decimal Points { get; set; }

        [DefaultValue(0)]
        public Int16 TransID { get; set; }
        [DefaultValue(0)]
        public Int16 LocationID { get; set; }

        public DateTime DocumentDate { get; set; }

        [DefaultValue(0)]
        public Int16 UnitNo { get; set; }
        [DefaultValue(0)]
        public long CashierID { get; set; }
        public DateTime DocumentTime { get; set; }

        [DefaultValue(0)]
        public decimal DiscPer { get; set; }

        [DefaultValue(0)]
        public decimal DiscAmt { get; set; }

        [DefaultValue(0)]
        public decimal PointsRate { get; set; }
        [DefaultValue(0)]
        public long Zno { get; set; }

        [StringLength(15)]
        [DefaultValue("")]
        public string CardNo { get; set; }
        [DefaultValue(0)]
        public int CardType { get; set; }
        [DefaultValue(0)]
        public int LoyaltyType { get; set; }
        [DefaultValue(false)]
        public bool IsGuidClaimed { get; set; }
        [DefaultValue(false)]
        public bool IsSync { get; set; }

        [StringLength(15)]
        [DefaultValue("")]
        public string CustomerCode { get; set; }

        [StringLength(50)]
        [DefaultValue("")]
        public string NIC { get; set; }


        [System.ComponentModel.DataAnnotations.StringLength(50)]
        [DefaultValue("")]
        public string RefNo { get; set; }
    }
}
