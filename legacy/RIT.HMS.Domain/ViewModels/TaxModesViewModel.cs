using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.ViewModels
{
    public class TaxModesViewModel
    {
        public int CMId { get; set; }
        public int TaxId { get; set; }
        public int TaxLocationId { get; set; }
        public int PayModeId { get; set; }
        public int CateringModeId { get; set; }
        public int LocationId { get; set; }
        public int CompanyId { get; set; }
        public string CreateUser { get; set; }
    }
}
