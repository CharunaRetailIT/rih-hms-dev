using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class Addons:BaseEntity
    {
        public int AddonsId { get; set; }
        public long ProductId { get; set; }
        public long ProductAddonId { get; set; }     
        public long DepartmentId { get; set; }
        public bool IsActive { get; set; }

        [NotMapped]
        public string AddonDesc { get; set; }
        [NotMapped]
        public string ProductDesc { get; set; }
        [NotMapped]
        public string DepartmentDesc { get; set; }     
        public decimal AddonSellingPrice { get; set; }        
        public decimal AddonQuantity { get; set; }

    }
}