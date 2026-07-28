using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain
{
   public class InvPosTerminalDetails
    {

        [Key]
        public long InvPosTerminalDetailsID { get; set; }

        [DefaultValue(0)]
        public int LocationID { get; set; }
        [DefaultValue(0)]
        public int TerminalId { get; set; }

        [DefaultValue("")]
        public string IP { get; set; }

        [DefaultValue("")]
        public string DBNAME { get; set; }

        [DefaultValue("")]
        public string UserId { get; set; }

        [DefaultValue("")]
        public string PWD { get; set; }

        [DefaultValue("")]
        public string JrnlPath { get; set; }

        [DefaultValue(0)]
        public int CompanyID { get; set; }
      
        [StringLength(50)]
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
            
        [StringLength(50)]
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int DataTransfer { get; set; }
  
    }
}
