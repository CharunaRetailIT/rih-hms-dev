using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class PrinterType
    {
        public int PrinterTypeId { get; set; }
        public string PrinterTypeName { get; set; }
        public bool IsDelete { get; set; }
    }
}