using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class UnitConversion : BaseEntity 
    {
        public long UnitConversionId { get; set; }


        [Required]    
        public long UnitOfMeasureId { get; set; }

        [Required]
        [DefaultValue("")]
        public string SubUnit { get; set; }

        [Required]
        [DefaultValue(1)]
        public decimal BaseUnitValue { get; set; }

        [Required]   
        public decimal SubUnitValue { get; set; }

        [Required]
        public string SubUnitSymbol { get; set; }

    }
}