using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class RstKotCategory : BaseEntity
    {
        public int RstKotCategoryID { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DataType(DataType.Text)]
        [DefaultValue(0)]
        public string RstKotCategoryCode { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DefaultValue("")]
        public string RstKotCategoryName { get; set; }

        [DefaultValue("")]
        public string IPAddress { get; set; }

        [DefaultValue("")]
        public string PrinterName { get; set; }

        [DefaultValue("")]
        public string COMName { get; set; }

        [DefaultValue(0)]
        public bool IsActive { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }
    }
}