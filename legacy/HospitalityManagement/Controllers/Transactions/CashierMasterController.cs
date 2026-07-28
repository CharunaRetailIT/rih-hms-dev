using Newtonsoft.Json;
using OfficeOpenXml;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using OfficeOpenXml.Style;
using System.Drawing;
namespace HospitalityManagement.Controllers.Transactions
{
    [SessionTimeout]
    public class CashierMasterController : Controller
    {
        BLL_CashierPermission _bllcashierpermission;
        BLL_Employee _bllemployee;
        BLL_Location _blllocation;
        BLL_UserGroupPermissions _blluserGroupPermissions;
        BLL_UserGroup _blluserGroup;
        BLL_Location _location;
        public CashierMasterController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
           _bllcashierpermission = new BLL_CashierPermission(cn);
           _bllemployee = new BLL_Employee(cn);
           _blllocation = new BLL_Location(cn);
           _blluserGroupPermissions = new BLL_UserGroupPermissions(cn);
           _blluserGroup = new BLL_UserGroup(cn);
           _location = new BLL_Location(cn);

        }

        [Authorize(Roles = "POSUsers")]
        [HttpGet]
        public ActionResult ViewCashiers()
        {
            var cashiers = _bllcashierpermission.GetPOSUsers(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            List<Employee> emps = new List<Employee>();
            foreach (var c in cashiers)
            {
                string[] employee = c.Split(',');
                Employee emp = new Employee();
                try
                {
                    var dbemp = _bllemployee.GetEmployeeById(Convert.ToInt64(employee[0]));
                    emp.EmployeeID = dbemp.EmployeeID;
                    emp.EmployeeCode = dbemp.EmployeeCode;
                    emp.EmployeeName = dbemp.EmployeeName;
                    emp.LocationName = _blllocation.GetLocationById(Convert.ToInt64(employee[1])).LocationName;
                    emps.Add(emp);
                }
                catch (Exception)
                {

                }
            }

            return View(emps);
        }

        [Authorize(Roles = "POSUsers")]
       
        public void HMSCashiers()
        {
            var cashiers = _bllcashierpermission.GetPOSUsers(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            List<Employee> emps = new List<Employee>();
            foreach (var c in cashiers)
            {
                string[] employee = c.Split(',');
                Employee emp = new Employee();
                try
                {
                    var dbemp = _bllemployee.GetEmployeeById(Convert.ToInt64(employee[0]));
                    emp.EmployeeID = dbemp.EmployeeID;
                    emp.EmployeeCode = dbemp.EmployeeCode;
                    emp.EmployeeName = dbemp.EmployeeName;
                    emp.LocationName = _blllocation.GetLocationById(Convert.ToInt64(employee[1])).LocationName;
                    emps.Add(emp);
                }
                catch (Exception)
                {

                }
            }

            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Users");
            //---------------2023/03/30 ----------Tharaka---------------
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

            Sheet.Cells[6, 2].Value = "Cashier Details Report";
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
            Sheet.Cells[8, 1].Value = "Cashier Code";
            Sheet.Cells[8, 2].Value = "Cashier Name";
            Sheet.Cells[8, 3].Value = "Location";

            //Sheet.Cells["A1"].Value = "CashierCode";
            //Sheet.Cells["B1"].Value = "CashierName";
            //Sheet.Cells["C1"].Value = "Location";  

            int row = 9;
            foreach (var item in emps)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.EmployeeCode;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.EmployeeName;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.LocationName;
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
            Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSCashiers.xlsx");

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                Ep.SaveAs(MyMemoryStream);
                MyMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }

        }

        [Authorize(Roles = "POSUsers")]
        [HttpGet]
        public ActionResult CashierGroupPermission()
        {
            return View();
        }
        [HttpGet]
        public JsonResult GetActivePOSFunctions()
        {
          
            var functions = _blluserGroup.GetCashierFunctions().ToList();
            return Json(JsonConvert.SerializeObject(functions, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetPermissions(long id)
        {

            var pers = _blluserGroupPermissions.GetByPOSGroupId(id);
            return Json(JsonConvert.SerializeObject(pers, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCashierFunctionById(long id)
        {
           
            var function = _blluserGroup.GetCashierFunctionById(id);
            return new JsonResult { Data = function, JsonRequestBehavior = JsonRequestBehavior.AllowGet };


            // return View("CreateGroupPermissions");
        }

        [HttpPost]
        public ActionResult CreatePOSUserPermission(List<CashierGroup> usrpermissions)
        {

            if (usrpermissions == null)
            {
                return View("CashierGroupPermission");
            }

            int k = _blluserGroupPermissions.DeletePermissionsByCashierGroup(usrpermissions.FirstOrDefault().EmployeeGroupId);
            int x = 0;

            foreach (CashierGroup grp in usrpermissions)
            {

                //grp.Order = grp.Order;
                //grp.Value = grp.Value;
                //grp.MaxValue = grp.MaxValue;
                grp.Type = null;
                grp.CreatedUser = Session["loggeduser"].ToString();
                grp.ModifiedUser= Session["loggeduser"].ToString();
                grp.CreatedDate = DateTime.Now;
                grp.ModifiedDate = DateTime.Now;
                grp.DataTransfer = 0;
                
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                x = _blluserGroupPermissions.SaveCashierGroup(grp);

            }

            return Json(usrpermissions.FirstOrDefault().EmployeeGroupId);     
        }

        [HttpGet]
        public ActionResult CreateCashier()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateCashier(CashierPermission cashierpermission)
        {
            cashierpermission.CreatedDate = DateTime.Now;
            cashierpermission.CreatedUser = Session["loggeduser"].ToString();
            cashierpermission.ModifiedDate = DateTime.Now;
            cashierpermission.ModifiedUser = Session["loggeduser"].ToString();
            cashierpermission.DataTransfer = 0;
            if (cashierpermission.Password != cashierpermission.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Password did not matched");
                return View(new CashierPermission());
            }

            if (cashierpermission.LocationId == 0)
            {
                ModelState.AddModelError("LocationId", "Select a location");
                return View(new CashierPermission());
            }
            if (cashierpermission.IsActive == true && cashierpermission.IsDelete == true)
            {
                @ViewBag.Message = "3";
                return View(new CashierPermission());
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            var res = _bllcashierpermission.SaveCashierPermissions(cashierpermission);
            if (res == 0)
            {
                @ViewBag.Message = "0";
                return View(new CashierPermission());
            }
            else if (res == 2)
            {
                @ViewBag.Message = "2";
                return View(new CashierPermission());
            }
            else if (res == 4)
            {
                @ViewBag.Message = "4";
                //return View(cashierpermission);
                return View(new CashierPermission());
            }
            else if (res > 0)
            {
                @ViewBag.Message = "1";
                return View(new CashierPermission());
            }

            return View(new CashierPermission());
        }

        [HttpGet]
        public ActionResult EditCashier(string code,int id)
        {

            var cashier = _bllcashierpermission.GetCashierByEmpId(id);
            @ViewBag.locationId = cashier.LocationId;
            @ViewBag.EmployeeCode = cashier.EmployeeId;
            cashier.ConfirmPassword = cashier.Password;
            return View(cashier);
        }

        [HttpPost]
        public ActionResult EditCashier(CashierPermission cashierpermission)
        {
            cashierpermission.CreatedDate = DateTime.Now;
            cashierpermission.CreatedUser = Session["loggeduser"].ToString();
            cashierpermission.ModifiedDate = DateTime.Now;
            cashierpermission.ModifiedUser = Session["loggeduser"].ToString();
            if (cashierpermission.Password != cashierpermission.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Password did not matched");
                return View(new CashierPermission());
            }

            if (cashierpermission.LocationId == 0)
            {
                ModelState.AddModelError("LocationId", "Select a location");
                return View(new CashierPermission());
            }
            if (cashierpermission.IsActive == true && cashierpermission.IsDelete == true)
            {
                @ViewBag.Message = "3";
                return View(new CashierPermission());
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            var res = _bllcashierpermission.SaveCashierPermissions(cashierpermission);
            if (res == 0)
            {
                @ViewBag.Message = "0";
                // return View();
                return View(new CashierPermission());
            }
            else if (res == 2)
            {
                @ViewBag.Message = "2";
                //return View();
                return View(new CashierPermission());
            }
            else if (res == 4)
            {
                @ViewBag.Message = "4";
                //return View(cashierpermission);
                return View(new CashierPermission());
            }
            else if (res > 0)
            {
                @ViewBag.Message = "1";
                ModelState.Clear();
                return View(new CashierPermission());
            }
            return View();
        }

        //[HttpGet]
        //public JsonResult GetActiveUserGroupes()
        //{
        //    BLL_UserGroup _blluserGroup = new BLL_UserGroup();
        //    var usergroupesdd = _blluserGroup.GetActivePOSUserGroups();
        //    return Json(JsonConvert.SerializeObject(usergroupesdd, Formatting.None, new JsonSerializerSettings
        //    { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        //}
    }
}