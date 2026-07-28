using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models.ViewModels.Reports
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
    }
}