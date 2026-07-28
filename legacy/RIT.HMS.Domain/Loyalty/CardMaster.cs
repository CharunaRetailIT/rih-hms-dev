using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Loyalty
{
    public class CardMaster : BaseEntity
    {
        public CardMaster()
        {
            LoyaltyCardSchems = new List<LoyaltyCardSchems>();
        }
        public virtual ICollection<LoyaltyCardSchems> LoyaltyCardSchems { get; set; }
        public long CardMasterId { get; set; }
        [DefaultValue(0)]
        [Range(1, int.MaxValue, ErrorMessage = "Select a card type !")]
        public int CardType { get; set; }
        [DefaultValue("")]
        [StringLength(15)]
        [Required(ErrorMessage = "Card Code field is required")]
        public string CardCode { get; set; }
        [DefaultValue("")]
        [StringLength(50)]
        [Required(ErrorMessage = "Card Name field is required")]
        public string CardName { get; set; }
        [DefaultValue(0)]     
        public decimal Discount { get; set; }
        [DefaultValue(0)]
        public decimal PointValue { get; set; }
        [DefaultValue(0)]
        public decimal MinimumPoints { get; set; }
        [DefaultValue(0)]
        public decimal ReDeemPointValue { get; set; }
        [DefaultValue("")]
        [StringLength(150)]
        public string Remark { get; set; }
        [DefaultValue(false)]
        public bool IsDelete { get; set; }

        [NotMapped]
        [DefaultValue(false)]
        public bool IsExists { get; set; }

        [NotMapped]
        [DefaultValue(false)]
        public string CardTypeName { get; set; }

    }
}
