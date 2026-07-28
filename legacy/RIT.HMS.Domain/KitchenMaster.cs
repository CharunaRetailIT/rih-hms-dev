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
    public class KitchenMaster:BaseEntity
    {
        [Key]
        public long KitchenID { get; set; }

        [DefaultValue("")]
        [Column(TypeName = "VARCHAR")]
        [StringLength(10)]
        public string KitchenCode { get; set; }

        [DefaultValue("")]
        [Column(TypeName = "VARCHAR")]
        [StringLength(20)]
        public string KitchenDesc { get; set; }

        [DefaultValue("")]
        [Column(TypeName = "VARCHAR")]
        [StringLength(100)]
        public string KitchenPrinterName { get; set; }

        [DefaultValue(0)] 
        public int KitchenPrinterType { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }

        [NotMapped]
        public string CompanyName { get; set; }

        [NotMapped]
        public string PrinterTypeDesc { get; set; }

        [NotMapped]
        public string LocationName { get; set; }


    }
}
