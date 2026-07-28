using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.Transactions
{
    public class StockAdjustmentDetail
    {
        public long StockAdjustmentDetailId { get; set; }
        public long StockAdjustmentHeaderId { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal AdjustStock { get; set; }      
        public decimal NewStock { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal AvgCost { get; set; }     
        public string BaseType { get; set; }
        public string Reason { get; set; }
        [NotMapped]
        public string ProductCode { get; set; }
    }
}