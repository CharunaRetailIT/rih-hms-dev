using RIT.HMS.Domain.Transactions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.Transactions
{
    public class ProductionNoteHeader:BaseEntity
    {

        public ProductionNoteHeader()
        {

            ProductionDetail = new List<ProductionNoteDetail>();

        }
        public long ProductionNoteHeaderId { get; set; }

        [Required(ErrorMessage ="Doc number required")]
        [MaxLength(15)]
        public string DocumentNo { get; set; }

        [Required(ErrorMessage = "Payment Method is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Location !")]
        [DefaultValue(0)]
        public long ProductionLocId { get; set; }

       
        [MaxLength(200)]
        [DefaultValue("")]
        public string Remark { get; set; }


        public long ProductId { get; set; }

        [DefaultValue(0)]
        public decimal ProductCostPrice { get; set; }

        [DefaultValue(0)]
        public decimal ProductSellingPrice { get; set; }

        [DefaultValue(0)]
        public decimal ProductDiscounts { get; set; }

        [DefaultValue(0)]
        public decimal ProductQty { get; set; }

        [DefaultValue(0)]
        public bool IsTempPN { get; set; }

        [NotMapped]
        public List<ProductionNoteDetail> ProductionDetail { get; set; }

        [NotMapped]
        public string ProductName { get; set; }
    
        [DefaultValue(false)]
        public bool IsFinished { get; set; }

        [DefaultValue(0)]
        public int DocumentId { get; set; }

        [NotMapped]
        public string Mode { get; set; }

        [DefaultValue(0)]
        public int ReceiptLocID { get; set; }

        [DefaultValue(0)]
        public int R_Zno { get; set; }

        [DefaultValue("")]
        [StringLength(40)]
        public string ReceiptNo { get; set; }

        [DefaultValue(0)]
        public int UnitNo { get; set; }

        [NotMapped]
        public int RequestNoteId { get; set; }

        [NotMapped]
        [DefaultValue(0)]
        public decimal ActualQty { get; set; }
    }
}