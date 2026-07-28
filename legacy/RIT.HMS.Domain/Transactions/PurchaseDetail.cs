using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.Transactions
{
    public class PurchaseDetail
    {
        public long PurchaseDetailID { get; set; }
        
        [Required]       
        public long PurchaseHeaderID { get; set; }
        
        [DefaultValue(0)]
        public int CostCentreID { get; set; }
        
        [DefaultValue(0)]
        public int DocumentID { get; set; }
        
        [DefaultValue(0)]
        [MaxLength(20)]
        public string DocumentNo { get; set; }
        
        [DefaultValue(0)]
        public long LineNo { get; set; }
        
        [DefaultValue(0)]
        public long ProductID { get; set; }
        
        [DefaultValue(0)]
        public bool IsBatch { get; set; }
        
        [DefaultValue("")]

        [MaxLength(50)]
        public string BatchNo { get; set; }
        
        [DefaultValue("")]
        [MaxLength(25)]
        public string StockCode { get; set; }
        
        //[DefaultValue("")]



        //[MaxLength(25)]



        //public string StockCodeOriginal { get; set; }
        
        [DefaultValue(0)]
        public long UnitOfMeasureID { get; set; }      
        [DefaultValue(0)]

        public long BaseUnitID { get; set; }
        
        [DefaultValue(0)]
        public bool IsExpiry { get; set; }
        
        [DefaultValue("1900-01-01")]

        public DateTime? ExpiryDate { get; set; }
        
        [DefaultValue(0)]       
        public decimal OrderQty { get; set; }

        [DefaultValue(0)]      
        public decimal Discount { get; set; }

        [DefaultValue(0)]     
        public decimal FreeQty { get; set; }
        
        [DefaultValue(0)]
        
        public decimal CurrentQty { get; set; }
        
        [DefaultValue(0)]
        
        public decimal ConvertFactor { get; set; }
        
        [DefaultValue(0)]
        
        public decimal BalanceQty { get; set; }
        
        [DefaultValue(0)]
        
        public decimal CostPrice { get; set; }
        
        [DefaultValue(0)]
        
        public decimal SellingPrice { get; set; }
        
        [DefaultValue(0)]
        
        public decimal AvgCost { get; set; }
        
        [DefaultValue(0)]
        
        public decimal GrossAmount { get; set; }
        
        [DefaultValue(0)]
        
        public decimal DiscountPercentage { get; set; }
        
        [DefaultValue(0)]
        
        public decimal DiscountAmount { get; set; }
        
        [DefaultValue(0)]
        
        public decimal SubTotalDiscount { get; set; }
        
        [DefaultValue(0)]

        public decimal TotalTax { get; set; }
        
        [DefaultValue(0)]

        public decimal NetAmount { get; set; }
        
        [DefaultValue(0)]

        public int DocumentStatus { get; set; }
        
        public DateTime DocumentDate { get; set; }
        
        [DefaultValue(" ")]

        [MaxLength(200)]
        public string ProductRemark { get; set; }
        
        [DefaultValue(0)]
        
        public decimal Packsize { get; set; }
        
        [DefaultValue(0)]
        public decimal profitMargin { get; set; }
        
        [DefaultValue(0)]
        public string SerialNo { get; set; }
        
        [DefaultValue(0)]
        public bool IsUsed { get; set; }

        [NotMapped]
        public string ProductName { get; set; }

        [NotMapped]
        public string UOM { get; set; }

        //[NotMapped]
        public decimal GRNQuantity { get; set; }

        [NotMapped]
        public string ProductCode { get; set; }
        public decimal CostValue { get; set; }
        public decimal TOGQty { get; set; }

 
        [Column(TypeName = "VARCHAR")]
        [StringLength(3)]
        [DefaultValue("")]
        public string DiscountType { get; set; }

        [NotMapped]
        public DateTime ExpiaryDate { get; set; }

        [NotMapped]
        public int CurrancyId { get; set; }
        [NotMapped]
        public int PaymentMethodId { get; set; }
        [NotMapped]
        public int PaymentTermId { get; set; }

        [NotMapped]
        public int GRNLocationId { get; set; }

        [NotMapped]
        public decimal CurrancyRate { get; set; }

        [DefaultValue(false)]
        public bool IsPRN { get; set; }

        
        [DefaultValue(0)]
        public decimal PRNQuantity { get; set; }

        [NotMapped]
        public decimal CPPRN { get; set; }

        [NotMapped]
        public decimal CurrnetStock { get; set; }

        [NotMapped]
        public decimal HeaderTaxTotal { get; set; }


        [NotMapped]
        [DefaultValue(0)]
        public decimal POQuantity { get; set; }

    }
}