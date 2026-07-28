using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models.Transactions
{
    public class TransferNoteHeader : BaseEntity
    {
        public TransferNoteHeader()
        {
            
            TOGDetail = new List<TransferNoteDetail>();
            
        }
        
        public long TransferNoteHeaderId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a From Location !")]
        [DefaultValue(0)]
        public int FromLocationId { get; set; }
        
        [DefaultValue(0)]

        public int CostCentreId { get; set; }
        
        [DefaultValue("")]

        [MaxLength(20)]

        public string DocumentNo { get; set; }
        

        public DateTime DocumentDate { get; set; }
        
        
        [DefaultValue(0)]

        public long ReferenceDocumentId { get; set; }
        
        [DefaultValue("")]

        [MaxLength(20)]

        public string ReferenceNo { get; set; }
        
       
        
        [NotMapped]

        [DefaultValue(0)]

        public int TransStatusId { get; set; }


        [Range(1, int.MaxValue, ErrorMessage = "Please select a To Location !")]
        [DefaultValue(0)]
        public int ToLocationId { get; set; }
        
        [DefaultValue(0)]

        public decimal GrossAmount { get; set; }
        

        [DefaultValue(0)]

        public decimal NetAmount { get; set; }
        
        [DefaultValue("")]

        [MaxLength(150)]

        public string Remark { get; set; }
        
        [DefaultValue(0)]

        public int DocumentStatus { get; set; }
        
        [DefaultValue(0)]

        public bool IsDelete { get; set; }
        
        public virtual ICollection<TransferNoteDetail> TransferNoteDetail { get; set; }
        
        [NotMapped]
        
        public List<TransferNoteDetail> TOGDetail { get; set; }


        [DefaultValue(0)]

        public decimal TotSellingPrice { get; set; }

        [DefaultValue(0)]

        public decimal TotCostPrice { get; set; }

        //[NotMapped]
        //[DefaultValue(false)]
        //public bool TOGType { get; set; }

       
        [DefaultValue("")]
        public string TOGType { get; set; }

     
        [DefaultValue("")]
        public int DocumentId { get; set; }


        [DefaultValue(0)]
        public decimal TotQty { get; set; }

        [DefaultValue(false)]
        public bool IsTempTOG { get; set; }


        [NotMapped]
        public string Company { get; set; }
        [NotMapped]
        public string From { get; set; }
        [NotMapped]
        public string To { get; set; }

        public DateTime TOGDate { get; set; }

        [NotMapped]
        public virtual SysCompany SysCompany { get; set; }

        [NotMapped]
        public int GRNId { get; set; }


    }
}