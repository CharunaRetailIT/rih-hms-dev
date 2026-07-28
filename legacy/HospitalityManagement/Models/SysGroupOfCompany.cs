using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class SysGroupOfCompany
    {
        public int SysGroupOfCompanyId { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        [DataType(DataType.Text)]
        [DefaultValue(0)]
        public string GroupOfCompanyCode { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DataType(DataType.Text)]
        [DefaultValue("")]
        public string GroupOfCompanyName { get; set; }

        [DefaultValue("")]
        public string CompanyGmail { get; set; }

        [DefaultValue("")]
        public string CompanyVatNumber { get; set; }

        [DefaultValue(0)]
        
        public bool IsActive { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }

        [DefaultValue("")]
        public string CompanyLogoType { get; set; }

        [DefaultValue("")]
        public string CompanyLogoName { get; set; }

        [DefaultValue("")]
        public byte[] CompanyLogo { get; set; }

        [NotMapped]
        public HttpPostedFileBase File { get; set; }
    }
}