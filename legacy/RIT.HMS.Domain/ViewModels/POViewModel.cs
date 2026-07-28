using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.ViewModels
{
    public class POViewModel
    {
        public long  ItemId { get; set; }
        public decimal Qty { get; set; }
        public decimal FreeQty { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal DiscountPrc { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TaxPrc { get; set; }
        public bool IsTaxOnTax { get; set; }
        public decimal ReOrderQty { get; set; }
        public int SupplierId { get; set; }
        public string ItemDesc { get; set; }
        public decimal POStock { get; set; }
        public decimal ROL { get; set; }
        public decimal ROQ { get; set; }
        public decimal CurrentStock { get; set; }
    }
}