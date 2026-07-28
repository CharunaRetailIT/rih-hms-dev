using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.ViewModels.DataUpload
{
    public class DataUploadProductTaxViewModel
    {
        public int LineNo { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public int TaxId { get; set; }
        public string TaxCode { get; set; }
       

        //----------------------------------------------

        public bool InCorrectProductCode { get; set; }
        public bool InCorrectTaxCode { get; set; }
      
    }
}
