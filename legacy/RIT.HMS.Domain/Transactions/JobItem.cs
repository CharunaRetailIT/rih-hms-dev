using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Transactions
{
    public class JobItem
    {

        public int JobItemId { get; set; }
        public int JobHeaderId { get; set; }
        public int ProductId { get; set; }
        public decimal SystemQty { get; set; }
        public decimal PhysicalQty { get; set; }
        public int Status { get; set; }
        public int DepartmentId { get; set; }
        public int LocationId { get; set; }
        [NotMapped]
        public string DepartmentCode { get; set; }
        [NotMapped]
        public string DepartmentName { get; set; }

        [NotMapped]
        public string ProductCode { get; set; }
        [NotMapped]
        public string ProductName { get; set; }

        public JobHeader JobHeader { get; set; }
    }
}
