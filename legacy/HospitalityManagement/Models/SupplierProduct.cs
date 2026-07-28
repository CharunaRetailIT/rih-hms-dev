using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class SupplierProduct : BaseEntity
    {
        public long SupplierProductId { get; set; }
        public int SupplierId { get; set; }
        public int ProductId { get; set; }
        [NotMapped]
        public string Supplier { get; set; }
    }
}