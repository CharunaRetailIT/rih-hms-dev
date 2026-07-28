using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.ViewModels.DataUpload
{
    public class DataUploadViewModel
    {

        public DataUploadViewModel()
        {
            DataUploadProductViewModel = new List<DataUploadProductViewModel>();
            DataUploadRecipePriceChangeViewModel = new List<DataUploadRecipePriceChangeViewModel>();
            DataUploadProductTaxViewModel = new List<DataUploadProductTaxViewModel>();
            DataUploadRecipeViewModel = new List<DataUploadRecipeViewModel>();
        }

        public string VerifyMessage { get; set; }
        public bool ProductTaxVerified { get; set; }
        public bool WithData { get; set; }
        public List<DataUploadProductViewModel> DataUploadProductViewModel { get; set; }
        public List<DataUploadRecipePriceChangeViewModel> DataUploadRecipePriceChangeViewModel { get; set; }
        public List<DataUploadProductTaxViewModel> DataUploadProductTaxViewModel { get; set; }
        public List<DataUploadRecipeViewModel> DataUploadRecipeViewModel { get; set; }
        public int StockLocationId { get; set; }

    }
}
