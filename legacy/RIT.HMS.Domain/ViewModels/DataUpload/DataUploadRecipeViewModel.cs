using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.ViewModels.DataUpload
{
    public class DataUploadRecipeViewModel
    {
        public int LineNo { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ServingUint { get; set; }
        public decimal ProductQuantity { get; set; }       
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }

        public int MaterialId { get; set; }
        public string MaterialCode { get; set; }
        public decimal MaterialQuantity { get; set; }
        public string SubUnit { get; set; }
        public string LocationCode { get; set; }

        // ---------------------------------------------------
        public bool InCorrectLocationCode { get; set; }
        public bool InCorrectProductCode { get; set; }
        public bool InCorrectServingUnitCode { get; set; }
        public bool InCorrectMaterialCode { get; set; }
        public bool InCorrectSubUnitCode { get; set; }

    }
}
