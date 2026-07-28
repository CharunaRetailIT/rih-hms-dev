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
    public class RequestNoteAccptanceHeader
    {

        public RequestNoteAccptanceHeader()
        {

            AcceptanceDetail = new List<RequestNoteAcceptanceDetail>();

        }

        public long RequestNoteAccptanceHeaderId { get; set; }

        [DefaultValue(0)]

        public int FromLocationId { get; set; }

        [DefaultValue(0)]

        public int FromDepartmentId { get; set; }

        [DefaultValue(0)]

        public int ToLocationId { get; set; }

        [DefaultValue(0)]

        public int ToDepartmentId { get; set; }

        [DefaultValue("")]

        [MaxLength(20)]

        public string DocumentNo { get; set; }
        public DateTime DocumentDate { get; set; }

       

        [NotMapped]

        [DefaultValue(0)]

        public int RequestStatus { get; set; }

        [DefaultValue("")]

        [MaxLength(150)]

        public string Remark { get; set; }

        [DefaultValue(0)]

        public bool IsActive { get; set; }
        [NotMapped]
        public List<RequestNoteAcceptanceDetail> AcceptanceDetail { get; set; }

        [DefaultValue(0)]

        public decimal TotSellingPrice { get; set; }

        [DefaultValue(0)]

        public decimal TotCostPrice { get; set; }


        [DefaultValue(false)]
        public bool IsTempRequest { get; set; }

        [DefaultValue("")]
        [MaxLength(20)]
        public string NewDocNumber { get; set; }

        [DefaultValue(false)]
        public bool IsTOG { get; set; }

        [DefaultValue("")]
        public string RequestType { get; set; }

        [DefaultValue(0)]
        public int CompanyId { get; set; }

        [DefaultValue(0)]
        public bool IsProductionComplete { get; set; }

        [DefaultValue(0)]
        public bool IsTOGComplete { get; set; }

        [DefaultValue(0)]
        public bool IsPOComplete { get; set; }


        [DefaultValue(0)]
        public long RequestnoteHeaderId { get; set; }

        [NotMapped]
        public decimal SIH { get; set; }
    }
}