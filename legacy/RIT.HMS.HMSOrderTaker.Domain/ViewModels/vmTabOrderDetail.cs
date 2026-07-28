using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.HMSOrderTaker.Domain.ViewModels
{
    public class vmTabOrderDetail
    {
        public int LocationId { get; set; }
        public int OrderSeqNumber { get; set; }
        public int ItemSeqId { get; set; }
        public int ItemId { get; set; }
        public string ItemCode { get; set; }
        public string ItemNameOnBill { get; set; }
        public string ItemName { get; set; }
        public int TableId { get; set; }
        public string TableCode { get; set; }
        public Nullable<decimal> ItemCostPrice { get; set; }
        public Nullable<decimal> ItemSellingPrice { get; set; }
        public Nullable<int> IsItemOnPromotion { get; set; }
        public Nullable<int> ItemQty { get; set; }
        public Nullable<int> ItemServingTypeid { get; set; }
        public string ItemServingTypeName { get; set; }
        public Nullable<int> IsWithAddOn { get; set; }
        public string OrderedItemRemark { get; set; }
        public Nullable<int> ItemKOTBOT { get; set; }
        public Nullable<int> ItemKOTBOTStatus { get; set; }
        public Nullable<System.DateTime> ItemKOTBOTStartDateTime { get; set; }
        public Nullable<System.DateTime> ItemKOTBOTEndDateTime { get; set; }
        public Nullable<int> OrderedItemStatus { get; set; }
        public Nullable<System.DateTime> CreatedDateTime { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public string CreatedMachine { get; set; }
        public Nullable<System.DateTime> ModifiedDateTime { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public string ModifiedMachine { get; set; }
    }
}
