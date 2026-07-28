using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain;

namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
    [Authorize(Roles = "BackOfficeUsers")]
    public class UserGroupPermissionController : Controller
    {
       
        BLL_UserGroupPermissions _blluserGroupPermissions;
        BLL_UserGroup _blluserGroup;

        public  UserGroupPermissionController()
        {
            try {
                string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
                _blluserGroupPermissions = new BLL_UserGroupPermissions(cn);
                _blluserGroup = new BLL_UserGroup(cn);
               
            }
            catch (Exception e)
            {
                 RedirectToAction("Login","Account");
            }
        }
        public ActionResult Index()
        {
            SysUserGroupPermission permission = new SysUserGroupPermission();
           // permission.SysUserFunctions = _blluserGroup.GetUserFunctionsToSelect(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return View("CreateGroupPermissions");
        }

        [HttpPost]
        public ActionResult Create(List<SysUserGroupPermission>  usrpermissions)
        {

            var granted = usrpermissions.Where(p => p.IsGrant == true);

            if (usrpermissions==null || granted.Count()==0)
            {
                return View("CreateGroupPermissions");
            }

            int k = _blluserGroupPermissions.DeletePermissionsByUserGrouypId(usrpermissions.FirstOrDefault().SysUserGroupId, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            int x = 0;

            foreach (SysUserGroupPermission grp in usrpermissions.Where(p=>p.IsGrant==true))
            {
                
                  
                var uF = _blluserGroup.GetUserFunction(grp.SysUserFunctionID);

                grp.FunctionName = uF.FunctionName;
                grp.FunctionDescription = uF.FunctionDescription;
                grp.FormId = uF.FormId;
                grp.TypeID = uF.TypeID;





                grp.GroupOfCompanyID = 1;
                grp.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                grp.Order = 1;
                grp.Value = 1;
                grp.MaxValue = 1;
               // grp.TypeID = 1;
                grp.IsAccess = true;
                grp.IsActive = true;
                grp.IsDelete = false;
                grp.LocationId = 1;
                grp.CreatedDate = DateTime.Now;
                grp.ModifiedDate = DateTime.Now;
                grp.CreatedUser = Session["loggeduser"].ToString();
                grp.ModifiedUser = Session["loggeduser"].ToString();
                grp.DataTransfer = 0;

               
               x= _blluserGroupPermissions.SavePermissions(grp);
                       
            }

            return Json(usrpermissions.FirstOrDefault().SysUserGroupId);

           // return View("CreateGroupPermissions");
        }

        [HttpGet]
        public JsonResult GetPermissions(long id)
        {
          
           // var pers = _blluserGroupPermissions.GetByGroupId(id, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var pers = _blluserGroupPermissions.CompairGroupFunctions(id, 
                Convert.ToInt32(Session["loggedusercompanyId"].ToString())
                ).OrderBy(f=>f.FormId).ThenBy(f=>f.FunctionName);
            return Json(JsonConvert.SerializeObject(pers, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Function(long id,string name)
        {
            SysUserFunction function = new SysUserFunction();
            if (!name.Contains("SSRS"))
            {
                 function = _blluserGroup.GetUserFunction(id);
            }
            else
            {
                function = _blluserGroup.GetUserFunctionReportInfo(id, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            }
            return new JsonResult { Data = function, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
          
        }
    }
}