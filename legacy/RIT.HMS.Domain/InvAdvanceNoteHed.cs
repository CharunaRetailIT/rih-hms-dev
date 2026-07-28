using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain
{
    public class InvAdvanceNoteHed
    {
        public InvAdvanceNoteHed()
        {
            InvAdvanceNoteDet = new List<InvAdvanceNoteDet>();
        }
        public long InvAdvanceNoteHedID { get; set; }
        [StringLength(15)]
        public string AdNoteNo { get; set; }
        [StringLength(15)]
        public string Receipt { get; set; }
        [DefaultValue(0)]
        public decimal Amount { get; set; }
        [DefaultValue(0)]
        public decimal Balance { get; set; }
        [DefaultValue(0)]
        public int LocationID { get; set; }
        public DateTime Date { get; set; }
        [DefaultValue(0)]
        public int UnitNo { get; set; }
        [DefaultValue(0)]
        public int CashierID { get; set; }
        public DateTime Time { get; set; }
        [DefaultValue(0)]
        public long Zno { get; set; }
        [DefaultValue(0)]
        public int RecallFromInvoice { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string Remark { get; set; }

        [DefaultValue(false)]
        public bool IsProduction { get; set; }

        [DefaultValue(0)]
        public int ProcessLoc { get; set; }

        [DefaultValue(0)]
        public int PickupLoc { get; set; }

        [DefaultValue(true)]
        public bool Status { get; set; }

        [NotMapped]
        public string ProcessLocName { get; set; }
        [NotMapped]
        public string PickupLocName { get; set; }

        [NotMapped]

        public List<InvAdvanceNoteDet> InvAdvanceNoteDet { get; set; }

        [NotMapped]

        public DateTime DateFrom { get; set; }

        [NotMapped]

        public DateTime DateTo { get; set; }

        [DefaultValue(0)]
        public int CompanyId { get; set; }


    }
}
