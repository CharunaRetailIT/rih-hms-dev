using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class RstRoomMaster : BaseEntity
    {
        public int RstRoomMasterID { get; set; }  

        [Required(ErrorMessage = "The field is required")]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        [DataType(DataType.Text)]
        [DefaultValue(0)]
        public string RoomMasterCode { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DefaultValue("")]
        public string RoomName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a RoomType !")]
        public int RoomType { get; set; }

        public int Floor { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DefaultValue("")]
        public string RoomNo { get; set; }

        [DefaultValue("")]
        public string InterComNo { get; set; }

        [DefaultValue("")]
        public string RFIDNo { get; set; }

        [DefaultValue(0)]
        public bool IsActive { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }
    }
}