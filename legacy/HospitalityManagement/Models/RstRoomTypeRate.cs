using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class RstRoomTypeRate : BaseEntity
    {
        public int RstRoomTypeRateID { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        [DataType(DataType.Text)]
        [DefaultValue(0)]
        public string RoomTypeRateCode { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DefaultValue("")]
        public string RoomTypeRateName { get; set; }

        [DefaultValue(0)]
        public decimal Rate { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        [DefaultValue(0)]
        public decimal ExtraAdultRate { get; set; }

        [DefaultValue(0)]
        public decimal ExtraChildRate { get; set; }

        [DefaultValue(0)]
        public decimal ForeignRate { get; set; }

        [DefaultValue("")]
        public string Package { get; set; }

        [DefaultValue(0)]
        public bool IsActive { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }
    }
}