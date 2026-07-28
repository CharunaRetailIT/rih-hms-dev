using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models.ViewModels.Reports
{
    public class CustomerDetailViewModel
    {
        public long LocationId { get; set; }
        public string Location { get; set; }

        public int CustomerID { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerTitle { get; set; }
        public string CustomerName { get; set; }
        public string CustomerType { get; set; }
        public string Address { get; set; }
        public string NIC { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }


    }
}