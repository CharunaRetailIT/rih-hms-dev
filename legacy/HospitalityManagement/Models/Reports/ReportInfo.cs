using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models.Reports
{
    public class ReportInfo
    {
        public long ReportInfoId { get; set; }

        [Required]
        [DefaultValue(0)]
        public long ReportCategoryId { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [StringLength(100, MinimumLength = 2)]
        [DefaultValue("")]
        public string ReportName { get; set; }
    
        [StringLength(200)]
        [DefaultValue("")]
        public string ReportPath { get; set; }

        [StringLength(150, MinimumLength = 2)]
        [DefaultValue("")]
        public string ReportFileName { get; set; }

        [StringLength(200)]
        [DefaultValue("")]
        public string ReportURL { get; set; }
        public int OrderId { get; set; }

    }
}