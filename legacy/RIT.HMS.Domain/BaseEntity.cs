using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class BaseEntity
    {                        // : IBaseEntity
        public BaseEntity()
        {
            CompanyID = 1;
            GroupOfCompanyID = 1;
            CreatedUser = "";
            CreatedDate = DateTime.Now;
            ModifiedUser = "";
            ModifiedDate = DateTime.Now;
            DataTransfer = 0;
        }

        public int GroupOfCompanyID { get; set; }
       // [Index(IsClustered = true, IsUnique = true)]
        public int CompanyID { get; set; }
      //  [Index(IsClustered = true, IsUnique = true)]
        public int LocationId { get; set; }

        [MaxLength(50)]
        public string CreatedUser { get; set; }
      
        public DateTime CreatedDate { get; set; }

        [MaxLength(50)]
        public string ModifiedUser { get; set; }

        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime ModifiedDate { get; set; }

        [DefaultValue(0)]
        public int DataTransfer { get; set; }
        [NotMapped]
        public string LocationName  { get; set; }

        [NotMapped]
        public int UnitID { get; set; }
        //[NotMapped]
        //public int ToLocationID { get; set; }
        [NotMapped]

        public decimal RequestQty { get; set; }

        //[NotMapped]
        //[DefaultValue(0)]
        //public decimal CostPrice { get; set; }

        //[NotMapped]
        //[DefaultValue(0)]
        //public decimal SellingPrice { get; set; }

        [NotMapped]

        public string ReqDocumentNo { get; set; }

        [NotMapped]

        public int ReqFromLocation { get; set; }

        [NotMapped]

        public int RequestnoteHeaderId { get; set; }
    }
}