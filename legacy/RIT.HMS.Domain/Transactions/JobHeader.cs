using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace RIT.HMS.Domain.Transactions
{
    public class JobHeader : BaseEntity
    {
        public JobHeader()
        {
            JobItem = new List<JobItem>();
        }

        public int JobHeaderId { get; set; }
        public string JobNumber { get; set; }
        public DateTime JobDate { get; set; }

        [NotMapped]
        public int DepartmentId { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int Status { get; set; }

        // Loaders
        [NotMapped]
        public List<SelectListItem> Locations { get; set; }
        [NotMapped]
        public List<SelectListItem> Departments { get; set; }

        [NotMapped]
        public int Messanger { get; set; }
        [NotMapped]
        public string Message { get; set; }
        [NotMapped]
        public int ExistsJobId { get; set; }
        public List<JobItem> JobItem { get; set; }
        

    }
}
