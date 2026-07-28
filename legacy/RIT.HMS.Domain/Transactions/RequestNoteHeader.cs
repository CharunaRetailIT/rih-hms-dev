using RIT.HMS.Domain.Transactions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.Transactions
{
    public class RequestNoteHeader 
    {
        public RequestNoteHeader()
        {
            RequestDetail = new List<RequestNoteDetail>();
        }

        public long RequestnoteHeaderId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a From Location !")]
        [DefaultValue(0)]

        public int FromLocationId { get; set; }

       [Range(0, int.MaxValue, ErrorMessage = "Please select a From Department !")]
        [DefaultValue(0)]

        public int FromDepartmentId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a To Location !")]

        [DefaultValue(0)]

        public int ToLocationId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Please select a To Department !")]
        [DefaultValue(0)]

        public int ToDepartmentId { get; set; }

        [DefaultValue("")]
        [MaxLength(20)]
        public string DocumentNo { get; set; }

        public DateTime DocumentDate { get; set; }
     
        [DefaultValue("")]
        [MaxLength(20)]
        public string ReferenceNo { get; set; }

        [DefaultValue("")]
        [MaxLength(150)]
        public string Remark { get; set; }

        [NotMapped]
        [DefaultValue(0)]
        public int RequestStatus { get; set; }
         
        [DefaultValue(0)]
        public bool IsActive { get; set; }  
          
        [NotMapped]
        public List<RequestNoteDetail> RequestDetail { get; set; }

        [DefaultValue(0)]

        public decimal TotSellingPrice { get; set; }

        [DefaultValue(0)]

        public decimal TotCostPrice { get; set; }

    
        [DefaultValue(false)]
        public bool IsTempRequest { get; set; }

        [DefaultValue(false)]
        public bool IsApproved { get; set; }

        [NotMapped]
        public string FromLocation { get; set; }

        [NotMapped]
        public string FromDepartment { get; set; }

        [NotMapped]
        public string ToLocation { get; set; }

        [NotMapped]
        public string ToDepartment { get; set; }
                
        [DefaultValue(0)]
        public int DocumentId { get; set; }

        public int DocumentStatus { get; set; }

        [DefaultValue("")]
        [MaxLength(20)]
        public string NewDocNumber { get; set; }


       // [NotMapped]
        [DefaultValue("")]
        [MaxLength(200)]
        public string RejectReason { get; set; }

      //  [NotMapped]
        [DefaultValue("")]
        [MaxLength(200)]
        public string CancelReason { get; set; }

        [NotMapped]
        public string RequestedBy { get; set; }

        // [NotMapped]
        [DefaultValue("")]
        public string RequestType { get; set; }

        [DefaultValue(0)]
        public int CompanyId { get; set; }

    
        [DefaultValue(0)]
        public bool IsPoTransfer { get; set; }

        [DefaultValue(false)]
        [NotMapped]
        public bool IsCreateBasedOnThis { get; set; }


        [NotMapped]
        [DefaultValue("")]
        public string BasedOnRequestNoteNo { get; set; }
        
        
        public DateTime ExpectedDeleveryDate { get; set; } = DateTime.Now;
        //  [NotMapped]

        
        public DateTime CanceledDate { get; set; } = DateTime.Parse("1999-09-29 00:00:00.000");

       
        public string CanceleddBy { get; set; }

        [NotMapped]
        public virtual SysCompany SysCompany { get; set; }

        [NotMapped]
        public string Company { get; set; }

        [NotMapped]
        public string DocState { get; set; }
        [NotMapped]
        public int PrintStatus { get; set; }
    }
}