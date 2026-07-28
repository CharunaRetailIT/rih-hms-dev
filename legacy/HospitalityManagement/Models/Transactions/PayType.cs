using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models.Transactions
{
    public class PayType
    {
        [Key]
        public int PaymentID { get; set; }
        public string Descrip { get; set; }
        public bool IsSwipe { get; set; }
        public int Type { get; set; }
        public decimal Rate { get; set; }
        public bool IsRefundable { get; set; }
        public bool IsActive { get; set; }
        public bool IsBillCopy { get; set; }
        public string PrintDescrip { get; set; }
        public string PreFix { get; set; }
        public int MaxLength { get; set; }


    }
}