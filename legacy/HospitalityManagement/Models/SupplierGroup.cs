using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class SupplierGroup : BaseEntity
    {
        public int SupplierGroupID { get; set; }

        [Required]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        [MaxLength(20)]
        public string SupplierGroupCode { get; set; }
        [Required]
        [MaxLength(50)]
        public string SupplierGroupName { get; set; }
        [DefaultValue("")]
        [MaxLength(150)]
        public string Remark { get; set; }

        [DefaultValue(0)]

        public bool IsDelete { get; set; }


    }
}