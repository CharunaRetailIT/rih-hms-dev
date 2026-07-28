using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Transactions
{
    public class invGiftVoucherPurchaseHeaders
    {
        [Key]
        public long InvGiftVoucherPurchaseHeaderID { get; set; }
        public long GiftVoucherPurchaseHeaderID { get; set; }
        public int CompanyID { get; set; }
        public int LocationID { get; set; }
        public int CostCentreID { get; set; }
        public int DocumentID { get; set; }
        public string DocumentNo { get; set; }
        public DateTime DocumentDate { get; set; }
        public long SupplierID { get; set; }
        public DateTime PartyInvoiceDate { get; set; }
        public DateTime DispatchDate { get; set; }
        public decimal GiftVoucherAmount { get; set; }
        public decimal GiftVoucherPercentage { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal TaxAmount1 { get; set; }
        public decimal TaxAmount2 { get; set; }
        public decimal TaxAmount3 { get; set; }
        public decimal TaxAmount4 { get; set; }
        public decimal TaxAmount5 { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal CreditLimit { get; set; }
        public int CreditPeriod { get; set; }
        public decimal ChequeLimit { get; set; }
        public int ChequePeriod { get; set; }
        public int GiftVoucherQty { get; set; }
        public string Remark { get; set; }
        public string ReferenceNo { get; set; }
        public string PartyInvoiceNo { get; set; }
        public string DispatchNo { get; set; }
        public int PaymentTermID { get; set; }
        public int PaymentPeriod { get; set; }
        public string DeliveryPerson { get; set; }
        public string DeliveryPersonNICNo { get; set; }
        public string VehicleNo { get; set; }
        public int ReferenceDocumentDocumentID { get; set; }
        public long ReferenceDocumentID { get; set; }
        public int VoucherType { get; set; }
        public int DocumentStatus { get; set; }
        public int GroupOfCompanyID { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int DataTransfer { get; set; }
    }
}
