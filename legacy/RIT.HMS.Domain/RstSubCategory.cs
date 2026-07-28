using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class RstSubCategory : BaseEntity
    {
        public int RstSubCategoryID { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a Category !")]
        [Required(ErrorMessage = "The field is required")]
        [DataType(DataType.Text)]
        [DefaultValue(0)]
        public int RstCategoryID { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        [DataType(DataType.Text)]
        [DefaultValue(0)]
        public string RstSubCategoryCode { get; set; }

        [Required(ErrorMessage = "The field is required")]
        [DataType(DataType.Text)]
        [DefaultValue("")]
        public string RstSubCategoryName { get; set; }

        [DefaultValue("")]
        public string Remark { get; set; }

        [DefaultValue(0)]
        public bool IsActive { get; set; }

        [DefaultValue(0)]
        public bool IsDelete { get; set; }

        [NotMapped]
        [DefaultValue("")]
        public string CategoryName { get; set; }

        [DefaultValue(null)]
        [NotMapped]
        public HttpPostedFileBase Photograph { get; set; }
        [DefaultValue("")]
        public byte[] SubCatImage { get; set; }
        [DefaultValue("")]
        public string SubCatImageName { get; set; }
        [DefaultValue("")]
        public string SubCatImageType { get; set; }

    }
}