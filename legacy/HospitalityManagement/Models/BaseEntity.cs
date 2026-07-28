using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class BaseEntity
    {                        // : IBaseEntity
        public BaseEntity()
        {
            CompanyID = 1;
            GroupOfCompanyID = 1;
            CreatedUser = "";
            CreatedDate = DateTime.Now;
            ModifiedUser = "";
            ModifiedDate = DateTime.Now;
            DataTransfer = 0;
        }

        public int GroupOfCompanyID { get; set; }
        public int CompanyID { get; set; }

        public int LocationId { get; set; }

        [MaxLength(50)]
        public string CreatedUser { get; set; }

        public DateTime CreatedDate { get; set; }

        [MaxLength(50)]
        public string ModifiedUser { get; set; }

        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime ModifiedDate { get; set; }

        [DefaultValue(0)]
        public int DataTransfer { get; set; }
    }
}