using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class CustomerCategory : BaseEntity
    {
        public int CustomerCategoryID { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DataType(DataType.Text)]
        [DefaultValue(0)]
        public string CustomerCategoryCode { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DataType(DataType.Text)]
        [DefaultValue("")]
        public string CustomerCategoryName { get; set; }

        [DefaultValue("")]
        public string Remark { get; set; }

        [DefaultValue(0)]
        public bool IsActive { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }

        [DefaultValue(0)]
        public bool IsVat { get; set; }

        [DefaultValue(0)]
        public decimal DiscountPrc { get; set; }
    }
}