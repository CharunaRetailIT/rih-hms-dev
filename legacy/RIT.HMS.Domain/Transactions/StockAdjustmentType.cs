using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.Transactions
{
    public class StockAdjustmentType
    {
        [Key]
        public int AdjustmentTypeId { get; set; }
        public string BaseType { get; set; }
        public string Type { get; set; }
        public bool IsActive { get; set; }
    }
}