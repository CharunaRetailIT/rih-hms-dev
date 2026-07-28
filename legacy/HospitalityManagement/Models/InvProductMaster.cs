using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class InvProductMaster : BaseEntity
    {
        public int InvProductMasterID { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [StringLength(20, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [DefaultValue(0)]
        public string ProductCode { get; set; }

        [DefaultValue("")]
        public string BarCode { get; set; }

        [DefaultValue("")]
        public string ReferenceCode { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [DefaultValue("")]
        public string ProductName { get; set; }

        [DataType(DataType.Text)]
        [DefaultValue("")]
        public string ProductDesp { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [DefaultValue("")]
        public string InvoicePrintName { get; set; }

        [DefaultValue("")]
        public string SinhalaDescription { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DefaultValue(0)]
        public int Department { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DefaultValue(0)]
        public int Category { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DefaultValue(0)]
        public int SubCategory { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DefaultValue(0)]
        public int SubCategory2 { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DefaultValue(0)]
        public int KitchecnBarCategory { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DefaultValue(0)]
        public int SuplierID { get; set; }

        [DefaultValue("")]
        public byte[] Image { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DefaultValue(0)]
        public decimal CostPrice { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DefaultValue(0)]
        public decimal OrderPrice { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DefaultValue(0)]
        public decimal AverageCost { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DefaultValue(0)]
        public decimal SellingPrice { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DefaultValue(0)]
        public decimal WholesalePrice { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DefaultValue(0)]
        public decimal MinimumPrice { get; set; }

        [DefaultValue(0)]
        public decimal FixedDiscount { get; set; }

        [DefaultValue(0)]
        public decimal MaximumDiscount { get; set; }

        [DefaultValue(0)]
        public decimal MaximumPrice { get; set; }
        [DefaultValue(0)]
        public decimal FixDiscountPercentage { get; set; }

        [DefaultValue(0)]
        public decimal MaximumDiscountPercentage { get; set; }

        [DefaultValue(0)]
        public decimal ReorderLevel { get; set; }

        [DefaultValue(0)]
        public decimal ReorderQty { get; set; }

        [DefaultValue(0)]
        public decimal ReorderPeriod { get; set; }

        [DefaultValue(0)]
        public string Remarks { get; set; }

        [DefaultValue(0)]
        public bool IsActive { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }

        




    }
}