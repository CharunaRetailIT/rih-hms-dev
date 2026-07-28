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
    public class Bank
    {
        public int BankId { get; set; }

        [Column(TypeName = "varchar")]
        [StringLength(100)]
        public string BankName { get; set; }

        

        [DefaultValue(true)]
        public bool ISActive { get; set; }
    }
}
