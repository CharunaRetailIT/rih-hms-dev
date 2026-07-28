using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models.ViewModels
{
    public class ProductLocationViewModel
    {
        public int LocationId { get; set; }
        public int ProductId { get; set; }
        public string Location { get; set; }
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

    }
}