using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class ProductStockMaster:BaseEntity
    {
        public long ProductStockMasterId { get; set; }

      
        [DefaultValue(0)]
        public int CostCentreId { get; set; }

     //   [Index(IsClustered = true, IsUnique = true)]
        [DefaultValue(0)]
        public long ProductId { get; set; }

        [Required]
        [MaxLength(25)]
        public string StockCode { get; set; }

        [DefaultValue(0)]
        public decimal Stock { get; set; }

        [DefaultValue(0)]
        public decimal CostPrice { get; set; }

        [DefaultValue(0)]
        public decimal SellingPrice { get; set; }

        [DefaultValue(0)]
        public decimal ForignCustomerPrice { get; set; }

        [NotMapped]
        [DefaultValue(0)]
        public decimal MinimumPrice { get; set; }
        
        [DefaultValue(0)]
        public decimal ReOrderLevel { get; set; }

        [DefaultValue(0)]
        public decimal ReOrderQuantity { get; set; }

        [DefaultValue(0)]
        public decimal ReOrderPeriod { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }

        [MaxLength(20)]
        public string ProductCode { get; set; }

        [MaxLength(100)]
        public string ProductName { get; set; }

        [MaxLength(30)]
        public string Barcode { get; set; }

        [MaxLength(30)]
        public string RefNo1 { get; set; }

        [MaxLength(30)]
        public string RefNo2 { get; set; }

        [DefaultValue(0)]
        public int ExtendedId { get; set; }

        [MaxLength(30)]
        public string ExtendedName { get; set; }


        [MaxLength(5)]
        public string PLUCode { get; set; }

        //[MaxLength(30)]
        //public string PLUName { get; set; }
        
        [DefaultValue(0)]
        public decimal WeightPerunit { get; set; }

        [DefaultValue(0)]
        public int UomId { get; set; }

        [MaxLength(10)]
        public string Unit { get; set; }

      
        [NotMapped]
        [DefaultValue(0)]
        public Decimal MaxPrice { get; set; }

        [DefaultValue(0)]
        public Decimal AvgCost { get; set; }
        
        [DefaultValue(0)]
        public Decimal FixedGP { get; set; }

        [DefaultValue(0)]
        public Decimal GP { get; set; }
        
        [DefaultValue(0)]
        public Decimal OpenBal { get; set; }

        [DefaultValue(0)]
        public Decimal InitSIH { get; set; }

        [DefaultValue(0)]
        public Decimal InitCost { get; set; }

        [DefaultValue(0)]
        public Decimal AdjQty { get; set; }

        //[DefaultValue(0)]
        //public bool IsWarranty { get; set; }

        [DefaultValue(0)]
        public bool IsDamage { get; set; }

        [DefaultValue(0)]
        public bool IsActive { get; set; }

        [DefaultValue(0)]
        public bool IsBundle { get; set; }

        [DefaultValue(0)]
        public bool IsInitialize { get; set; }
        
        [DefaultValue(0)]
        public int DataTransfer { get; set; }
        
        [DefaultValue(false)]
        public bool Ispacksize { get; set; }

        [DefaultValue(false)]
        public bool Iscommission { get; set; }

        [DefaultValue(false)]
        public bool Isdecimal { get; set; }

        [DefaultValue(0)]
        public decimal DiscountPrc { get; set; }

        [NotMapped]
        public string Location { get; set; }
        [DefaultValue(0)]
        [MaxLength(20)]
        public string DocumentNo { get; set; }

        [DefaultValue("1900-01-01")]
        public DateTime? LastUpdatedDate { get; set; }


        // 24/11/2018

        [DefaultValue(0)]
        public decimal MaximumDiscount { get; set; }
        [DefaultValue(0)]
        public decimal FixedDiscountPercentage { get; set; }

        [DefaultValue(0)]
        public decimal FixedDiscountAmount { get; set; }

        [DefaultValue(0)]
        public decimal MaximumDiscountPercentage { get; set; }

        [NotMapped]
        public string UOMDesc { get; set; }

        //24/11/2018
    //    [Range(1, int.MaxValue, ErrorMessage = "Select a printer!")]
        [DefaultValue(0)]
        public int PrinterType_Id { get; set; }

        [NotMapped]
        public decimal SubUnitValue { get; set; }

        [NotMapped]
        public string LocationCode { get; set; }

        [NotMapped]
        public string SubUnit { get; set; }
    }
}