using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class PaidInType
    {

        public int PaidInTypeId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool IsSalesSummery { get; set; }
        public bool IsDelete { get; set; }
        public int GroupOfCompanyId { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int DataTransfer { get; set; }

     
    }
}