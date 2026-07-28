using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.ViewModels.Reports
{
    public class JournalViewModel
    {
       public JournalViewModel()
        {
            InvProducts = new List<InvPruduct>();
            InvRecipts = new List<Recipts>();
        }

        [DefaultValue(0)]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Location !")]
        public int  LocationId { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        [DataType(DataType.Text)]
        [DefaultValue(0)]
        public string UnitNo { get; set; }  
        public string ReciptNo { get; set; }
        public string VatRegNo { get; set; }      
        public string CateringMode { get; set; }
        public int NoOfGuests { get; set; }       
        public string Cashier { get; set; }
        public string AdvanceNoteNo { get; set; }
        public Decimal SubTotal { get; set; }
        public Decimal NetTotal { get; set; }
        public Decimal Cash { get; set; }
        public Decimal Balance { get; set; }
        public Decimal NoOfItems { get; set; }
        public Decimal NoOfPics { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public int Zno { get; set; }
        public int DocumentId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress1 { get; set; }
        public string CompanyAddress2 { get; set; }

        public string CompanyAddress3 { get; set; }

        public List<InvPruduct> InvProducts { get; set; }
        public List<Recipts> InvRecipts { get; set; }
        public class Recipts
        {
            public int TransactionDetId { get; set; }
            public string UnitNo { get; set; }
            public string ReciptNo { get; set; }
            public int Zno { get; set; }
            public DateTime Date { get; set; }
            public int LocationId { get; set; }
        }
        public class InvPruduct
        {
            public int ProductId { get; set; }
            public string ProductCode { get; set; }
            public string ProductName { get; set; }
            public decimal Qty { get; set; }
            public decimal Price { get; set; }
            public decimal Amount { get; set; }
            public decimal SubTotal { get; set; }
            public decimal NetTotal { get; set; }
        }
    }
}
