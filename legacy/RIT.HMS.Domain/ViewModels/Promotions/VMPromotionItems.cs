using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.ViewModels.Promotions
{
    public class VMPromotionItems
    {
        public string PromotionName { get; set; }
        public string PromotionCode { get; set; }
        public int ProductType { get; set; }

        public int PromotionItemId { get; set; }
        public string PromotionItem { get; set; }
        public string PromotionItemCode { get; set; }
        public string PromotionItemServingUnit { get; set; }    
        public decimal PromotionItemQty { get; set; }
        public decimal BillValueFrom { get; set; }
        public decimal BillValueTo { get; set; }

        public string FreeItem { get; set; }
        public int FreeItemId { get; set; }
        public string FreeItemCode { get; set; }
        public string FreeItemServingUnit { get; set; }
        public decimal FreeItemQty { get; set; }
        public decimal FreeDiscountPrc { get; set; }
        public decimal FreeDiscountAmt { get; set; }
    }
}
