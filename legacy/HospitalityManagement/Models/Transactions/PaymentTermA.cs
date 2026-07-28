using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models.Transactions
{
    public class PaymentTermA : BaseEntity
    {
        public long PaymenttermId { get; set; }

        public string PaymentTermCode { get; set; }

        public string PaymentTermName { get; set; }


        public int CreditPeriod { get; set; }

        public bool IsDelete { get; set; }


    }
}