using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class SysCompany
    {
        public int SysCompanyID { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DataType(DataType.Text)]
        [DefaultValue(0)]
        public string CompanyCode { get; set; }

        [Required(ErrorMessage = "The field is required")]
        //[RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        [DataType(DataType.Text)]
        [DefaultValue("")]
        public string CompanyName { get; set; }

        //[Range(1, int.MaxValue, ErrorMessage = "Please select a Group of Company !")]
        [DefaultValue(0)]
        public int SysGroupOfCompanyId { get; set; }

        [DefaultValue("")]
        public string OtherBusinessName1 { get; set; }

        [DefaultValue("")]
        public string OtherBusinessName2 { get; set; }

        [DefaultValue("")]
        public string OtherBusinessName3 { get; set; }

        [DefaultValue("")]
        public string Address1 { get; set; }

        [DefaultValue("")]
        public string Address2 { get; set; }

        [DefaultValue("")]
        public string Address3 { get; set; }

        [DefaultValue("")]
       // [RegularExpression("^[0-9]*$", ErrorMessage = "Only Numbers allowed")]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        public string Telephone { get; set; }

        [DefaultValue("")]
       // [RegularExpression("^[0-9]*$", ErrorMessage = "Only Numbers allowed")]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        public string Mobile { get; set; }

        [DefaultValue("")]
        //[RegularExpression("^[0-9]*$", ErrorMessage = "Only Numbers allowed")]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        public string Fax { get; set; }

        [DefaultValue("")]
        public string ContactPerson { get; set; }

        [DefaultValue("")]
        public string Website { get; set; }

        public string TaxID1 { get; set; }

        public string TaxID2 { get; set; }

        public string TaxID3 { get; set; }

        public string TaxRegistrationNo1 { get; set; }

        public string TaxRegistrationNo2 { get; set; }

        public string TaxRegistrationNo3 { get; set; }

        public bool IsVat { get; set; }

        [DefaultValue(0)]
        public bool IsActive { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }
        [NotMapped]
        public string GroupOfCompanyName { get; set; }
    }
}