using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class CustomoerPreviousVisits : BaseEntity
    {
        public int CustomoerPreviousVisitsID { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DataType(DataType.Text)]
        [DefaultValue(0)]
       
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DataType(DataType.Text)]
        [DefaultValue(0)]
        public string CustomoerPreviousVisitsCode { get; set; }

        [DefaultValue("")]
        public string Description { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }
    }
}