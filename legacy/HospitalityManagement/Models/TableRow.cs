using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class TableRow
    {
        public int ProductId { get; set; }
        public string Product { get; set; }
        public int ItemId { get; set; }
        public string Item { get; set; }
        public int RequestQuantity { get; set; }
        public int IssueQty { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public string RequestedBy { get; set; }
        public string ServingUnit { get; set; }
        public int ServingUnitId { get; set; }
    }
}