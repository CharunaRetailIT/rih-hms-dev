using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class KitchenPrinterTypes : BaseEntity
    {
        public long ID { get; set; }

        [DefaultValue(0)]
        public int ProductID { get; set; }

        [DefaultValue(0)]
        public int LocationID { get; set; }

        [DefaultValue(0)]
        public int PrinterID { get; set; }

    }
}
