using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.ViewModels
{
    public class ProductLocationViewModel
    {
        public int LocationId { get; set; }
        public int ProductId { get; set; }
        public string Location { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal ReOrdderLevel { get; set; }
        public decimal ReOrderQuantity { get; set; }
        public decimal ReOrderPeriod { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal MinPrice { get; set; }
        public decimal DiscountPrc { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string BarCode { get; set; }
        public decimal ForignCustomerPrice { get; set; }
        public decimal AverageCost { get; set; }

        public int PrinterType_Id { get; set; }

       
    }
}