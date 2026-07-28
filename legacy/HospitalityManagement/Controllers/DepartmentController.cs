//using HospitalityManagement.Models;
//using HospitalityManagement.Service;
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
    public class DepartmentController : Controller
    {
        BLL_Department _blldepartment;
        BLL_Location _location; 
        public DepartmentController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _blldepartment = new BLL_Department(cn);
            _location = new BLL_Location(cn);
        }

        // GET: Department
        [Authorize(Roles = "DCreatee")]
        public ActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "DEdit")]
        public ActionResult Edit(long id)
        {
          
          
            var exists = _blldepartment.GetDepartmentById(id);
            @ViewBag.FileName = exists.DeptImageName;
            return View(exists);
        }

        [Authorize(Roles = "DEdit")]
        [HttpPost]
        public ActionResult Edit(RstDepartment sysdept)
        {

            Common common = new Common();
            if (sysdept.Photograph != null && common.CheckImageType(sysdept.Photograph.ContentType) == false)
            {
                ModelState.AddModelError("Photograph", "Only an Image required !");
                return View(sysdept);
            }
            if (sysdept.IsActive == true && sysdept.IsDelete == true)
            {
                @ViewBag.Message = "4";
                return View(sysdept);
            }
            
            
           
            var exists = _blldepartment.GetDepartmentById(sysdept.RstDepartmentID);
            exists.DepartmentCode = sysdept.DepartmentCode;
            exists.DepartmentName = sysdept.DepartmentName;
            exists.Remark = sysdept.Remark;
            exists.IsActive = sysdept.IsActive;
            exists.IsDelete = sysdept.IsDelete;
            exists.ModifiedDate = DateTime.Now;
            exists.ModifiedUser = Session["loggeduser"].ToString();
            //exists.GroupOfCompanyID = 0;
            //exists.CompanyID = 0;
            exists.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            exists.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            //exists.DataTransfer = 0;

            if (sysdept.Photograph != null)
            {
                byte[] photo;
                using (BinaryReader br = new BinaryReader(sysdept.Photograph.InputStream))
                {
                    photo = br.ReadBytes(sysdept.Photograph.ContentLength);
                    exists.DeptImage = photo;
                    exists.DeptImageName = sysdept.Photograph.FileName;
                    exists.DeptImageType = sysdept.Photograph.ContentType;
                }
            }
           

            var errors = ModelState.Values.SelectMany(p=>p.Errors);

            if (ModelState.IsValid == false)
            {
                ViewBag.DeptCode = exists.DepartmentCode;
                return View(exists);
            }


            if (_blldepartment.UpdateDepatment(exists) == 1)
            {
                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.Message = "0";
            }
            ViewBag.DeptCode = exists.DepartmentCode;
            return View(exists);
        }

        [Authorize(Roles = "DCreatee")]
        [HttpPost]
        public ActionResult Create(RstDepartment dept)
        {

            Common common = new Common();
            if (dept.Photograph != null && common.CheckImageType(dept.Photograph.ContentType) == false)
            {
                ModelState.AddModelError("Photograph", "Only an Image required !");
                return View(dept);
            }


            if (!ModelState.IsValid)
            {
                return View();

            }



           
            dept.CreatedDate = DateTime.Now;
            dept.CreatedUser = Session["loggeduser"].ToString();            
            //dept.GroupOfCompanyID = 0;
            //dept.CompanyID = 0;
            dept.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            //dept.DataTransfer = 0;

            //Added by pavithra on 2019-11-30
            dept.IsActive = true;

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            dept.CompanyID = companyid;
            var existsdept = _blldepartment.GetDeptByCode(dept.DepartmentCode,companyid);
            if (existsdept != null)
            {
                ViewBag.Message = "3";
                return View("Create", dept);
            }

            if (dept.Photograph != null)
            {
                byte[] photo;
                using (BinaryReader br = new BinaryReader(dept.Photograph.InputStream))
                {
                    photo = br.ReadBytes(dept.Photograph.ContentLength);
                    dept.DeptImage = photo;
                    dept.DeptImageName = dept.Photograph.FileName;
                    dept.DeptImageType = dept.Photograph.ContentType;
                }
            }



            if (_blldepartment.SaveDepartment(dept) == 1)
            {
                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.Message = "2";
            }

            ViewBag.DeptCode = dept.DepartmentCode;
            return View("Create", dept);

        }

        [Authorize(Roles = "DView")]
        public ActionResult ViewDepartments()
        {
            
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var depts = _blldepartment.GetDepartments(companyid);
            return View(depts);
        }

        [Authorize(Roles = "DView")]
        public void HMSDepartments()
        {

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var depts = _blldepartment.GetDepartments(companyid);
                  
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Departments");

            string compName, Address1, Address2, Address3, Tele, Fax, website, ReportHead = "";
            compName = _location.GetCompanyDetails().CompanyName;
            Address1 = _location.GetCompanyDetails().Address1;
            Address2 = _location.GetCompanyDetails().Address2;
            Address3 = _location.GetCompanyDetails().Address3;
            Tele = _location.GetCompanyDetails().Telephone;
            Fax = _location.GetCompanyDetails().Fax;
            website = _location.GetCompanyDetails().Website;

            #region

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

            Sheet.Cells[6, 2].Value = "Department Report";
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

            Sheet.Cells[8, 1].Value = "Department Code";
            Sheet.Cells[8, 2].Value = "Department Name";
            Sheet.Cells[8, 3].Value = "Active";

            int row = 9;
            foreach (var item in depts)
            {

                Sheet.Cells[row, 1].Value = item.DepartmentCode;
                Sheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left; 
                Sheet.Cells[row, 2].Value = item.DepartmentName;
                Sheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left; 
                if (item.IsActive == true)
                {
                    Sheet.Cells[row, 3].Value = "Yes";
                    Sheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                }
                else
                {
                    Sheet.Cells[row, 3].Value = "No";
                    Sheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                }

                row++;
            }
            #region
            Color colFromHexHeading = System.Drawing.ColorTranslator.FromHtml("#919089");

            Sheet.Cells[8, 1, 8, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells[8, 1, 8, 3].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);

            var table = Sheet.Cells[8, 1, 8, 3];
            table.Style.Border.Top.Style =
            table.Style.Border.Left.Style =
            table.Style.Border.Right.Style =
            table.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            table.Style.Font.Bold = true;
            table.Style.Font.Name = "Calibri";
            table.AutoFitColumns();

            #endregion



            //Sheet.Cells["A1"].Value = "DepartmentCode";
            //Sheet.Cells["B1"].Value = "DepartmentName";
            //Sheet.Cells["c1"].Value = "Active";
            
            //int row = 2;
            //foreach (var item in depts)
            //{
            //    Sheet.Cells[string.Format("A{0}", row)].Value = item.DepartmentCode;
            //    Sheet.Cells[string.Format("B{0}", row)].Value = item.DepartmentName;
            //    Sheet.Cells[string.Format("c{0}", row)].Value = item.IsActive;

            //row++;
            //}

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSDepartments.xlsx");

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                Ep.SaveAs(MyMemoryStream);
                MyMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }

        }


        [HttpGet]
        public JsonResult GetActiveDepartments()
        {
           
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var departments = _blldepartment.GetActiveDepartments(companyid).ToList();
            
            return Json(JsonConvert.SerializeObject(departments, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetActiveDepartmentsByLocationId(int locationid)
        {
           
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var departments = _blldepartment.GetActiveDepartmentsByLocationId(companyid,locationid).ToList();

            return Json(JsonConvert.SerializeObject(departments, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        //Added by Pavithra on 2019-11-30
        [HttpGet]
        public JsonResult CheckDepartmentCode(string code)
        {
           
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var dep = _blldepartment.FindByCode(code,companyid);
            return new JsonResult { Data = dep, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }
    }
}