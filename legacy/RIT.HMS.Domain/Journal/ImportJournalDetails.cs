using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Journal
{
    public class ImportJournalDetails
    {
        [Key]
        public decimal REFINDEX { get; set; }

        [Column(TypeName = "Varchar")]
        [StringLength(15)]
        public string EXBATCH { get; set; }

        [Column(TypeName = "nchar")]
        [StringLength(2)]
        public string TRANTYPE { get; set; }

        [Column(TypeName = "Varchar")]
        [StringLength(15)]
        public string DOCNO { get; set; }

        [Column(TypeName = "Varchar")]
        [StringLength(15)]
        public string DOCNO1 { get; set; }

        public DateTime DATE { get; set; }


        public DateTime DUEDATE { get; set; }
       
        public long SEQNO { get; set; }
        [Column(TypeName = "Varchar")]
        [StringLength(15)]
        public string ACODE { get; set; }
        [Column(TypeName = "Varchar")]
        [StringLength(3)]
        public string CCODE { get; set; }
        [Column(TypeName = "Varchar")]
        [StringLength(1)]
        public string DRCR { get; set; }
        [Column(TypeName = "Varchar")]
        [StringLength(250)]
        public string DESCRIPTION { get; set; }
        [Column(TypeName = "numeric")]
        public decimal AMOUNT { get; set; }
        [Column(TypeName = "varchar")]
        public string CQNO { get; set; }
        public DateTime? CQDATE { get; set; }
        [Column(TypeName = "Varchar")]
        [StringLength(4)]
        public string BANK { get; set; }
        [Column(TypeName = "Varchar")]
        [StringLength(4)]
        public string BANKBRANCH { get; set; }
        public bool PROCESS { get; set; }
        public bool GLPOST { get; set; }
        [StringLength(10)]
        public string GLPOSTUSER { get; set; }
        public DateTime GLPOSTDATETIME { get; set; }
        [StringLength(50)]
        public string GLPOSTCPNAME { get; set; }
        public bool CUSTOMER { get; set; }
        [StringLength(250)]
        public string CUSTOMERCODE { get; set; }
        public bool SUPPLIER { get; set; }
        public bool ISTAX { get; set; }
        public bool ADDITION { get; set; }
        public bool DEDUCTION { get; set; }
        public bool ISPAIDIN { get; set; }
        public bool ISPAIDOUT { get; set; }
        public bool ISCREDITED { get; set; }


        
    }
}
