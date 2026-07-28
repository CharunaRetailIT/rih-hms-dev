using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class KitchenPrinters : BaseEntity
    {
        public long KitchenID { get; set; }
        public string KitchenCode { get; set; }
        public string KitchenDesc { get; set; }
        public string KitchenPrinterName { get; set; }
        public int KitchenPrinterType { get; set; }
        public bool IsActive { get; set; }
        public int GroupOfCompanyID { get; set; }
        public int CompanyID { get; set; }
        public int LocationId { get; set; }
        public string CreatedUser { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public int DataTransfer { get; set; }
    }
}