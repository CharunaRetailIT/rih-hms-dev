using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class RstRoomType : BaseEntity
    {
        public int RstRoomTypeID { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        [DataType(DataType.Text)]
        [DefaultValue(0)]
        public string RoomTypeCode { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DefaultValue("")]
        public string RoomTypeName { get; set; }

        [DefaultValue("")]
        public string BedType { get; set; }

        [DefaultValue(0)]
        public int MaxAdult { get; set; }

        [DefaultValue(0)]
        public int MaxChild { get; set; }

        [DefaultValue(0)]
        public int MaxInfant { get; set; }

        [DefaultValue(0)]
        public bool IsAC { get; set; }

        [DefaultValue(0)]
        public bool IsSmoking { get; set; }

        [DefaultValue(0)]
        public bool IsMiniBar { get; set; }

        [DefaultValue(0)]
        public bool IsNormalView { get; set; }

        [DefaultValue(0)]
        public bool IsOceanView { get; set; }

        [DefaultValue(0)]
        public bool IsLandside { get; set; }

        [DefaultValue(0)]
        public bool IsBalcony { get; set; }

        [DefaultValue(0)]
        public bool IsActive { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }
    }
}