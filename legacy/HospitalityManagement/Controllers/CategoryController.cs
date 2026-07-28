
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain;
using OfficeOpenXml;
using System.Data;
using OfficeOpenXml.Style;
using System.Drawing;

namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
    public class CategoryController : Controller
    {
        BLL_CheckDependency _bllcheckDependency;
        BLL_Category _bllcategory;
        BLL_Department _blldepartment;
        BLL_Location _location; 
        public CategoryController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllcheckDependency = new BLL_CheckDependency(cn);
            _bllcategory = new BLL_Category(cn);
            _blldepartment = new BLL_Department(cn);
            _location = new BLL_Location(cn);
        }

        // GET: Category
        [Authorize(Roles = "CatCreatee")]
        public ActionResult Create()
        {

           
           
            if (_bllcheckDependency.CheckDependency("Category"))
            {
                ViewBag.CatDependency = "1";
            }
            else
            {
                ViewBag.CatDependency = "0";
            }

            return View("Create");
        }

        [Authorize(Roles = "CatEdit")]
        public ActionResult Edit(long id)
        {
            
            if (_bllcheckDependency.CheckDependency("Category"))
            {
                ViewBag.CatDependency = "1";
            }
            else
            {
                ViewBag.CatDependency = "0";
            }

           
           
            var exists = _bllcategory.GetCategoryById(id);
            ViewBag.RstDepartmentID = exists.RstDepartmentID;
            @ViewBag.FileName = exists.CatImageName;
            return View(exists);
        }

        [Authorize(Roles = "CatEdit")]
        [HttpPost]
        public ActionResult Edit(RstCategory cat)
        {
            Common common = new Common();
            if (cat.Photograph != null && common.CheckImageType(cat.Photograph.ContentType) == false)
            {
                ModelState.AddModelError("Photograph", "Only an Image required !");
                return View(cat);
            }
            if (cat.IsActive == true && cat.IsDelete == true)
            {
                @ViewBag.Message = "4";
                return View(cat);
            }
           
            if (_bllcheckDependency.CheckDependency("Category"))
            {
                ViewBag.CatDependency = "1";
            }
            else
            {
                ViewBag.CatDependency = "0";
            }

            if (!ModelState.IsValid)
            {
                @ViewBag.RstDepartmentID = cat.RstDepartmentID;
                return View(cat);
            }

           
            var exists = _bllcategory.GetCategoryById(cat.RstCategoryID);
            exists.RstCategoryName = cat.RstCategoryName;
            exists.Remark = cat.Remark;
            exists.IsActive = cat.IsActive;
            exists.IsDelete = cat.IsDelete;
            exists.ModifiedDate = DateTime.Now;
            exists.ModifiedUser = Session["loggeduser"].ToString();
            exists.DataTransfer = 0;
            exists.IsDelete = cat.IsDelete;
            exists.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            exists.RstDepartmentID = cat.RstDepartmentID;
            ViewBag.CatCode = exists.RstCategoryCode;

            if (cat.Photograph != null)
            {
                byte[] photo;
                using (BinaryReader br = new BinaryReader(cat.Photograph.InputStream))
                {
                    photo = br.ReadBytes(cat.Photograph.ContentLength);
                    exists.CatImage = photo;
                    exists.CatImageName = cat.Photograph.FileName;
                    exists.CatImageType = cat.Photograph.ContentType;
                }
            }

            if (_bllcategory.UpdateCategory(exists)==1)
            {
                ViewBag.Message = "1";
                return View(new RstCategory());
            }
            else
            {
                ViewBag.Message = "2";
                return View(cat);
            }
            
            
          
        }

        [Authorize(Roles = "CatCreatee")]
        [HttpPost]
        public ActionResult Create(RstCategory category)
        {

            Common common = new Common();
            if (category.Photograph != null && common.CheckImageType(category.Photograph.ContentType) == false)
            {
                ModelState.AddModelError("Photograph", "Only an Image required !");
                return View(category);
            }


           
            if (_bllcheckDependency.CheckDependency("Category"))
            {
                ViewBag.CatDependency = "1";
            }
            else
            {
                ViewBag.CatDependency = "2";
            }


            if (!ModelState.IsValid)
            {
                @ViewBag.RstDepartmentID = category.RstDepartmentID;
                return View("Create",category);
            }

          
            category.GroupOfCompanyID = 0;
            category.CompanyID = 0;
            category.CreatedDate = DateTime.Now;
            category.CreatedUser = Session["loggeduser"].ToString();
            category.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            category.DataTransfer = 0;
            //Added by pavithra on 2019-11-30
            category.IsActive = true;

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            category.CompanyID = companyid;
            var existscat = _bllcategory.GetCatByCode(category.RstCategoryCode,companyid);
            ViewBag.CatCode = category.RstCategoryCode;
            if (existscat != null)
            {
                ViewBag.Message = "3";
                @ViewBag.RstDepartmentID = category.RstCategoryID;
                return View("Create",category);
            }


            if (category.Photograph != null)
            {
                byte[] photo;
                using (BinaryReader br = new BinaryReader(category.Photograph.InputStream))
                {
                    photo = br.ReadBytes(category.Photograph.ContentLength);
                    category.CatImage = photo;
                    category.CatImageName = category.Photograph.FileName;
                    category.CatImageType = category.Photograph.ContentType;
                }
            }

            if (_bllcategory.SaveCategory(category) == 1)
            {
                ViewBag.Message = "1";
                return View("Create",new RstCategory());
            }
            else
            {
                ViewBag.Message = "2";
                return View("Create", category);
            }
                
        }


        [Authorize(Roles = "CatView")]
        public ActionResult ViewCategories()
        {
          
           
            
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var exists = _bllcategory.GetCategories(companyid);
            try
            {
               
                exists.ToList().ForEach(c =>
                {
                    if (c.RstDepartmentID == 0)
                    {
                        c.DepartmentName = "";
                    }
                    else
                    {
                        c.DepartmentName = _blldepartment.GetDepartmentById(c.RstDepartmentID).DepartmentName;
                    }
                });

                return View(exists);
            }
            catch (NullReferenceException ex)
            {
                return View(exists);
            }
            
           
        }


        [Authorize(Roles = "CatView")]
        public void HMSCategories()
        {

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var exists = _bllcategory.GetCategories(companyid);
            exists.ToList().ForEach(c =>
            {
                if (c.RstDepartmentID == 0)
                {
                    c.DepartmentName = "";
                }
                else
                {
                    c.DepartmentName = _blldepartment.GetDepartmentById(c.RstDepartmentID).DepartmentName;
                }
            });
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Categories");
            //---------------2023/03/27 ----------Tharaka---------------
            string compName, Address1, Address2, Address3, Tele, Fax, website ="";
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

            Sheet.Cells[6, 2].Value = "Category Report";
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

            Sheet.Cells[8, 1].Value = "Category Code";
            Sheet.Cells[8, 2].Value = "Category Name";
            Sheet.Cells[8, 3].Value = "Department Name";
            Sheet.Cells[8, 4].Value = "Active";

            //Sheet.Cells["A1"].Value = "CategoryCode";
            //Sheet.Cells["B1"].Value = "CategoryName";
            //Sheet.Cells["C1"].Value = "DepartmentName";
            //Sheet.Cells["D1"].Value = "Active";
            int row = 9;
            foreach (var item in exists)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.RstCategoryCode;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.RstCategoryName;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.DepartmentName;
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
            Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSCategories.xlsx");

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                Ep.SaveAs(MyMemoryStream);
                MyMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
        }
        [HttpGet]
        public JsonResult GetActiveCategories()
        {
           
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var categories = _bllcategory.GetActiveCategory(companyid);
            return Json(JsonConvert.SerializeObject(categories, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetCategoriesByDepartmentId(long id)
        {
           
            var categoriesbydept = _bllcategory.GetCategoryByDepartmentId(id);
            return Json(JsonConvert.SerializeObject(categoriesbydept, Formatting.None, new JsonSerializerSettings
                { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        //Added by Pavithra on 2019-11-30
        [HttpGet]
        public JsonResult CheckCategoryCode(string code)
        {
            
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var cat = _bllcategory.FindByCode(code,companyid);
            return new JsonResult { Data = cat, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }

    }
}