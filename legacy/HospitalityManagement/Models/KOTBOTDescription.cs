using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class KOTBOTDescription
    {
      
        
       
        public long KOTBOTDescriptionId { get; set; }

        [Required(ErrorMessage = "*")]
        public string Description { get; set; }

        [Required]
        [DefaultValue("")]
        public string Type { get; set; }
        public bool IsActive { get; set; }

        public DateTime ModifiedDate { get; set; }

        [NotMapped]
        public long ProductId { get; set; }
    }
}