using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class UnitOfMeasure : BaseEntity
    {
        public long UnitOfMeasureId { get; set; }


        [Required]
        [MaxLength(15)]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        public string UnitOfMeasureCode { get; set; }


        [Required]
        [MaxLength(50)]
        public string UnitOfMeasureName { get; set; }

        [DefaultValue("")]
        [MaxLength(150)]
        public string Remark { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }
    }
}