using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models.DeliveryModule
{
    public class DeliveryModuleProduct
    {

        public DeliveryModuleProduct()
        {
            //Receipes = new List<Receipe>();
            //ProductTax = new List<ProductTax>();
            //ProductServingUnit = new List<ProductServingUnit>();
            //SupplierProduct = new List<SupplierProduct>();
            //ProductLocationViewModel = new List<ViewModels.ProductLocationViewModel>();
        }

        [Key]
        public int ProductId { get; set; }

        // general
        [Required(ErrorMessage = "The field is required")]
        [DataType(DataType.Text)]
        [DefaultValue("")]
        [StringLength(10)]
        public string ProductCode { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DataType(DataType.Text)]
        [DefaultValue("")]
        [StringLength(100)]
        public string ProductName { get; set; }

        // 24/11/2018

        [DataType(DataType.Text)]
        [DefaultValue("")]
        [StringLength(50)]
        public string NameOnInvoice { get; set; }

        [DefaultValue(0)]
        public bool IsPackItem { get; set; }

        [DefaultValue(0)]
        public decimal PackSize { get; set; }

        [DefaultValue(0)]
        public decimal PackPrice { get; set; }

        [DefaultValue(0)]
        public bool IsPromotion { get; set; }

        [DefaultValue(0)]
        public bool IsFreeIssue { get; set; }

        [DefaultValue(0)]
        public bool IsExpiry { get; set; }

        [DefaultValue(0)]
        public bool IsTax { get; set; }

        [DefaultValue(0)]
        public int WeightPerUnit { get; set; }

        [DefaultValue(0)]
        public bool IsUnderCost { get; set; }
        [DefaultValue(0)]
        public bool IsBundle { get; set; }

        [DefaultValue(0)]
        public decimal MaxPrice { get; set; }

        [DefaultValue(0)]
        public decimal MinPrice { get; set; }

        [DefaultValue(0)]
        public decimal DiscountPrecentage { get; set; }

        [DefaultValue(0)]
        public decimal MaximumDiscount { get; set; }

        [DefaultValue(0)]
        public decimal FixedDiscountPercentage { get; set; }

        [DefaultValue(0)]
        public decimal FixedDiscountAmount { get; set; }

        [DefaultValue(0)]
        public decimal MaximumDiscountPercentage { get; set; }

        [DataType(DataType.Text)]
        [DefaultValue("")]
        [StringLength(100)]
        public string ProductNameInSinhala { get; set; }

        [DefaultValue(true)]
        public bool IsRowMaterial { get; set; }
        [DefaultValue(false)]
        public bool IsAddon { get; set; }
        [DefaultValue(true)]
        public bool IsCountable { get; set; }

        [DefaultValue(true)]
        public bool IsDiscount { get; set; }

        [DefaultValue(true)]
        public bool IsCostOnReceipe { get; set; }

        [DefaultValue(false)]
        public bool IsScaleItem { get; set; }

        [Required(ErrorMessage = "*")]
        [DefaultValue(false)]
        public bool IsActive { get; set; }

        [Required(ErrorMessage = "*")]
        [DefaultValue(false)]
        public bool IsDelete { get; set; }

        [DefaultValue(null)]
        [NotMapped]
        public HttpPostedFileBase Photograph { get; set; }
        [DefaultValue("")]
        public byte[] ProductImage { get; set; }

        [DefaultValue("")]
        public string ProductImageName { get; set; }
        [DefaultValue("")]
        public string ProductImageType { get; set; }


        // dept / category /sub cat ----------------------------------------------------------


  
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Department !")]
        public int DepartmentId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a Category !")]
        public int CategoryId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a Sub Category !")]
        public int SubCategoryId { get; set; }


        // pricing - stock````````````````````````````````````````````````````````````````````
        [Required(ErrorMessage = "The field is required")]
        [DefaultValue(0)]
        public decimal CostPrice { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DefaultValue(0)]
        public decimal SellingPrice { get; set; }

        [Required(ErrorMessage = "*")]
        [DefaultValue(0)]
        public decimal ReOrderLevel { get; set; }

        [DefaultValue(0)]
        public decimal ReOrderQuantity { get; set; }

        [DefaultValue(0)]
        public decimal LocationWiseStock { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DefaultValue(0)]
        public decimal WastagePrc { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a Purchasing Unit!")]
        [Required(ErrorMessage = "The field is required")]
        [DefaultValue(0)]
        public int PurchasingUnit { get; set; }

        // configurations ------------------------------------------------------


        [DataType(DataType.Text)]
        [DefaultValue("")]
        [StringLength(10)]
        public string Printer { get; set; }

        [DataType(DataType.Text)]
        [DefaultValue("")]
        [StringLength(200)]
        public string Barcode { get; set; }

        [DefaultValue(false)]
        public bool IsItemLock { get; set; }

        // references ------------------------------------------------------

        [DataType(DataType.Text)]
        [DefaultValue("")]
        [StringLength(200)]
        public string RefCode01 { get; set; }

        [DataType(DataType.Text)]
        [DefaultValue("")]
        [StringLength(200)]
        public string RefCode02 { get; set; }
        
        [NotMapped]
        public List<Receipe> Receipes { get; set; }

        [NotMapped]
        public List<ProductTax> ProductTax { get; set; }

        [NotMapped]
        public List<ProductServingUnit> ProductServingUnit { get; set; }

        [NotMapped]
        public int ReceipeProductId { get; set; }

        [NotMapped]
        public int TempProductId { get; set; }
        [NotMapped]
        public List<SupplierProduct> SupplierProduct { get; set; }
    
        [NotMapped]
        public List<ViewModels.ProductLocationViewModel> ProductLocationViewModel { get; set; }

        [NotMapped]
        public string UOM { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Select a printer!")]
        public int PrinterTypeId { get; set; }

        [DefaultValue(0)]     
        public long? AddonCategoryMasterId { get; set; }

    }
}