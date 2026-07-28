using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace HospitalityManagement.Models
{
    public class SysUserMaster : BaseEntity
    {

        public SysUserMaster()
        {
            SysUserGroupPermission = new List<SysUserGroupPermission>();
        }

        public int SysUserMasterID { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [MaxLength(15)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [MaxLength(100)]
        public string UserDescription { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 3)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("Password", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a Group !")]

        public long UserGroupID { get; set; }

        [DefaultValue(0)]
        public bool IsActive { get; set; }

        [DefaultValue(0)]
        public bool IsUserCantChangePassword { get; set; }

        [DefaultValue(0)]
        public bool IsUserMustChangePassword { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }


        //[Range(0, int.MaxValue, ErrorMessage = "Please select an Employee !")]
        [MaxLength(15)]
        public string EmployeeCode { get; set; }

        //[NotMapped]
        //public string permissionlist { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DefaultValue("")]
        public string Email { get; set; }

        [NotMapped]
        public string OldPassword { get; set; }

        [NotMapped]
        public List<SysUserGroupPermission> SysUserGroupPermission { get; set; }

      
    }
}