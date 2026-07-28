using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class StewardsMaster : BaseEntity
    {
        public int StewardsMasterID { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DataType(DataType.Text)]
        [DefaultValue(0)]
        public string StewardCode { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DataType(DataType.Text)]
        [DefaultValue("")]
        public string StewardTitle { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DataType(DataType.Text)]
        [DefaultValue("")]
        public string StewardName { get; set; }

        [DefaultValue("")]
        public string Address1 { get; set; }

        [DefaultValue("")]
        public string Address2 { get; set; }

        [DefaultValue("")]
        public string Address3 { get; set; }

        public DateTime DOB { get; set; }

        public string NIC { get; set; }

        public string Passport { get; set; }

        public string Telephone { get; set; }

        public string Mobile { get; set; }

        public string Fax { get; set; }

        public string Email { get; set; }

        public string Target { get; set; }

        public decimal Commission { get; set; }

        [DefaultValue(0)]
        public bool IsDeliveryPerson { get; set; }

        [DefaultValue(0)]
        public bool IsKarokeGirl { get; set; }

        [DefaultValue("")]
        public byte[] Picture { get; set; }

        [DefaultValue(0)]
        public bool IsActive { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }

    }
}