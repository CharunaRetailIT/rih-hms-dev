using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.ViewModels.Promotions
{
    public class VMProductDiscounts
    {
        public long InvPromotionDetailsProductDisId { get; set; }    
        public long InvPromotionMasterID { get; set; }    
        public int CompanyID { get; set; }   
        public int LocationID { get; set; }      
        public int ProductID { get; set; }
        public string ProductCode { get; set; }
        public int ServingUnitId { get; set; }      
        public long UnitOfMeasureID { get; set; }     
        public decimal Rate { get; set; }     
        public decimal FromQty { get; set; }      
        public decimal ToQty { get; set; }      
        public long Points { get; set; }    
        public decimal DiscountPercentage { get; set; }      
        public decimal DiscountAmount { get; set; }
        public string ProductName { get; set; }
        public string ServingUnit { get; set; }
        public string UOM { get; set; }
        public long DepartmentId { get; set; }
        public long CategoryId { get; set; }
        public long SubCategoryId { get; set; }
    }
}
