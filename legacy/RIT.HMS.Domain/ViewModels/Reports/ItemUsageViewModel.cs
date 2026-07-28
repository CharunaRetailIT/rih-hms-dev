using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace RIT.HMS.Domain.ViewModels.Reports
{
   public class ItemUsageViewModel
    {
        public ItemUsageViewModel()
        {
            Details = new List<Detail>();
        }
        public int CompanyId { get; set; }
        public string KitchenId { get; set; }
        public int DepartmentId { get; set; }
        public int ProductId { get; set; }
        public int LocationId { get; set; }

        public List<SelectListItem> Products { get; set; }
        public List<SelectListItem> Departments { get; set; }
        public List<SelectListItem> Kitchens { get; set; }

        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public List<Detail> Details { get; set; }
        public class Detail
        {
            public int ProductId { get; set; }
            public string ProductCode { get; set; }
            public string ProductName { get; set; }
            public string ServingUnit { get; set; }
            public decimal ProductQty { get; set; }
            public int ItemId { get; set; }
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public string SubUnit { get; set; }
            public decimal ItemQty { get; set; }
            public int DepartmentId { get; set; }

        }
    }
}
