using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
namespace RIT.HMS.Domain.Transactions
{
    public class RequestNoteParameter
    {
        public long locationID { get; set; }
        public string DocumentNo { get; set; }
        public int SupplierID { get; set; }
        public long RequestNoteHeaderID { get; set; }
    }
}
