using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class PaymentTerm : BaseEntity
    {
        public long PaymenttermId { get; set; }

        [Required(ErrorMessage="*")]
        [DefaultValue("")]

        public string PaymentTermCode { get; set; }

        [Required(ErrorMessage = "*")]
        [DefaultValue("")]
        public string PaymentTermName { get; set; }

        [Required(ErrorMessage = "*")]
        [DefaultValue("")]
        public int CreditPeriod { get; set; }

        public bool IsDelete { get; set; }

      
    }
}