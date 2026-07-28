using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
namespace RIT.HMS.Domain
{
    public class ChairMaster : BaseEntity
    {
        public int ChairMasterID { get; set; }

        [Required]
        [MaxLength(10)]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        public string ChairCode { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Table!")]
        public int TableID { get; set; }

        [DefaultValue("")]
        public string ChairName { get; set; }

        [DefaultValue(0)]
        public int TicketID { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; } 
    }
}