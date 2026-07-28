using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.HMSOrderTaker.Domain.DTOs
{
    public class DTO_Product
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductNameInSinhala { get; set; }
        public bool IsRowMaterial { get; set; }
        public bool IsCountable { get; set; }
        public bool IsScaleItem { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public byte[] ProductImage { get; set; }
        public string ProductImageName { get; set; }
        public string ProductImageType { get; set; }
        public int DepartmentId { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal ReOrderLevel { get; set; }
        public decimal ReOrderQuantity { get; set; }
        public decimal LocationWiseStock { get; set; }
        public string Printer { get; set; }
        public string Barcode { get; set; }
        public bool IsItemLock { get; set; }
        public int GroupOfCompanyID { get; set; }
        public int CompanyID { get; set; }
        public int LocationId { get; set; }
        public string CreatedUser { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public int DataTransfer { get; set; }
        public string RefCode01 { get; set; }
        public string RefCode02 { get; set; }
        public decimal WastagePrc { get; set; }
        public int PurchasingUnit { get; set; }
        public bool IsDiscount { get; set; }
        public bool IsCostOnReceipe { get; set; }
        public bool IsAddon { get; set; }
        public string NameOnInvoice { get; set; }
        public bool IsPackItem { get; set; }
        public decimal PackSize { get; set; }
        public decimal PackPrice { get; set; }
        public bool IsPromotion { get; set; }
        public bool IsFreeIssue { get; set; }
        public bool IsExpiry { get; set; }
        public bool IsTax { get; set; }
        public int WeightPerUnit { get; set; }
        public bool IsUnderCost { get; set; }
        public bool IsBundle { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal MinPrice { get; set; }
        public decimal DiscountPrecentage { get; set; }
        public decimal MaximumDiscount { get; set; }
        public decimal FixedDiscountPercentage { get; set; }
        public decimal FixedDiscountAmount { get; set; }
        public decimal MaximumDiscountPercentage { get; set; }
        public int PrinterTypeId { get; set; }
        public Nullable<long> AddonCategoryMasterId { get; set; }
        public bool IsTaxInclude { get; set; }
        public bool IsOpenItem { get; set; }
        public int AutoProduction { get; set; }
        public string ServingUnit { get; set; }
        public int ProductServingUnitId { get; set; }
        [NotMapped]
        public List<DTO_Product> ProductList { get; set; }
    }
}
