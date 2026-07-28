using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RIT.HMS.Domain
{
    public class ServingUnit : BaseEntity
    {
        public long ServingUnitId { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DataType(DataType.Text)]
        [DefaultValue("")]
        public string ServingUnitName { get; set; }

        public bool IsActive { get; set; }

        public bool IsDelete { get; set; }
    }
}
