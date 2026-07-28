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
    public class SysUserPermission : BaseEntity
    {
        public int SysUserPermissionID { get; set; }

       

        [Required]
        [DefaultValue(0)]
        public int EmployeeID { get; set; }

        [Required]
        [DefaultValue("")]
        public string EmployeeCode { get; set; }

        [MaxLength(50)]
        public string EnCode { get; set; }

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

       
        [DefaultValue(0)]
        public long GroupId { get; set; }


        //[NotMapped]
        //public bool IsGrant { get; set; }

        //[NotMapped]
        //public bool IsGrantVal { get; set; }
    }
}