using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class ProductAddons : BaseEntity
    {    
        public int ProductAddonsId { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DefaultValue(0)]
        public string ProductAddonCode { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DefaultValue("")]
        public string ProductAddonName { get; set; }
        public long ProductId { get; set; }      
        public long DepartmentId { get; set; }
        public bool IsActive { get; set; }
    }
}