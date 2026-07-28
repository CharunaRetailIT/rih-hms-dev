using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Loyalty
{
    public class LoyaltyCardSchems :BaseEntity
    {
        public long LoyaltyCardSchemsID { get; set; }
        [DefaultValue(0)]
        public long CardMasterId { get; set; }
        [DefaultValue(0)]
        public decimal BillFromValue { get; set; }
        [DefaultValue(0)]
        public decimal BillToValue { get; set; }
        [DefaultValue(0)]
        public decimal Increment { get; set; }
        [DefaultValue(0)]
        public decimal PointValue { get; set; }
        [DefaultValue(0)]
        public decimal PointPer { get; set; }
        [DefaultValue(false)]
        public bool IsDelete { get; set; }
        public virtual CardMaster CardMaster { get; set; }

    }
}
