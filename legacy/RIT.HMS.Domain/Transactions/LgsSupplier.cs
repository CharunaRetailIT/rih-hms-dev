using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Transactions
{
   public class LgsSupplier
    {
        public long LgsSupplierID { get; set; }
        [MaxLength(15)]
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        [DefaultValue(0)]
        public bool IsDelete { get; set; }
    }
}
