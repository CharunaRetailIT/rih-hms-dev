using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain
{
    public class ProductKitchenMapper
    {
        [DefaultValue(1)]
        public Int64 Id { get; set; }
        public int ProductId { get; set; }
        public int SubLocationId { get; set; }
        public int GroupOfCompanyID { get; set; }
        public int CompanyID { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int DataTransfer { get; set; }
        public bool IsActive { get; set; }
    }
}
