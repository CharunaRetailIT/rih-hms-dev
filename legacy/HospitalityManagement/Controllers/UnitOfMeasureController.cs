//using HospitalityManagement.Models;
//using HospitalityManagement.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain;
using OfficeOpenXml;
using System.IO;
using System.Data;
using OfficeOpenXml.Style;
using System.Drawing;

namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
    [Authorize(Roles = "PrdEdit")]
    public class UnitOfMeasureController : Controller
    {
        BLL_UnitOfMeasure _bllunitOfMeasure;
        BLL_Location _location; 
        public UnitOfMeasureController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
             _bllunitOfMeasure = new BLL_UnitOfMeasure(cn);
             _location = new BLL_Location(cn);
        }

        public ActionResult Index()
        {
            return View();
        }


       public ActionResult Clear()
       {

           return View("Edit");
       }
            
    
       public ActionResult Edit(long id)
       {
         
           var unitofmeasure = _bllunitOfMeasure.GetUnitOfMeasureById(id);
           ViewBag.UnitOfMeasureID = unitofmeasure.UnitOfMeasureId;
           return View(unitofmeasure);
       }

      
       [HttpPost]
       public ActionResult Edit(UnitOfMeasure unitofmeasures)
       {
            var unitofmeasure = _bllunitOfMeasure.GetUnitOfMeasureById(unitofmeasures.UnitOfMeasureId);
           unitofmeasure.UnitOfMeasureCode = unitofmeasures.UnitOfMeasureCode;
           unitofmeasure.UnitOfMeasureName = unitofmeasures.UnitOfMeasureName;
           unitofmeasure.Remark = unitofmeasures.Remark;
           unitofmeasure.IsDelete = unitofmeasures.IsDelete;
           unitofmeasure.ModifiedDate = DateTime.UtcNow;
           unitofmeasure.ModifiedUser = Session["loggeduser"].ToString();
            unitofmeasure.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            if (_bllunitOfMeasure.UpdateUnitOfMeasure(unitofmeasure) == 1)
           {

               ViewBag.Message = "1";
           }
           else
           {
               ViewBag.Message = "0";
           }

           return View();
       }


       public ActionResult ViewUnitOfMeasures()
       {

         
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var unitofmeasures = _bllunitOfMeasure.GetUnitOfMeasures(companyid).OrderBy(c => c.UnitOfMeasureCode);

           unitofmeasures.ToList().ForEach(c =>
           {

               //    c.UserName = userreporsitory.GetSysUserMasterID(c.SysUserMasterID).GroupOfCompanyName;
           });

           return View(unitofmeasures);
       }


        public void HMSUnitOfMeasures()
        {


            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var unitofmeasures = _bllunitOfMeasure.GetUnitOfMeasures(companyid).OrderBy(c => c.UnitOfMeasureCode);

            unitofmeasures.ToList().ForEach(c =>
            {

                //    c.UserName = userreporsitory.GetSysUserMasterID(c.SysUserMasterID).GroupOfCompanyName;
            });

            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("UnitOfMeasures");
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

            Sheet.Cells[6, 2].Value = "Unit of Measure Report";
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
            Sheet.Cells[8, 1].Value = "Code";
            Sheet.Cells[8, 2].Value = "Name";
            Sheet.Cells[8, 3].Value = "Remark";
            Sheet.Cells[8, 4].Value = "Is Delate";
            //Sheet.Cells["A1"].Value = "Code";
            //Sheet.Cells["B1"].Value = "Name";
            //Sheet.Cells["C1"].Value = "Remark";
            //Sheet.Cells["D1"].Value = "IsDelate";
            int row = 9;
            foreach (var item in unitofmeasures)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.UnitOfMeasureCode;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.UnitOfMeasureName;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.Remark;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.IsDelete;
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
            Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSUnitOfMeasures.xlsx");

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                Ep.SaveAs(MyMemoryStream);
                MyMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }


        }

        [HttpPost]
       public ActionResult Create(UnitOfMeasure unitofmeasures)
       {
           unitofmeasures.CreatedUser = Session["loggeduser"].ToString();
           unitofmeasures.CreatedDate = DateTime.Now;
           unitofmeasures.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
           unitofmeasures.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());                                                                                                                                                                                                                                                                                                                                                                                                                                                                            

            if (!ModelState.IsValid)
           {
               return View("Index", unitofmeasures);
           }

         
           int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
           var exists = _bllunitOfMeasure.GetUnitOfMeasureByCode(unitofmeasures.UnitOfMeasureCode,companyid);
           if (exists != null)
           {
               ViewBag.Message = "3";
               return View("Index", unitofmeasures);
           }
           if (_bllunitOfMeasure.SaveUnitOfMeasure(unitofmeasures) == 1)
           {
               @ViewBag.Message = "1";
               //unitofmeasures = null;
           }
           else
           {
               @ViewBag.Message = "2";
           }

           @ViewBag.UnitOfMeasureCode = unitofmeasures.UnitOfMeasureCode;
           return View("Index", unitofmeasures);
       }


       [HttpGet]
       public JsonResult GetActiveUnitOfMeasures()
       {
          
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var unitofmeasures = _bllunitOfMeasure.GetUnitOfMeasures(companyid);
           return Json(JsonConvert.SerializeObject(unitofmeasures, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
       }
          
	}
}