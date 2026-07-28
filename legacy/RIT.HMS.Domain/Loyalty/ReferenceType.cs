using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Loyalty
{
    public class ReferenceType : BaseEntity
    {
        public int ReferenceTypeId { get; set; }
        [StringLength(25)]
        public string LookupType { get; set; }
        [DefaultValue(0)]
        public int LookupKey { get; set; }
        [StringLength(100)]
        public string LookupValue { get; set; }
        [StringLength(100)]
        public string Remark { get; set; }
        [DefaultValue(false)]
        public int IsDelete { get; set; }
      
      
    }
}
