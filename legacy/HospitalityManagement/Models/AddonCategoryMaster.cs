using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class AddonCategoryMaster : BaseEntity
    {
        public long AddonCategoryMasterId { get; set; }
        public string AddonCatCode { get; set; }
        public bool IsActive { get; set; }
        public string AddonCatName { get; set; }
        public int MaxAddons { get; set; }
        public bool IsDelete { get; set; }
   

  

    }
}