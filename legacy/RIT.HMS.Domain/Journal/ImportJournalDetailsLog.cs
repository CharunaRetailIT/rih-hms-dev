using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Journal
{
   public class ImportJournalDetailsLog
    {
        [Key]
        public int Id { get; set; }
        [Column(TypeName = "Varchar")]
        [StringLength(50)]
        public string DocumentNumber { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        
    }
}
