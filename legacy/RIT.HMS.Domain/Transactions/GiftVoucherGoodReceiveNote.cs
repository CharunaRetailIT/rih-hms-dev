using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Transactions
{
    public class GiftVoucherGoodReceiveNote : BaseEntity
    {
        [Key]
        public int InvGiftVoucherBookCodeID { get; set; }
        [DefaultValue(0)]
        public int InvGiftVoucherGroupID { get; set; }

        [MaxLength(20)]
        public string BookCode { get; set; }

        [MaxLength(50)]
        public string BookName { get; set; }

        [MaxLength(4)]
        public string BookPrefix { get; set; }

        [DefaultValue(0)]

        public decimal GiftVoucherValue { get; set; }

        [DefaultValue(0)]

        public decimal GiftVoucherPercentage { get; set; }
        [DefaultValue(0)]
        public int ValidityPeriod { get; set; }
        [DefaultValue(0)]
        public int VoucherType { get; set; }

        [DefaultValue(0)]
        public int StartingNo { get; set; }

        [DefaultValue(0)]
        public int CurrentSerialNo { get; set; }

        [DefaultValue(0)]
        public int SerialLength { get; set; }

        [DefaultValue(0)]
        public int PageCount { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }

        [DefaultValue(0)]
        public int BasedOn { get; set; }        
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
        public DateTime? DocumentDate { get; set; }
        public DateTime? PartyInvoDate { get; set; }
        public DateTime? DispatchDate { get; set; }
        public string DispatchLocation { get; set; }
        public int TotQuantity { get; set; }
        public double GrossAmount { get; set; }
        public double Discount { get; set; }
        public double OtherCharges { get; set; }
        public double NetAmount { get; set; }
        //public virtual InvGiftVoucherGroup InvGiftVoucherGroups { get; set; }
        //public virtual ICollection<InvGiftVoucherMaster> InvGiftVoucherMasters { get; set; }

        //[DefaultValue(0)]
        //public int GroupOfCompanyID { get; set; }

        //[MaxLength(50)]
        //public string CreatedUser { get; set; }

        //public DateTime? CreatedDate { get; set; }

        //[MaxLength(50)]
        //public string ModifiedUser { get; set; }

        //public DateTime? ModifiedDate { get; set; }
        //public int DataTransfer { get; set; }
    }
}
