using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain
{
    public class InvAdvancePaymentDet
    {
        public long InvAdvancePaymentDetId { get; set; }      
        [DefaultValue(0)]
        public long Idx { get; set; }
        [DefaultValue(0)]
        public long RowNo { get; set; }
        [DefaultValue(0)]
        public int PayTypeID { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }    
        public DateTime SDate { get; set; }
        [Column(TypeName = "char")]
        [StringLength(10)]
        [Required]
        public string Receipt { get; set; }
        public int LocationID { get; set; }
        public long CashierID { get; set; }
        public int UnitNo { get; set; }
        public int BillTypeID { get; set; }
        [Column(TypeName = "varchar")]
        [StringLength(30)]
        [Required]
        public string RefNo { get; set; }
        public long BankId { get; set; }
        [Column(TypeName = "Date")]
        public DateTime? ChequeDate { get; set; }
        public bool IsRecallAdv { get; set; }
        [Column(TypeName = "varchar")]
        [StringLength(10)]
        [Required]
        public string RecallNo { get; set; }
        [Column(TypeName = "varchar")]
        [StringLength(20)]
        [Required]
        public string Descrip { get; set; }
        [Column(TypeName = "varchar")]
        [StringLength(50)]
        [Required]
        public string EnCodeName { get; set; }
        [Column(TypeName = "nchar")]
        [StringLength(50)]
        [Required]
        public string SuspendNo { get; set; }
        public bool SuspendBy { get; set; }
        public bool IsDeleteOnRecall { get; set; }

        [Column(TypeName = "varchar")]
        [StringLength(20)]
        [Required]
        public string AdvanceNumber { get; set; }
        
    }
}
