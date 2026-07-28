using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain
{
    public class TourAgent : BaseEntity
    {
        [Key]
        public long TourAgentID { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        [DataType(DataType.Text)]
        [DefaultValue("")]
        public string AgentCode { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public string TourAgentTitle { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DefaultValue("")]
        public string TourAgentName { get; set; }
        
        [Required(ErrorMessage = "This field is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DefaultValue("")]
        public string BillingAddress1 { get; set; }


        [Required(ErrorMessage = "This field is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DefaultValue("")]
        public string BillingAddress2 { get; set; }

        [DefaultValue("")]
        public string BillingAddress3 { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [MaxLength(12, ErrorMessage = "NIC should less than 12 characters"), MinLength(10, ErrorMessage = "Invalid NIC")]
        public string NIC { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Only Numbers allowed")]
        public string Mobile { get; set; }

        public string Email { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(200)]
        [DefaultValue("")]
        public string Remarks { get; set; }

        [DefaultValue("")]
      //  [Range(1, int.MaxValue, ErrorMessage = "Please select a Tour Agent Company !")]
        public string TourAgentCompanyCode { get; set; }

        [DefaultValue(0)]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Tour Agent Company !")]
        public int TourAgentCompanyID { get; set; }

        [DefaultValue(0)]
        public decimal TourAmount { get; set; }

        [DefaultValue(0)]
        public decimal TourPercentage { get; set; }

        [DefaultValue(false)]
        public bool IsTourAgent { get; set; }

        [DefaultValue(false)]
        public bool IsActive { get; set; }

        [DefaultValue(0)]
        public int TCompanyID { get; set; }

    }
}
