using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.Transactions
{
    public class PurchaseOrderDetail
    {
       
        public long PurchaseOrderDetailId { get; set; }  
                
        [DefaultValue(0)]
        public long PurchaseOrderHeaderId { get; set; }
        
        [DefaultValue(0)]
        public long ProductId { get; set; }

        [DefaultValue(0)]
        public bool IsBatch { get; set; }

        [DefaultValue("")]
        [MaxLength(25)]
        public string StockCode { get; set; }      
        public decimal OrderQty { get; set; }
        [DefaultValue(0)]
        public decimal FreeQty { get; set; }

        [DefaultValue(0)]
        public decimal CurrentQty { get; set; }

        [DefaultValue(0)]
        public decimal BalanceQty { get; set; }

        [DefaultValue(0)]
        public decimal BalanceFreeQty { get; set; }

        [DefaultValue(0)]
        public decimal CostPrice { get; set; }

        [DefaultValue(0)]
        public decimal SellingPrice { get; set; }

        [DefaultValue(0)]
        public string PackSize { get; set; }

        [DefaultValue(0)]
        public long UnitOfMeasureId { get; set; }

        [DefaultValue(0)]
        public long BaseUnitId { get; set; }
        [DefaultValue(0)]
        public decimal ConvertFactor { get; set; }
        [DefaultValue(0)]
        public decimal GrossAmount { get; set; }
        [DefaultValue(0)]
        public decimal DiscountPercentage { get; set; }

        [DefaultValue(0)]
        public decimal DiscountAmount { get; set; }

        [DefaultValue(0)]
        public decimal SubTotalDiscount { get; set; }
      
        [DefaultValue(0)]
        public decimal NetAmount { get; set; }

        [DefaultValue(0)]
        
        public decimal ItemTaxTotal { get; set; }

        [DefaultValue(0)]
        public long LineNo { get; set; }
        
        public string BatchNo { get; set; }
        
        [NotMapped]
        public byte[] ProductImage { get; set; }

        [MaxLength(200)]
        public string ProductRemark { get; set; }

        [NotMapped]
        public string ProductName { get; set; }

        [NotMapped]
        public decimal Discounts { get; set; }

        //[NotMapped]
        //public decimal ItemTaxTotal { get; set; }

        [NotMapped]
        public decimal TotSellingPrice { get; set; }
        [NotMapped]
        public decimal TotCostPrice { get; set; }

        [NotMapped]
        public decimal TotDiscounts { get; set; } 

        [NotMapped]
        public string UOM { get; set; }

        [NotMapped]
        public string ProductCode { get; set; }

        [DefaultValue(0)]
        public decimal CostValue { get; set; }

        [DefaultValue(0)]
        public decimal GRNQuantity { get; set; }

        [DefaultValue(false)]
        public bool IsGRN { get; set; }

        [NotMapped]

        public int LocationID { get; set; }

        [NotMapped]

        public int CompanyID { get; set; }

        [NotMapped]

        public bool IsTempPO { get; set; }

        [NotMapped]

        public int SupplierId { get; set; }

        [NotMapped]
        [DefaultValue(0)]
        public decimal RecQty { get; set; }


        
    }
}