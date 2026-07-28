using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.ViewModels.Reports
{
   public class CustomerStatementViewModel
    {
        public CustomerStatementViewModel()
        {
            Detail = new List<PointsDetail>();
        }

        public int CustomerId { get; set; }
        public int LocationId { get; set; }
        public int CompanyId { get; set; }
        public List<PointsDetail> Detail { set; get; }

        public class PointsDetail
        {
            public string CustomerName { get; set; }
            public string CardNumber { get; set; }
            public string Recipt { get; set; }
            public decimal Points { get; set; }
            public decimal Amount { get; set; }
        }
    }
}
