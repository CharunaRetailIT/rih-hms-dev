using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class SysConfiguration
    {
        [Key]
        public long ConfigId { get; set; }
        public string SysName { get; set; }
        public decimal VAT { get; set; }
        public decimal NBT { get; set; }
        public int MaxLoginAttemts  { get; set; }
        public bool BatchWiseGRN { get; set; }
        public string Version { get; set; }
        public bool IsTaxInclusiveToCost { get; set; }

      



    }
}