using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class SysUserFunction :BaseEntity
    {
        public int SysUserFunctionID { get; set; }

        [Required]
        [MaxLength(30)]
        public string FunctionName { get; set; }

        [Required]
        [MaxLength(100)]
        public string FunctionDescription { get; set; }

     
        public int Order { get; set; }

        public int TypeID { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }

        [DefaultValue(1)]
        public bool IsValue { get; set; }
    }
}