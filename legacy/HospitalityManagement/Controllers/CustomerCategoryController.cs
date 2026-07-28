using Newtonsoft.Json;
using OfficeOpenXml;
using RIT.HMS.BLL.Common;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using OfficeOpenXml.Style;
using System.Drawing;

namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
    public class CustomerCategoryController : Controller
    {
        BLL_CustomerCategory _bllcustomerCategory;
        BLL_Common _bllcommon;
        BLL_Location _location; 
        public CustomerCategoryController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllcustomerCategory = new BLL_CustomerCategory(cn);
            _bllcommon = new BLL_Common(cn);
            _location = new BLL_Location(cn);
        }

        [Authorize(Roles = "CusGroupCreatee")]
        public ActionResult Create()
        {
            return View();
        }
        [Authorize(Roles = "CusGroupEdit")]
        public ActionResult Edit(long id)
        {
           
            var exists = _bllcustomerCategory.GetCustomercategoryById(id);
            return View(exists);
        }
        [Authorize(Roles = "CusGroupEdit")]
        [HttpPost]
        public ActionResult Edit(CustomerCategory cuscat)
        {
            if (cuscat.IsActive == true && cuscat.IsDelete == true)
            {
                @ViewBag.Message = "4";
                return View(cuscat);
            }
        
            var exists = _bllcustomerCategory.GetCustomercategoryById(cuscat.CustomerCategoryID);
            exists.CustomerCategoryCode = cuscat.CustomerCategoryCode;
            exists.CustomerCategoryName = cuscat.CustomerCategoryName;
            exists.DiscountPrc = cuscat.DiscountPrc;
            exists.Remark = cuscat.Remark;
            exists.IsVat = cuscat.IsVat;
            exists.IsActive = cuscat.IsActive;
            exists.IsDelete = cuscat.IsDelete;
            exists.ModifiedDate = DateTime.Now;
            exists.ModifiedUser = Session["loggeduser"].ToString();
            exists.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            exists.CompanyID= Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            var errors = ModelState.Values.SelectMany(p => p.Errors);

            if (ModelState.IsValid == false)
            {
                ViewBag.CustomerCategoryCode = exists.CustomerCategoryCode;
                return View(exists);
            }

            if (_bllcustomerCategory.UpdateCustomerCategory(exists) == 1)
            {
                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.Message = "0";
            }
            ViewBag.CustomerCategoryCode = exists.CustomerCategoryCode;
            return View(exists);
        }

        [Authorize(Roles = "CusGroupCreatee")]
        [HttpPost]
        public ActionResult Create(CustomerCategory cuscat)
        {
            

            if (!ModelState.IsValid)
            {
                return View();

            }

           
            cuscat.CreatedDate = DateTime.Now;
            cuscat.CreatedUser = Session["loggeduser"].ToString();
            cuscat.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            //Added by pavi on 2019-12-01
            cuscat.IsActive = true;
            cuscat.CompanyID= Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            var existscuscat = _bllcustomerCategory.GetCustomercategoryByCode(cuscat.CustomerCategoryCode, cuscat.CompanyID);
            if (existscuscat != null)
            {
                ViewBag.Message = "3";
                return View("Create", cuscat);
            }


            if (_bllcustomerCategory.SaveCustomerCategory(cuscat) == 1)
            {
                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.Message = "2";
            }

            ViewBag.CustomerCategoryCode = cuscat.CustomerCategoryCode;
            return View("Create", cuscat);
        }

        [Authorize(Roles = "CusGroupView")]
        public ActionResult ViewCustomerCategories()
        {
          
            int compayid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var cuscat = _bllcustomerCategory.GetCustomerCategories(compayid);
            return View(cuscat);
        }

        [Authorize(Roles = "CusGroupView")]
        public void HMSCustomerGroups()
        {
            int compayid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var cuscat = _bllcustomerCategory.GetCustomerCategories(compayid);

            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("CustomerGroups");
            //---------------2023/03/27 ----------Tharaka---------------
            string compName, Address1, Address2, Address3, Tele, Fax, website = "";
            compName = _location.GetCompanyDetails().CompanyName;
            Address1 = _location.GetCompanyDetails().Address1;
            Address2 = _location.GetCompanyDetails().Address2;
            Address3 = _location.GetCompanyDetails().Address3;
            Tele = _location.GetCompanyDetails().Telephone;
            Fax = _location.GetCompanyDetails().Fax;
            website = _location.GetCompanyDetails().Website;
            #region Headings

            Sheet.Cells[1, 2].Value = compName;
            Sheet.Cells[1, 2, 3, 12].Merge = true;
            Sheet.Cells[1, 2, 3, 12].Style.Font.Size = 12;
            Sheet.Cells[1, 2, 3, 12].Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;

            Sheet.Cells[4, 2].Value = Address1 + " " + Address2 + " " + Address3;
            ;
            Sheet.Cells[4, 2, 4, 12].Merge = true;
            Sheet.Cells[4, 2, 4, 12].Style.Font.Size = 10;

            Sheet.Cells[5, 2].Value = "Tel:- " + Tele + " / " + ",  Fax:- " + Fax + ",  Web Site:- " + website;
            Sheet.Cells[5, 2, 5, 12].Merge = true;
            Sheet.Cells[5, 2, 5, 12].Style.Font.Size = 10;

            Sheet.Cells[6, 2].Value = "Customer Group Report" ;
            Sheet.Cells[6, 2, 6, 12].Merge = true;
            Sheet.Cells[6, 2, 6, 12].Style.Font.Size = 12;

            var businessUnitDetail = Sheet.Cells[1, 2, 6, 12];
            businessUnitDetail.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            businessUnitDetail.Style.Font.Bold = true;
            businessUnitDetail.Style.Font.Name = "Calibri";

            #endregion
            #region Print Detail Box

            var printDetailBox = Sheet.Cells[3, 13, 5, 14];
            printDetailBox.Style.Font.Size = 8;
            Sheet.Cells[3, 13, 5, 13].Style.Font.Bold = true;

            Sheet.Cells[3, 13].Value = "Date";
            Sheet.Cells[4, 13].Value = "Time";
            Sheet.Cells[5, 13].Value = "Req. By";

            Sheet.Cells[3, 14].Value = DateTime.Now.Date.ToShortDateString();
            Sheet.Cells[4, 14].Value = DateTime.Now.ToString("h:mm tt");

            if (Session["loggeduser"] != null)
                Sheet.Cells[5, 14].Value = Session["loggeduser"].ToString();
            else
                Sheet.Cells[5, 14].Value = " ";
            #endregion
            Sheet.Cells[8, 1].Value = "Customer Group Code";
            Sheet.Cells[8, 2].Value = "Customer Group Name";
            Sheet.Cells[8, 3].Value = "Discount %";
            Sheet.Cells[8, 4].Value = "Is Vat";
            Sheet.Cells[8, 5].Value = "Is Active";

            //Sheet.Cells["A1"].Value = "CustomerGroupCode";
            //Sheet.Cells["B1"].Value = "CustomerGroupName";
            //Sheet.Cells["C1"].Value = "Discount%";
            //Sheet.Cells["D1"].Value = "IsVat";
            //Sheet.Cells["E1"].Value = "IsActive";

            int row = 9;
            foreach (var item in cuscat)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.CustomerCategoryCode;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.CustomerCategoryName;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.DiscountPrc;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.IsVat;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.IsActive;

                row++;
            }
            #region Header Bold
            Color colFromHexHeading = System.Drawing.ColorTranslator.FromHtml("#919089");

            Sheet.Cells[8, 1, 8, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells[8, 1, 8, 5].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);

            var table = Sheet.Cells[8, 1, 8, 5];
            table.Style.Border.Top.Style =
            table.Style.Border.Left.Style =
            table.Style.Border.Right.Style =
            table.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            table.Style.Font.Bold = true;
            table.Style.Font.Name = "Calibri";
            table.AutoFitColumns();

            #endregion
            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSCustomerGroups.xlsx");

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                Ep.SaveAs(MyMemoryStream);
                MyMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }

        }

        [HttpGet]
        public JsonResult GetActiveCustomerCategories()
        {
           
            int compayid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var cuscats = _bllcustomerCategory.GetActiveCustomerCategories(compayid).ToList();

            return Json(JsonConvert.SerializeObject(cuscats, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }
    }
}