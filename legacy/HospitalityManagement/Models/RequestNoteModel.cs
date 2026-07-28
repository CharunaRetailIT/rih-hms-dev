using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class RequestNoteModel
    {
        public string ProductCode { get; set; }
        public string Remark { get; set; }
        public List<TableRow> Rows { get; set; }
    }
}