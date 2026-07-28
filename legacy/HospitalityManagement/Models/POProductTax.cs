using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Web;

namespace HospitalityManagement.Models
{
    public class POProductTax
    {
        public long POProductTaxId { get; set; }
        public long PurchaseOrderHeaderId { get; set; }
        public long ProductId { get; set; }
        public long TaxId { get; set; }
        public decimal TaxPrecentage { get; set; }


    }
}