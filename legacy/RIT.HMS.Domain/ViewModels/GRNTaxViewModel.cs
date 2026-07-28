using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.ViewModels
{
    public class GRNTaxViewModel
    {
        public long ProductId { get; set; }
        public decimal Qty { get; set; }
        public decimal FreeQty { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public long TaxId { get; set; }
        public decimal TaxPrc { get; set; }
        public decimal TaxAmount { get; set; }     
        public string ItemDesc { get; set; }
        public string UOM { get; set; }
        public decimal Discounts { get; set; }
        public decimal CostValue { get; set; }
        public bool IsExpiry { get; set; }
        public int LocationId { get; set; }
        public int CurrencyId { get; set; }
        public int PaymentMethodId { get; set; }
        public int PaymentTermId { get; set; }
        public decimal CurrencyRate { get; set; }

        public decimal TotalTaxAmount { get; set; }
        public Int32 EventId { get; set; }
        public string ReferenceNo { get; set; }
        public string Remark { get; set; }

        public decimal OrderQuantity { get; set; }
        public decimal GRNQuantity { get; set; }
        public decimal POQuantity { get; set; }
    }
}