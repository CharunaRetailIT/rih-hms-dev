using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;


namespace RIT.HMS.Domain
{
    public class RstCategory : BaseEntity
    {
        public int RstCategoryID { get; set; }


        [Required(ErrorMessage = "Department is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Department !")]
        [DataType(DataType.Text)]
        [DefaultValue(0)]
        public int RstDepartmentID { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DefaultValue(0)]
        public string RstCategoryCode { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [DefaultValue("")]
        public string RstCategoryName { get; set; }

        [DefaultValue("")]
        public string Remark { get; set; }

        [DefaultValue(0)]
        public bool IsActive { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }

        [NotMapped]
        [DefaultValue("")]
        public string DepartmentName { get; set; }


        [DefaultValue(null)]
        [NotMapped]
        public HttpPostedFileBase Photograph { get; set; }
        [DefaultValue("")]
        public byte[] CatImage { get; set; }
        [DefaultValue("")]
        public string CatImageName { get; set; }
        [DefaultValue("")]
        public string CatImageType { get; set; }
    }
}