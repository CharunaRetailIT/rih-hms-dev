using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Promotions
{
   public class InvPromotionDetailsProductDis
    {

        public long InvPromotionDetailsProductDisId { get; set; }
        [DefaultValue(0)]
        public long InvPromotionMasterID { get; set; }
        [DefaultValue(0)]
        public int CompanyID { get; set; }
        [DefaultValue(0)]
        public int LocationID { get; set; }
        [DefaultValue(0)]
        public int ProductID { get; set; }
        [DefaultValue(0)]
        public int ServingUnitId { get; set; }
        [DefaultValue(0)]
        public long UnitOfMeasureID { get; set; }
        [DefaultValue(0)]
        public decimal Rate { get; set; }
        [DefaultValue(0)]
        public decimal FromQty { get; set; }
        [DefaultValue(0)]
        public decimal ToQty { get; set; }
        [DefaultValue(0)]
        public long Points { get; set; }
        [DefaultValue(0)]
        public decimal DiscountPercentage { get; set; }
        [DefaultValue(0)]
        public decimal DiscountAmount { get; set; }
        public long DepartmentId { get; set; }
        public long CategoryId { get; set; }
        public long SubCategoryId { get; set; }


    }
}
