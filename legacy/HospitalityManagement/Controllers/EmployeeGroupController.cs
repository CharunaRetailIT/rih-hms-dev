//using HospitalityManagement.Models;
//using HospitalityManagement.Service;
using Newtonsoft.Json;
using RIT.HMS.BLL.MasterData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RIT.HMS.Domain;
using System.Web.Mvc;
using OfficeOpenXml;
using System.IO;
using System.Data;
using OfficeOpenXml.Style;
using System.Drawing;
namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
    public class EmployeeGroupController : Controller
    {

        BLL_EmployeeGroup _employeeGroup;
        BLL_Location _location; 
        public EmployeeGroupController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _employeeGroup = new BLL_EmployeeGroup(cn);
            _location = new BLL_Location(cn);
        }

        [Authorize(Roles = "EmpGroupCreatee")]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Clear()
        {

            return View("Edit");
        }

        [Authorize(Roles = "EmpGroupEdit")]
        public ActionResult Edit(long id)
        {
            //EmployeeGroupService empgroupreporsitory = new EmployeeGroupService();
         
            var empgroup = _employeeGroup.GetEmployeeGroupById(id);
            ViewBag.EmployeeGroupID = empgroup.EmployeeGroupID;
            return View(empgroup);
        }

        [Authorize(Roles = "EmpGroupEdit")]
        [HttpPost]
        public ActionResult Edit(EmployeeGroup employeegroup)
        {
            //EmployeeGroupService empgroupreporsitory = new EmployeeGroupService();
          
            var empgroup = _employeeGroup.GetEmployeeGroupById(employeegroup.EmployeeGroupID);
            empgroup.EmployeeGroupCode = employeegroup.EmployeeGroupCode;
            empgroup.EmployeeGroupName = employeegroup.EmployeeGroupName;
            empgroup.IsDelete = employeegroup.IsDelete;
            empgroup.ModifiedDate = DateTime.Now;
            empgroup.ModifiedUser = Session["loggeduser"].ToString();
            empgroup.IsSteward = employeegroup.IsSteward;
            empgroup.CompanyID= Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            if (_employeeGroup.UpdateEmployeeGroup(empgroup) == 1)
            {

                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.Message = "0";
            }

            return View();
        }

        [Authorize(Roles = "EmpGroupView")]
        public ActionResult ViewEmployeeGroups()
        {
           
            //EmployeeGroupService empgroupreporsitory = new EmployeeGroupService();
            //GroupOfCompanyService gocreporsitory = new GroupOfCompanyService();
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var empgroups = _employeeGroup.GetEmployeeGroups(companyid).OrderBy(c => c.EmployeeGroupCode);

            empgroups.ToList().ForEach(c =>
            {

                //    c.UserName = userreporsitory.GetSysUserMasterID(c.SysUserMasterID).GroupOfCompanyName;
            });

            return View(empgroups);
        }


        [Authorize(Roles = "EmpGroupView")]
        public void HMSEmployeeGroups()
        {

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var empgroups = _employeeGroup.GetEmployeeGroups(companyid).OrderBy(c => c.EmployeeGroupCode);
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Customers");
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

            Sheet.Cells[6, 2].Value = "Employee Group Report";
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
            Sheet.Cells[8, 1].Value = "Employee Group Code";
            Sheet.Cells[8, 2].Value = "Employee Group Name";            
            //Sheet.Cells["A1"].Value = "EmployeeGroupCode";
            //Sheet.Cells["B1"].Value = "EmployeeGroupName";      

            int row = 9;
            foreach (var item in empgroups)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.EmployeeGroupCode;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.EmployeeGroupName;
               
                row++;
            }
            #region Header Bold
            Color colFromHexHeading = System.Drawing.ColorTranslator.FromHtml("#919089");

            Sheet.Cells[8, 1, 8, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells[8, 1, 8, 2].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);

            var table = Sheet.Cells[8, 1, 8, 2];
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
            Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSEmployeeGroups.xlsx");

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                Ep.SaveAs(MyMemoryStream);
                MyMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
        }

        [Authorize(Roles = "EmpGroupCreatee")]
        [HttpPost]
        public ActionResult Create(EmployeeGroup employeegroup)
        {
            employeegroup.CreatedUser = Session["loggeduser"].ToString();
            employeegroup.CreatedDate = DateTime.UtcNow;
            employeegroup.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            employeegroup.CompanyID= Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            if (!ModelState.IsValid)
            {
                return View("Index",employeegroup);
            }

         
            //EmployeeGroupService reporsitory = new EmployeeGroupService();
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var existsempgrp = _employeeGroup.GetEmpGroupByCode(employeegroup.EmployeeGroupCode,companyid);
            if (existsempgrp != null)
            {
                ViewBag.Message = "3";
                return View("Index",employeegroup);
            }
            if (_employeeGroup.SaveEmployeeGroup(employeegroup) == 1)
            {
                @ViewBag.Message = "1";
                //employeegroup = null;
            }
            else
            {
                @ViewBag.Message = "2";
            }

            ViewBag.EmpGroupCode = employeegroup.EmployeeGroupCode;
            return View("Index",employeegroup);
        }
        
        [HttpGet]
        public JsonResult GetActiveEmployeeGroups()
        {
            //EmployeeGroupService reporsitory = new EmployeeGroupService();
           
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var empgroups = _employeeGroup.GetEmployeeGroups(companyid);
            return Json(JsonConvert.SerializeObject(empgroups, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }
	}
}