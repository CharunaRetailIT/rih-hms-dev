using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RIT.HMS.Domain
{
   public class TourAgentCompany : BaseEntity
   {
        [Key]
        public int TourAgentCompanyID { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        [DataType(DataType.Text)]
        [DefaultValue(0)]
        public string TourAgentCompanyCode { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DefaultValue("")]
        public string TourAgentCompanyName { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DefaultValue("")]
        public string Address1 { get; set; }


        [Required(ErrorMessage = "This field is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DefaultValue("")]
        public string Address2 { get; set; }


        [Required(ErrorMessage = "This field is required")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Only Numbers allowed")]
        public string Telephone { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Only Numbers allowed")]
        public string Mobile { get; set; }

        public string FaxNo { get; set; }
        public string Email { get; set; }

        
        [StringLength(200)]
        [DefaultValue("")]
        public string WebAddress { get; set; }

        [StringLength(200)]
        [DefaultValue("")]
        public string ContactPerson { get; set; }

        [DefaultValue(false)]
        public bool IsDelete { get; set; }

        [DefaultValue(0)]
        public decimal CommissionAmount { get; set; }

        [DefaultValue(false)]
        public bool IsActive { get; set; }


   


    }
}
