using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Transactions
{
    public class CustomProductionNoteHeader : BaseEntity
    {
        //public CustomProductionNoteHeader()
        //{

        //    CustomProductionNoteDetail = new List<CustomProductionNoteDetail>();

        //}

        //[NotMapped]
        //public List<CustomProductionNoteDetail> CustomProductionNoteDetail { get; set; }

        public virtual ICollection<CustomProductionNoteDetail> CustomProductionNoteDetail { get; set; }
        public long CustomProductionNoteHeaderId { get; set; }

        [Required(ErrorMessage = "Doc number required")]
        [MaxLength(15)]
        public string DocumentNo { get; set; }
      
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Location !")]
        [DefaultValue(0)]
        public long ProcessLocId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a Location !")]
        [DefaultValue(0)]
        public long PickupLocId { get; set; }

        [MaxLength(200)]
        [DefaultValue("")]
        public string Remark { get; set; }

        [DefaultValue(false)]
        public bool IsFinished { get; set; }

        [DefaultValue(0)]
        public int DocumentId { get; set; }
       
        [DefaultValue(0)]
        public int ReciptId { get; set; }

        // required for pos ----------------------------------------------------------------------
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
        public DateTime DateFrom { get; set; }

        [NotMapped]
        public DateTime DateTo { get; set; }

        
    }
}
