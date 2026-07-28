using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class PaymentMethod :BaseEntity
    {
        public long PaymentMethodId { get; set; }

        [Required]
        [DefaultValue("")]
        public string PaymentMethodCode { get; set; }

        [Required]
        [DefaultValue("")]
        public string PaymentMethodName { get; set; }

        [Required]
        [DefaultValue(0.0)]
        public decimal CommissionRate { get; set; }

        [Required]
        [DefaultValue(0)]
        public decimal PaymentType { get; set; }

        public bool IsPaymentType { get; set; }

        public bool IsReceiptType { get; set; }

        public bool IsActive { get; set; }

        public bool IsDelete { get; set; }
        
       
    }
}