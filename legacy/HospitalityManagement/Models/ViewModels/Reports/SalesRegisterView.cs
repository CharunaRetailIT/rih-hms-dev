using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models.ViewModels.Reports
{
    public class SalesRegisterView
    {

        public int CustomerIDS { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
        public int DepartmentId { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int LocationIDF { get; set; }
        public int LocationIDT { get; set; }
        public bool IsAsAtDate { get; set; }

        public List<Models.ViewModels.Reports.SalesRegisterViewModel> salesmodelList = new List<Models.ViewModels.Reports.SalesRegisterViewModel>();

    }
}