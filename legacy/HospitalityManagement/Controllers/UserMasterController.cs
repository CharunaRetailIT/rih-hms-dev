
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

namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
    public class UserMasterController : Controller
    {

        private readonly BLL_UserMaster _bllusermaster;
        private readonly BLL_UserGroup _bllusergroup;
        private readonly BLL_Employee _bllemployee;
        private readonly AccountController _accountcontroller;
        BLL_Location _location; 
        public UserMasterController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllusermaster = new BLL_UserMaster(cn);
            _bllusergroup = new BLL_UserGroup(cn);
            _bllemployee = new BLL_Employee(cn);
            _accountcontroller = new AccountController();
            _location = new BLL_Location(cn);
        }


        [Authorize(Roles = "BackOfficeUsers")]
        public ActionResult Index()
        {
           // _accountcontroller = new AccountController();
            // var ss =Session["loggeduserempcode"];
            //_accountcontroller.SetPermissionCookie(1, Convert.ToString(""));
            return View();
        }

        public ActionResult Clear()
        {

            return View("Edit");
        }

        [Authorize(Roles = "BackOfficeUsers")]
        public ActionResult Edit(long id)
        {
        
           
            var user = _bllusermaster.GetUserById(id);          
            user.SysUserGroupPermission = _bllusermaster.CompairPermissions(user.UserGroupID,user.EmployeeCode);
            ViewBag.SysUserMasterID = user.SysUserMasterID;
            ViewBag.UserGroupID = user.UserGroupID;
            @ViewBag.GroupName = _bllusergroup.GetUserGroupById(user.UserGroupID).UserGroupName;
            return View(user);
        }

        [Authorize(Roles = "BackOfficeUsers")]
        [HttpPost]
        public ActionResult Edit(SysUserMaster sysusermaster)
        {
           
           // var errors = ModelState.Values.SelectMany(v => v.Errors);
          
            long groupid = _bllusermaster.GetUserById(sysusermaster.SysUserMasterID).UserGroupID;
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (!ModelState.IsValid)
            {
                //ViewBag.UserGroupID = userService.GetUserById(sysusermaster.SysUserMasterID).UserGroupID;              
                ViewBag.GroupName = _bllusergroup.GetUserGroupById(groupid).UserGroupName;                
                return View(sysusermaster);
            }
            if (sysusermaster.SysUserGroupPermission.Where(p=>p.IsGrant==true).Count()==0)
            {
                @ViewBag.GroupName = _bllusergroup.GetUserGroupById(sysusermaster.UserGroupID).UserGroupName;
                ViewBag.UserGroupID = _bllusermaster.GetUserById(sysusermaster.SysUserMasterID).UserGroupID;
                ViewBag.Message = "2";
                return View(sysusermaster);
            }
            sysusermaster.SysUserGroupPermission.ForEach(p=>{p.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString()); });

            var user = _bllusermaster.GetUserById(sysusermaster.SysUserMasterID);           
            user.EmployeeCode = sysusermaster.EmployeeCode;
            user.UserName = sysusermaster.UserName;
            user.Email = sysusermaster.Email;
            user.UserDescription = sysusermaster.UserDescription;
            user.Password = sysusermaster.Password;
            user.ConfirmPassword = sysusermaster.ConfirmPassword;
            user.UserGroupID = sysusermaster.UserGroupID;
            user.IsActive = sysusermaster.IsActive;
            user.IsUserCantChangePassword = sysusermaster.IsUserCantChangePassword;
            user.IsUserMustChangePassword = sysusermaster.IsUserMustChangePassword;
            user.ModifiedDate = DateTime.Now;
            user.CreatedDate = DateTime.Now;
            user.ModifiedUser = Session["loggeduser"].ToString();
            user.SysUserGroupPermission = sysusermaster.SysUserGroupPermission;
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            var emp = _bllemployee.GetEmployeeByCode(sysusermaster.EmployeeCode, companyid);
            if(emp != null)
            user.LocationId = emp.LocationId;
            


            //Added by pavi on 2019-12-01
            user.IsDelete = sysusermaster.IsDelete;

            //   var errors = ModelState.Values.SelectMany(v => v.Errors);
            @ViewBag.Uname = sysusermaster.UserName;

            if (_bllusermaster.UpdateUserMaster(user))
            {

                ViewBag.Message = "1";
                ModelState.Clear();
                return View(new SysUserMaster());
            }
            else
            {
                ViewBag.Message = "0";
                return View(sysusermaster);
            }

          
        }

        [Authorize(Roles = "BackOfficeUsers")]
        public ActionResult ViewUsers()
        {

            //var ss = Session["loggeduserempcode"];
            //_accountcontroller.SetPermissionCookie(1, Convert.ToString(ss));

            //GroupOfCompanyService gocreporsitory = new GroupOfCompanyService();
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var users = _bllusermaster.GetUsers(companyid).OrderBy(c => c.EmployeeCode);
            users.ToList().ForEach(c =>
            {

            //    c.UserName = userreporsitory.GetSysUserMasterID(c.SysUserMasterID).GroupOfCompanyName;
            });

            return View(users);
        }

        [Authorize(Roles = "BackOfficeUsers")]
        public void HMSUsers()
        {

            
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var users = _bllusermaster.GetUsers(companyid).OrderBy(c => c.EmployeeCode);
            users.ToList().ForEach(c =>
            {

                //    c.UserName = userreporsitory.GetSysUserMasterID(c.SysUserMasterID).GroupOfCompanyName;
            });


            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Users");
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

            Sheet.Cells[6, 2].Value = "User Details Report";
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
            Sheet.Cells[8, 1].Value = "User Code";
            Sheet.Cells[8, 2].Value = "User Name";
            Sheet.Cells[8, 3].Value = "Description";
            Sheet.Cells[8, 4].Value = "Is Active";
            //Sheet.Cells["A1"].Value = "UserCode";
            //Sheet.Cells["B1"].Value = "UserName";
            //Sheet.Cells["C1"].Value = "Description";
            //Sheet.Cells["D1"].Value = "IsActive";
            int row = 9;
            foreach (var item in users)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.EmployeeCode;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.UserName;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.UserDescription;
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
            Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSUsers.xlsx");

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
        public ActionResult Create(SysUserMaster sysusermaster)
        {
            sysusermaster.CreatedUser = Session["loggeduser"].ToString();
            sysusermaster.CreatedDate = DateTime.Now;
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var employee = _bllemployee.GetEmployeeByCode(sysusermaster.EmployeeCode,companyid);
            sysusermaster.LocationId = employee.LocationId;
            ///sysusermaster.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());

            //Added by pavi on 2019-12-01
            sysusermaster.IsActive = true;
            sysusermaster.CompanyID = companyid;
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            ViewBag.EmployeeCode = sysusermaster.EmployeeCode;
            ViewBag.UserGroupID = sysusermaster.UserGroupID;
            ViewBag.EmpName = employee.EmployeeName;
            if (!ModelState.IsValid)
            {
                
                return View("Index", sysusermaster);
            }
            //UserServise reporsitory = new UserServise();
            var existuser = _bllusermaster.GetUserByUserName(sysusermaster.UserName);
            if (existuser == null)
            {
                var exists = _bllusermaster.GetUserEmpNoAndUserName(sysusermaster.EmployeeCode, sysusermaster.UserName);
                if (exists == null)
                {

                    if (_bllusermaster.SaveUserMaster(sysusermaster))
                    {

                        @ViewBag.UMID = sysusermaster.UserName;
                        @ViewBag.Message = "1";
                        return View("Index", new SysUserMaster());
                    }
                    else
                    {
                        @ViewBag.Message = "0";
                        @ViewBag.UMID = sysusermaster.UserName;
                        return View("Index", sysusermaster);
                    }
                }
                else
                {

                    @ViewBag.Message = "2";
                    var users = _bllusermaster.GetUsers(companyid).OrderBy(c => c.EmployeeCode);
                    @ViewBag.UMID = sysusermaster.UserName;
                    return View("Index", new SysUserMaster());

                }
            }
            else
            {
                @ViewBag.Message = "3";
                var users = _bllusermaster.GetUsers(companyid).OrderBy(c => c.EmployeeCode);
                @ViewBag.UMID = sysusermaster.UserName;
                return View("Index", new SysUserMaster());
            }
        }
        [HttpGet]
        public JsonResult GetActiveUsers()
        {
            //  UserServise reporsitory = new UserServise();
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var users = _bllusermaster.GetUsers(companyid);
            return Json(JsonConvert.SerializeObject(users, Formatting.None, 
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), 
            JsonRequestBehavior.AllowGet);
        }

        public JsonResult CompairPermissions(long groupid,string empcode)
        {
           // UserServise reporsitory = new UserServise();
            var permissions = _bllusermaster.CompairPermissions(groupid,empcode).ToList();        
            return Json(permissions, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ChangePassword()
        {
            SysUserMaster usermaster = new SysUserMaster();
            usermaster.UserName = Session["loggeduser"].ToString();
            return View(usermaster);
        }

        [HttpPost]
        public ActionResult ChangePassword(SysUserMaster user)
        {
        
          //  UserServise userservice = new UserServise();
            var u = _bllusermaster.GetUser(user.UserName,user.OldPassword);
            if (u != null)
            {
                if (user.Password != null && user.ConfirmPassword != null)
                {
                    if ((user.Password == user.ConfirmPassword))
                    {
                        u.Password = user.Password;
                        u.ConfirmPassword = user.ConfirmPassword;

                        var res = _bllusermaster.ChangePassword(u);
                        if (res == 1)
                        {
                            @ViewBag.Message = "1";
                            return RedirectToAction("Login", "Account");
                        }
                        else
                        {
                            @ViewBag.Message = "0";
                        }
                    }
                    else
                    {
                        @ViewBag.Message = "2";
                    }
                }
                else
                {
                    @ViewBag.Message = "2";
                }
            }
            else
            {
                ModelState.AddModelError("OldPassword","Invalid Old Password");
                return View(new SysUserMaster {UserName= Session["loggeduser"].ToString() });
               
            }
                  
            return View(u);
        }

       
    }
}