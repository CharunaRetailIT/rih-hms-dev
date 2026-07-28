using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class KitchenPrinterTypes
    {
        public long ID { get; set; }

        [DefaultValue(0)]
        public int ProductID { get; set; }

        [MaxLength(50)]
        public string PrinterName { get; set; }

        [DefaultValue(0)]
        public int LocationID { get; set; }

        [DefaultValue(0)]
        public int PrinterID { get; set; }

        public DateTime CreatedDate { get; set; }

        [MaxLength(50)]
        public string CreatedUser { get; set; }

        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime ModifiedDate { get; set; }
        

        [MaxLength(50)]
        public string ModifiedUser { get; set; }


    }
}
