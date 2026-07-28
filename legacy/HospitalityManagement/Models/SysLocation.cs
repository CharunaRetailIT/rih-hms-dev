using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class SysLocation
    {
        public int SysLocationID { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DataType(DataType.Text)]
        [DefaultValue(0)]
        public string LocationCode { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DataType(DataType.Text)]
        [DefaultValue("")]
        public string LocationName { get; set; }

        [DefaultValue("")]
        public string Address1 { get; set; }

        [DefaultValue("")]
        public string Address2 { get; set; }

        [DefaultValue("")]
        public string Address3 { get; set; }

        [DefaultValue("")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Only Numbers allowed")]
        public string Telephone { get; set; }

        [DefaultValue("")]
        public string CostCenter { get; set; }

        [DefaultValue("")]
        public string Fax { get; set; }

        [DefaultValue("")]
        public string Email { get; set; }

        public string ContactPersonName { get; set; }

        public string OtherBusinessName { get; set; }

        public string LocationPrefixCode { get; set; }

        [DefaultValue(1)]
        public bool IsVAT { get; set; }

        [DefaultValue(1)]
        public bool IsStockLocation { get; set; }

        public bool IsHeadOffice { get; set; }

        public string LocationIP { get; set; }

        public bool IsActive { get; set; }

        public bool IsDelete { get; set; }

       
        public int GroupOfCompanyID { get; set; }
        public int CompanyID { get; set; }

        [MaxLength(50)]
        public string CreatedUser { get; set; }

        public DateTime? CreatedDate { get; set; }

        [MaxLength(50)]
        public string ModifiedUser { get; set; }

        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? ModifiedDate { get; set; }

        [DefaultValue(0)]
        public int DataTransfer { get; set; }

        [NotMapped]
        public string CompanyName { get; set; }

        [NotMapped]
        public string GOCName { get; set; }

        [DefaultValue(true)]
        public bool IsShowRoom { get; set; }

        [NotMapped]
        public bool InheritProducts { get; set; }


    }
}