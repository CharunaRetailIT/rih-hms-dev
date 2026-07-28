using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class TableMaster : BaseEntity
    {
        public int TableMasterID { get; set; }
        
        [Required]
        [MaxLength(10)]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        public string TableCode { get; set; }

       // [Required(ErrorMessage = "Table Name Is Required!")]
        [DefaultValue("")]
       
        public string TableName { get; set; }

        [NotMapped]
        [DefaultValue(0)]
        public int TableNumber { get; set; }
      
        [DefaultValue(0)]
        public int NumberOfSeats { get; set; }
    
        [DefaultValue(0)]
        public int TablePositionX { get; set; }
 
        [DefaultValue(0)]
        public int TablePositionY { get; set; }
        
        [DefaultValue("")]
        public string TableState { get; set; }

    
        [DefaultValue(0)]
        public int InterDeptId { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }
      
        [NotMapped]
        public int TicketID { get; set; }


    }
}