using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.ViewModels.DataUpload
{
    public class DataUploadRecipePriceChangeViewModel
    {
        public int LineNo { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ServingUint { get; set; }
        public string LocationCode { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }

        //----------------------------------------------

        public bool InCorrectProductCode { get; set; }
        public bool InCorrectServingUnit { get; set; }
        public bool InCorrectLocationCode { get; set; }


    }
}
