using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.ViewModels
{
    public class RequestNoteViewModel
    {
        public long LocationId { get; set; }
        public string Location { get; set; }
        public string ToLocation { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductCostPrice { get; set; }
        public decimal ProductSellingPrice { get; set; }
        public decimal ProductQuantity { get; set; }
        public decimal ProductDbStock { get; set; }
        public string ProductCode { get; set; }
        public long ProductUOMId { get; set; }
        public string ProductUOMName { get; set; }
        public long MaterialId { get; set; }
        public string MaterialName { get; set; }
        public decimal MaterialCostPrice { get; set; }
        public decimal MaterialSellingPrice { get; set; }
        public decimal MaterialQuantity { get; set; }
        public decimal MaterialDbStock { get; set; }
        public string MaterialCode { get; set; }
        public long MaterialUOMId { get; set; }
        public string MaterialUOMName { get; set; }
        public long ReceipeId { get; set; }
        public string Remark { get; set; }
        public int DocumentStatus { get; set; }
        public string RequestedBy { get; set; }
        public string ServingUnit { get; set; }
        public long ServingUnitId { get; set; }
        public string RequestType { get; set; }

        public string DocumentNo { get; set; }
        public string LocationCode { get; set; }

        public string CreateDate { get; set; }

        public string AcceptedUser { get; set; }

        public bool IsActive { get; set; }

        public decimal CostPrice { get; set; }

        public decimal SellingPrice { get; set; }

        public decimal RequestQty { get; set; }

        public decimal GrossAmount { get; set; }

        public int supplierID { get; set; }
        public long requestnoteheaderid { get; set; }

        public int CompanyId { get; set; }

        public string Approve { get; set; }

        public string ExpectedDeleveryDate { get; set; } 
        public decimal SIH { get; set; }
    }
}