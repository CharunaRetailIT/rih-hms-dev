using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.ViewModels
{
    public class TOGViewModel
    {
        public decimal SellingPrice { get; set; }
        public decimal CostPrice { get; set; }     
        public decimal Qty { get; set; }
        public long ItemId { get; set; }
        public string ItemDesc { get; set; }
        public decimal CrrQty { get; set; }
        public bool Availablility { get; set; }
        public string FromLoc { get; set; }
        public string ToLoc { get; set; }
        public string Message { get; set; }
        public string StockMessage { get; set; }
        public string UOM { get; set; }
        public decimal UnitSelling { get; set; }
        public decimal UnitCost { get; set; }
        public int HasItem { get; set; }
        public string ProductCode { get; set; }
        public long PurchaseHeaderId { get; set; }
        public string DiscountType { get; set; }
        public decimal IssueQty { get; set; }
        public decimal CurrentStock { get; set; }
        public string ServingUnit { get; set; }
        public long ServingUnitId { get; set; }

        public long RequestNoteHeaderID { get; set; }     
        public DateTime ReqNoteCreatedDate { get; set; }
        public string RequestNoteDocumentNo { get; set; }

        
        public int Sequence { get; set; }
        public decimal SIH { get; set; }
        public decimal CostValue { get; set; }
        public string ProductName { get; set; }
        public string Remark { get; set; }
    }
}