using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.HMSOrderTaker.Domain.ViewModels
{
    public class vmTableMaster
    {
        public int TableMasterID { get; set; }
        public string TableCode { get; set; }
        public string TableName { get; set; }
        public bool IsDelete { get; set; }
        public int GroupOfCompanyID { get; set; }
        public int CompanyID { get; set; }
        public int LocationId { get; set; }
        public string CreatedUser { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public int DataTransfer { get; set; }
        public int NumberOfSeats { get; set; }
        public string TableState { get; set; }
        public int TablePositionX { get; set; }
        public int TablePositionY { get; set; }
        public int InterDeptId { get; set; }
    }
}
