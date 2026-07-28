using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class CashierGroup
    {

        
        public int CashierGroupId { get; set; }
        public int EmployeeGroupId{ get; set; }
        [Required]
        [DefaultValue("")]
        public string FunctionName { get; set; }
        [Required]
        [DefaultValue("")]
        public string FunctionDescription { get; set; }
        [Required]
        [DefaultValue(0)]
        public int Order { get; set; }
        [Required]
        [DefaultValue(0)]
        public decimal Value { get; set; }     
        [DefaultValue("")]
        public string Type { get; set; }
        [Required]
        [DefaultValue(0)]
        public int TypeId { get; set; }
        [Required]
        [DefaultValue(0)]
        public bool IsAccess { get; set; }

        [Required]
        [DefaultValue(0)]
        public bool IsValue { get; set; }

        [Required]
        [DefaultValue(0)]
        public int GroupOfCompanyId { get; set; }

        [Required]
        [DefaultValue("")]
        public string CreatedUser { get; set; }

        [Required]     
        public DateTime CreatedDate { get; set; }

        [Required]
        [DefaultValue("")]
        public string ModifiedUser { get; set; }

        [Required]
        public DateTime ModifiedDate { get; set; }

        [Required]
        [DefaultValue(0)]
        public int DataTransfer { get; set; }

        [NotMapped]
        public bool  IsGrant { get; set; }

        [NotMapped]
        public decimal MaxValue { get; set; }


    }
}