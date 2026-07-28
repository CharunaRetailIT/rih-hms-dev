using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class SysUserGroupPermission : BaseEntity
    {
        public int SysUserGroupPermissionID { get; set; }

        [Required]
       
        public int SysUserGroupId { get; set; }


        [Required]
        [MaxLength(100)]
        public string FunctionName { get; set; }

        [Required]
        [MaxLength(250)]
        public string FunctionDescription { get; set; }

        [DefaultValue(0)]
        public int Order { get; set; }

        [Required]
        [DefaultValue(0)]
        public decimal Value { get; set; }

        [Required]
        [DefaultValue(0)]
        public decimal MaxValue { get; set; }

        public string Type { get; set; }

        public int TypeID { get; set; }

        [DefaultValue(0)]
        public bool IsActive { get; set; }

        [DefaultValue(0)]
        public bool IsAccess { get; set; }

        [MaxLength(500)]
        public string Remarks { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }

        [NotMapped]
        [DefaultValue(0)]
        public bool IsGrant { get; set; }
    }
}