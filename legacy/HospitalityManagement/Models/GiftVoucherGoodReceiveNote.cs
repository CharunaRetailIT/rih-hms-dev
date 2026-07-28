using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class GiftVoucherGoodReceiveNote
    {
        public long InvGiftVoucherBookCodeID { get; set; }
        public int InvGiftVoucherGroupID { get; set; }
        public int  invGiftVoucherPurchaseHeader { get; set; }
        public string BookCode { get; set; }
        public string BookName { get; set; }
        public string BookCode1 { get; set; }
        public string BookPrefix { get; set; }
        public string SupplierIDhidden { get; set; }
        public decimal GiftVoucherValue { get; set; }
        [DefaultValue(0)]
        public decimal GiftVoucherPercentage { get; set; }
        [DefaultValue(0)]
        public int ValidityPeriod { get; set; }
        public int VoucherType { get; set; }
        public int StartingNo { get; set; }
        public int CurrentSerialNo { get; set; }
        public int SerialLength { get; set; }
        public int PageCount { get; set; }
        public bool IsDelete { get; set; }
        public int BasedOn { get; set; }
        public int GroupOfCompanyID { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }        
        public int DocumentID { get; set; }
        public string DocumentNo { get; set; }
        public int PurchaseOrderID { get; set; }
        public string PurchaseOrderNo { get; set; }
        public int SupplierID { get; set; }
        public string SupplierName { get; set; }
        public string DeliveryPersonName { get; set; }
        public string DeliveryPersonID { get; set; }
        public string DeliveryVehicle { get; set; }
        public string Remarks { get; set; }
        public string ReferanseNo { get; set; }
        public string PartyInvNo { get; set; }
        public string DispatchNo { get; set; }
        public string PaymentTerms { get; set; }
        public string Paydates { get; set; }
        public DateTime DocumentDate { get; set; }
        public DateTime PartyInvoDate { get; set; }
        public DateTime DispatchDate { get; set; }
        public string DispatchLocation { get; set; }
        public int TotQuantity { get; set; }
        public decimal GrossAmount { get; set; }
        public double Discount { get; set; }
        public decimal OtherCharges { get; set; }
        public long InvGiftVoucherPurchaseOrderHeaderID { get; set; }
        public DateTime ExpectedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        [DefaultValue(0)]
        public int PaymentTermID { get; set; }
        [DefaultValue(0)]
        public int PaymentPeriod { get; set; }
        [DefaultValue(0)]
        public decimal GiftVoucherAmount { get; set; }
        [DefaultValue(0)]
        public decimal DiscountAmount { get; set; }
        [DefaultValue(0)]
        public decimal DiscountPercentage { get; set; }
        [DefaultValue(0)]
        public decimal TaxAmount1 { get; set; }
        [DefaultValue(0)]
        public decimal TaxAmount2 { get; set; }
        [DefaultValue(0)]
        public decimal TaxAmount3 { get; set; }
        [DefaultValue(0)]
        public decimal TaxAmount4 { get; set; }
        [DefaultValue(0)]
        public decimal TaxAmount5 { get; set; }
        [DefaultValue(0)]
        public decimal NetAmount { get; set; }
        [DefaultValue(0)]
        public decimal TaxAmount { get; set; }
        [DefaultValue(0)]
        public decimal CreditLimit { get; set; }
        [DefaultValue(0)]
        public int CreditPeriod { get; set; }
        [DefaultValue(0)]
        public decimal ChequeLimit { get; set; }
        [DefaultValue(0)]
        public int ChequePeriod { get; set; }
        public int GiftVoucherQty { get; set; }
        [DefaultValue("")]
        [MaxLength(150)]
        public string Remark { get; set; }
        [DefaultValue("")]
        [MaxLength(20)]
        public string ReferenceNo { get; set; }
        [DefaultValue(0)]
        public int DocumentStatus { get; set; }
        public string GiftVoucherGroupCode { get; set; }
        public string SelectionCriteria { get; set; }  
        public int PaymentTermhidden { get; set; }
        public int LocationAhidden { get; set; }
        public int PurchaseOrderIDhidden1 { get; set; }
        public string GiftVoucherGroupCodehidden { get; set; }
        public string BookCodehidden { get; set; }
        public string VoucherPrefix { get; set; }
        public int VoucherCount { get; set; }
        public string VoucherNo { get; set; }
        public int VoucherNoSerial { get; set; }
        public string VoucherSerial { get; set; }
        public int VoucherSerialNo { get; set; }
        public int VoucherStatus { get; set; }
        public int ToLocationID { get; set; }
        public int BlockedUnitID { get; set; }
    }
}