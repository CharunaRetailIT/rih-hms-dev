
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain;
using RIT.HMS.BLL.Configurations;
using RIT.HMS.BLL.Reports;
using OfficeOpenXml;
using System.IO;
using System.Data;
using OfficeOpenXml.Style;
using System.Drawing;

namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
  
    public class UserGroupController : Controller
    {
        BLL_UserGroup _blluserGroup;
        BLL_UserGroupPermissions _blluserGroupPermissions;
        private readonly BLL_Configuration _bllconfiguration;
        BLL_Reports _bllreports;
        BLL_Location _location; 
        public UserGroupController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
             _blluserGroup = new BLL_UserGroup(cn);
            _blluserGroupPermissions = new BLL_UserGroupPermissions(cn);
            _bllconfiguration = new BLL_Configuration(cn);
            _bllreports = new BLL_Reports(cn);
            _location = new BLL_Location(cn);
        }


        [Authorize(Roles = "BackOfficeUsers")]
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Clear()
        {

            return View("Edit");
        }

        [Authorize(Roles = "BackOfficeUsers")]
        public ActionResult Edit(long id)
        {
           
            var usergroup = _blluserGroup.GetUserGroupById(id);
            ViewBag.SysUserGroupID = usergroup.SysUserGroupID;
            return View(usergroup);
        }

        [Authorize(Roles = "BackOfficeUsers")]
        [HttpPost]
        public ActionResult Edit(SysUserGroup sysusergroup)
        {

            if (!ModelState.IsValid)
            {
                return View(sysusergroup);
            }


          
            var usergroup = _blluserGroup.GetUserGroupById(sysusergroup.SysUserGroupID);
            usergroup.UserGroupCode = sysusergroup.UserGroupCode;
            usergroup.UserGroupName = sysusergroup.UserGroupName;
            usergroup.IsDelete = sysusergroup.IsDelete;
            usergroup.ModifiedDate = DateTime.Now;
            usergroup.ModifiedUser = Session["loggeduser"].ToString();
            usergroup.CompanyID= Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            if (_blluserGroup.UpdateUserGroup(usergroup) == 1)
            {

                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.Message = "0";
            }

            return View();
        }

        [Authorize(Roles = "BackOfficeUsers")]
        public ActionResult ViewUserGroups()
        {

          
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var usergroups = _blluserGroup.GetUserGroups(companyid).OrderBy(c => c.UserGroupCode);

            usergroups.ToList().ForEach(c =>
            {

                //    c.UserName = userreporsitory.GetSysUserMasterID(c.SysUserMasterID).GroupOfCompanyName;
            });

            return View(usergroups);
        }

        [Authorize(Roles = "BackOfficeUsers")]
        public void HMSUserGroups()
        {


            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var usergroups = _blluserGroup.GetUserGroups(companyid).OrderBy(c => c.UserGroupCode);

            usergroups.ToList().ForEach(c =>
            {

                //    c.UserName = userreporsitory.GetSysUserMasterID(c.SysUserMasterID).GroupOfCompanyName;
            });

            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("UserGroups");
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

            Sheet.Cells[6, 2].Value = "User Group Report";
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
            Sheet.Cells[8, 1].Value = "User Group Code";
            Sheet.Cells[8, 2].Value = "User Group Name";
            Sheet.Cells[8, 3].Value = "Is Delete";

            //Sheet.Cells["A1"].Value = "UserGroupCode";
            //Sheet.Cells["B1"].Value = "UserGroupName";
            //Sheet.Cells["C1"].Value = "IsDelete";
           
            int row = 9;
            foreach (var item in usergroups)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.UserGroupCode;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.UserGroupName;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.IsDelete;   
                row++;
            }
            #region Header Bold
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
            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSUserGroups.xlsx");

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                Ep.SaveAs(MyMemoryStream);
                MyMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }



        }


        [Authorize(Roles = "BackOfficeUsers")]
        [HttpPost]
        public ActionResult Create(SysUserGroup sysusergroup)
        {
            sysusergroup.CreatedUser = Session["loggeduser"].ToString();
            sysusergroup.CreatedDate = DateTime.Now;
            sysusergroup.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            sysusergroup.CompanyID= Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            if (!ModelState.IsValid)
            {
                return View("Index", sysusergroup);
            }

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var existusergroup = _blluserGroup.GetUserGroupByCode(sysusergroup.UserGroupCode,companyid);
            if (existusergroup != null)
            {
                ViewBag.Message = "3";
                return View("Index", sysusergroup);
            }
            if (_blluserGroup.SaveUserGroup(sysusergroup) == 1)
            {
                @ViewBag.Message = "1";
                //sysusergroup = null;
            }
            else
            {
                @ViewBag.Message = "2";
            }

            ViewBag.UserGroupCode = sysusergroup.UserGroupCode;
            return View("Index", sysusergroup);
        }

        public JsonResult GetAllPermissions(long id)
        {
          
           
            var permissions = _blluserGroupPermissions.GetByGroupId(id, Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ToList();
            foreach (var permission in permissions)
            {
                permission.IsGrant = true;
            }
            return Json(permissions, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAllPOSPermissions(long id, int employeeid)
         {
         
            var permissions = _blluserGroupPermissions.GetByPOSGroupId(id).ToList();
            var userpermisions = _blluserGroupPermissions.GetByEmpId(employeeid).ToList();
           
            foreach (var permission in permissions)               
            {
               // if (userpermisions.Any(a => a.FunctionName == permission.FunctionName) || userpermisions.Count == 0)
                if (userpermisions.Any(a => a.FunctionName == permission.FunctionName) && userpermisions.Count !=  0)
                {
                    permission.IsGrant = true;
                    permission.Value = userpermisions.Where(d => d.FunctionName == permission.FunctionName).FirstOrDefault().Value;
                    permission.MaxValue = userpermisions.Where(d => d.FunctionName == permission.FunctionName).FirstOrDefault().MaxValue;
                }
                else
                {
                    permission.IsGrant = false;
                }
               
            }
            return Json(permissions, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetActiveUserGroups()
        {
          
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var usergroups = _blluserGroup.GetUserGroups(companyid);
            return Json(JsonConvert.SerializeObject(usergroups, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetActiveUserGroupes()
        {
           
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var usergroupesdd = _blluserGroup.GetActiveUserGroups(companyid);
            return Json(JsonConvert.SerializeObject(usergroupesdd, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetActivePOSUserGroupes()
        {
           
            var posusergroups = _blluserGroup.GetActivePOSUserGroups();
            return Json(JsonConvert.SerializeObject(posusergroups, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetActiveFunctions()
        {
           
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var functions = _blluserGroup.GetUserFunctions(companyid).ToList();

            if (_bllconfiguration.GetConfiguration("UReports",Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {
                          
                foreach (var s in _bllreports.GetSSRSReports(companyid))
                {
                    SysUserFunction ssrsreport = new SysUserFunction();
                    ssrsreport.SysUserFunctionID = Convert.ToInt32(s.ReportInfoId);
                    ssrsreport.FunctionName = "SSRS" + s.ReportName.Trim().Replace(" ", "");
                    ssrsreport.FunctionDescription = s.ReportName;
                    ssrsreport.Order = 1;
                    ssrsreport.TypeID = 1;
                    ssrsreport.IsDelete = false;
                    ssrsreport.IsValue = true;
                    ssrsreport.GroupOfCompanyID = 1;
                    ssrsreport.CompanyID = 1;
                    ssrsreport.LocationId = 1;
                    ssrsreport.CreatedDate = DateTime.Now;
                    ssrsreport.CreatedUser = Session["loggeduser"].ToString();
                    ssrsreport.ModifiedUser = "";
                    ssrsreport.ModifiedDate = DateTime.Now;
                    ssrsreport.DataTransfer = 1;
                    ssrsreport.FormId = 999;
                    functions.Add(ssrsreport);
                }


            }
           return Json(JsonConvert.SerializeObject(functions, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }


    }
}      