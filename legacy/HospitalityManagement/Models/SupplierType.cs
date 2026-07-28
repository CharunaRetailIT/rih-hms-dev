using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class SupplierType : BaseEntity
    {
        public int SupplierTypeID { get; set; }



        [Required]

        [MaxLength(20)]

        public string SupplierTypeCode { get; set; }



        [Required]

        [MaxLength(50)]

        public string SupplierTypeName { get; set; }



        [DefaultValue("")]

        [MaxLength(150)]

        public string Remark { get; set; }



        [DefaultValue(0)]

        public bool IsDelete { get; set; }
    }
}