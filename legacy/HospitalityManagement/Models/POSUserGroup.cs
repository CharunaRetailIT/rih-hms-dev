using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class POSUserGroup
    {
        public int POSUserGroupId { get; set; }

        [Required]
        [DefaultValue("")]
        public string POSUserGroupName { get; set; }

        [Required]
        [DefaultValue("")]
        public string POSUserGroupDesc { get; set; }

        [Required]
        [DefaultValue("")]
        public string CreatedUser{ get; set; }

        [Required]
        [DefaultValue("")]
        public DateTime CreatedDate { get; set; }

        [Required]
        [DefaultValue("")]
        public string ModifiedUser { get; set; }

        [Required]
        [DefaultValue("")]
        public DateTime ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
    }
}