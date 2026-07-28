using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.ViewModels
{
    public class ProductUploadViewModel
    {
        public int CompanyId { get; set; }
        public List<Product> ProductList { get; set; }
        public List<ProductStockMaster> ProductStockMasterList { get; set; }
        public List<SupplierProduct> SupplierProductList { get; set; }
        public List<ProductTax> ProductTaxList { get; set; }
        public List<ReceipeViewModel> RecipeList { get; set; }
        public List<ReceipeViewModel> RecipeUploadList { get; set; }


    }

}
