using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HospitalityManagement.Models.ViewModels.Promotions;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models.Promotions
{
    public class InvPromotionMaster
    {
        public long InvPromotionMasterID { get; set; }

        [DefaultValue(0)]
        public int CompanyID { get; set; }

        [DefaultValue(0)]
        public int LocationID { get; set; }

        [DefaultValue(0)]
        public int CostCentreID { get; set; }

        [Required()]
        [MaxLength(15)]
        public string PromotionCode { get; set; }

        [Required()]
        [MaxLength(50)]
        public string PromotionName { get; set; }

        [DefaultValue(0)]
        public bool IsAutoApply { get; set; }

        [DefaultValue(0)]
        public int PromotionTypeID { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [DefaultValue(0)]
        public bool IsMonday { get; set; }

        [DefaultValue(0)]
        public bool IsTuesday { get; set; }

        [DefaultValue(0)]
        public bool IsWednesday { get; set; }

        [DefaultValue(0)]
        public bool IsThuresday { get; set; }

        [DefaultValue(0)]
        public bool IsFriday { get; set; }

        [DefaultValue(0)]
        public bool IsSaturday { get; set; }

        [DefaultValue(0)]
        public bool IsSunday { get; set; }

        [DefaultValue(0)]
        public bool IsMondayTime { get; set; }

        [DefaultValue(0)]
        public bool IsTuesdayTime { get; set; }

        [DefaultValue(0)]
        public bool IsWednesdayTime { get; set; }

        [DefaultValue(0)]
        public bool IsThuresdayTime { get; set; }

        [DefaultValue(0)]
        public bool IsFridayTime { get; set; }

        [DefaultValue(0)]
        public bool IsSaturdayTime { get; set; }

        [DefaultValue(0)]
        public bool IsSundayTime { get; set; }

        public DateTime? MondayStartTime { get; set; }

        public DateTime? MondayEndTime { get; set; }

        public DateTime? TuesdayStartTime { get; set; }

        public DateTime? TuesdayEndTime { get; set; }

        public DateTime? WednesdayStartTime { get; set; }

        public DateTime? WednesdayEndTime { get; set; }

        public DateTime? ThuresdayStartTime { get; set; }

        public DateTime? ThuresdayEndTime { get; set; }

        public DateTime? FridayStartTime { get; set; }

        public DateTime? FridayEndTime { get; set; }

        public DateTime? SaturdayStartTime { get; set; }

        public DateTime? SaturdayEndTime { get; set; }

        public DateTime? SundayStartTime { get; set; }

        public DateTime? SundayEndTime { get; set; }

        [DefaultValue(0)]
        public int PaymentMethodID { get; set; }

        [DefaultValue(0)]
        public bool IsProvider { get; set; }

        [DefaultValue(0)]
        public bool IsAllLocations { get; set; }

        [DefaultValue(0)]
        public bool IsAllType { get; set; }

        [DefaultValue(0)]
        public bool IsValueRange { get; set; }

        [DefaultValue(0)]
        public decimal MinimumValue { get; set; }

        [DefaultValue(0)]
        public decimal MaximumValue { get; set; }

        [DefaultValue(0)]
        public decimal DiscountValue { get; set; }

        [DefaultValue(0)]
        public decimal DiscountPercentage { get; set; }

        [DefaultValue(0)]
        public decimal Points { get; set; }

        [MaxLength(150)]
        public string Remark { get; set; }

        [MaxLength(150)]
        public string DisplayMessage { get; set; }

        [MaxLength(150)]
        public string CashierMessage { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }

        [DefaultValue(0)]
        public bool IsRaffle { get; set; }

        [DefaultValue(0)]
        public bool IsIncreseQty { get; set; }

        [DefaultValue(0)]
        public int PromotionTypeNew { get; set; }

        public int SupplierID { get; set; }

        public DateTime ModifiedDate { get; set; }


        //from chamodi's

        [NotMapped]
        public List<VMPromotionSchedular> VMPromotionSchedular { get; set; }

        [NotMapped]
        public virtual ICollection<InvPromoBusinessType> InvPromoBusinessType { get; set; }

        [NotMapped]

        public List<InvPromoBusinessType> PromoBusinessType { get; set; }

        [NotMapped]

        public string[] BusinessTypeID { get; set; }

        [NotMapped]

        public string[] CustomerGroupId { get; set; }

        [NotMapped]
        public virtual ICollection<InvPromoCustomerCategory> InvPromoCustomerCategory { get; set; }

        [NotMapped]

        public List<InvPromoCustomerCategory> PromoCustomerCategory { get; set; }



        //public virtual ICollection<InvPromotionDetailsBuyXProduct> InvPromotionDetailsBuyXProduct { get; set; }
        //public virtual ICollection<InvPromotionDetailsBuyXDepartment> InvPromotionDetailsBuyXDepartment { get; set; }
        //public virtual ICollection<InvPromotionDetailsBuyXCategory> InvPromotionDetailsBuyXCategory { get; set; }
        //public virtual ICollection<InvPromotionDetailsBuyXSubCategory> InvPromotionDetailsBuyXSubCategory { get; set; }
        //public virtual ICollection<InvPromotionDetailsBuyXSubCategory2> InvPromotionDetailsBuyXSubCategory2 { get; set; }
        //public virtual ICollection<InvPromotionDetailsGetYDetails> InvPromotionDetailsGetYDetails { get; set; }
        //public virtual ICollection<InvPromotionDetailsProductDis> InvPromotionDetailsProductsDis { get; set; }
        //public virtual ICollection<InvPromotionDetailsDepartmentDis> InvPromotionDetailsDepartmentDis { get; set; }
        //public virtual ICollection<InvPromotionDetailsCategoryDis> InvPromotionDetailsCategoryDis { get; set; }
        //public virtual ICollection<InvPromotionDetailsSubCategoryDis> InvPromotionDetailsSubCategoryDis { get; set; }
        //public virtual ICollection<InvPromotionDetailsSubCategory2Dis> InvPromotionDetailsSubCategory2Dis { get; set; }
        //public virtual ICollection<InvPromotionDetailsSubTotal> InvPromotionDetailsSubTotal { get; set; }
        //public virtual ICollection<InvPromotionDetailsValueBase> InvPromotionDetailsValueBase { get; set; }
        //public virtual ICollection<InvPromotionDetailsAllowLocations> InvPromotionDetailsAllowLocations { get; set; }
        //public virtual ICollection<InvPromotionDetailsAllowTypes> InvPromotionDetailsAllowTypes { get; set; }

    }
}