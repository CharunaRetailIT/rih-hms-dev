using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class CashierFunction
    {

        public long CashierFunctionId { get; set; }

        [Required]
        [DefaultValue("")]
        public string FunctionName { get; set; }

        [Required]
        [DefaultValue("")]
        public string FunctionDescription { get; set; }

        [Required]
        [DefaultValue(0)]
        public long Order { get; set; }

        [Required]
        [DefaultValue(0)]
        public int TypeID { get; set; }

        [Required]
        [DefaultValue(0)]
        public bool IsDelete { get; set; }


        [Required]
        [DefaultValue(0)]
        public bool IsValue { get; set; }

        [Required]
        [DefaultValue(0)]
        public int GroupOfCompanyID { get; set; }


        [Required]
        [DefaultValue("")]
        public string CreatedUser { get; set; }

        [Required]
        [DefaultValue("")]
        public DateTime CreatedDate { get; set; }



        [Required]
        [DefaultValue("")]
        public string ModifiedUser { get; set; }

        [Required]
        [DefaultValue("")]
        public DateTime ModifiedDate { get; set; }



    }
}