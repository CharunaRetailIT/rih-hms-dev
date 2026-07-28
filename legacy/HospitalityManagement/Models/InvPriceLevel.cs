using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
namespace HospitalityManagement.Models
{
    public class InvPriceLevel : BaseEntity
    {
        
        [Key]
        public long InvPriceLevelID { get; set; }
        [MaxLength(15)]
        public string PriceLevelCode { get; set; }
        [MaxLength(100)]
        public string PriceLevelName { get; set; }
        [DefaultValue(0)]
        public int ServingUnitID { get; set; }

        [MaxLength(15)]
        public string ServingUnit { get; set; }

        [DefaultValue(0)]
        public decimal CostPrice { get; set; }

        [DefaultValue(0)]
        public decimal SellingPrice { get; set; }

        [DefaultValue(0)]
        public decimal Qty { get; set; }

        [MaxLength(150)]
        public string Remark { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }

        [DefaultValue(0)]
        public int GroupOfCompanyID { get; set; }
     

        [MaxLength(50)]

        public string CreatedUser { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]

        public string ModifiedUser { get; set; }

        public int DataTransfer { get; set; }

        [DefaultValue(0)]
        public int LocationId { get; set; }

        [DefaultValue(0)]
        public int CompanyID { get; set; }
    }
}