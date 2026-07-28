using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.Reports
{
    public class ReportCategory
    {
        public int ReportCategoryId { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DataType(DataType.Text)]
        [DefaultValue(0)]
        public string ReportCategoryCode { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DefaultValue("")]
        public string ReportCategoryName { get; set; }
        public int OrderId { get; set; }
        public string Permission { get; set; }
    }
}