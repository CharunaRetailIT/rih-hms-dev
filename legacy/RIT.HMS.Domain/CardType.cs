using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain
{
    public class CardType
    {
        public int CardTypeId { get; set; }

        [Column(TypeName = "varchar")]
        [StringLength(50)]
        public string CardTypeName { get; set; }
        public bool IsActive { get; set; }
    }
}
