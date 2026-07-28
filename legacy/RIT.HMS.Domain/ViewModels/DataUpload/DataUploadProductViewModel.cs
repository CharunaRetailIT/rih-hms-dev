using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.ViewModels.DataUpload
{
    public class DataUploadProductViewModel
    {

        public int LineNo { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string NameOnInvoice { get; set; }
        public bool IsScaleItem { get; set; }
        public bool IsRawMaterial { get; set; }
        public string DepartmentCode { get; set; }
        public string ServingUnit { get; set; }
        public string RstCategoryCode { get; set; }
        public string RstSubCategoryCode { get; set; }
        public string UnitOfMeasureCode { get; set; }
        public string SubUnit { get; set; }
        public string PrinterType { get; set; }
        public bool IsDiscount { get; set; }
        public bool IsCostOnReceipe { get; set; }
        public bool IsAddon { get; set; }
        public bool IsPromotion { get; set; }
        public bool IsExpiry { get; set; }
        public bool IsTax { get; set; }
        public bool IsUnderCost { get; set; }
        public bool IsTaxInclude { get; set; }
        public bool IsOpenItem { get; set; }
        public bool AutoProduction { get; set; }
        public bool IsNoEffectCostforMenu { get; set; }
        public string SupplierCode { get; set; }
        public string LocationCode { get; set; }
        public decimal Stock { get; set; }
        public decimal AvgCost { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal CostPrice { get; set; }
        public decimal ReOrderLevel { get; set; }
        public decimal ReOrderQuantity { get; set; }

        // -------------------------------------------------

        [DefaultValue(true)]
        public bool InCorrectDepartmentCode { get; set; }
        public bool InCorrectRstCategoryCode { get; set; }
        public bool InCorrectRstSubCategoryCode { get; set; }
        public bool InCorrectUnitOfMeasureName { get; set; }
        public bool InCorrectSubUnit { get; set; }
        public bool InCorrectSuppliereCode { get; set; }
        public bool InCorrectLocationCode { get; set; }

        public bool IsActive { get; set; }

        public int DepartmentID { get; set; }

        public int CategoryID { get; set; }

        public int SubCategoryID { get; set; }

        public int LocationID { get; set; }

        public int supplierid { get; set; }

    }
}
