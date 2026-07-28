using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class RstMealType : BaseEntity
    {
        public int RstMealTypeId { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        [MaxLength(10)]
        public string RstMealTypeCode { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DefaultValue("")]
        public string Description { get; set; }

        [DefaultValue(0)]
        public bool IsActive { get; set; }

    }
}