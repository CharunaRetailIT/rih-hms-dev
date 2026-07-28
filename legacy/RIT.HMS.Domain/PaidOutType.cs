using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class PaidOutType
    {

        public int PaidOutTypeId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool IsSalesSummery { get; set; }
        public bool IsDelete { get; set; }
        public int DayFrom { get; set; }
        public int DayTo { get; set; }
        public int GroupOfCompanyId { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int DataTransfer { get; set; }


    }
}