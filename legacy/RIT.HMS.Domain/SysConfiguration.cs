using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
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
        [DefaultValue(false)]
        public bool CreateGroupOfCompanies { get; set; }
        [DefaultValue(false)]
        public bool CreateCompanies { get; set; }

    }
}