using RIT.HMS.HMSOrderTaker.BLL.Masters;
using RIT.HMS.HMSOrderTaker.Domain.ViewModels;
using RIT.HMS.HMSOrderTaker.BLL.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HMSOrderTaker.Controllers.STOS
{
    public class STOSSysUserController : Controller
    {
        BLL_Locations _blllocations = null;
        BLL_Auth _bllauth = null;
        public STOSSysUserController()
        {
            _blllocations = new BLL_Locations();
            _bllauth = new BLL_Auth();
        }
        public ActionResult Index()
        {        

           return View("~/Views/STOSSysUser/UserLogin.cshtml", new vmAccount());        
        }
       // [AllowAnonymous]
        public ActionResult UserLogin(vmAccount user)
        {
            
            var exists = _bllauth.GetUserDetailsByPassword(user.Password).FirstOrDefault();
            if (exists != null)
            {

                Session["loggeduser"] = exists.JournalName;
                Session["MessageToUser"] = "";
                Session["CompanyId"] = 1;
                Session["SignedIn"] = true;
                Session["LocationId"] = exists.LocationId;

                return RedirectToAction("BookTable", "STOSOrder");
            }
            else
            {
                Session["MessageToUser"] = "Invalid Login Attempt...!";
                return RedirectToAction("Index", "STOSSysUser");
            }
        }

        [AllowAnonymous]
        public ActionResult LogOut()
        {
            Session["SignedIn"] = false;
            return View("~/Views/STOSSysUser/UserLogin.cshtml", new vmAccount());
        }
            
    }
}