//using HospitalityManagement.Models;
//using HospitalityManagement.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.IO;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain;
using OfficeOpenXml;
using System.Data;
using OfficeOpenXml.Style;
using System.Drawing;
namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
    public class SubCategoryController : Controller
    {
        BLL_SubCategory _bllsubCategory;     
        BLL_Category _bllcategory;
        BLL_CheckDependency _bllcheckDependency;
        BLL_Location _location; 
        public SubCategoryController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();

            _bllsubCategory = new BLL_SubCategory(cn);
            _bllcategory = new BLL_Category(cn);
            _bllcheckDependency = new BLL_CheckDependency(cn);
            _location = new BLL_Location(cn);
        }


        [Authorize(Roles = "SCatView")]
        public ActionResult ViewSubCategories()
        {
           
            //CategoryService catservice = new CategoryService();
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var subcats = _bllsubCategory.GetSubCategories(companyid);
            try
            {



                subcats.ToList().ForEach(c =>
                {
                    if (c.RstCategoryID != 0)
                        c.CategoryName = _bllcategory.GetCategoryById(c.RstCategoryID).RstCategoryName;
                });
            }
            catch (NullReferenceException) { }
            return View("ViewSubCategories", subcats);

        }

        [Authorize(Roles = "SCatView")]
        public void HMSSubCategories()
        {

          
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var subcats = _bllsubCategory.GetSubCategories(companyid);
            try
            {
                subcats.ToList().ForEach(c =>
                {
                    if (c.RstCategoryID != 0)
                        c.CategoryName = _bllcategory.GetCategoryById(c.RstCategoryID).RstCategoryName;
                });
            }
            catch (NullReferenceException) { }

            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("SubCategories");

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

            Sheet.Cells[6, 2].Value = "Sub Category Report";
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
            Sheet.Cells[8, 1].Value = "Sub Category Code";
            Sheet.Cells[8, 2].Value = "Sub Category Name";
            Sheet.Cells[8, 3].Value = "Category Name";
            Sheet.Cells[8, 4].Value = "Active";

            //Sheet.Cells["A1"].Value = "SubCategoryCode";
            //Sheet.Cells["B1"].Value = "SubCategoryName";
            //Sheet.Cells["C1"].Value = "CategoryName";
            //Sheet.Cells["D1"].Value = "Active";

            int row = 9;
            foreach (var item in subcats)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.RstSubCategoryCode;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.RstSubCategoryName;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.CategoryName;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.IsActive;

                row++;
            }
            #region Header Bold
            Color colFromHexHeading = System.Drawing.ColorTranslator.FromHtml("#919089");

            Sheet.Cells[8, 1, 8, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells[8, 1, 8, 4].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);

            var table = Sheet.Cells[8, 1, 8, 4];
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
            Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSSubCategories.xlsx");

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                Ep.SaveAs(MyMemoryStream);
                MyMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }

        }

        [Authorize(Roles = "SCatCreatee")]
        public ActionResult Create()
        {
          
            if (_bllcheckDependency.CheckDependency("SubCategory"))
            {
                ViewBag.SubCatDependency = "1";
            }
            else
            {
                ViewBag.SubCatDependency = "0";
            }

            return View();
        }

        [Authorize(Roles = "SCatEdit")]

        [HttpPost]
        public ActionResult Edit(RstSubCategory subcat)
        {

            Common common = new Common();
            if (subcat.Photograph != null && common.CheckImageType(subcat.Photograph.ContentType) == false)
            {
                ModelState.AddModelError("Photograph", "Only an Image required !");
                return View(subcat);
            }
            if (subcat.IsActive == true && subcat.IsDelete == true)
            {
                @ViewBag.Message = "4";
                return View(subcat);
            }

         
            if (_bllcheckDependency.CheckDependency("SubCategory"))
            {
                ViewBag.SubCatDependency = "1";
            }
            else
            {
                ViewBag.SubCatDependency = "0";
            }

            if (!ModelState.IsValid)
            {
                @ViewBag.RstCategoryID = subcat.RstCategoryID;
                return View(subcat);
            }

         
            var exists = _bllsubCategory.GetSubCategoryById(subcat.RstSubCategoryID);

            exists.RstSubCategoryCode = subcat.RstSubCategoryCode;
            exists.RstSubCategoryName = subcat.RstSubCategoryName;
            exists.Remark = subcat.Remark;
            exists.IsActive = subcat.IsActive;
            exists.IsDelete = subcat.IsDelete;
            exists.RstCategoryID = subcat.RstCategoryID;
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            exists.CompanyID = companyid;
            @ViewBag.RstCategoryID = subcat.RstCategoryID;

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var err in errors)
            {
                var k = err;
            }

            if (subcat.Photograph != null)
            {
                byte[] photo;
                using (BinaryReader br = new BinaryReader(subcat.Photograph.InputStream))
                {
                    photo = br.ReadBytes(subcat.Photograph.ContentLength);
                    exists.SubCatImage = photo;
                    exists.SubCatImageName = subcat.Photograph.FileName;
                    exists.SubCatImageType = subcat.Photograph.ContentType;
                }
            }



            if (_bllsubCategory.UpdateSubCategory(exists, subcat) == 1)
            {
                ViewBag.Message = "1";
                return View(new RstSubCategory());
            }
            else
            {
                ViewBag.Message = "2";
                return View(subcat);
            }


        }

        [Authorize(Roles = "SCatEdit")]
        public ActionResult Edit(long id)
        {
            
            if (_bllcheckDependency.CheckDependency("SubCategory"))
            {
                ViewBag.SubCatDependency = "1";
            }
            else
            {
                ViewBag.SubCatDependency = "0";
            }

           
            var exists = _bllsubCategory.GetSubCategoryById(id);
            ViewBag.RstCategoryID = exists.RstCategoryID;
            @ViewBag.FileName = exists.SubCatImageName;
            return View(exists);
        }
        [Authorize(Roles = "SCatCreatee")]
        [HttpPost]
        public ActionResult Create(RstSubCategory subcat)
        {
            Common common = new Common();
            if (subcat.Photograph != null && common.CheckImageType(subcat.Photograph.ContentType) == false)
            {
                ModelState.AddModelError("Photograph", "Only an Image required !");
                return View(subcat);
            }

            subcat.IsActive = true;

          
            if (_bllcheckDependency.CheckDependency("SubCategory"))
            {
                ViewBag.SubCatDependency = "1";
            }
            else
            {
                ViewBag.SubCatDependency = "0";
            }


            if (!ModelState.IsValid)
            {
                @ViewBag.RstCategoryID = subcat.RstCategoryID;
                return View(subcat);
            }


         
            subcat.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            subcat.CompanyID = companyid;
            var existssubcat = _bllsubCategory.GetSubCatByCode(subcat.RstSubCategoryCode,companyid);
            if (existssubcat != null)
            {
                ViewBag.Message = "3";
                @ViewBag.RstCategoryID = subcat.RstCategoryID;
                return View("Create", subcat);
            }

            ViewBag.SubCatCode = subcat.RstSubCategoryCode;

            if (subcat.Photograph != null)
            {
                byte[] photo;
                using (BinaryReader br = new BinaryReader(subcat.Photograph.InputStream))
                {
                    photo = br.ReadBytes(subcat.Photograph.ContentLength);
                    subcat.SubCatImage = photo;
                    subcat.SubCatImageName = subcat.Photograph.FileName;
                    subcat.SubCatImageType = subcat.Photograph.ContentType;
                }
            }




            if (_bllsubCategory.SaveSubCategory(subcat) == 1)
            {
                ViewBag.Message = "1";
                return View("Create", new RstSubCategory());
            }
            else
            {
                ViewBag.Message = "2";
                return View("Create", subcat);
            }



        }

    }
}