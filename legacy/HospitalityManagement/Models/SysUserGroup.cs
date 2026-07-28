using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class SysUserGroup:BaseEntity
    {
        public int SysUserGroupID { get; set; }

        [Required()]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        [MaxLength(15)]
        public string UserGroupCode { get; set; }

        [Required()]
        [MaxLength(50)]
        public string UserGroupName { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }

      

    }
}