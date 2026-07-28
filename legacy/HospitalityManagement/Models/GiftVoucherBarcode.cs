using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class GiftVoucherBarcode
    {
        public string GVGRNNO { get; set; }
        public int GVTransferID { get; set; }
        public int InvGiftVoucherGroupID { get; set; }
        public string BookCode { get; set; }
        public string FromLocation { get; set; }
        public string ToLocation { get; set; }
        public string BookName { get; set; }
        public string BookPrefix { get; set; }
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
        public DateTime? CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime? ModifiedDate { get; set; }        
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
        public DateTime? TransferDate { get; set; }
        public DateTime? PartyInvoDate { get; set; }
        public DateTime? DispatchDate { get; set; }
        public string DispatchLocation { get; set; }
        public int TotQuantity { get; set; }
        public double GrossAmount { get; set; }
        public double Discount { get; set; }
        public double OtherCharges { get; set; }
        public double NetAmount { get; set; }
        public string TagSetup { get; set; }
        public string PrintDocumentNo { get; set; }
        public string Transaction { get; set; }
    }
}