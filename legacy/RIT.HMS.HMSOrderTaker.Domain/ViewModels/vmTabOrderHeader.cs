using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.HMSOrderTaker.Domain.ViewModels
{
    public  class vmTabOrderHeader
    {
        public vmTabOrderHeader()
        {
            Tables = new List<TableMaster>();
        }

        public int LocationId { get; set; }
        public int OrderSeqNumber { get; set; }
        public int TableId { get; set; }
        public string TableCode { get; set; }
        public string RunningOrderNumber { get; set; }
        public string OrderName { get; set; }
        public string LocationCode { get; set; }
        public Nullable<int> LoggedInUserId { get; set; }
        public string LoggedInUseName { get; set; }
        public Nullable<int> CustomeId { get; set; }
        public string CustomeName { get; set; }
        public Nullable<int> isLoyatyCustomer { get; set; }
        public Nullable<int> isPromotionItems { get; set; }
        public Nullable<int> TabOrderType { get; set; }
        public Nullable<System.DateTime> TabOrderStartDateTime { get; set; }
        public Nullable<System.DateTime> TabOrderCompleteDateTime { get; set; }
        public Nullable<decimal> OrderBillFinalizedPayment { get; set; }
        public Nullable<decimal> OrderBillDiscount { get; set; }
        public Nullable<decimal> OrderSubtotal { get; set; }
        public Nullable<decimal> OrderServiceCharge { get; set; }
        public Nullable<int> OrderItemCount { get; set; }
        public string Remark { get; set; }
        public Nullable<int> TabOrderStatus { get; set; }
        public Nullable<System.DateTime> CreatedDateTime { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public string CreatedMachine { get; set; }
        public Nullable<System.DateTime> ModifiedDateTime { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public string ModifiedMachine { get; set; }

        //Jquary Date Convert
        public string RealDateTime { get; set; }
        public List<vmTabOrderDetail> TabOrderDetailsList { get; set; }
        public List<TableMaster> Tables { get; set; }
    }
}
