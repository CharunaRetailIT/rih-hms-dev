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
    public class PayTypeTax:BaseEntity
    {
        public long PayTypeTaxId { get; set; }
        [Required]
        [DefaultValue(0)]
        public long PayTypeId { get; set; }
        [Required]
        [DefaultValue(0)]
        public long TaxId { get; set; }
        [NotMapped]
        public string TaxDescription { get; set; }
        [DefaultValue(0)]
        public decimal TaxPracentage { get; set; }
        [DefaultValue(0)]
        public int TaxSequence { get; set; }

    }
}
