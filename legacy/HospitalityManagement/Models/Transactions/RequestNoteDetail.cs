using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models.Transactions
{
    public class RequestNoteDetail
    {
        public long RequestNoteDetailId { get; set; }
        [Required]
        [DefaultValue(0)]
        public long RequestnoteHeaderId { get; set; }
        [DefaultValue(0)]
        public long LineNo { get; set; }
        [DefaultValue(0)]
        public long ProductId { get; set; }
        
        [DefaultValue(0)]
        public decimal AvgCost { get; set; }
        [DefaultValue(0)]
        public decimal CostPrice { get; set; }
        [DefaultValue(0)]
        public decimal SellingPrice { get; set; }
        [DefaultValue(0)]
        public decimal RequestQty { get; set; }
        [DefaultValue(0)]
        public long UnitOfMeasureId { get; set; }
        [NotMapped]
        public string ProductName { get; set; }
        [NotMapped]
        public string UOM { get; set; }
    }
}