using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain
{
    public class ProductMapperToKitchen
    {
        public int GeneralLocationId { get; set; }
        public Product Product { get; set; }
        public List<SysLocation> KitchenLocationList { get; set; }
        public List<ProductKitchenMapper> ProductKitchenMapper { get; set; }

        public int GroupOfCompanyID { get; set; }
        public int CompanyID { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int DataTransfer { get; set; }
        public bool IsActive { get; set; }
        public ProductMapperToKitchen()
        {
            KitchenLocationList = new List<SysLocation>();
            ProductKitchenMapper = new List<ProductKitchenMapper>();
            Product = new Product();
        }
    }
}
