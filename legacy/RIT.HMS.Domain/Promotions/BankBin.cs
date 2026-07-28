using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Promotions
{
   public class BankBin
    {
       // [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BankBinId { get; set; }

        [Column(TypeName = "nchar")]
        [StringLength(100)]       
        public string CardPfx { get; set; }

        [Column(TypeName = "nchar")]
        [StringLength(250)]
        public string CardName { get; set; }

        [Column(TypeName = "nchar")]
        [StringLength(250)]
        public string CardType { get; set; }
        public int CardID { get; set; }
        public int BankID { get; set; }
        public string BankName { get; set; }
        public decimal Rate { get; set; }
        public decimal DiscountPercentage { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal ValueFrom { get; set; }
        public decimal ValueTo { get; set; }
        public decimal DiscountAmount { get; set; }

        public int LocationId { get; set; }

        [DefaultValue(false)]
        public bool IsValidForGVSales { get; set; }

        [DefaultValue(false)]
        public bool IsCombined { get; set; }  
        public int PromotionID { get; set; }

        [NotMapped]
        public string[] LocationIds { get; set; }
        [NotMapped]
        public string Location { get; set; }

        [DefaultValue(0)]
        public int CompanyId { get; set; }
    }
}
