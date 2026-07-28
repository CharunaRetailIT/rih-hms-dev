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
  
    public class TableMasterController : Controller
    {
        BLL_Table _blltable;
        BLL_Location _location; 
        public TableMasterController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
             _blltable = new BLL_Table(cn);
             _location = new BLL_Location(cn);
        }


        [Authorize(Roles = "TablesAndChairs")]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Clear()
        {

            return View("Edit");
        }

        [Authorize(Roles = "TablesAndChairs")]
        public ActionResult Edit(long id)
        {
          
            var tablemaster = _blltable.GetTableById(id);
            ViewBag.TableMasterID = tablemaster.TableMasterID;
            ViewBag.LocationId = tablemaster.LocationId;
            ViewBag.InterDeptId = tablemaster.InterDeptId;
            return View(tablemaster);
        }

        [Authorize(Roles = "TablesAndChairs")]
        [HttpPost]
        public ActionResult Edit(TableMaster tablemasters)
        {

            if (!ModelState.IsValid)
            {
                return View();
            }

         
            var tablemaster = _blltable.GetTableById(tablemasters.TableMasterID);
            tablemaster.TableCode = tablemasters.TableCode;
            tablemaster.TableName = tablemasters.TableName;
            tablemaster.LocationId = tablemasters.LocationId;
            tablemaster.InterDeptId = tablemasters.InterDeptId;
            tablemaster.NumberOfSeats = tablemasters.NumberOfSeats;
            tablemaster.TablePositionX = tablemasters.TablePositionX;
            tablemaster.TablePositionY = tablemasters.TablePositionY;
            tablemaster.IsDelete = tablemasters.IsDelete;
            tablemaster.ModifiedDate = DateTime.Now;
            tablemaster.ModifiedUser = Session["loggeduser"].ToString();
            tablemaster.TableState = tablemasters.TableState;
            tablemaster.CompanyID = tablemasters.CompanyID;
            tablemaster.LocationId = tablemasters.LocationId;
            tablemaster.InterDeptId = tablemaster.InterDeptId;
            tablemaster.TableState = "Empty";
            tablemaster.CompanyID= Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            ViewBag.LocationId = tablemaster.LocationId;
            ViewBag.InterDeptId = tablemaster.InterDeptId;



            if (_blltable.UpdateTable(tablemaster) == 1)
            {

                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.Message = "0";
            }

            return View();
        }

        [Authorize(Roles = "TablesAndChairs")]
        public void HMSTables()
        {

         
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var tables = _blltable.GetTables(companyid).OrderBy(c => c.TableCode);

            tables.ToList().ForEach(c =>
            {

                //    c.UserName = userreporsitory.GetSysUserMasterID(c.SysUserMasterID).GroupOfCompanyName;
            });

            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Tables");
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

            Sheet.Cells[6, 2].Value = "Table Master Report";
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
            Sheet.Cells[8, 1].Value = "Table Code";
            Sheet.Cells[8, 2].Value = "Table Name";
            Sheet.Cells[8, 3].Value = "Status";
            Sheet.Cells[8, 4].Value = "Is Delate";
            //Sheet.Cells["A1"].Value = "TableCode";
            //Sheet.Cells["B1"].Value = "TableName";
            //Sheet.Cells["C1"].Value = "Status";
            //Sheet.Cells["D1"].Value = "IsDelate";
            int row = 9;
            foreach (var item in tables)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.TableCode;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.TableName;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.TableState;
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
            Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSTables.xlsx");

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                Ep.SaveAs(MyMemoryStream);
                MyMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }



        }

        [Authorize(Roles = "TablesAndChairs")]
        public ActionResult ViewTables()
        {


            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var tables = _blltable.GetTables(companyid).OrderBy(c => c.TableCode);

            tables.ToList().ForEach(c =>
            {

                //    c.UserName = userreporsitory.GetSysUserMasterID(c.SysUserMasterID).GroupOfCompanyName;
            });

            return View(tables);
        }

        [Authorize(Roles = "TablesAndChairs")]
        [HttpPost]
        public ActionResult Create(TableMaster tablemasters)
        {
            tablemasters.CreatedUser = Session["loggeduser"].ToString();
            tablemasters.CreatedDate = DateTime.UtcNow;
            //tablemasters.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            tablemasters.CompanyID= Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            if (!ModelState.IsValid)
            {
                return View("Index");
            }

         
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var existstbl = _blltable.GetTableByCode(tablemasters.TableCode,companyid);
            if (existstbl != null)
            {
                ViewBag.Message = "3";
                return View("Index", tablemasters);
            }
            if (_blltable.SaveTable(tablemasters) == 1)
            {
                @ViewBag.Message = "1";
                //tablemasters = null;
            }
            else
            {
                @ViewBag.Message = "2";
            }

            ViewBag.TblCode = tablemasters.TableCode;
            return View("Index", tablemasters);

        }


        [HttpGet]
        public JsonResult GetActiveTables()
        {
           
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var tables = _blltable.GetTables(companyid);
            return Json(JsonConvert.SerializeObject(tables, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

	}
}