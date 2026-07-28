using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class SysLocationType
    {
        [DefaultValue(0)]
        public int Id { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }
        [DefaultValue(true)]
        public bool IsActive { get; set; }
    }
}
