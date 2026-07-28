using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class DocumentNumber : BaseEntity
    {
        public long DocumentNumberId { get; set; }

        [Required]
        public int DocumentId { get; set; }

        [Required]
        [DefaultValue("")]
        public string DocumentName { get; set; }
     

        [Required]
        [DefaultValue(0)]
        public long DocumentNo { get; set; }

        [Required]
        [DefaultValue("")]
        public long TempDocumentNo { get; set; }

        [Required]
        [DefaultValue("")]
        public string TemplateDocumentNo { get; set; }

        [Required]
        [DefaultValue(0)]
        public int DocumentYear { get; set; }

        [Required]
        [DefaultValue("")]
        public string PrefixCode { get; set; }
        
    }
}