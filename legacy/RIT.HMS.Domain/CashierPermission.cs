using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class CashierPermission
    {

        public CashierPermission()
        {
            CashierGroupPermissions = new List<CashierGroup>();
        }
        public long CashierPermissionId { get; set; }

        [Required]
        [DefaultValue(0)]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Location!")]
        public long LocationId { get; set; }

        [Required]
        [DefaultValue(0)]
        public long CashierId { get; set; }

        [Required]
        [DefaultValue(0)]
        [Range(1, int.MaxValue, ErrorMessage = "Please select an Employee!")]
        public long EmployeeId { get; set; }

        [Required]
        [DefaultValue("")]
        public string Password { get; set; }

        [NotMapped]
        public string ConfirmPassword { get; set; }

        [Required]
        [DefaultValue("")]
        public string JournalName { get; set; }

        [Required]
        [DefaultValue("")]
        public string EnCode { get; set; }

        [Required]
        [DefaultValue("")]
        public string FunctionName { get; set; }

        [Required]
        [DefaultValue("")]
        public string FunctionDescription { get; set; }

        [Required]
        [DefaultValue("")]
        public long Order { get; set; }

        [Required]
        [DefaultValue(0)]
        public long Value { get; set; }

        [Required]
        [DefaultValue(0)]
        public decimal MaxValue { get; set; }

        [Required]
        [DefaultValue("")]
        public string Type { get; set; }

        [Required]
        [DefaultValue("")]
        public string TypeID { get; set; }

        [Required]
        [DefaultValue(0)]
        public bool IsActive { get; set; }

        [Required]
        [DefaultValue(0)]
        public bool IsAccess { get; set; }

        [Required]
        [DefaultValue("")]
        public string Remarks { get; set; }

        [Required]
        [DefaultValue(0)]
        public bool IsDelete { get; set; }

        [Required]
        [DefaultValue(0)]
        public bool IsValue { get; set; }

        [Required]
        [DefaultValue(0)]
        public int GroupOfCompanyId { get; set; }

        [Required]
        [DefaultValue(0)]
        public string CreatedUser { get; set; }

        [Required]
        [DefaultValue(0)]
        public DateTime CreatedDate { get; set; }

        [Required]
        [DefaultValue("")]
        public string ModifiedUser { get; set; }

        [Required]
        [DefaultValue("")]
        public DateTime ModifiedDate { get; set; }

        [Required]
        [DefaultValue(0)]
        public int DataTransfer { get; set; }

        [NotMapped]
        public string CashierName { get; set; }

        [NotMapped]
        public List<CashierGroup> CashierGroupPermissions { get; set; }

        //    [] [int] NOT NULL,


    }
}