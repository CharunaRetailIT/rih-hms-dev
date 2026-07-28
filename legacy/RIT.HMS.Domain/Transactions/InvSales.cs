using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.Transactions
{
    public class InvSales
    {


        public Int64 InvSalesId { get; set; }

        [DefaultValue(0)]
        public Int64 SalesId { get; set; }

        [DefaultValue(0)]
        public int CompanyId { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }

        [DefaultValue(0)]
        public int LocationId { get; set; }
        public string LocationCode { get; set; }
        public string LocationName { get; set; }

        [DefaultValue(0)]
        public int CostCentreId { get; set; }

        [DefaultValue(0)]
        public int DocumentId { get; set; }
        public string DocumentNo { get; set; }
        public string ReferenceNo { get; set; }
        public DateTime DocumentDate { get; set; }
        public DateTime TransactionTime { get; set; }

        [DefaultValue(0)]
        public int CustomerType { get; set; }

        [DefaultValue(0)]
        public long CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }

        [DefaultValue(0)]
        public long SupplierID { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }

        [DefaultValue(0)]
        public long SalesPersonId { get; set; }
        public string SalesPersonCode { get; set; }
        public string SalesPersonName { get; set; }

        [DefaultValue(0)]
        public decimal GrossAmount { get; set; }

        [DefaultValue(0)]
        public decimal DiscountPercentage { get; set; }

        [DefaultValue(0)]
        public decimal DiscountAmount { get; set; }

        [DefaultValue(0)]
        public decimal NetAmount { get; set; }

        [DefaultValue(0)]
        public decimal SubTotalDiscountPercentage { get; set; }

        [DefaultValue(0)]
        public decimal SubTotalDiscountAmount { get; set; }

        [DefaultValue(0)]
        public int CurrencyId { get; set; }

        [DefaultValue(0)]
        public int CurrencyRate { get; set; }

        [DefaultValue(0)]
        public int DepartmentId { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }

        [DefaultValue(0)]
        public long CategoryId { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }

        [DefaultValue(0)]
        public long SubCategoryId { get; set; }
        public string SubCategoryCode { get; set; }
        public string SubCategoryName { get; set; }

        [DefaultValue(0)]
        public long ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string BarCode { get; set; }
        public string BatchNo { get; set; }
        public DateTime ExpiryDate { get; set; }

        [DefaultValue(0)]
        public decimal Qty { get; set; }

        [DefaultValue(0)]
        public long UnitOfMeasureId { get; set; }
        public string UnitOfMeasureName { get; set; }

        [DefaultValue(0)]
        public decimal PackSize { get; set; }

        [DefaultValue(0)]
        public decimal SellingPrice { get; set; }

        [DefaultValue(0)]
        public decimal WholeSalePrice { get; set; }

        [DefaultValue(0)]
        public decimal CostPrice { get; set; }

        [DefaultValue(0)]
        public decimal AverageCost { get; set; }

        [DefaultValue(0)]
        public int DocumentStatus { get; set; }
        public bool IsFreeIssue { get; set; }
        public string TerminalNo { get; set; }
        public bool IsDispatch { get; set; }
        public bool IsUpLoad { get; set; }
        public bool IsDelete { get; set; }

        [DefaultValue(0)]
        public int UnitNo { get; set; }

        [DefaultValue(false)]
        public bool IsBackOffice { get; set; }
        public long ZNo { get; set; }

        [DefaultValue(0)]
        public int GroupOfCompanyId { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int DataTransfer { get; set; }
        public int SerialNo { get; set; }

        [DefaultValue(0)]
        public decimal CorporatePrice { get; set; }


    }
}